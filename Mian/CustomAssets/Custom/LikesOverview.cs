using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Topic_of_Love.Mian.Patches;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Input = UnityEngine.Input;

namespace Topic_of_Love.Mian.CustomAssets.Custom;

public class LikesOverview :
    UnitElement,
    IInitializePotentialDragHandler,
    IEventSystemHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public const float DRAGGING_SMOOTHING_TIME = 0.1f;
    public const float ROTATION_BOUNDS = 0.7f;
    public const float ROTATION_BOUNDS_MARGIN = 1.05f;
    public const float DRAG_SPEED = 0.46f;
    public const float DRAG_ROTATE_SPEED = 0.005f;
    public const float MIN_NEURON_CURSOR_DISTANCE = 40f;
    public const float RADIUS_NEURONS = 70f;
    public const float NEURON_SCALE_MIN = 0.8f;
    public const float NEURON_SCALE_MAX = 1.5f;
    public const float BASE_AXON_DISTANCE = 250f;
    public const float DISTANCE_SCALING_FACTOR = 1.5f;
    public static LikesOverview instance;

    public LikeNeuronElement _prefab_neuron;
    public LikeNerveImpulseElement _prefab_nerve_impulse;
    public LikeAxonElement _prefab_axon;

    public RectTransform _parent_axons;
    public RectTransform _parent_nerve_impulses;
    public RectTransform _parent_neurons;
    public GameObject _mind_main;
    public UnitWindow mainWindow;

    public UnitTextManager _text_phrases = NeuronsOverview.instance._text_phrases;
    
    public List<LikeNeuronElement> _neurons = new();
    public LikeNeuronElement _last_activated_neuron;
    public List<LikeNerveImpulseElement> _active_impulses = new();
    public Color _color_neuron_disabled_front = Toolbox.makeColor("#111111");
    public Color _color_neuron_disabled_back = Toolbox.makeColor("#111111", 0.3f);
    public Color _color_neuron_back = Toolbox.makeColor("#A9A9A9", 0.3f);
    public Color _color_neuron_front = Toolbox.makeColor("#DDDDDD");
    public Color _color_axon_default = Toolbox.makeColor("#FFFFFF", 0.1f);
    public Color _color_axon_default_center = Toolbox.makeColor("#FF6666", 0.1f);
    public Color _color_light_axon = Toolbox.makeColor("#3AFFFF", 0.54f);
    public Color _neuron_highlighted = Toolbox.makeColor("#FFFFFF");
    public float _offset_target_x = -0.015f;
    public float _offset_target_y = 0.07f;
    public bool _is_dragging;
    public Vector2 _last_mouse_delta;
    public float _offset_x;
    public float _offset_y;
    public IEnumerable<Like> _likes;
    public LikeNeuronElement _active_neuron;
    public LikeNeuronElement _latest_touched_neuron;
    public bool _all_state = true;
    public ObjectPoolGenericMono<LikeAxonElement> _pool_axons;
    public ObjectPoolGenericMono<LikeNerveImpulseElement> _pool_impulses;
    public ObjectPoolGenericMono<LikeNeuronElement> _pool_neurons;
    
    public override void Awake()
    {
        base.Awake();
        _pool_neurons = new ObjectPoolGenericMono<LikeNeuronElement>(_prefab_neuron, _parent_neurons);
        _pool_impulses =
            new ObjectPoolGenericMono<LikeNerveImpulseElement>(_prefab_nerve_impulse, _parent_nerve_impulses);
        _pool_axons = new ObjectPoolGenericMono<LikeAxonElement>(_prefab_axon, _parent_axons);
    }

    public void Start()
    {
        instance = this;
    }

    public void Update()
    {
        if (!_is_dragging)
        {
            smoothOffsets();
            if (InputHelpers.mouseSupported)
            {
                _active_neuron = getHighlightedNeuron();
                highlightNeuron(_active_neuron);
            }

            if ((InputHelpers.mouseSupported || _latest_touched_neuron == null ? 1 :
                    !Tooltip.isShowingFor(_latest_touched_neuron) ? 1 : 0) != 0)
                updateNeuronsVisual();
        }

        updateNeuronImpulseAutoSpawn();
        updateAxonPositions();
        updateImpulseSpawn();
        updateImpulses();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _mind_main.transform.DOKill();
        _mind_main.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        _mind_main.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack);
        checkLikes();
        clearMind();
        initStartPositions();
        loadLastLikeForCenter();
        prepareAxons();
        _is_dragging = false;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _offset_x = _offset_target_x = 0.0f;
        _offset_y = _offset_target_y = 0.0f;
        clearHighlight();
        Tooltip.hideTooltipNow();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _is_dragging = true;
        var delta = eventData.delta;
        if (delta.magnitude > (double)_last_mouse_delta.magnitude)
            highlightAllAxons(0.35f);
        _last_mouse_delta = delta;
        _offset_x = (float)(-(double)delta.y * 0.46000000834465027);
        _offset_y = delta.x * 0.46f;
        updateNeuronsVisual();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _is_dragging = false;
        var delta = eventData.delta;
        _offset_target_x += (float)(-(double)delta.y * 0.004999999888241291);
        _offset_target_y += delta.x * 0.005f;
        if (Mathf.Abs(_offset_target_x) > 0.699999988079071 || Mathf.Abs(_offset_target_y) > 0.699999988079071)
        {
            if (Mathf.Abs(_offset_target_x) > (double)Mathf.Abs(_offset_target_y))
                _offset_target_y = (float)(_offset_target_y / (double)Mathf.Abs(_offset_target_x) * 0.699999988079071);
            else
                _offset_target_x = (float)(_offset_target_x / (double)Mathf.Abs(_offset_target_y) * 0.699999988079071);
        }

        _offset_target_x = Mathf.Clamp(_offset_target_x, -0.7f, 0.7f);
        _offset_target_y = Mathf.Clamp(_offset_target_y, -0.7f, 0.7f);
        highlightAllAxons(1f);
        fireImpulsesEverywhere();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
        _last_mouse_delta = Vector2.zero;
    }

    public void highlightAllAxons(float pLight)
    {
        foreach (var axonElement in _pool_axons.getListTotal())
            if (axonElement.mod_light <= (double)pLight)
                axonElement.mod_light = pLight;
    }

    public void fireImpulsesEverywhere()
    {
        for (var index = 0; index < _neurons.Count; ++index)
            fireImpulseWaveFromHere(_neurons[index], 2);
    }

    public void highlightNeuron(LikeNeuronElement pHighlighted = null)
    {
        foreach (var neuron in _neurons)
            if (!(neuron == pHighlighted) && neuron.highlighted)
            {
                neuron.highlighted = false;
                Tooltip.hideTooltipNow();
            }

        pHighlighted?.setHighlighted();
    }

    public LikeNeuronElement getClosestNeuronToCursor()
    {
        var closestNeuronToCursor = (LikeNeuronElement)null;
        var num1 = float.MaxValue;
        var mousePosition = (Vector2)Input.mousePosition;
        foreach (var neuron in _neurons)
        {
            var position = (Vector2)neuron.transform.position;
            var num2 = Vector2.Distance(mousePosition, position);
            if (num2 <= 40.0)
            {
                if (neuron == _active_neuron)
                    return neuron;
                if (num2 < (double)num1)
                {
                    num1 = num2;
                    closestNeuronToCursor = neuron;
                }
            }
        }

        return closestNeuronToCursor;
    }

    public void prepareAxons()
    {
        var num = (float)(250.0 / Mathf.Sqrt(_neurons.Count) * 1.5);
        for (var index1 = 0; index1 < _neurons.Count - 1; ++index1)
        {
            var neuron1 = _neurons[index1];
            if (!neuron1.isCenter())
                for (var index2 = index1 + 1; index2 < _neurons.Count; ++index2)
                {
                    var neuron2 = _neurons[index2];
                    if (!neuron2.isCenter() &&
                        Vector3.Distance(neuron1.transform.localPosition, neuron2.transform.localPosition) <=
                        (double)num)
                        makeAxon(neuron1, neuron2);
                }
        }
    }

    public LikeAxonElement makeAxon(LikeNeuronElement pNeuron1, LikeNeuronElement pNeuron2)
    {
        var next = _pool_axons.getNext();
        next.neuron_1 = pNeuron1;
        next.neuron_2 = pNeuron2;
        pNeuron1.addConnection(pNeuron2, next);
        pNeuron2.addConnection(pNeuron1, next);
        return next;
    }

    public void checkLikes()
    {
        _likes = LikesManager.GetValidLikesFromAssets()
            .Where(like =>
            {
                var isSubspecies = like.LikeAsset.LikeGroup.ID.Equals("subspecies");
                if (!isSubspecies)
                    return true;
                if (!actor.isSapient())
                    return false;
                var id = long.Parse(like.LikeAsset.ID);
                var subspecies = MapBox.instance.subspecies.get(id);
                if (subspecies == null)
                    return false;

                return true;
            });
    }

    public void updateNeuronsVisual()
    {
        var quaternion = Quaternion.Euler(_offset_x, _offset_y, 0.0f);
        foreach (var neuron in _neurons)
        {
            neuron.updateColorsAndTooltip();
            var vector3 = quaternion * neuron.transform.localPosition;
            neuron.transform.localPosition = vector3;
            calculateNeuronDepth(neuron, 70f);
            updateNeuronColorAndScale(neuron);
        }

        sortNeuronsByDepth();
    }

    public void updateNeuronImpulseAutoSpawn()
    {
        foreach (var neuron in _neurons)
            neuron.updateSpawnTimer();
    }

    public void sortNeuronsByDepth()
    {
        foreach (Component neuron in _neurons)
            neuron.transform.SetAsLastSibling();
        _neurons.Sort((a, b) => a.render_depth.CompareTo(b.render_depth));
    }

    public void calculateNeuronDepth(LikeNeuronElement pNeuronElement, float pRadius)
    {
        var z = pNeuronElement.transform.localPosition.z;
        var num = Mathf.InverseLerp(-pRadius, pRadius, z);
        pNeuronElement.render_depth = num;
    }

    public void updateNeuronColorAndScale(LikeNeuronElement pElement)
    {
        if (!pElement.isLikeEnabled())
        {
            var pColor = Color.Lerp(_color_neuron_disabled_back, _color_neuron_disabled_front, pElement.render_depth);
            pElement.setColor(pColor);
        }
        else if (pElement.highlighted)
        {
            var pColor = Color.Lerp(_color_neuron_back, _neuron_highlighted, pElement.render_depth);
            pElement.setColor(pColor);
        }
        else
        {
            var pColor = Color.Lerp(_color_neuron_back, _color_neuron_front, pElement.render_depth);
            pElement.setColor(pColor);
        }

        var num = Mathf.Lerp(0.8f, 1.5f, pElement.render_depth) * (pElement.scale_mod_spawn * pElement.bonus_scale);
        pElement.transform.localScale = new Vector3(num, num, num);
    }

    public void updateAxonPositions()
    {
        foreach (var axonElement in _pool_axons.getListTotal())
        {
            axonElement.update();
            var y = 1f;
            var neuron1 = axonElement.neuron_1;
            var neuron2 = axonElement.neuron_2;
            if (neuron1.highlighted || neuron2.highlighted)
                y = 6f;
            var a = _color_axon_default;
            if (axonElement.axon_center)
            {
                a = _color_axon_default_center;
                y = 7f;
            }

            if (axonElement.mod_light > 0.0)
            {
                var color = Color.Lerp(a, _color_light_axon, axonElement.mod_light);
                axonElement.image.color = color;
            }
            else
            {
                axonElement.image.color = a;
            }

            var localPosition1 = (Vector2)neuron1.transform.localPosition;
            var localPosition2 = (Vector2)neuron2.transform.localPosition;
            var vector2 = (localPosition1 + localPosition2) / 2f;
            axonElement.transform.localPosition = vector2;
            var x = Vector3.Distance(localPosition1, localPosition2);
            axonElement.transform.localScale = new Vector3(x, y, 1f);
            var vector3 = (Vector3)(localPosition2 - localPosition1);
            var z = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
            axonElement.transform.rotation = Quaternion.Euler(0.0f, 0.0f, z);
        }
    }

    public void smoothOffsets()
    {
        _offset_x = Mathf.Lerp(_offset_x, _offset_target_x, 0.1f);
        _offset_y = Mathf.Lerp(_offset_y, _offset_target_y, 0.1f);
    }

    public void fireImpulseWaveFromHere(LikeNeuronElement pNeuron, int pWaves = 4)
    {
        if (!pNeuron.isLikeEnabled())
            return;
        foreach (var connectedNeuron in pNeuron.connected_neurons)
            if (connectedNeuron.isLikeEnabled())
                fireImpulse(pNeuron, connectedNeuron, pWaves);
    }

    public void fireImpulseFrom(
        LikeNeuronElement pPresynapticNeuron,
        int pWave,
        LikeNeuronElement pIgnoreNeuron = null)
    {
        if (pPresynapticNeuron.connected_neurons.Count == 0)
            return;
        LikeNeuronElement random;
        if (pIgnoreNeuron == null)
            random = pPresynapticNeuron.connected_neurons.GetRandom();
        else
            using (var list = new ListPool<LikeNeuronElement>())
            {
                foreach (var connectedNeuron in pPresynapticNeuron.connected_neurons)
                    if (!(pIgnoreNeuron == connectedNeuron))
                        list.Add(connectedNeuron);
                if (list.Count() == 0)
                    return;
                random = list.GetRandom();
            }

        if (random == null)
            return;
        fireImpulse(pPresynapticNeuron, random, pWave);
    }

    public void fireImpulse(
        LikeNeuronElement pPresynapticNeuron,
        LikeNeuronElement pPostsynapticNeuron,
        int pWave)
    {
        var next = _pool_impulses.getNext();
        next.energize(pPresynapticNeuron, pPostsynapticNeuron, pWave);
        pPresynapticNeuron.spawnImpulseFromHere();
        _active_impulses.Add(next);
    }

    public void updateImpulses()
    {
        for (var index = _active_impulses.Count - 1; index >= 0; --index)
        {
            var activeImpulse = _active_impulses[index];
            var impulseReachResult = activeImpulse.moveTowardsNextNeuron();
            var postsynapticNeuron = activeImpulse.postsynaptic_neuron;
            var presynapticNeuron = activeImpulse.presynaptic_neuron;
            switch (impulseReachResult)
            {
                case ImpulseReachResult.Done:
                    postsynapticNeuron?.receiveImpulse();
                    _active_impulses.RemoveAt(index);
                    _pool_impulses.release(activeImpulse);
                    break;
                case ImpulseReachResult.Split:
                    postsynapticNeuron?.receiveImpulse();
                    fireImpulseFrom(postsynapticNeuron, activeImpulse.wave, presynapticNeuron);
                    break;
            }
        }
    }

    public LikeNeuronElement getHighlightedNeuron()
    {
        if (_is_dragging)
            return null;
        if (_offset_x > 1.0499999523162842 || _offset_x < -1.0499999523162842)
            return null;
        return _offset_y > 1.0499999523162842 || _offset_y < -1.0499999523162842 ? null : getClosestNeuronToCursor();
    }

    public void startNewWhat()
    {
        _text_phrases.startNewWhat();
    }

    public void updateImpulseSpawn()
    {
        foreach (var neuron in _neurons)
            if (neuron.hasLike() && neuron.readyToSpawnImpulse())
                fireImpulseFrom(neuron, 1);
    }

    public void initStartPositions()
    {
        var pNeuronIndex = 0;
        foreach (var like in _likes)
        {
            var next = _pool_neurons.getNext();
            next.setupLikeAndActor(like, actor);
            var positionOnSphere = getPositionOnSphere(pNeuronIndex, _likes.Count());
            next.transform.localPosition = positionOnSphere;
            _neurons.Add(next);
            pNeuronIndex++;
        }

        updateNeuronsVisual();
    }

    public void loadLastLikeForCenter()
    {
        // var pNeuron1 = (LikeNeuronElement)null;
        // if (!string.IsNullOrEmpty(actor.last_decision_id))
        //     foreach (var neuron in _neurons)
        //         if (neuron.decision.id == actor.last_decision_id)
        //         {
        //             pNeuron1 = neuron;
        //             break;
        //         }
        
        _last_activated_neuron = _pool_neurons.getNext();
        _last_activated_neuron.transform.localPosition = Vector3.zero;
        _last_activated_neuron.clearImages();
        var gameObj = new GameObject();
        gameObj.name = "brain";
        gameObj.AddOrGetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/icons/iconBrain");
        gameObj.transform.SetParent(_last_activated_neuron.transform, false);
        gameObj.transform.localScale = new Vector3(0.1f, 0.1f);
        
        _last_activated_neuron.bonus_scale = 1.5f;
        _last_activated_neuron.setCenter(true);
        _last_activated_neuron.actor = actor;
        _neurons.Add(_last_activated_neuron);
        
        var neuron = _pool_neurons.getFirstActive();

        if (!(neuron != null))
            return;
        makeAxon(neuron, _last_activated_neuron).axon_center = true;
    }

    public Vector3 getPositionOnSphere(int pNeuronIndex, int pTotalNeurons)
    {
        var f1 = Mathf.Acos((float)(1.0 - 2 * (pNeuronIndex + 1) / (double)pTotalNeurons));
        var f2 = (float)(3.1415927410125732 * (1.0 + Mathf.Sqrt(5f))) * pNeuronIndex;
        var x = 70.0 * Mathf.Cos(f2) * Mathf.Sin(f1);
        var num1 = 70f * Mathf.Sin(f2) * Mathf.Sin(f1);
        var num2 = 70f * Mathf.Cos(f1);
        var y = (double)num1;
        var z = (double)num2;
        return new Vector3((float)x, (float)y, (float)z);
    }

    public void clearMind()
    {
        foreach (var neuron in _neurons)
            neuron.clear();
        _active_impulses.Clear();
        _pool_axons.clear();
        _pool_neurons.clear();
        _pool_impulses.clear();
        _neurons.Clear();
        foreach (var axonElement in _pool_axons.getListTotal())
            axonElement.clear();
    }

    public bool isDragging()
    {
        return _is_dragging;
    }

    public void clearHighlight()
    {
        if (_active_neuron == null)
            return;
        _active_neuron.highlighted = false;
        _active_neuron = null;
    }

    public static void debugTool(DebugTool pTool)
    {
        LikesOverview.instance?.debug(pTool);
    }

    public void debug(DebugTool pTool)
    {
        pTool.setText("offset_target_x:", NeuronsOverview.getFloat(_offset_target_x));
        pTool.setText("offset_target_y:", NeuronsOverview.getFloat(_offset_target_y));
        pTool.setSeparator();
        pTool.setText("offset_x:", NeuronsOverview.getFloat(_offset_x));
        pTool.setText("offset_y:", NeuronsOverview.getFloat(_offset_y));
        pTool.setSeparator();
        var quaternion = Quaternion.Euler(_offset_x, _offset_y, 0.0f);
        pTool.setText("combined_rotation.x:", NeuronsOverview.getFloat(quaternion.x));
        pTool.setText("combined_rotation.y:", NeuronsOverview.getFloat(quaternion.y));
        pTool.setText("combined_rotation.z:", NeuronsOverview.getFloat(quaternion.z));
        pTool.setText("combined_rotation.w:", NeuronsOverview.getFloat(quaternion.w));
        pTool.setSeparator();
        pTool.setText("is_dragging:", _is_dragging);
        pTool.setText("last_mouse_delta:", _last_mouse_delta);
        pTool.setSeparator();
        pTool.setText("likes:", _likes.Count());
        pTool.setSeparator();
        pTool.setText("neuron selected:", _active_neuron);
    }

    public static string getFloat(float pFloat)
    {
        if (pFloat < 1.0 / 1000.0 && pFloat > -1.0 / 1000.0)
            return pFloat.ToString("F6", CultureInfo.InvariantCulture);
        return pFloat > 0.0
            ? $"<color=#75D53A>{pFloat.ToString("F6", CultureInfo.InvariantCulture)}</color>"
            : $"<color=#DB2920>{pFloat.ToString("F6", CultureInfo.InvariantCulture)}</color>";
    }

    public void setLatestTouched(LikeNeuronElement pNeuron)
    {
        _latest_touched_neuron = pNeuron;
    }

    public void switchAllNeurons()
    {
        _all_state = !isAnyEnabled() || (!isAllEnabled() && !_all_state);
        foreach (var neuron in _neurons)
            if (neuron.hasLike())
                if(_all_state)
                    actor.data.set(neuron.like.IDWithLoveType, _all_state);
                else
                    actor.data.removeBool(neuron.like.IDWithLoveType);
        fireImpulsesEverywhere();
        Orientations.RollOrientationLabel(actor);
        actor.removeAllCachedLikes();
        StatPatch.UpdateOrientationStats(mainWindow);
    }

    public bool getAllState()
    {
        return _all_state;
    }

    public bool isAnyEnabled()
    {
        foreach (var neuron in _neurons)
            if (neuron.hasLike() && actor.HasLike(neuron.like))
                return true;
        return false;
    }

    public bool isAllEnabled()
    {
        foreach (var neuron in _neurons)
            if (neuron.hasLike() && !actor.HasLike(neuron.like))
                return false;
        return true;
    }
}