using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using NeoModLoader.General;
using NeoModLoader.services;
using UnityEngine;
using Random = UnityEngine.Random;

#if TOPICOFIDENTITY
using Topic_of_Identity.Mian;
#endif

/**
 * Preferences based on:
 * Femininity/masculinity (expression, based on sex if without topic of identity)
 * Gender (sex if without topic of identity, include ENBY if topic of identity is in the game)
 * Penis/Vagina (sexual preference, will have patch for topic of identity)
 * Determine orientations based on preferences
 * Opinions on sex/romance (Determines asexuality/aromantic) (adding any of these traits will remove sexual/romantic orientation traits)
 */

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class OrientationType
    {
        public static readonly List<OrientationType> Orientations = new();
        public readonly string Orientation;
        public readonly string SexualPathLocale;
        public readonly string RomanticPathLocale;
        public readonly bool IsHomo;
        public readonly string HexCode;
        public readonly bool IsHetero;
        public readonly string SexualPathIcon;
        public readonly string RomanticPathIcon;
        public readonly Func<Actor, bool, bool> Criteria;
        private OrientationType(string orientation, string sexualPathLocale, string romanticPathLocale, string sexualPathIcon, string romanticPathIcon, bool isHomo, bool isHetero, string hexCode, Func<Actor, bool, bool> criteriaCheck)
        {
            Orientation = orientation;
            SexualPathLocale = sexualPathLocale;
            RomanticPathLocale = romanticPathLocale;
            Criteria = criteriaCheck;
            IsHomo = isHomo;
            HexCode = hexCode;
            IsHetero = isHetero;
            SexualPathIcon = sexualPathIcon;
            RomanticPathIcon = romanticPathIcon;
        }

        public static OrientationType Create(string orientation, string romanticVariant, bool isHomo, bool isHetero, string hexCode, Func<Actor, bool, bool> fitsCriteria)
        {
            var pathLocale = "orientations_" + orientation;
            var romanticPathLocale = "orientations_" + orientation + "_romantic";
            var sexualPathIcon = "orientations/"+orientation;
            var romanticPathIcon = "orientations/" + romanticVariant;
            var orientationType = new OrientationType(orientation, pathLocale, romanticPathLocale, sexualPathIcon, romanticPathIcon, isHomo, isHetero, hexCode, fitsCriteria);
            Orientations.Add(orientationType);
            
            LM.AddToCurrentLocale(pathLocale, char.ToUpper(orientation.First()) + orientation.Substring(1));
            LM.AddToCurrentLocale(romanticPathLocale, char.ToUpper(romanticVariant.First()) + romanticVariant.Substring(1));
            return orientationType;
        }

        public static OrientationType GetOrientation(string orientation)
        {
            return Orientations.Find(orientationType => orientationType.Orientation.Equals(orientation));
        }
    }
    public class PreferenceTrait : ActorTrait
    {
        public string WithoutOrientationID;
        public bool IsSexual;
    }
    public class PreferenceTraits
    {
        // private static List<ActorTrait> _sexualityTraits = new List<ActorTrait>();
        // private static List<ActorTrait> _romanticTraits = new List<ActorTrait>();
        private static List<PreferenceTrait> _allTraits = new();
        private static Dictionary<string, List<PreferenceTrait>> _preferenceTypes = new();

        public static Dictionary<string, List<PreferenceTrait>> PreferenceTypes => _preferenceTypes;
        
        public static void Init()
        {
            
            if(TOLUtil.IsTOIInstalled())
                AddPreferenceType("expression", "#C900FF", List.Of("femininity", "masculinity"));
            
            var identities = List.Of("female", "male");

            if (TOLUtil.IsTOIInstalled()) 
                identities.Add("xenogender");
            
            AddPreferenceType("identity", "#B57EDC", identities);
            
            if(TOLUtil.IsTOIInstalled())
                AddPreferenceType("sex_organ", "#B77E7E", List.Of("penis", "vagina"), false);

            AddOrientations();
            
            Finish();
        }
        
        // if all preferences of a preference type is added, remove them all (no reason for preferences if all are included?)
        private static void AddPreferenceType(string type, string hexColor, List<string> preferences, bool canBeRomantic=true)
        {
            if (_preferenceTypes.ContainsKey(type))
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
                    romanticTrait.path_icon = "ui/Icons/preference_traits/" + preference + "_romantic";
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
                _allTraits.AddRange(romanticTraits);
                preferenceTraits.AddRange(romanticTraits);
            }
            
            var sexualTraits = new List<PreferenceTrait>();
            foreach (var preference in preferences)
            {
                var sexualTrait = CreateBaseTrait();
                sexualTrait.IsSexual = true;
                sexualTrait.id = preference + "_sexual";
                sexualTrait.WithoutOrientationID = preference;
                sexualTrait.path_icon = "ui/Icons/preference_traits/" + preference + "_sexual";
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
            _allTraits.AddRange(sexualTraits);
            preferenceTraits.AddRange(sexualTraits);
            
            _preferenceTypes.Add(type, preferenceTraits);
        }

        private static void AddOrientations()
        {
            OrientationType.Create("lesbian", "lesbiromantic", true, false, "#FF9A56", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if ((IsFemale(actor) || IsEnby(actor)) && actor.isSapient())
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
            OrientationType.Create("gay", "gayromantic", true, false, "#26CEAA", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if ((IsMale(actor) || IsEnby(actor)) && actor.isSapient())
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
            OrientationType.Create("straight", "straightromantic", false, true, "#FFFFFF", (actor, isSexual) =>
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
            OrientationType.Create("heterosexual", "heteroromantic",false, true, "#FFFFFF", (actor, isSexual) =>
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
            OrientationType.Create("homosexual", "homoromantic", true, false, "#732982", (actor, isSexual) =>
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
            OrientationType.Create("bisexual", "biromantic", true, true, "#9B4F96", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (GetActorPreferencesFromType(actor, "identity", isSexual).Count >= 2)
                    return true;
                return false;
            });
            OrientationType.Create("pansexual", "panromantic", true, true, "#FFD800", (actor, isSexual) =>
            {
                if (Dislikes(actor, isSexual))
                    return false;
                
                if (GetActorPreferencesFromType(actor, "identity", isSexual).Count == 0)
                    return true;
                return false;
            });

            OrientationType.Create("asexual", "aromantic", false, false, "#FFFFFF", Dislikes);

            // var lesbianFits = List.Of(List.Of("woman", "xenogender"));
            // if(TOLUtil.IsTOIInstalled())
            //     lesbianFits.Add(List.Of("femininity", "masculinity"));
            // OrientationType.Create("lesbiromantic", lesbianFits, lesbianFits);

            // OrientationType.Create("homosexual", 
            //     List.Of(List.Of("woman", "man")), 
            //     List.Of(""));
            // OrientationType.Create("homoromantic");
            // OrientationType.Create("bisexual");
            // OrientationType.Create("biromantic");
            // OrientationType.Create("");
            // OrientationType.Create("homoromantic");
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
            dislikeSex.path_icon = "ui/Icons/preference_traits/dislike_sex";
            dislikeSex.IsSexual = true;
            dislikeSex.opposite_traits = new HashSet<ActorTrait>();
            
            var dislikeRomance = CreateBaseTrait();
            dislikeRomance.id = "dislike_romance";
            dislikeRomance.group_id = "dislikes";
            dislikeRomance.path_icon = "ui/Icons/preference_traits/dislike_romance";
            dislikeRomance.opposite_traits = new HashSet<ActorTrait>();

            _allTraits.Add(dislikeRomance);
            _allTraits.Add(dislikeSex);

            foreach (var trait in _allTraits)
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

        // may return null if the variant does not exist
        public static PreferenceTrait GetOtherVariant(PreferenceTrait trait)
        {
            _preferenceTypes.TryGetValue(trait.group_id, out var preferenceGroup);
            var id = trait.WithoutOrientationID;
            return preferenceGroup.Find(match => match.WithoutOrientationID.Equals(id) && !match.id.Equals(trait.id));
        }

        public static bool HasPreference(Actor actor, string preference, bool sexual = false)
        {
            foreach (var list in _preferenceTypes.Values)
            {
                var trait = list.Find(trait => trait.WithoutOrientationID.Equals(preference) && trait.IsSexual == sexual);
                if (trait != null)
                    return actor.hasTrait(trait);
            }
            return false;
        }

        public static List<ActorTrait> GetActorPreferencesFromType(Actor actor, string type, bool sexual = false)
        {
            return actor.traits.Where(
                trait => trait is PreferenceTrait preferenceTrait && preferenceTrait.group_id.Equals(type) && preferenceTrait.IsSexual == sexual
            ).ToList();
        }
        
        public static List<PreferenceTrait> GetPreferencesFromType(string type, bool sexual = false)
        {
            return _preferenceTypes[type].Where(
                trait => trait.IsSexual == sexual
            ).ToList();
        }

        // randomizes if multiple orientations fit the criteria
        public static OrientationType GetOrientationFromActor(Actor actor, bool sexual = false)
        {
            var orientations =
                OrientationType.Orientations.Where(orientationType => orientationType.Criteria(actor, sexual)).ToList();
            
            return orientations.GetRandom(); // at the very least should be pansexual
        }

        public static void CreateOrientations(Actor actor)
        {
            var sexualOrientation = GetOrientationFromActor(actor, true);
            var romanticOrientation = GetOrientationFromActor(actor, false);
            actor.data.set("romantic_orientation", romanticOrientation.Orientation);
            actor.data.set("sexual_orientation", sexualOrientation.Orientation);
        }
        
        public static void CreateOrientationBasedOnPrefChange(Actor actor, PreferenceTrait newTrait)
        {
            var orientation = GetOrientationFromActor(actor, newTrait.IsSexual);
            if(newTrait.IsSexual)
                actor.data.set("sexual_orientation", orientation.Orientation);
            else
                actor.data.set("romantic_orientation", orientation.Orientation);
            
            // var text = preferenceTrait.IsSexual
            //     ? LM.Get(orientation.SexualPathLocale)
            //     : LM.Get(orientation.RomanticPathLocale);
            // LogService.LogInfo("New orientation label based on new preferences: " + text);
        }


        public static string GetIdentity(Actor actor)
        {
            if (TOLUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "female" : "male";
        }
        public static bool IsEnby(Actor actor)
        {
            return GetIdentity(actor).Equals("xenogender");
        }
        
        public static bool IsFemale(Actor actor)
        {
            return GetIdentity(actor).Equals("female");
        }

        public static bool IsMale(Actor actor)
        {
            return GetIdentity(actor).Equals("male");
        }
        
        // #if TOPICOFIDENTITY
        // public static Expression GetExpression(Actor actor)
        // {
        //     // Topic of identity compat code
        //     // return TOIUtil.GetExpression(actor);
        // }
        // #endif

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
    public enum Preference
    {
        All,
        SameSex,
        DifferentSex,
        SameOrDifferentSex,
        Neither,
        Inapplicable
    }

    public class QueerTrait : ActorTrait
    {
        public Preference preference;
    }
    
    // this will be improved upon in the future cuz it's not perfect representation
    public class Orientations
    {
        private static List<QueerTrait> _sexualityTraits = new List<QueerTrait>();
        private static List<QueerTrait> _romanticTraits = new List<QueerTrait>();
        private static List<QueerTrait> _allTraits = new List<QueerTrait>();
        public static void Init()
        {
            // maybe add actions to break up when romantic traits change
            AddQueerTrait("heterosexual", Preference.DifferentSex, true);
            AddQueerTrait("homosexual", Preference.SameSex, true);
            AddQueerTrait("bisexual", Preference.SameOrDifferentSex, true);
            AddQueerTrait("asexual", Preference.Neither, true);
            AddQueerTrait("abrosexual", Preference.Inapplicable, true);
            
            AddQueerTrait("heteroromantic", Preference.DifferentSex, false);
            AddQueerTrait("homoromantic", Preference.SameSex, false);
            AddQueerTrait("biromantic", Preference.SameOrDifferentSex, false);
            AddQueerTrait("aromantic", Preference.Neither, false);
            AddQueerTrait("abroromantic", Preference.Inapplicable, false);
            
            Finish();
        }

        public static void Finish()
        {
            foreach(var trait in _sexualityTraits)
            {
                if (trait.preference == Preference.Inapplicable) continue;
                
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (var traitToAdd in _sexualityTraits)
                {
                    if (trait == traitToAdd || traitToAdd.preference == Preference.Inapplicable) continue;
                    trait.opposite_traits.Add(traitToAdd);
                }

                // trait.traits_to_remove_ids = trait.opposite_list.ToArray();
            }
            
            foreach(var trait in _romanticTraits)
            {
                if (trait.preference == Preference.Inapplicable) continue;
                
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (var traitToAdd in _romanticTraits)
                {
                    if (trait == traitToAdd || traitToAdd.preference == Preference.Inapplicable) continue;
                    trait.opposite_traits.Add(traitToAdd);
                }
                
                // trait.traits_to_remove_ids = trait.opposite_list.ToArray();
            }
            _allTraits = _sexualityTraits.ConvertAll(trait => trait);
            _allTraits.AddRange(_romanticTraits);
        }

        public static void CleanQueerTraits(Actor pActor, bool sexual, bool clearInapplicable=false)
        {
            var list = (sexual ? _sexualityTraits : _romanticTraits).Where(trait => !trait.preference.Equals(Preference.Inapplicable) || clearInapplicable).ToList();
            foreach (var trait in list)
            {
                pActor.removeTrait(trait);
            }
        }

        public static bool HasQueerTraits(Actor pActor)
        {
            foreach (var trait in _allTraits)
            {
                if (pActor.hasTrait(trait))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<QueerTrait> GetQueerTraits(Actor pActor, bool excludeInapplicable=false)
        {
            List<QueerTrait> list = new List<QueerTrait>();
            foreach (var trait in _sexualityTraits)
            {
                if (pActor.hasTrait(trait) && (!trait.preference.Equals(Preference.Inapplicable) || !excludeInapplicable))
                {
                    list.Add(trait);
                }
            }
            foreach (var trait in _romanticTraits)
            {
                if (pActor.hasTrait(trait) && (!trait.preference.Equals(Preference.Inapplicable) || !excludeInapplicable))
                {
                    list.Add(trait);
                }
            }

            return list;
        }

        public static void GiveQueerTraits(Actor pActor, bool equalChances, bool clearInapplicable = false)
        {
            if (!pActor.hasSubspecies()) return;
            var currentTraits = GetQueerTraits(pActor);
            foreach (var trait in currentTraits)
            {
                if (!clearInapplicable && trait.preference.Equals(Preference.Inapplicable)) continue;
                pActor.removeTrait(trait);
            }
            
            var queerTraits = RandomizeQueerTraits(pActor, equalChances, currentTraits);
            for(int i = 0; i < queerTraits.Count; i++)
            {
                if (queerTraits[i] != null)
                {
                    pActor.addTrait(queerTraits[i]);
                }
            }
        }

        public static Preference GetSexualPrefBasedOnReproduction(Actor pActor)
        {
            if (!pActor.hasSubspecies()) return Preference.Neither;
            if (pActor.subspecies.hasTraitReproductionSexual())
                return Preference.DifferentSex;
            if (pActor.subspecies.hasTraitReproductionSexualHermaphroditic())
                return Preference.SameOrDifferentSex;
            if (pActor.hasSubspeciesTrait("reproduction_same_sex"))
                return Preference.SameSex;
            return Preference.Neither;
        }

        // randomizes chances based on what actor's reproduction methods
        public static List<QueerTrait> RandomizeQueerTraits(Actor pActor, bool equalChances, List<QueerTrait> exclude)
        {
            if (!pActor.hasSubspecies()) return null;
            // randomize for sexual
            var matchingPreference = equalChances ? Preference.All : GetSexualPrefBasedOnReproduction(pActor);

            List<QueerTrait> randomPool = new List<QueerTrait>();
            foreach (var trait in _sexualityTraits)
            {
                if (exclude.Contains(trait) || trait.preference.Equals(Preference.Inapplicable)) continue;
                
                if (trait.preference.Equals(matchingPreference))
                {
                    for (int i = 0; i < 27; i++)
                    {
                        randomPool.Add(trait);
                    }
                }
                else
                {
                    randomPool.Add(trait);
                }
            }

            var sexualTrait = randomPool[Random.Range(0, randomPool.Count)];
            var romanticTrait = GetOppositeVariant(sexualTrait);
            
            // random chance that romantic trait will not fit sexuality trait
            if (Randy.randomChance(0.03f))
            {
                romanticTrait = _romanticTraits[Random.Range(0, _romanticTraits.Count)];
            }
            
            var queerTraits = List.Of(sexualTrait, romanticTrait);
            
            // randomize non-preference traits
            if (Randy.randomChance(0.05f))
            {
                randomPool = _sexualityTraits.Where(trait => trait.preference.Equals(Preference.Inapplicable)).ToList();
                queerTraits.Add(randomPool[Random.Range(0, randomPool.Count)]);
            }
            
            if (Randy.randomChance(0.05f))
            {
                randomPool = _romanticTraits.Where(trait => trait.preference.Equals(Preference.Inapplicable)).ToList();
                queerTraits.Add(randomPool[Random.Range(0, randomPool.Count)]);
            }

            return queerTraits;
        }
        
        // gets the romantic/sexual version of the trait that matches preferences
        public static QueerTrait GetOppositeVariant(QueerTrait trait)
        {
            return (_sexualityTraits.Contains(trait) ? _romanticTraits : _sexualityTraits).Find(traitInList => traitInList.preference.Equals(trait.preference));
        }
        public static Preference GetPreferenceFromActor(Actor pActor, bool sexual)
        {
            List<QueerTrait> list = sexual ? _sexualityTraits : _romanticTraits;
            foreach (QueerTrait trait in list)
            {
                if(pActor.hasTrait(trait) && !trait.preference.Equals(Preference.Inapplicable))
                    return trait.preference;
            }

            return Preference.Inapplicable; // if they have no preference, then they like neither
        }

        public static bool IncludesHomoPreference(Preference preference)
        {
            return preference.Equals(Preference.SameSex) || preference.Equals(Preference.SameOrDifferentSex) || preference.Equals(Preference.All);
        }
        
        // Important note that this checks FROM the first actor's point of view, you should also use PreferenceMatches on the other actor to confirm they both like each other!
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, bool sexual)
        {
            if (pActor == null || pTarget == null) return false;
            var preference = GetPreferenceFromActor(pActor, sexual);
            switch (preference)
            {
                case Preference.SameOrDifferentSex:
                    return true;
                case Preference.SameSex:
                    return pActor.data.sex == pTarget.data.sex;
                case Preference.DifferentSex:
                    return pActor.data.sex != pTarget.data.sex;
                default:
                    return false;
            }
        }

        public static bool BothPreferencesMatch(Actor actor1, Actor actor2)
        {
            return PreferenceMatches(actor1, actor2, false) && PreferenceMatches(actor1, actor2, true);
        }

        public static bool BothActorsPreferencesMatch(Actor actor1, Actor actor2, bool sexual)
        {
            return PreferenceMatches(actor1, actor2, sexual) && PreferenceMatches(actor2, actor1, sexual);
        }

        internal static void AddQueerTrait(string name, Preference preference, bool sexual)
        {
            var trait = new QueerTrait
            {
                id = name,
                path_icon = "ui/Icons/actor_traits/" + name,
                group_id = "mind",
                spawn_random_trait_allowed = false,
                rate_birth = 0,
                rate_acquire_grow_up = 0,
                is_mutation_box_allowed = false,
                type = TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R3_Legendary,
                needs_to_be_explored = true,
                affects_mind = true,
                preference = preference
            };
            if(sexual)
                _sexualityTraits.Add(trait);
            else
            {
                _romanticTraits.Add(trait);
            }

            AssetManager.traits.add(trait);
        }
    }
}