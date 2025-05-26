using EpPathFinding.cs;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(DecisionAsset))]
public class DecisionAssetPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DecisionAsset.isPossible))]
    static bool IsPossiblePatch(Actor pActor, DecisionAsset __instance, ref bool __result)
    {
        // this is for decision asset: sexual_reproduction_try, this basically cancels sex with their partner
        // stops people with mismatching sexual preferences from attempting sex in the vanilla game 
        if (__instance.id.Equals("sexual_reproduction_try"))
        {
            var pParentA = pActor;
            var pParentB = pActor.lover;
            if (pActor.hasLover() && 
                (!Preferences.PreferenceMatches(pParentA, pParentB, true)
                 || !Preferences.PreferenceMatches(pParentB, pParentA, true) 
                 || !TolUtil.CouldReproduce(pParentA, pParentB)
                 || !BabyHelper.canMakeBabies(pParentA) || !BabyHelper.canMakeBabies(pParentB)))
            {
                __result = false;
                return false;
            }
        }
        
        if (__instance.id.Equals("find_lover") && TolUtil.IsOrientationSystemEnabledFor(pActor) && pActor.isSapient())
        {
            __result = false;
            return false;
        }
        
        return true;
    }
}