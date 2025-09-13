using HarmonyLib;
using UnityEngine;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(SpriteTextureLoader))]
public class SpriteTextureLoaderPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpriteTextureLoader.getSprite))]
    public static void GetSprite(string pPath, ref Sprite __result)
    {
        if (__result == null)
        {
            TolUtil.Debug(pPath + " is an invalid sprite!");
            __result = (Sprite) Resources.Load("ui/Icons/wip", typeof(Sprite));
        }
    }
}