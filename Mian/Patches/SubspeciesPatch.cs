using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(Subspecies))]
public class SubspeciesPatch
{
    // I don't bother to use a transpiler for this since we pretty much rewrite the entire method
    [HarmonyPatch(nameof(Subspecies.isPartnerSuitableForReproduction))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)] // allows other mods to patch without us breaking the method for them.. hopefully they don't err do wonky things
    [HarmonyAfter("netdot.mian.topicofidentity")]
        static bool SuitableReproductionPatch(Actor pActor, Actor pTarget, Subspecies __instance, ref bool __result)
        {
            if (!pActor.hasSubspecies() || !pTarget.hasSubspecies())
            {
                __result = false;
                return false;
            }

            if (TolUtil.CanDoAnySexType(pActor))
            {
                __result = true;
                return false;
            }
            
            var actorGenitalia = Preferences.GetGenitalia(pActor);
            var targetGenitalia = Preferences.GetGenitalia(pTarget);
            
            if (__instance.needOppositeSexTypeForReproduction())
            {
                if ((!actorGenitalia.Equals(targetGenitalia) && pTarget.subspecies.isReproductionSexual()) || TolUtil.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            } else if (TolUtil.NeedSameSexTypeForReproduction(pActor))
            {
                if ((actorGenitalia.Equals(targetGenitalia) && TolUtil.NeedSameSexTypeForReproduction(pTarget)) || TolUtil.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
}