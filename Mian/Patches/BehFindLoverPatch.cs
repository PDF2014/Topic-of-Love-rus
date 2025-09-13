using ai.behaviours;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehFindLover))]
public class BehFindLoverPatch
{
    // [HarmonyPrefix]
    // [HarmonyPriority(Priority.Last)]
    // [HarmonyPatch(nameof(BehFindLover.execute))]
    // static bool FindLoverPatch(Actor pActor, ref BehResult __result, BehFindLover __instance)
    // {
    //     if (pActor.hasLover())
    //     {
    //         __result = BehResult.Stop;
    //         return false;
    //     }
    //     Actor pTarget = __instance.findLoverAround(pActor) ?? __instance.checkCityLovers(pActor);
    //     if (pTarget != null)
    //         if (TolUtil.SocializedLoveCheck(__instance, pActor, pTarget)){
    //             __result = BehResult.Skip;
    //             return false;
    //         }
    //
    //     __result = BehResult.Continue;
    //     return false;
    // }
    //
    [HarmonyPostfix]
    // [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(BehFindLover.checkIfPossibleLover))]
    static void IsPossibleLover(Actor pActor, Actor pTarget, ref bool __result)
    {
        if (pTarget.hasLover() || pActor.hasLover())
        {
            __result = false;
        }
    }
}