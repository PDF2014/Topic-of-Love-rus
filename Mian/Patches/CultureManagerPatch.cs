using Topic_of_Love.Mian.CustomAssets;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(CultureManager))]
public class CultureManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CultureManager.newCulture))]
    static void OnCultureCreat(Actor pFounder, ref Culture __result)
    {
        // scar_of_incest prevents us from modifying the incest trait
        if (TolUtil.IsDyingOut(pFounder) && !__result.hasTrait("scar_of_incest"))
        {
            __result.addTrait("incest");
        }
    }
}