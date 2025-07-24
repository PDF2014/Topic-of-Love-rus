using System.Reflection;
using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(Subspecies))]
public class SubspeciesPatch
{
    [HarmonyPatch]
    static class WindowInputPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(WindowMetaGeneric<Subspecies, SubspeciesData>), nameof(WindowMetaGeneric<Subspecies, SubspeciesData>.applyInputName));
        }

        static void Postfix(WindowMetaGeneric<Subspecies, SubspeciesData> __instance)
        {
            if (__instance.meta_object.GetType() == typeof(Subspecies) && __instance.meta_object.isSapient())
            {
                var name = __instance.meta_object.name;
                var id = __instance.meta_object.id.ToString();
                MapBox.instance.map_stats.custom_data.get("custom_like_"+id, out string oldName);
                if (oldName.Equals(name))
                    return;
                MapBox.instance.map_stats.custom_data.set("custom_like_" + id, name);
                LikesManager.RenameLikeAssetLocale(LikesManager.GetAssetFromID(id),
                    name);
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Subspecies.newSpecies))]
    static void SpeciesPatch(Subspecies __instance)
    {
        if (__instance.isSapient())
        {
            LikesManager.AddDynamicLikeAsset(__instance.id, __instance.name, "subspecies", LoveType.Both);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SubspeciesManager), nameof(SubspeciesManager.removeObject))]
    static void OnRemoveSpecies(Subspecies pObject)
    {
        if(pObject.isSapient())
            LikesManager.RemoveDynamicLikeAsset(pObject.getID());
    }
    
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