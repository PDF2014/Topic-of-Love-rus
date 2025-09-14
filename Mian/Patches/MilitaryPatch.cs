using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(City))]
public class MilitaryPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(City.checkCanMakeWarrior))]
    public static void CanMakeWarrior(Actor pActor, City __instance, ref bool __result)
    {
        if (__instance.hasCulture())
        {
            if (__instance.culture.hasTrait("homophobic") && _Orientation.IsAHomo(pActor))
            {
                __result = false;
            }
            if (__instance.culture.hasTrait("heterophobic") && _Orientation.IsAHetero(pActor))
            {
                __result = false;
            }
        }
    }
}