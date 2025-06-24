using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

public class CustomDataPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.newCivKingdom))]
    public static void OnNewCiv(Kingdom __instance, Actor pActor)
    {
        __instance.data.set("founder_orientation", Orientation.GetOrientation(pActor, true).OrientationType);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CitiesManager), nameof(CitiesManager.newCity))]
    public static void OnNewCity(Actor pOriginalActor, City __result)
    {
        __result.data.set("founder_orientation", Orientation.GetOrientation(pOriginalActor, true).OrientationType);
    }
}