using System;
using System.Collections.Generic;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.services;
using Topic_of_Love.Mian.CustomAssets.Traits;
using UnityEngine;
using UnityEngine.UI;

namespace Topic_of_Love.Mian.Patches;

// Big credits to xing_yao on Discord for this!
[HarmonyPatch(typeof(UnitWindow))]
public class StatPatch
{
    private class Stat<T>
    {
        public readonly string Name;
        public readonly string IconPath;
        public readonly Func<Actor, T> Value;
        
        public Stat(string name, Func<Actor, T> value, string iconPath=null)
        {
            Name=name;
            IconPath=iconPath;
            Value=value;
        }
    }
    private static Stat<float>[] _icons = {
        new ("intimacy_happiness", actor =>
        {
            actor.data.get("intimacy_happiness", out float happiness);
            return happiness;
        }, "ui/Icons/iconLovers"),
    };
    private static Stat<Dictionary<string, string>>[] _stats = {
        new ("sexual_orientation", actor =>
        {
            actor.data.get("sexual_orientation", out string sexualOrientation);
            if (sexualOrientation != null)
            {
                var orientationType = OrientationType.GetOrientation(sexualOrientation);

                Dictionary<string, string> dict = new();
                dict.Add("value", LM.Get(orientationType.SexualPathLocale));
                dict.Add("hex_code", orientationType.HexCode);
                dict.Add("icon", orientationType.SexualPathIcon);

                return dict;
            }
            return null;
        }),
        new ("romantic_orientation", actor =>
        {
            actor.data.get("romantic_orientation", out string romanticOrientation);
            if (romanticOrientation != null)
            {
                var orientationType = OrientationType.GetOrientation(romanticOrientation);
                
                Dictionary<string, string> dict = new();
                dict.Add("value", LM.Get(orientationType.RomanticPathLocale));
                dict.Add("hex_code", orientationType.HexCode);
                dict.Add("icon", orientationType.RomanticPathIcon);

                return dict;            
            }
            return null;
        })
    };
    
    private static bool _initializedIcons;
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitWindow.OnEnable))]
    static void WindowCreatureInfo(UnitWindow __instance)
    {
        if (__instance.actor == null || !__instance.actor.isAlive())
            return;
        
        if (!_initializedIcons)
        {
            _initializedIcons = true;
            Initialize(__instance);
        }

        OnEnable(__instance);
    }
    
    private static void OnEnable(UnitWindow window)
    {
        foreach (var stat in _icons)
        {
            window.setIconValue(stat.Name, stat.Value(window.actor));
        }
        window.showInfo();
    }
    
    private static void Initialize(UnitWindow window)
        {
            window
                .gameObject.transform.Find("Background/Scroll View")
                .GetComponent<ScrollRect>()
                .enabled = true;
            window
                .gameObject.transform.Find("Background/Scroll View/Viewport")
                .GetComponent<Mask>()
                .enabled = true;
            window
                .gameObject.transform.Find("Background/Scroll View/Viewport")
                .GetComponent<Image>()
                .enabled = true;

            var moreIconsTransform = window.gameObject.transform.Find(
                "Background/Scroll View/Viewport/Content/content_more_icons"
            ); // where the icons are

            moreIconsTransform.gameObject.AddOrGetComponent<StatsIconContainer>();
            
            var iconGroupTemplate = moreIconsTransform.GetChild(4);
            var iconGroup = GameObject.Instantiate(iconGroupTemplate, moreIconsTransform);
            
            var iconTemplate = iconGroup.Find("i_kills");
            
            for (var i = iconGroup.transform.childCount - 1; i >= 0; i--)
            {
                var child = iconGroup.transform.GetChild(i);
                if(child.name != "i_kills")
                    GameObject.DestroyImmediate(child.gameObject);
            }

            foreach (var iconData in _icons)
            {
                var baseIcon = GameObject.Instantiate(iconTemplate, iconGroup);
                var icon = baseIcon.GetComponent<StatsIcon>();
                var iconText = baseIcon.GetComponent<TipButton>();
                iconText.textOnClick = LM.Get("stats_icon_"+iconData.Name);
                icon.name = iconData.Name;
                icon.getIcon().sprite = Resources.Load<Sprite>(iconData.IconPath);
            }
            
            GameObject.DestroyImmediate(iconTemplate.gameObject);
            
            iconGroup.name = "tol_icons";
            iconGroup.transform.localScale = new Vector3(1, 1, 1);
        }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitWindow.showStatsRows))]
    static void ShowStatsRowsPatch(UnitWindow __instance)
    {
        foreach (var stat in _stats)
        {
            var value = stat.Value(__instance.actor);
            if (value != null)
            {
                value.TryGetValue("hex_code", out var hexCode);
                value.TryGetValue("icon", out var icon);
                __instance.showStatRow(stat.Name, value["value"], hexCode, pColorText: true, pIconPath: icon);
            }
        }
    }
}