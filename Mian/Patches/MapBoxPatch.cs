
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(MapBox))]
public class MapBoxPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MapBox.updateObjectAge))]
    static void OnAgePatch(MapBox __instance)
    {
        foreach (var culture in __instance.cultures.list)
        {
            if (culture.hasTrait("incest") && !culture.hasTrait("scar_of_incest") 
                                           && culture.getAge() > 30
                                           && culture.countFamilies() >= 10 && culture.countUnits() >= 60
                                           && Randy.randomChance(0.1f))
            {
                culture.removeTrait("incest");
            }
        }
    }
}