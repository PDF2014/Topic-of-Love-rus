using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(City))]
public class MilitaryPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(City.checkCanMakeWarrior))]
    public static bool CanMakeWarrior(City __instance, Actor pActor, ref bool __result)
    {
        if (__instance.hasCulture())
        {
            if (__instance.culture.hasTrait("homophobic") && Orientation.IsAHomo(pActor))
            {
                __result = false;
                return false;
            }
            if (__instance.culture.hasTrait("heterophobic") && Orientation.IsAHetero(pActor))
            {
                __result = false;
                return false;
            }
        }
        return true;
    }
}