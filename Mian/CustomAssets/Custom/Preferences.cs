﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCMS.Extensions;
using NeoModLoader.General;
using NeoModLoader.services;
#if TOPICOFIDENTITY
using Topic_of_Identity.Mian;
#endif

namespace Topic_of_Love.Mian.CustomAssets.Custom
{
    public class Preference
    {
        public string id;
        public string path_icon;
        public string group_id;
        public string WithoutOrientationID;
        public bool IsSexual;
    }
    public static class Preferences
    {
        private static readonly List<Preference> AllPreferences = new();
        public static readonly Dictionary<string, List<Preference>> PreferenceTypes = new();
        private static readonly Dictionary<string, List<string>> MatchingSets = new();
        
        public static void Init()
        {
            var identities = List.Of("female", "male");

            if (TolUtil.IsTOIInstalled())
                identities.Add("xenogender");
            
            AddPreferenceType("identity", "#B57EDC", identities);
            
            if(TolUtil.IsTOIInstalled())
                AddPreferenceType("expression", "#C900FF", List.Of("feminine", "masculine"));
            
            if(TolUtil.IsTOIInstalled())
                AddPreferenceType("genital", "#B77E7E", List.Of("phallus", "vulva"), false);

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

            var preferenceTraits = new List<Preference>();

            if (canBeRomantic)
            {
                var romanticTraits = new List<Preference>();
                foreach (var preference in preferences)
                {
                    var romanticTrait = new Preference
                    {
                        id = preference + "_romantic",
                        group_id = type,
                        WithoutOrientationID = preference,
                        path_icon = "ui/Icons/preference_traits/romantic/" + preference
                    };
                    // romanticTrait.group_id = type;
                    // romanticTrait.opposite_traits = new HashSet<ActorTrait>();
                    romanticTraits.Add(romanticTrait);

                    if (!LM.Has("trait_" + preference + "_romantic"))
                        LM.AddToCurrentLocale("trait_" + preference + "_romantic", 
                            "Prefers " + preference.Substring(0, 1).ToUpper() + preference.Substring(1) + " (Romantic)");
                    if (!LM.Has("trait_" + preference + "_romantic_info"))
                        LM.AddToCurrentLocale("trait_" + preference + "_romantic_info", "Romantically prefers " + preference);
                    if (!LM.Has("trait_" + preference + "_romantic_info_2"))
                        LM.AddToCurrentLocale("trait_" + preference + "_romantic_info_2", "");
                }
                AllPreferences.AddRange(romanticTraits);
                preferenceTraits.AddRange(romanticTraits);
            }
            
            var sexualTraits = new List<Preference>();
            foreach (var preference in preferences)
            {
                var sexualTrait = new Preference
                {
                    id = preference + "_sexual",
                    group_id = type,
                    WithoutOrientationID = preference,
                    path_icon = "ui/Icons/preference_traits/sexual/" + preference
                };
                // sexualTrait.group_id = type;
                // sexualTrait.opposite_traits = new HashSet<ActorTrait>();
                sexualTraits.Add(sexualTrait);
                
                if (!LM.Has("trait_" + preference + "_sexual"))
                    LM.AddToCurrentLocale("trait_" + preference + "_sexual", 
                        "Prefers " + preference.Substring(0, 1).ToUpper() + preference.Substring(1) + (canBeRomantic ? " (Sexual)" : ""));
                if (!LM.Has("trait_" + preference + "_sexual_info"))
                    LM.AddToCurrentLocale("trait_" + preference + "_sexual_info", "Sexually prefers " + preference);
                if (!LM.Has("trait_" + preference + "_sexual_info_2"))
                    LM.AddToCurrentLocale("trait_" + preference + "_sexual_info_2", "");            
            }
            AllPreferences.AddRange(sexualTraits);
            preferenceTraits.AddRange(sexualTraits);
            
            PreferenceTypes.Add(type, preferenceTraits);
        }
        private static void AddMatchingSets()
        {
            MatchingSets.Add("female", List.Of("feminine", "vulva"));
            MatchingSets.Add("male", List.Of("masculine", "phallus"));
        }
        private static void Finish()
        {
            // AssetManager.trait_groups.add(new ActorTraitGroupAsset
            // {
            //     id = "dislikes",
            //     name = "trait_group_dislikes",
            //     color = "#8B0000"
            // });
            // var dislikeSex = CreateBaseTrait();
            // dislikeSex.id = "dislike_sex";
            // dislikeSex.group_id = "dislikes";
            // dislikeSex.path_icon = "ui/Icons/orientations/asexual";
            // dislikeSex.IsSexual = true;
            // dislikeSex.base_stats = new();
            // dislikeSex.base_stats["multiplier_intimacy_happiness"] = 0.5f;
            // dislikeSex.opposite_traits = new HashSet<ActorTrait>();
            //
            // var dislikeRomance = CreateBaseTrait();
            // dislikeRomance.id = "dislike_romance";
            // dislikeRomance.group_id = "dislikes";
            // dislikeRomance.path_icon = "ui/Icons/orientations/aromantic";
            // dislikeRomance.base_stats = new();
            // dislikeRomance.base_stats["multiplier_intimacy_happiness"] = 0.5f;
            // dislikeRomance.opposite_traits = new HashSet<ActorTrait>();
            //
            // AllPreferences.Add(dislikeRomance);
            // AllPreferences.Add(dislikeSex);
            //
            // foreach (var trait in AllPreferences)
            // {
            //     AssetManager.traits.add(trait);
            //
            //     if (trait != dislikeSex && trait != dislikeRomance)
            //     {
            //         if (trait.IsSexual)
            //         {
            //             dislikeSex.opposite_traits.Add(trait);
            //             trait.opposite_traits.Add(dislikeSex);
            //         }
            //         else
            //         {
            //             dislikeRomance.opposite_traits.Add(trait);
            //             trait.opposite_traits.Add(dislikeRomance);
            //         }
            //     }
            // }
            LM.ApplyLocale("en");
        }

        // checks for one actor based on a type, if you are checking for multiple types or multiple actors, then use the other methods
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, string type, bool sexual)
        {
            if (pActor == null || pTarget == null) return false;
            
            // preferences do not matter
            if (pActor.hasCultureTrait("orientationless"))
                return true;

            var list = GetActorPreferencesFromType(pActor, type, sexual)
                .Select(trait => ((Preference)trait).WithoutOrientationID).ToList();
            var targetType = GetType(pTarget, type);

            if (targetType != null && list.Count > 0 && !list.Contains(targetType))
                return false;
            
            return true;
        }
        
        // checks for one actor, if you are checking for both, use the other methods
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, bool sexual)
        {
            return PreferenceMatches(pActor, pTarget, "identity", sexual) 
                   && PreferenceMatches(pActor, pTarget, "expression", sexual)
                   && (!sexual || PreferenceMatches(pActor, pTarget, "genital", true));
        }

        public static bool PreferenceMatches(Actor actor1, Actor actor2)
        {
            return PreferenceMatches(actor1, actor2, false) && PreferenceMatches(actor1, actor2, true);
        }

        public static bool BothActorsPreferenceMatch(Actor actor1, Actor actor2, bool sexual)
        {
            return PreferenceMatches(actor1, actor2, sexual) && PreferenceMatches(actor2, actor1, sexual);
        }
        
        public static bool BothActorsPreferenceMatch(Actor actor1, Actor actor2)
        {
            return PreferenceMatches(actor1, actor2, false) && PreferenceMatches(actor2, actor1, false)
                && PreferenceMatches(actor2, actor1, true) &&  PreferenceMatches(actor1, actor2, true);;
        }

        public static List<Preference> GetRandomPreferences(Actor actor)
        {
            var bio = GetBiologicalSex(actor);
            var preferredSets = new Dictionary<string, List<string>>();
            
            if (TolUtil.NeedDifferentSexTypeForReproduction(actor))
            {
                var oppositeSex = bio.Equals("female") ? "male" : "female";
                preferredSets.Add(oppositeSex, MatchingSets[oppositeSex]);
            } else if (TolUtil.NeedSameSexTypeForReproduction(actor))
            {
                preferredSets.Add(bio, MatchingSets[bio]);
            }

            var preferences = new List<Preference>();

            if (preferredSets.Count <= 0)
            {
                if (Randy.randomChance(0.33f))
                    return new List<Preference>();
                if(Randy.randomChance(0.33f))
                    for (var i = 0; i < Randy.randomInt(1, GetAllRegisteredPreferences().Count / 2); i++)
                    {
                        var count = 0;
                        Preference preference = null;
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
                    preferredSets.AddRange(MatchingSets);
                }
            }

            if (preferredSets.Count > 0)
            {
                var keys = preferredSets.Keys.ToList();
                string randomKey;

                if (Randy.randomChance(0.8f))
                {
                    randomKey = keys.GetRandom();

                    var sexualMatches = Randy.randomChance(0.7f);
                    
                    var preferredIdentitySexual = sexualMatches ? 
                        GetPreferenceTraitFromID(randomKey, true) : RandomPreferenceFromType("identity", true, new[]{randomKey});
                    var preferredIdentityRomantic = Randy.randomChance(0.95f) ?
                        GetOtherVariant(preferredIdentitySexual) : RandomPreferenceFromType("identity", true, new[]{preferredIdentitySexual.id});

                    preferences.Add(preferredIdentitySexual);
                    preferences.Add(preferredIdentityRomantic);
                    
                    // multiple preferences chance
                    if (Randy.randomChance(0.4f) && sexualMatches)
                    {
                        var randomSexual = RandomPreferenceFromType("identity", true, new []{preferredIdentitySexual.id});
                        preferences.Add(randomSexual);

                        var randomRomantic = Randy.randomChance(0.95f) ? 
                            GetOtherVariant(randomSexual) : RandomPreferenceFromType("identity", false, new[]{preferredIdentityRomantic.id});
                        preferences.Add(randomRomantic);
                    }
                }
                
                if (TolUtil.IsTOIInstalled())
                {
                    randomKey = keys.GetRandom();
                    if (Randy.randomChance(0.2f))
                    {
                        var preferredExpressionSexual = Randy.randomChance(0.5f) ? 
                            GetPreferenceTraitFromID(preferredSets[randomKey][0], true) : RandomPreferenceFromType("expression", true);
                        var preferredExpressionRomantic = Randy.randomChance(0.5f) ?
                            GetOtherVariant(preferredExpressionSexual) : RandomPreferenceFromType("expression");   
                        preferences.Add(preferredExpressionSexual);
                        preferences.Add(preferredExpressionRomantic);
                    }

                    if (Randy.randomChance(0.2f))
                    {
                        randomKey = keys.GetRandom();
                        var preferredGenitalSexual = Randy.randomChance(0.8f) ? 
                            GetPreferenceTraitFromID(preferredSets[randomKey][1], true) : RandomPreferenceFromType("genital", true);
                    
                        preferences.Add(preferredGenitalSexual);   
                    }
                }   
            }
            
            // if (Randy.randomChance(0.05f))
                // return List.Of(
                    // AllPreferences.Find(trait => trait.id.Equals("dislike_sex")), 
                    // AllPreferences.Find(trait => trait.id.Equals("dislike_romance")));
            
            return preferences;
        }
        
        // may return null if the variant does not exist
        public static Preference GetOtherVariant(Preference preference)
        {
            PreferenceTypes.TryGetValue(preference.group_id, out var preferenceGroup);
            var id = preference.WithoutOrientationID;
            return preferenceGroup.Find(match => match.WithoutOrientationID.Equals(id) && !match.id.Equals(preference.id));
        }
        public static bool HasPreference(this Actor actor, string preference, bool sexual = false)
        {
            actor.data.get(preference + (sexual ? "_sexual" : "_romantic"), out var result, false);
            return result;
            // foreach (var list in PreferenceTypes.Values)
            // {
            //     var trait = list.Find(trait => trait.WithoutOrientationID.Equals(preference) && trait.IsSexual == sexual);
            //     if (trait != null)
            //         return actor.hasTrait(trait);
            // }
            // return false;
        }

        public static bool HasAPreference(this Actor actor)
        {
            return GetActorPreferences(actor).Count > 0 || GetActorPreferences(actor, true).Count > 0;
            // var traits = actor.traits;
            // foreach(var trait in traits)
            // {
            //     if (trait is Preference)
            //         return true;
            // }
            // return false;
        }
        public static List<Preference> GetActorPreferencesFromType(this Actor actor, string type, bool sexual = false)
        {
            return GetActorPreferences(actor, sexual).Where(preference => preference.group_id.Equals(type)).ToList();
        }
        
        public static List<Preference> GetActorPreferences(this Actor actor, bool sexual = false)
        {
            var preferences = new List<Preference>();
            foreach (var preference in AllPreferences.Where(preference => preference.IsSexual == sexual))
            {
                // actor.data.get(preference.id + (sexual ? "_sexual" : "_romantic"), out var enabled, false);
                if(actor.HasPreference(preference.WithoutOrientationID, sexual))
                    preferences.Add(preference);
            }

            return preferences;
        }

        public static void TogglePreference(this Actor actor, Preference preference, bool? value=null)
        {
            actor.data.get(preference.id, out bool result);
            var opposite = !result;
            actor.data.set(preference.id, value.HasValue ? value.Value : opposite);
            
            Orientations.CreateOrientationBasedOnPrefChange(actor, preference);
        }
        public static List<Preference> GetRegisteredPreferencesFromType(string type, bool sexual = false)
        {
            return PreferenceTypes[type].Where(
                trait => trait.IsSexual == sexual
            ).ToList();
        }
        public static List<Preference> GetAllRegisteredPreferences()
        {
            var list = new List<Preference>();
            PreferenceTypes.Values.ForEach(list.AddRange);
            return list;
        }
        public static Preference RandomPreferenceFromType(string type, bool sexual = false, string[] exclude=null)
        {
            return GetRegisteredPreferencesFromType(type, sexual).Where(trait => exclude == null || !exclude.Contains(trait.id)).ToList().GetRandom();
        }
        
        public static Preference RandomPreference(bool sexual = false)
        {
            var preferences = GetRegisteredPreferencesFromType(PreferenceTypes.Keys.ToList().GetRandom(), sexual);
            return preferences.Count > 0 ? preferences.GetRandom() : null;
        }
        
        public static Preference GetPreferenceTraitFromID(string id, bool sexual = false)
        {
            return AllPreferences.Find(trait => trait.IsSexual == sexual && trait.WithoutOrientationID.Equals(id));
        }

        public static string GetType(this Actor actor, string type)
        {
            if (type.Equals("identity"))
                return GetIdentity(actor);
            if (type.Equals("expression"))
                return GetExpression(actor);
            if (type.Equals("genital"))
                return GetGenitalia(actor);
            return null;
        }

        // randomizes if multiple orientations fit the criteria

        // Run this when actors change identity (will patch Topic of Identity code)
        // add in a button that lets users regenerate orientation labels
        public static string GetIdentity(this Actor actor)
        {
            if (TolUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "female" : "male";
        }

        public static string GetExpression(this Actor actor)
        {
            if (TolUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "feminine" : "masculine";
        }
        public static string GetGenitalia(this Actor actor)
        {
            if (TolUtil.IsTOIInstalled())
            {
                // some code;
            }

            return actor.isSexFemale() ? "vulva" : "phallus";
        }
        public static bool IsMasculine(this Actor actor)
        {
            return GetExpression(actor).Equals("masculine");
        }
        public static bool IsFeminine(this Actor actor)
        {
            return GetExpression(actor).Equals("feminine");
        }
        
        // uterus is more correct, vulva is only needed for external pregnancies
        // we will later intervene with Topic of Identity to replace these methods
        public static bool HasVulva(this Actor actor)
        {
            return GetGenitalia(actor).Equals("vulva");
        }
        public static bool HasPenis(this Actor actor)
        {
            return GetGenitalia(actor).Equals("phallus");
        }
        
        
        public static string GetBiologicalSex(this Actor actor)
        {
            return actor.isSexFemale() ? "female" : "male";
        }
        public static bool IsEnby(this Actor actor)
        {
            return GetIdentity(actor).Equals("xenogender");
        }
        public static bool IdentifiesAsWoman(this Actor actor)
        {
            return GetIdentity(actor).Equals("female");
        }
        public static bool IdentifiesAsMan(this Actor actor)
        {
            return GetIdentity(actor).Equals("male");
        }

        public static bool Dislikes(this Actor actor, bool sexual = false)
        {
            return sexual ? actor.hasTrait("dislike_sex") : actor.hasTrait("dislike_romance");
        }

        // private static Preference CreateBaseTrait()
        // {
        //     return new Preference
        //     {
        //         spawn_random_trait_allowed = false,
        //         rate_birth = 0,
        //         rate_acquire_grow_up = 0,
        //         is_mutation_box_allowed = false,
        //         type = TraitType.Other,
        //         unlocked_with_achievement = false,
        //         rarity = Rarity.R3_Legendary,
        //         needs_to_be_explored = true,
        //         can_be_in_book = false,
        //     };
        // }
    }
}