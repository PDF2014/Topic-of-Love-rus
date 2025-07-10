using System.Collections.Generic;
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
        var nonSapientList = new []{"homosexual", "heterosexual"};

        if (homophobic || heterophobic)
        {
            List<Orientation> topOrientations;

            if (homophobic)
            {
                topOrientations = Orientation.RegisteredOrientations.Values.Where(orientation => 
                    orientation.IsHetero && !orientation.IsHomo && !nonSapientList.Contains(orientation.OrientationType)).ToList();
            }
            else
            {
                topOrientations = Orientation.RegisteredOrientations.Values
                    .Where(orientation => orientation.IsHomo && !orientation.IsHetero && !nonSapientList.Contains(orientation.OrientationType))
                    .ToList();

                // if (pCulture.hasTrait("patriarchy") || pCulture.hasTrait("matriarchy"))
                // {
                //     // prioritize gay or lesbian depending on woman/man priority
                //     var expectedType = pCulture.hasTrait("patriarchy") ? "gay" : "lesbian";
                //     
                //     topOrientations.Sort((orientation1, orientation2) =>
                //     {
                //         if (orientation1.OrientationType.Equals(orientation2.OrientationType))
                //             return 0;
                //             
                //         return orientation1.OrientationType.Equals(expectedType) ? -1 : 1;
                //     });
                // }
            }
            
            pUnits.Sort((a1, a2) => TolUtil.SortUnitsByOrientations(a1, a2, topOrientations, true));
        }
    }
}