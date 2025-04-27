using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class SubspeciesPatch
{
    [HarmonyPatch(typeof(Subspecies), nameof(Subspecies.isPartnerSuitableForReproduction))]
    class SuitableReproductionPatch
    {
        static bool Prefix(Actor pActor, Actor pTarget, Subspecies __instance, ref bool __result)
        {
            if (!pActor.hasSubspecies() || !pTarget.hasSubspecies())
            {
                __result = false;
                return false;
            }

            if (TOLUtil.CanDoAnySexType(pActor))
            {
                __result = true;
                return false;
            }
            
            if (__instance.needOppositeSexTypeForReproduction())
            {
                if ((pActor.data.sex != pTarget.data.sex && pTarget.subspecies.isReproductionSexual()) || TOLUtil.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            } else if (TOLUtil.NeedSameSexTypeForReproduction(pActor))
            {
                if ((pActor.data.sex == pTarget.data.sex && TOLUtil.NeedSameSexTypeForReproduction(pTarget)) || TOLUtil.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }
}