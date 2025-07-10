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

            if (!pActor.ReproducesSexually() || !pTarget.ReproducesSexually())
            {
                __result = false;
                return false;
            }

            // if ((pActor.HasVulva() && pTarget.HasPenis()) || (pActor.HasPenis() && pTarget.HasVulva()))
            // {
            //     __result = true;
            //     return false;
            // }

            if (pActor.CanDoAnySexType() || pTarget.CanDoAnySexType())
            {
                __result = true;
                return false;
            }
            
            var actorGenitalia = pActor.GetGenitalia();
            var targetGenitalia = pTarget.GetGenitalia();
            
            if (pTarget.GetBiologicalSex().Equals(pActor.GetBiologicalSex()) &&
                pActor.NeedSameSexTypeForReproduction() && pTarget.NeedSameSexTypeForReproduction())
            {
                __result = true;
                return false;
            }
            if (!pTarget.GetBiologicalSex().Equals(pActor.GetBiologicalSex()) && pActor.NeedDifferentSexTypeForReproduction() &&
                                                                   pTarget.NeedDifferentSexTypeForReproduction())
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }
}