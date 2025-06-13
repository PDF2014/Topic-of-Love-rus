using EpPathFinding.cs;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BabyHelper))]
public class BabyHelperPatch
{
    // [HarmonyPrefix]
    // [HarmonyPatch(nameof(BabyHelper.canMakeBabies))]
    // [HarmonyAfter("netdot.mian.topicofidentity")]
    // // run after TOI because they check for genitalia there
    // static bool CanMakeBabiesPatch(Actor pActor, ref bool __result)
    // {
    //     __result = pActor.canBreed() && pActor.isAdult() && !pActor.hasReachedOffspringLimit() && BabyHelper.checkMetaLimitsResult(pActor);
    //     return false;
    // }
    //
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BabyHelper.isMetaLimitsReached))]
    static bool isMetaLimitsReached(Actor pActor, ref bool __result)
    {
        if (pActor.subspecies.hasReachedPopulationLimit())
            return true;
        if (!pActor.hasCity())
            return true;
        if (pActor.city.hasReachedWorldLawLimit())
            return true;
        Actor lover = pActor.lover;
        __result =((!pActor.isImportantPerson() ? 0 : (!pActor.hasReachedOffspringLimit() ? 1 : 0)) | (lover == null || !lover.isImportantPerson() ? (false ? 1 : 0) : (!lover.hasReachedOffspringLimit() ? 1 : 0))) == 0 && 
                  ((!pActor.subspecies.isReproductionSexual() && !TolUtil.NeedSameSexTypeForReproduction(pActor)) || pActor.current_children_count != 0) && !pActor.city.hasFreeHouseSlots();
        return false;
    }
    
    public static void TraitsInherit(Actor pActorTarget, params Actor[] parents)
    {
        using (var listPool = new ListPool<ActorTrait>(256 /*0x80*/))
        {
            var totalCount = 0;
            foreach (var parent in parents)
            {
                if (parent == null)
                    continue;
                BabyHelper.addTraitsFromParentToList(parent, listPool, out var counter);
                totalCount += counter;
            }
            for (var index = 0; index < totalCount; ++index)
            {
                var random = listPool.GetRandom<ActorTrait>();
                pActorTarget.addTrait(random.id);
            }
        }
    }
}