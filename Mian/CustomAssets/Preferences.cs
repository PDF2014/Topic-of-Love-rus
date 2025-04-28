using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using EpPathFinding.cs;
using NCMS.Extensions;
using NeoModLoader.General;
using NeoModLoader.services;
using UnityEngine;
using Random = UnityEngine.Random;

#if TOPICOFIDENTITY
using Topic_of_Identity.Mian;
#endif

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class Orientation
    {
        public static readonly List<Orientation> Orientations = new();
        public readonly string OrientationType;
        public readonly string SexualPathLocale;
        public readonly string RomanticPathLocale;
        public readonly bool IsHomo;
        public readonly string HexCode;
        public readonly bool IsHetero;
        public readonly string SexualPathIcon;
        public readonly string RomanticPathIcon;
        private Sprite _sexualSprite;
        private Sprite _romanticSprite;
        public readonly Func<Actor, bool, bool> Criteria;
        private Orientation(string orientationType, string sexualPathLocale, string romanticPathLocale, string sexualPathIcon, string romanticPathIcon, bool isHomo, bool isHetero, string hexCode, Func<Actor, bool, bool> criteriaCheck)
        {
            OrientationType = orientationType;
            SexualPathLocale = sexualPathLocale;
            RomanticPathLocale = romanticPathLocale;
            Criteria = criteriaCheck;
            IsHomo = isHomo;
            HexCode = hexCode;
            IsHetero = isHetero;
            SexualPathIcon = sexualPathIcon;
            RomanticPathIcon = romanticPathIcon;
        }

        public Sprite GetSprite(bool sexual)
        {
            if (sexual)
            {
                if (_sexualSprite == null)
                    _sexualSprite = Resources.Load<Sprite>("ui/Icons/" + SexualPathIcon);
                return _sexualSprite;
            }

            if (_romanticSprite == null)
                _romanticSprite = Resources.Load<Sprite>("ui/Icons/" + RomanticPathIcon);
            return _romanticSprite;
        }
        
        public static Orientation Create(string orientation, string romanticVariant, bool isHomo, bool isHetero, string hexCode, Func<Actor, bool, bool> fitsCriteria)
        {
            var pathLocale = "orientations_" + orientation;
            var romanticPathLocale = "orientations_" + orientation + "_romantic";
            var sexualPathIcon = "orientations/"+orientation;
            var romanticPathIcon = "orientations/" + romanticVariant;
            var orientationType = new Orientation(orientation, pathLocale, romanticPathLocale, sexualPathIcon, romanticPathIcon, isHomo, isHetero, hexCode, fitsCriteria);
            Orientations.Add(orientationType);
            
            LM.AddToCurrentLocale(pathLocale, char.ToUpper(orientation.First()) + orientation.Substring(1));
            LM.AddToCurrentLocale(romanticPathLocale, char.ToUpper(romanticVariant.First()) + romanticVariant.Substring(1));
            return orientationType;
        }
        public static Orientation GetOrientation(string orientation)
        {
            return Orientations.Find(orientationType => orientationType.OrientationType.Equals(orientation));
        }
        public static Orientation GetOrientation(Actor actor, bool sexual)
        {
            var text = sexual ? "sexual_orientation" : "romantic_orientation";
            actor.data.get(text, out var orientation, "");
            return GetOrientation(orientation);
        }
        public static bool IsAHomo(Actor actor)
        {
            return GetOrientation(actor, false).IsHomo || GetOrientation(actor, true).IsHomo;
        }
        public static bool IsAHetero(Actor actor)
        {
            return GetOrientation(actor, false).IsHetero || GetOrientation(actor, true).IsHetero;
        }
    }
    public class PreferenceTrait : ActorTrait
    {
        public string WithoutOrientationID;
        public bool IsSexual;
    }
    public class Preferences
    {
        private static readonly List<PreferenceTrait> AllTraits = new();
        public static readonly Dictionary<string, List<PreferenceTrait>> PreferenceTypes = new();
        private static readonly Dictionary<string, List<string>> MatchingSets = new();
        
        public static void Init()
        {
            var identities = List.Of("female", "male");

            if (TOLUtil.IsTOIInstalled()) 
                identities.Add("xenogender");
            
            AddPreferenceType("identity", "#B57EDC", identities);
            
            if(TOLUtil.IsTOIInstalled())
                AddPreferenceType("expression", "#C900FF", List.Of("feminine", "masculine"));
            
            if(TOLUtil.IsTOIInstalled())
                AddPreferenceType("genital", "#B77E7E", List.Of("phallus", "vulva"), false);

            AddOrientations();
            AddMatchingSets();
            
            Finish();
        }
        
        // if all preferences of a preference type is added, remove them all (no reason for preferences if all are included?)
        private static void AddPreferenceType(string type, string hexColor, List<string> preferences, bool canBeRomantic=true)
        {
            if (PreferenceTypes.ContainsKey(type))
            {
                LogService.LogError(type + " is already an added preference type!");
                return;
            }

            AssetManager.trait_groups.add(new ActorTraitGroupAsset
            {
                id = type,
                name = "trait_group_"+type,
                color = hexColor
            });

            var withSpaces = type.Replace("_", " ");
            var endCombination = withSpaces.EndsWith("y") ? "ies" : "s";
            var nameWithPlural = withSpaces.TrimEnd('y') + endCombination;

            var stringBuilder = new StringBuilder(nameWithPlural);
            stringBuilder[0] = char.ToUpper(stringBuilder[0]);
            for(var i = 0; i < stringBuilder.Length - 1; i++)
            {
                if (stringBuilder[i].Equals(' '))
                {
                    stringBuilder[i + 1] = char.ToUpper(stringBuilder[i + 1]);
                }
            }

            nameWithPlural = stringBuilder.ToString();
            
            if(!LM.Has("trait_group_"+type))
                LM.AddToCurrentLocale("trait_group_"+type, "Preferred " + nameWithPlural);

            var preferenceTraits = new List<PreferenceTrait>();

            if (canBeRomantic)
            {
                var romanticTraits = new List<PreferenceTrait>();
                foreach (var preference in preferences)
                {
                    var romanticTrait = CreateBaseTrait();
                    romanticTrait.id = preference + "_romantic";
                    romanticTrait.WithoutOrientationID = preference;
                    romanticTrait.path_icon = "ui/Icons/preference_traits/romantic/" + preference;
                    romanticTrait.group_id = type;
                    romanticTrait.opposite_traits = new HashSet<ActorTrait>();
                    romanticTraits.Add(romanticTrait);
                    
                    if (!LM.Has("trait_" + preference + "_romantic"))
                        LM.AddToCurrentLocale("trait_" + preference + "_romantic", 
                            "Prefers " + preference.Substring(0, 1).ToUpper() + preference.Substring(1) + " (Romantic)");
                    if (!LM.Has("trait_" + preference + "_romantic_info"))
                        LM.AddToCurrentLocale("trait_" + preference + "_romantic_info", "Romantically prefers " + preference);
                    if (!LM.Has("trait_" + preference + "_romantic_info_2"))
                        LM.AddToCurrentLocale("trait_" + preference + "_romantic_info_2", "");
                }
                // _romanticTraits.AddRange(romanticTraits);
                AllTraits.AddRange(romanticTraits);
                preferenceTraits.AddRange(romanticTraits);
            }
            
            var sexualTraits = new List<PreferenceTrait>();
            foreach (var preference in preferences)
            {
                var sexualTrait = CreateBaseTrait();
                sexualTrait.IsSexual = true;
                sexualTrait.id = preference + "_sexual";
                sexualTrait.WithoutOrientationID = preference;
                sexualTrait.path_icon = "ui/Icons/preference_traits/sexual/" + preference;
                sexualTrait.group_id = type;
                sexualTrait.opposite_traits = new HashSet<ActorTrait>();
                sexualTraits.Add(sexualTrait);
                
                if (!LM.Has("trait_" + preference + "_sexual"))
                    LM.AddToCurrentLocale("trait_" + preference + "_sexual", 
                        "Prefers " + preference.Substring(0, 1).ToUpper() + preference.Substring(1) + (canBeRomantic ? " (Sexual)" : ""));
                if (!LM.Has("trait_" + preference + "_sexual_info"))
                    LM.AddToCurrentLocale("trait_" + preference + "_sexual_info", "Sexually prefers " + preference);
                if (!LM.Has("trait_" + preference + "_sexual_info_2"))
                    LM.AddToCurrentLocale("trait_" + preference + "_sexual_info_2", "");            
            }
            // _sexualityTraits.AddRange(sexualTraits);
            AllTraits.AddRange(sexualTraits);
            preferenceTraits.AddRange(sexualTraits);
            
            PreferenceTypes.Add(type, preferenceTraits);
        }
        private static void AddOrientations()
        {
            Orientation.Create("lesbian", "lesbiromantic", true, false, "#FF9A56", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if ((IdentifiesAsFemale(actor) || IsEnby(actor)) && actor.isSapient())
                {
                    var preferredIdentities = GetActorPreferencesFromType(actor, "identity", isSexual);
                    
                    if (preferredIdentities.Count == 1
                        && (HasPreference(actor, "female", isSexual) || HasPreference(actor, "xenogender", isSexual)))
                        return true;
                    
                    if (preferredIdentities.Count == 2 &&
                             HasPreference(actor, "female", isSexual) && HasPreference(actor, "xenogender", isSexual))
                        return true;
                }
                return false;
            });
            Orientation.Create("gay", "gayromantic", true, false, "#26CEAA", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if ((IdentifiesAsMale(actor) || IsEnby(actor)) && actor.isSapient())
                {
                    var preferredIdentities = GetActorPreferencesFromType(actor, "identity", isSexual);
                    
                    if (preferredIdentities.Count == 1
                        && (HasPreference(actor, "male", isSexual) || HasPreference(actor, "xenogender", isSexual)))
                        return true;
                    
                    if (preferredIdentities.Count == 2 && HasPreference(actor, "male", isSexual) && HasPreference(actor, "xenogender", isSexual))
                        return true;
                }
                return false;
            });
            Orientation.Create("straight", "straightromantic", false, true, "#FFFFFF", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (actor.isSapient())
                {
                    var preferredIdentities = GetActorPreferencesFromType(actor, "identity", isSexual);
                    if (preferredIdentities.Count == 1)
                    {
                        if (((PreferenceTrait) preferredIdentities.First()).WithoutOrientationID != GetIdentity(actor))
                            return true;
                    }
                }
                return false;
            });
            Orientation.Create("heterosexual", "heteroromantic",false, true, "#FFFFFF", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (!actor.isSapient())
                {
                    var preferredIdentities = GetActorPreferencesFromType(actor, "identity", isSexual);
                    if (preferredIdentities.Count == 1)
                    {
                        if (((PreferenceTrait) preferredIdentities.First()).WithoutOrientationID != GetIdentity(actor))
                            return true;
                    }
                }
                return false;
            });
            Orientation.Create("homosexual", "homoromantic", true, false, "#BB07DF", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (!actor.isSapient())
                {
                    var preferredIdentities = GetActorPreferencesFromType(actor, "identity", isSexual);

                    if (actor.isSexFemale())
                    {
                        if (preferredIdentities.Count == 1
                            && HasPreference(actor, "female", isSexual))
                            return true;   
                    }
                    else
                    {
                        if (preferredIdentities.Count == 1
                            && HasPreference(actor, "male", isSexual))
                            return true;   
                    }
                }
                return false;
            });
            Orientation.Create("bisexual", "biromantic", true, true, "#9B4F96", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (GetActorPreferencesFromType(actor, "identity", isSexual).Count >= 2)
                    return true;
                return false;
            });
            Orientation.Create("pansexual", "panromantic", true, true, "#FFD800", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (GetActorPreferencesFromType(actor, "identity", isSexual).Count == 0)
                    return true;
                return false;
            });

            Orientation.Create("asexual", "aromantic", false, false, "#FFFFFF", Dislikes);
        }
        private static void AddMatchingSets()
        {
            MatchingSets.Add("female", List.Of("feminine", "vulva"));
            MatchingSets.Add("male", List.Of("masculine", "phallus"));
        }
        private static void Finish()
        {
            AssetManager.trait_groups.add(new ActorTraitGroupAsset
            {
                id = "dislikes",
                name = "trait_group_dislikes",
                color = "#8B0000"
            });
            var dislikeSex = CreateBaseTrait();
            dislikeSex.id = "dislike_sex";
            dislikeSex.group_id = "dislikes";
            dislikeSex.path_icon = "ui/Icons/orientations/asexual";
            dislikeSex.IsSexual = true;
            dislikeSex.opposite_traits = new HashSet<ActorTrait>();
            
            var dislikeRomance = CreateBaseTrait();
            dislikeRomance.id = "dislike_romance";
            dislikeRomance.group_id = "dislikes";
            dislikeRomance.path_icon = "ui/Icons/orientations/aromantic";
            dislikeRomance.opposite_traits = new HashSet<ActorTrait>();

            AllTraits.Add(dislikeRomance);
            AllTraits.Add(dislikeSex);

            foreach (var trait in AllTraits)
            {
                AssetManager.traits.add(trait);
                trait.unlock(true); // for testing, remove later

                if (trait != dislikeSex && trait != dislikeRomance)
                {
                    if (trait.IsSexual)
                    {
                        dislikeSex.opposite_traits.Add(trait);
                        trait.opposite_traits.Add(dislikeSex);
                    }
                    else
                    {
                        dislikeRomance.opposite_traits.Add(trait);
                        trait.opposite_traits.Add(dislikeRomance);
                    }
                }
            }
            LM.ApplyLocale("en");
        }
        
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, bool sexual)
        {
            if (pActor == null || pTarget == null) return false;
            var identities = GetActorPreferencesFromType(pActor, "identity", sexual)
                .Select(trait => ((PreferenceTrait)trait).WithoutOrientationID).ToList();
            
            // identity matches
            if (!identities.Contains(GetIdentity(pTarget)) && identities.Count > 0)
                return false;

            if (TOLUtil.IsTOIInstalled())
            {
                var expressions = GetActorPreferencesFromType(pActor, "expression", sexual)
                    .Select(trait => ((PreferenceTrait) trait).WithoutOrientationID).ToList();
                
                if (!expressions.Contains(GetExpression(pTarget)) && expressions.Count > 0)
                    return false;

                if (sexual)
                {
                    var genitalias = GetActorPreferencesFromType(pActor, "genitalia")
                        .Select(trait => ((PreferenceTrait) trait).WithoutOrientationID).ToList();
                
                    if (!genitalias.Contains(GetGenitalia(pTarget)) && genitalias.Count > 0)
                        return false;
                }
            }
            
            return true;
        }

        public static bool BothPreferencesMatch(Actor actor1, Actor actor2)
        {
            return PreferenceMatches(actor1, actor2, false) && PreferenceMatches(actor1, actor2, true);
        }

        public static bool BothActorsPreferencesMatch(Actor actor1, Actor actor2, bool sexual)
        {
            return PreferenceMatches(actor1, actor2, sexual) && PreferenceMatches(actor2, actor1, sexual);
        }

        public static List<PreferenceTrait> GetRandomPreferences(Actor actor)
        {
            var bio = GetBiologicalSex(actor);
            var givenSets = new Dictionary<string, List<string>>();
            
            if (TOLUtil.NeedDifferentSexTypeForReproduction(actor))
            {
                var oppositeSex = bio.Equals("female") ? "male" : "female";
                givenSets.Add(oppositeSex, MatchingSets[oppositeSex]);
            } else if (TOLUtil.NeedSameSexTypeForReproduction(actor))
            {
                givenSets.Add(bio, MatchingSets[bio]);
            }

            var preferences = new List<PreferenceTrait>();

            if (givenSets.Count <= 0)
            {
                if (Randy.randomChance(0.33f))
                    return new List<PreferenceTrait>();
                if(Randy.randomChance(0.33f))
                    for (var i = 0; i < Randy.randomInt(1, GetAllPreferences().Count / 2); i++)
                    {
                        var count = 0;
                        PreferenceTrait preference = null;
                        while (!preferences.Contains(preference) && count < 5)
                        {
                            preference = RandomPreference(Randy.randomBool());
                            count++;
                        }
                        if(preference != null)
                            preferences.Add(preference);
                    }
                else
                {
                    givenSets.AddRange(MatchingSets);
                }
            }

            if (givenSets.Count > 0)
            {
                var keys = givenSets.Keys.ToList();
                string randomKey;

                if (Randy.randomChance(0.7f))
                {
                    randomKey = keys.GetRandom();
                
                    var preferredIdentitySexual = Randy.randomChance(0.95f) ? 
                        GetPreferenceTraitFromID(randomKey, true) : RandomPreferenceFromType("identity", true);
                    var preferredIdentityRomantic = Randy.randomChance(0.95f) ?
                        GetOtherVariant(preferredIdentitySexual) : RandomPreferenceFromType("identity");

                    preferences.Add(preferredIdentitySexual);
                    preferences.Add(preferredIdentityRomantic);
                    
                    if (Randy.randomChance(0.5f))
                    {
                        var randomSexual = RandomPreferenceFromType("identity", true);
                        while(preferences.Contains(randomSexual))
                            randomSexual = RandomPreferenceFromType("identity", true);
                        preferences.Add(randomSexual);

                        var randomRomantic = Randy.randomChance(0.95f)
                            ? GetOtherVariant(randomSexual)
                            : RandomPreferenceFromType("identity");
                        while(preferences.Contains(randomRomantic))
                            randomRomantic = RandomPreferenceFromType("identity");
                        preferences.Add(randomRomantic);
                    }
                }
                
                if (TOLUtil.IsTOIInstalled())
                {
                    randomKey = keys.GetRandom();
                    if (Randy.randomChance(0.2f))
                    {
                        var preferredExpressionSexual = Randy.randomChance(0.5f) ? 
                            GetPreferenceTraitFromID(givenSets[randomKey][0], true) : RandomPreferenceFromType("expression", true);
                        var preferredExpressionRomantic = Randy.randomChance(0.5f) ?
                            GetOtherVariant(preferredExpressionSexual) : RandomPreferenceFromType("expression");   
                        preferences.Add(preferredExpressionSexual);
                        preferences.Add(preferredExpressionRomantic);
                    }

                    if (Randy.randomChance(0.2f))
                    {
                        randomKey = keys.GetRandom();
                        var preferredGenitalSexual = Randy.randomChance(0.8f) ? 
                            GetPreferenceTraitFromID(givenSets[randomKey][1], true) : RandomPreferenceFromType("genital", true);
                    
                        preferences.Add(preferredGenitalSexual);   
                    }
                }   
            }
            
            if (Randy.randomChance(0.05f))
                return List.Of(
                    AllTraits.Find(trait => trait.id.Equals("dislike_sex")), 
                    AllTraits.Find(trait => trait.id.Equals("dislike_romance")));
            
            return preferences;
        }
        
        // may return null if the variant does not exist
        public static PreferenceTrait GetOtherVariant(PreferenceTrait trait)
        {
            PreferenceTypes.TryGetValue(trait.group_id, out var preferenceGroup);
            var id = trait.WithoutOrientationID;
            return preferenceGroup.Find(match => match.WithoutOrientationID.Equals(id) && !match.id.Equals(trait.id));
        }
        public static bool HasPreference(Actor actor, string preference, bool sexual = false)
        {
            foreach (var list in PreferenceTypes.Values)
            {
                var trait = list.Find(trait => trait.WithoutOrientationID.Equals(preference) && trait.IsSexual == sexual);
                if (trait != null)
                    return actor.hasTrait(trait);
            }
            return false;
        }
        public static List<ActorTrait> GetActorPreferencesFromType(Actor actor, string type, bool sexual = false)
        {
            return GetActorPreferences(actor, sexual).Where(trait => trait.group_id.Equals(type)).ToList();
        }
        
        public static List<ActorTrait> GetActorPreferences(Actor actor, bool sexual = false)
        {
            return actor.traits.Where(
                trait => trait is PreferenceTrait preferenceTrait && preferenceTrait.IsSexual == sexual
            ).ToList();
        }
        public static List<PreferenceTrait> GetPreferencesFromType(string type, bool sexual = false)
        {
            return PreferenceTypes[type].Where(
                trait => trait.IsSexual == sexual
            ).ToList();
        }
        public static List<PreferenceTrait> GetAllPreferences()
        {
            var list = new List<PreferenceTrait>();
            PreferenceTypes.Values.ForEach(list.AddRange);
            return list;
        }
        public static PreferenceTrait RandomPreferenceFromType(string type, bool sexual = false)
        {
            return GetPreferencesFromType(type, sexual).GetRandom();
        }
        
        public static PreferenceTrait RandomPreference(bool sexual = false)
        {
            var preferences = GetPreferencesFromType(PreferenceTypes.Keys.ToList().GetRandom(), sexual);
            return preferences.Count > 0 ? preferences.GetRandom() : null;
        }
        
        public static PreferenceTrait GetPreferenceTraitFromID(string id, bool sexual = false)
        {
            return AllTraits.Find(trait => trait.IsSexual == sexual && trait.WithoutOrientationID.Equals(id));
        }

        // randomizes if multiple orientations fit the criteria
        public static Orientation GetOrientationFromActor(Actor actor, bool sexual = false)
        {
            var orientations =
                Orientation.Orientations.Where(orientationType => orientationType.Criteria(actor, sexual)).ToList();
            
            return orientations.GetRandom(); // at the very least should be pansexual
        }
        public static void CreateOrientations(Actor actor)
        {
            var sexualOrientation = GetOrientationFromActor(actor, true);
            var romanticOrientation = GetOrientationFromActor(actor);
            actor.data.set("romantic_orientation", romanticOrientation.OrientationType);
            actor.data.set("sexual_orientation", sexualOrientation.OrientationType);
        }
        public static void CreateOrientationBasedOnPrefChange(Actor actor, PreferenceTrait newTrait)
        {
            var orientation = GetOrientationFromActor(actor, newTrait.IsSexual);
            if(newTrait.IsSexual)
                actor.data.set("sexual_orientation", orientation.OrientationType);
            else
                actor.data.set("romantic_orientation", orientation.OrientationType);
        }
        public static string GetIdentity(Actor actor)
        {
            if (TOLUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "female" : "male";
        }

        public static string GetExpression(Actor actor)
        {
            if (TOLUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "feminine" : "masculine";
        }
        public static string GetGenitalia(Actor actor)
        {
            if (TOLUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "vulva" : "phallus";
        }
        public static bool IsMasculine(Actor actor)
        {
            return GetExpression(actor).Equals("masculine");
        }
        public static bool IsFeminine(Actor actor)
        {
            return GetExpression(actor).Equals("feminine");
        }
        public static bool HasVulva(Actor actor)
        {
            return GetGenitalia(actor).Equals("vulva");
        }
        public static bool HasPenis(Actor actor)
        {
            return GetGenitalia(actor).Equals("phallus");
        }
        public static string GetBiologicalSex(Actor actor)
        {
            return actor.isSexFemale() ? "female" : "male";
        }
        public static bool IsEnby(Actor actor)
        {
            return GetIdentity(actor).Equals("xenogender");
        }
        public static bool IdentifiesAsFemale(Actor actor)
        {
            return GetIdentity(actor).Equals("female");
        }
        public static bool IdentifiesAsMale(Actor actor)
        {
            return GetIdentity(actor).Equals("male");
        }

        public static bool Dislikes(Actor actor, bool sexual = false)
        {
            return sexual ? actor.hasTrait("dislike_sex") : actor.hasTrait("dislike_romance");
        }

        private static PreferenceTrait CreateBaseTrait()
        {
            return new PreferenceTrait
            {
                spawn_random_trait_allowed = false,
                rate_birth = 0,
                rate_acquire_grow_up = 0,
                is_mutation_box_allowed = false,
                type = TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R3_Legendary,
                needs_to_be_explored = true,
                affects_mind = true,
                can_be_in_book = false,
            };
        }
    }
}