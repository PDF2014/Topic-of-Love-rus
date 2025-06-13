using ai.behaviours;
using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehCheckReproductionBasics))]
public class BehCheckReproductionBasicsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehCheckReproductionBasics.execute))]
    static bool CheckBasicsPatch(Actor pActor, ref BehResult __result)
    {
        var pParentA = pActor;
        var pParentB = pActor.lover;

        if (pActor.hasLover() &&
            (!Preferences.BothActorsPreferenceMatch(pParentA, pParentB, true)
             || !TolUtil.CouldReproduce(pParentA, pParentB)
             || !BabyHelper.canMakeBabies(pParentA)))
        {
            __result = BehResult.Stop;
            return false;
        }

        return true;
    }
}