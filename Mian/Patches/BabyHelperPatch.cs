using EpPathFinding.cs;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BabyHelper))]
public class BabyHelperPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BabyHelper.canMakeBabies))]
    [HarmonyAfter("netdot.mian.topicofidentity")]
    // run after TOI because they check for genitalia there
    static bool CanMakeBabiesPatch(Actor pActor, ref bool __result)
    {
        __result = pActor.canBreed() && pActor.isAdult() && !pActor.hasReachedOffspringLimit() && BabyHelper.checkMetaLimitsResult(pActor);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BabyHelper.checkMetaLimitsResult))]
    static bool CheckMetaLimitsResult(Actor pActor, ref bool __result)
    {
        __result = !pActor.subspecies.hasReachedPopulationLimit() && (!pActor.hasCity() || !pActor.city.hasReachedWorldLawLimit() 
            && (pActor.subspecies.isReproductionSexual() || pActor.subspecies.hasTraitReproductionSexualHermaphroditic() 
                                                         || TOLUtil.NeedSameSexTypeForReproduction(pActor)
                                                         && pActor.current_children_count == 0 || pActor.city.hasFreeHouseSlots()));
        return false;
    }
}