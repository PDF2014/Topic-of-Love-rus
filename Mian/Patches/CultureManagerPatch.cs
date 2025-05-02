using Topic_of_Love.Mian.CustomAssets;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class CultureManagerPatch
{
    [HarmonyPatch(typeof(CultureManager), nameof(CultureManager.newCulture))]
    class OnCultureCreatePatch
    {
        static void Postfix(Actor pFounder, ref Culture __result)
        {
            // scar_of_incest prevents us from modifying the incest trait
            if (TolUtil.IsDyingOut(pFounder) && !__result.hasTrait("scar_of_incest"))
            {
                __result.addTrait("incest");
            }

            // newest patch randomizes culture traits now
            
            // if (Randy.randomChance(0.35f))
            // {
            //     if (Util.NeedDifferentSexTypeForReproduction(pActor))
            //         __result.addTrait("homophobic");
            //     if (Util.NeedSameSexTypeForReproduction(pActor))
            //         __result.addTrait("heterophobic");
            //     if (!Util.NeedDifferentSexTypeForReproduction(pActor) && !Util.NeedSameSexTypeForReproduction(pActor) && !Util.CanDoAnySexType(pActor))
            //         __result.addTrait("orientationless");
            // } else if (Randy.randomChance(0.35f))
            // {
            //     var preference = QueerTraits.GetPreferenceFromActor(pActor, false);
            //     if(preference == Preference.DifferentSex)
            //         __result.addTrait("homophobic");
            //     if(preference == Preference.SameSex)
            //         __result.addTrait("heterophobic");
            // }
        }
    }
}