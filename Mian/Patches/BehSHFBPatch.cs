using ai.behaviours;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehSpawnHeartsFromBuilding))]
public class BehSHFBPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehSpawnHeartsFromBuilding.execute))]
    static bool SpawnHearts(Actor pActor, ref BehResult __result, BehSpawnHeartsFromBuilding __instance)
    {
        var target = pActor.beh_actor_target != null ? pActor.beh_actor_target.a : pActor.lover;
        if (target == null)
        {
            TolUtil.Debug(pActor.getName()+": Cant do sex because target is null");
            __result = BehResult.Stop;
            return false;
        }
        // pActor.addAfterglowStatus();
        // target.addAfterglowStatus();
        __instance.spawnHearts(pActor);
        __result = BehResult.Continue;
        return false;
    }
}