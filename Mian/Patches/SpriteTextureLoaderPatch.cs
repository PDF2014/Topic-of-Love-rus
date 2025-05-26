using HarmonyLib;
using UnityEngine;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(SpriteTextureLoader))]
public class SpriteTextureLoaderPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpriteTextureLoader.getSprite))]
    public static void GetSprite(ref Sprite __result)
    {
        if (__result == null)
        {
            __result = (Sprite) Resources.Load("ui/Icons/wip", typeof(Sprite));
        }
    }
}