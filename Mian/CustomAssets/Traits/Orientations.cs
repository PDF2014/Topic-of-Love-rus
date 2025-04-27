using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoModLoader.General;
using NeoModLoader.services;
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
        public readonly string PathLocale;
        private OrientationType(string orientation, string pathLocale)
        {
            Orientation = orientation;
            PathLocale = pathLocale;
        }

        public static OrientationType Create(string orientation, String sexualVariant, Func<Actor, bool> fitsCriteria)
        {
            var pathLocale = "orientations_" + orientation;
            var orientationType = new OrientationType(orientation, pathLocale);
            Orientations.Add(orientationType);
            
            LM.AddToCurrentLocale(pathLocale, orientation.ToUpper());
            return orientationType;
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
            
            var identities = List.Of("woman", "man");

            if (TOLUtil.IsTOIInstalled()) 
                identities.Add("xenogender");
            
            AddPreferenceType("identity", "#B57EDC", identities);
            
            if(TOLUtil.IsTOIInstalled())
                AddPreferenceType("sex_organ", "#B77E7E", List.Of("penis", "vagina"), false);

            OrientationType.Create("lesbiromantic", false, actor =>
            {
                
            });
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
            
            var dislikeRomance = CreateBaseTrait();
            dislikeRomance.id = "dislike_romance";
            dislikeRomance.group_id = "dislikes";
            dislikeRomance.path_icon = "ui/Icons/preference_traits/dislike_romance";
            
            _allTraits.Add(dislikeRomance);
            _allTraits.Add(dislikeSex);

            foreach (var trait in _allTraits)
            {
                AssetManager.traits.add(trait);
                trait.unlock(true); // for testing, remove later
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

        public static string GetOrientationBasedOnPreferences(Actor actor, bool sexual = false)
        {
            // var traits = GetActorPreferencesFromType(actor, "expression", sexual);
            // for now let's add TOI compat later;
            
            var preferredIdentities = GetActorPreferencesFromType(actor, "identity", sexual);
            var dislikes = Dislikes(actor, sexual);

            if (dislikes)
                return LM.Get("");


        }

        public static string GetIdentity(Actor actor)
        {
            
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
            return sexual ? actor.hasTrait("dislikes_sex") : actor.hasTrait("dislikes_romance");
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