using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(ActorManager))]
public class ActorManagerPatch
{
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ActorManager.createNewUnit))]
    static void CreateNewUnit(Actor __result)
    {
        TOLUtil.GivePreferences(__result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ActorManager.createActorFromData))]
    static void CreateActorFromData(Actor __result)
    {
        TOLUtil.GivePreferences(__result);
    }
}