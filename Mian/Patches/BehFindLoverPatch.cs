using ai.behaviours;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehFindLover))]
public class BehFindLoverPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehFindLover.execute))]
    static bool FindLoverPatch(Actor pActor, ref BehResult __result)
    {
        if (pActor.hasLover())
        {
            __result = BehResult.Stop;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehFindLover.checkIfPossibleLover))]
    static bool IsPossibleLover(Actor pActor, Actor pTarget, ref bool __result)
    {
        // since canFallInLover allows for actors to fall in love even if they have lovers already since they have orientation system
        if (pTarget.hasLover())
        {
            __result = false;
            return false;
        }

        return true;
    }
}