using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class DecisionAssetPatch
{
    // stops people with mismatching sexual preferences from attempting sex in the vanilla game 
    [HarmonyPatch(typeof(DecisionAsset), nameof(DecisionAsset.isPossible))]
    class DecisionPatch
    {
        static bool Prefix(Actor pActor, DecisionAsset __instance, ref bool __result)
        {
            // this is for decision asset: sexual_reproduction_try, this basically cancels sex with their partner
            if (__instance.id.Equals("sexual_reproduction_try"))
            {
                var pParentA = pActor;
                var pParentB = pActor.lover;
                if (pActor.hasLover() && 
                    (!Preferences.PreferenceMatches(pParentA, pParentB, true)
                     || !Preferences.PreferenceMatches(pParentB, pParentA, true) 
                     || !TOLUtil.CanReproduce(pParentA, pParentB)
                    || !BabyHelper.canMakeBabies(pParentA) || !BabyHelper.canMakeBabies(pParentB)))
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}