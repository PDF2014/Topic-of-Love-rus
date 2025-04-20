using System.Linq;
using NeoModLoader.services;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets;

public class Opinions
{
    public static void Init()
    {
        Add(new OpinionAsset
        {
            id = "opinion_homosexual",
            translation_key = "opinion_homosexual",
            calc = (pMain, pTarget) =>
            {
                var requirement = pTarget.getUnits().Count() / 2;
                var homoUnits = pTarget.getUnits().Count(unit => 
                    QueerTraits.IncludesHomoPreference(QueerTraits.GetPreferenceFromActor(unit, false)) ||
                    QueerTraits.IncludesHomoPreference(QueerTraits.GetPreferenceFromActor(unit, true)));
                
                if (pMain.hasCulture() && pMain.culture.hasTrait("homophobic"))
                {
                    if (pTarget.hasCulture() && pTarget.culture.hasTrait("homophobic"))
                        return 20;
                    
                    if (homoUnits > requirement)
                    {
                        return -50;
                    }
                }

                if (!pTarget.hasCulture() || (pTarget.hasCulture() && !pTarget.culture.hasTrait("homophobic")))
                {
                    if(homoUnits > requirement)
                    {
                        
                        requirement = pMain.getUnits().Count() / 2;
                        homoUnits = pMain.getUnits().Count(unit => 
                            QueerTraits.IncludesHomoPreference(QueerTraits.GetPreferenceFromActor(unit, false)) ||
                            QueerTraits.IncludesHomoPreference(QueerTraits.GetPreferenceFromActor(unit, true)));
                        if (homoUnits > requirement)
                            return 20;
                    }   
                } else if (pTarget.hasCulture() && pTarget.culture.hasTrait("homophobic"))
                {
                    requirement = pMain.getUnits().Count() / 2;
                    homoUnits = pMain.getUnits().Count(unit => 
                        QueerTraits.IncludesHomoPreference(QueerTraits.GetPreferenceFromActor(unit, false)) ||
                        QueerTraits.IncludesHomoPreference(QueerTraits.GetPreferenceFromActor(unit, true)));
                    if (homoUnits > requirement)
                        return -50;
                }
                
                return 0;
            }
        });
        
        Add(new OpinionAsset
        {
            id = "opinion_heterosexual",
            translation_key = "opinion_heterosexual",
            calc = (pMain, pTarget) =>
            {
                var requirement = pTarget.getUnits().Count() / 2;
                var heteroUnits = pTarget.getUnits().Count(unit => 
                    QueerTraits.GetPreferenceFromActor(unit, false).Equals(Preference.DifferentSex) ||
                    QueerTraits.GetPreferenceFromActor(unit, true).Equals(Preference.DifferentSex));
                
                if (pMain.hasCulture() && pMain.culture.hasTrait("heterophobic"))
                {
                    if (pTarget.hasCulture() && pTarget.culture.hasTrait("heterophobic"))
                        return 20;
                    
                    if (heteroUnits > requirement)
                    {
                        return -50;
                    }
                }

                if (!pTarget.hasCulture() || (pTarget.hasCulture() && !pTarget.culture.hasTrait("heterophobic")))
                {
                    if(heteroUnits > requirement)
                    {
                        requirement = pMain.getUnits().Count() / 2;
                        heteroUnits = pMain.getUnits().Count(unit => 
                            QueerTraits.GetPreferenceFromActor(unit, false).Equals(Preference.DifferentSex) ||
                            QueerTraits.GetPreferenceFromActor(unit, true).Equals(Preference.DifferentSex));
                        if (heteroUnits > requirement)
                            return 20;
                    }
                }else if (pTarget.hasCulture() && pTarget.culture.hasTrait("heterophobic"))
                {
                    requirement = pMain.getUnits().Count() / 2;
                    heteroUnits = pMain.getUnits().Count(unit => 
                        QueerTraits.GetPreferenceFromActor(unit, false).Equals(Preference.DifferentSex) ||
                        QueerTraits.GetPreferenceFromActor(unit, true).Equals(Preference.DifferentSex));
                    if (heteroUnits > requirement)
                        return -50;
                }
                
                return 0;
            }
        });
        
        Add(new OpinionAsset
        {
            id = "king_lovers",
            translation_key = "opinion_king_lovers",
            calc = (pMain, pTarget) =>
            {
                if (pMain.king.lover == pTarget.king)
                    return 100;
                return 0;
            }
        });
    }

    private static void Add(OpinionAsset opinionAsset)
    {
        AssetManager.opinion_library.add(opinionAsset);
    }
}