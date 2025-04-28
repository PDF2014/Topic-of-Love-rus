using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.Patches;

public class SubspeciesPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Subspecies), nameof(Subspecies.isPartnerSuitableForReproduction))]
    [HarmonyAfter("netdot.mian.topicofidentity")]
        static bool SuitableReproductionPatch(Actor pActor, Actor pTarget, Subspecies __instance, ref bool __result)
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
            
            var actorGenitalia = Preferences.GetGenitalia(pActor);
            var targetGenitalia = Preferences.GetGenitalia(pTarget);
            
            if (__instance.needOppositeSexTypeForReproduction())
            {
                if ((!actorGenitalia.Equals(targetGenitalia) && pTarget.subspecies.isReproductionSexual()) || TOLUtil.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            } else if (TOLUtil.NeedSameSexTypeForReproduction(pActor))
            {
                if ((actorGenitalia.Equals(targetGenitalia) && TOLUtil.NeedSameSexTypeForReproduction(pTarget)) || TOLUtil.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
}