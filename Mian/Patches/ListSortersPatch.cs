using System.Linq;
using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(ListSorters))]
public class ListSortersPatch
{
    [HarmonyPatch(nameof(ListSorters.sortUnitsSortedByAgeAndTraits))]
    static void SortUnits(ListPool<Actor> pUnits, Culture pCulture)
    {
        var homophobic = pCulture.hasTrait("homophobic");
        var heterophobic = pCulture.hasTrait("heterophobic");

        if (homophobic || heterophobic)
        {
            Orientation topOrientation;

            if (homophobic)
            {
                topOrientation = Orientation.Orientations.Values.Where(orientation => orientation.IsHetero && !orientation.IsHomo).ToList().GetRandom();
            }
            else
            {
                topOrientation = Orientation.Orientations.Values
                    .Where(orientation => orientation.IsHomo && !orientation.IsHetero).ToList().GetRandom();
            }
            
            pUnits.Sort((a1, a2) => TolUtil.sortUnitsByOrientation(a1, a2, topOrientation, true));
        }
    }
}