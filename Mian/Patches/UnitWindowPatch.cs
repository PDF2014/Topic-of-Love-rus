using System;
using HarmonyLib;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace Topic_of_Love.Mian.Patches;

// Big credits to xing_yao on Discord for this!
[HarmonyPatch(typeof(UnitWindow))]
public class UnitWindowPatch
{
    private class Stat
    {
        public readonly string Name;
        public readonly string IconPath;
        public readonly Func<Actor, float> Value;
        
        public Stat(string name, string iconPath, Func<Actor, float> value)
        {
            Name=name;
            IconPath=iconPath;
            Value=value;
        }
    }
    private static Stat[] _stats = {
        new ("intimacy_happiness", "ui/Icons/iconLovers", actor =>
        {
            actor.data.get("intimacy_happiness", out float happiness);
            return happiness;
        }),
    };
    
    private static bool _initialized;
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitWindow.OnEnable))]
    static void WindowCreatureInfo(UnitWindow __instance)
    {
        if (__instance.actor == null || !__instance.actor.isAlive())
            return;
        
        if (!_initialized)
        {
            _initialized = true;
            Initialize(__instance);
        }

        OnEnable(__instance);
    }
    
    private static void OnEnable(UnitWindow window)
    {
        foreach (var stat in _stats)
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

            var contentTransform = window.gameObject.transform.Find(
                "Background/Scroll View/Viewport/Content/content_more_icons"
            ); // where the icons are

            contentTransform.gameObject.AddOrGetComponent<StatsIconContainer>();
            
            var iconGroupTemplate = contentTransform.GetChild(4);
            var iconGroup = GameObject.Instantiate(iconGroupTemplate, contentTransform);
            
            var iconTemplate = iconGroup.Find("i_kills");
            
            for (var i = iconGroup.transform.childCount - 1; i >= 0; i--)
            {
                var child = iconGroup.transform.GetChild(i);
                if(child.name != "i_kills")
                    GameObject.DestroyImmediate(child.gameObject);
            }

            foreach (var iconData in _stats)
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
}