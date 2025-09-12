using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Topic_of_Love.Mian.Patches;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Topic_of_Love.Mian.CustomAssets.Custom;

public class LikeNeuronElement :
    MonoBehaviour,
    IBeginDragHandler,
    IEventSystemHandler,
    IDragHandler,
    IEndDragHandler,
    IInitializePotentialDragHandler
{
    public const float SCALE_HIGHLIGHTED = 1.6f;
    public const float SCALE_SPAWN_IMPULSE = 1.5f;
    public const float SCALE_RECEIVE_IMPULSE_INCREASE = 0.1f;
    public const float SCALE_NORMAL = 1f;
    public float render_depth;
    public bool highlighted;
    public List<LikeNeuronElement> connected_neurons = new();
    public Image[] images = Array.Empty<Image>();
    public float scale_mod_spawn = 1f;
    public float bonus_scale = 1f;
    public List<LikeAxonElement> _axons = new();
    public LikesOverview _neurons_overview;
    public float _spawn_timer;
    public float _spawn_interval;
    public bool _center;
    public TooltipData _tooltip_data;
    public Actor actor;
    public Like like;

    public void Start()
    {
        _neurons_overview = gameObject.GetComponentInParent<LikesOverview>();
        initClick();
        initTooltip();
    }

    public void OnBeginDrag(PointerEventData pEventData)
    {
        _neurons_overview?.SendMessage(nameof(OnBeginDrag), pEventData);
    }

    public void OnDrag(PointerEventData pEventData)
    {
        _neurons_overview?.SendMessage(nameof(OnDrag), pEventData);
    }

    public void OnEndDrag(PointerEventData pEventData)
    {
        _neurons_overview?.SendMessage(nameof(OnEndDrag), pEventData);
    }

    public void OnInitializePotentialDrag(PointerEventData pEventData)
    {
        _neurons_overview?.SendMessage(nameof(OnInitializePotentialDrag), pEventData);
    }

    public void initClick()
    {
        Button component;
        if (!TryGetComponent(out component))
            return;
        component.onClick.AddListener(setPressed);
    }

    public void initTooltip()
    {
        TipButton component;
        if (!TryGetComponent(out component))
            return;
        Destroy(component);
    }

    public void showTooltip()
    {
        if (isCenter())
        {
            Tooltip.show(this, "tip", new TooltipData
            {
                tip_name = "toggle_all_neurons",
                tip_description = "toggle_all_neurons_description"
            });
        }
        else
        {
            if (!hasLike())
                return;
            _tooltip_data = new ExtendedToolTipData
            {
                like_neuron = this,
                tooltip_scale = Mathf.Lerp(0.4f, 1f, render_depth)
            };
            Tooltip.show(this, "like_neuron", _tooltip_data);
        }
    }

    public void clearImages()
    {
        images = Array.Empty<Image>();
        
        for (var i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    public void setupLikeAndActor(Like pLike, Actor pActor)
    {
        like = pLike;
        actor = pActor;

        clearImages();
        
        var icon = like.GetIcon();

        var localScale = icon.transform.localScale;
        icon.transform.SetParent(transform, false);
        icon.transform.localScale = new Vector3(localScale.x + 0.1325f, localScale.y + 0.1325f);
        
        images = transform.GetComponentsInChildren<Image>();
        
        _spawn_interval = Randy.randomFloat(3f, 15f);
        _spawn_timer = Randy.randomFloat(0.0f, _spawn_interval);
    }

    public void updateColorsAndTooltip()
    {
        if (!highlighted)
        {
            scale_mod_spawn -= Time.deltaTime * 2f;
            scale_mod_spawn = Mathf.Max(1f, scale_mod_spawn);
        }
        else
        {
            if (!hasLike() || !Tooltip.isShowingFor(this))
                return;
            _tooltip_data.tooltip_scale = Mathf.Lerp(0.4f, 1f, render_depth);
        }
    }

    public void updateSpawnTimer()
    {
        if (_spawn_timer <= 0.0 || !isLikeEnabled())
            return;
        _spawn_timer -= Time.deltaTime;
    }

    public void spawnImpulseFromHere()
    {
        scale_mod_spawn = Math.Max(1.5f, scale_mod_spawn);
        _spawn_timer = _spawn_interval;
        foreach (var axon in _axons)
            axon.mod_light = 1f;
    }

    public bool isLikeEnabled()
    {
        return !hasLike() || actor.HasLike(like);
    }

    public void setHighlighted()
    {
        if (highlighted)
            return;
        highlighted = true;
        scale_mod_spawn = 1.6f;
        showTooltip();
    }

    public void setPressed()
    {
        if (_neurons_overview.isDragging() || (!hasLike() && !isCenter()))
            return;
        _neurons_overview.setLatestTouched(this);
        if (!InputHelpers.mouseSupported)
        {
            if (!Tooltip.isShowingFor(this))
            {
                showTooltip();
                return;
            }
        }
        else
        {
            showTooltip();
        }

        scale_mod_spawn = 1.6f;
        if (isCenter())
            centerBrainClick();
        else
        {
            actor.ToggleLike(like);
            StatPatch.UpdateOrientationStats(_neurons_overview.mainWindow);
        }
        actor.makeConfused(pColorEffect: true);
        _neurons_overview.fireImpulseWaveFromHere(this);
        _neurons_overview.startNewWhat();
        actor.updateStats();
        GetComponentInParent<StatsWindow>().updateStats();
        AchievementLibrary.mindless_husk.check(actor);
    }

    public void centerBrainClick()
    {
        _neurons_overview.switchAllNeurons();
        if (_neurons_overview.getAllState())
        {
            foreach (var image in images)
            {
                image.color = Toolbox.color_clear;
            }
        }
        else
        {
            foreach (var image in images)
            {
                image.color = Toolbox.color_white;
            }
        }
    }

    public void receiveImpulse()
    {
        scale_mod_spawn += 0.1f;
        scale_mod_spawn = Mathf.Min(1.5f, scale_mod_spawn);
    }

    public bool readyToSpawnImpulse()
    {
        return isLikeEnabled() && _spawn_timer <= 0.0;
    }

    public int getSimulatedTimer()
    {
        return (int)_spawn_timer;
    }

    public void clear()
    {
        like = null;
        actor = null;
        connected_neurons.Clear();
        _axons.Clear();
        _center = false;
    }

    public void setColor(Color pColor)
    {
        foreach (var image in images)
        {
            image.color = pColor;
        }
    }

    public bool hasLike()
    {
        return like != null;
    }

    public void addConnection(LikeNeuronElement pConnection, LikeAxonElement pAxon)
    {
        connected_neurons.Add(pConnection);
        _axons.Add(pAxon);
    }

    public void setCenter(bool pState)
    {
        _center = pState;
    }

    public bool isCenter()
    {
        return _center;
    }
}