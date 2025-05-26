using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(ActorManager))]
public class ActorManagerPatch
{
    public static void NewUnit(Actor actor)
    {
        TolUtil.NewPreferences(actor);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ActorManager.createNewUnit))]
    static void CreateNewUnit(Actor __result)
    {
        NewUnit(__result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ActorManager.createActorFromData))]
    static void CreateActorFromData(Actor __result)
    {
        NewUnit(__result);
    }
}