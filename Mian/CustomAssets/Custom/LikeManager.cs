using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NCMS.Extensions;
using NeoModLoader.General;
using NeoModLoader.services;
#if TOPICOFIDENTITY
using Topic_of_Identity.Mian;
#endif

namespace Topic_of_Love.Mian.CustomAssets.Custom
{
    public enum LoveType
    {
        Sexual,
        Romantic,
        Both
    }
    public class LikeGroup
    {
        public readonly string ID;
        public readonly string HexCode;

        public LikeGroup(string id, string hexCode)
        {
            ID = id;
            HexCode = hexCode;
        }
        
        public override bool Equals(object obj)
        {
            return obj is LikeGroup compare && compare.ID.Equals(ID);
        }

        public override int GetHashCode() => ID.GetHashCode();
    }
    public class LikeAsset
    {
        public readonly string ID;
        public readonly string SexualPathIcon;
        public readonly string RomanticPathIcon;
        public readonly LikeGroup LikeGroup;

        public LoveType ApplicableLoveType;

        public LikeAsset(string id, string sexualPathIcon, string romanticPathIcon, LikeGroup likeGroup, LoveType applicable)
        {
            ID = id;
            SexualPathIcon = sexualPathIcon;
            RomanticPathIcon = romanticPathIcon;
            LikeGroup = likeGroup;
            ApplicableLoveType = applicable;
        }
        
        public override bool Equals(object obj)
        {
            return obj is LikeAsset compare && compare.ID.Equals(ID);
        }

        public override int GetHashCode() => ID.GetHashCode();
    }

    public class Like
    {
        public readonly LikeAsset LikeAsset;
        public readonly LoveType LoveType;

        public string ID => LikeAsset.ID + (LoveType.Equals(LoveType.Sexual) ? "_sexual" : "_romantic");

        public Like(LikeAsset asset, LoveType type)
        {
            if (type == LoveType.Both)
                throw new Exception("Likes cannot have a LoveType of both!");
            LikeAsset = asset;
            LoveType = type;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Like compare && compare.LikeAsset.Equals(LikeAsset) && compare.LoveType.Equals(LoveType);
        }

        public override int GetHashCode() =>
            (LikeAsset, LoveType).GetHashCode();
    }
    public static class LikeManager
    {
        private static readonly Dictionary<(LikeAsset, LoveType), Like> CachedLikes = new();
        private static readonly List<LikeAsset> AllLikeAssets = new();
        public static readonly Dictionary<LikeGroup, List<LikeAsset>> LikeTypes = new();
        private static readonly Dictionary<string, List<string>> MatchingSets = new();
        
        public static void Init()
        {
            var identities = List.Of("female", "male");

            if (TolUtil.IsTOIInstalled())
                identities.Add("xenogender");
            
            AddLikeType("identity", "#B57EDC", identities, LoveType.Both);
            
            if(TolUtil.IsTOIInstalled())
                AddLikeType("expression", "#C900FF", List.Of("feminine", "masculine"), LoveType.Both);
            
            if(TolUtil.IsTOIInstalled())
                AddLikeType("genital", "#B77E7E", List.Of("phallus", "vulva"), LoveType.Sexual);

            AddMatchingSets();
            
            Finish();
        }
        
        // if all preferences of a preference type is added, remove them all (no reason for preferences if all are included?)
        private static void AddLikeType(string groupType, string hexColor, List<string> preferences, LoveType prefType)
        {
            if (LikeTypes.Any(type => type.Key.ID.Equals(groupType)))
            {
                LogService.LogError(groupType + " is already an added like type!");
                return;
            }

            // AssetManager.trait_groups.add(new ActorTraitGroupAsset
            // {
            //     id = groupType,
            //     name = "trait_group_"+groupType,
            //     color = hexColor
            // });

            var withSpaces = groupType.Replace("_", " ");
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
            
            if(!LM.Has("like_group_"+groupType))
                LM.AddToCurrentLocale("like_group_"+groupType, "Preferred " + nameWithPlural);

            var likeAssets = new List<LikeAsset>();
            var likeGroup = new LikeGroup(groupType, hexColor);
            
            foreach (var likeName in preferences)
            {
                var like = new LikeAsset
                (
                    likeName,
                    "ui/Icons/preference_traits/sexual/" + likeName,
                    "ui/Icons/preference_traits/romantic/" + likeName,
                    likeGroup,
                    prefType
                );
                // romanticTrait.group_id = type;
                // romanticTrait.opposite_traits = new HashSet<ActorTrait>();
                likeAssets.Add(like);

                if (!LM.Has("like_" + likeName + "_romantic"))
                    LM.AddToCurrentLocale("like_" + likeName + "_romantic", 
                        "Prefers " + likeName.Substring(0, 1).ToUpper() + likeName.Substring(1) + " (Romantic)");
                if (!LM.Has("like_" + likeName + "_romantic_info"))
                    LM.AddToCurrentLocale("like_" + likeName + "_romantic_info", "Romantically prefers " + likeName);
                if (!LM.Has("like_" + likeName + "_romantic_info_2"))
                    LM.AddToCurrentLocale("like_" + likeName + "_romantic_info_2", "");
                    
                if (!LM.Has("like_" + likeName + "_sexual"))
                    LM.AddToCurrentLocale("like_" + likeName + "_sexual", 
                        "Prefers " + likeName.Substring(0, 1).ToUpper() + likeName.Substring(1) + "(Sexual) ");
                if (!LM.Has("like_" + likeName + "_sexual_info"))
                    LM.AddToCurrentLocale("like_" + likeName + "_sexual_info", "Sexually prefers " + likeName);
                if (!LM.Has("like_" + likeName + "_sexual_info_2"))
                    LM.AddToCurrentLocale("like_" + likeName + "_sexual_info_2", "");            
            }
            AllLikeAssets.AddRange(likeAssets);
            LikeTypes.Add(likeGroup, likeAssets);

            // var sexualTraits = new List<Preference>();
            // foreach (var preference in preferences)
            // {
            //     var sexualTrait = new Preference
            //     {
            //         ID = preference + "_sexual",
            //         GroupId = groupType,
            //         WithoutOrientationID = preference,
            //         SexualPathIcon = "ui/Icons/preference_traits/sexual/" + preference,
            //         IsSexual = true
            //     };
            //     // sexualTrait.group_id = type;
            //     // sexualTrait.opposite_traits = new HashSet<ActorTrait>();
            //     sexualTraits.Add(sexualTrait);
            //     
            //     if (!LM.Has("trait_" + preference + "_sexual"))
            //         LM.AddToCurrentLocale("trait_" + preference + "_sexual", 
            //             "Prefers " + preference.Substring(0, 1).ToUpper() + preference.Substring(1) + (canBeRomantic ? " (Sexual)" : ""));
            //     if (!LM.Has("trait_" + preference + "_sexual_info"))
            //         LM.AddToCurrentLocale("trait_" + preference + "_sexual_info", "Sexually prefers " + preference);
            //     if (!LM.Has("trait_" + preference + "_sexual_info_2"))
            //         LM.AddToCurrentLocale("trait_" + preference + "_sexual_info_2", "");            
            // }
            // AllPreferences.AddRange(sexualTraits);
            // preferenceTraits.AddRange(sexualTraits);
            //
            // PreferenceTypes.Add(groupType, preferenceTraits);
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

        public static LoveType GetRandomLoveType()
        {
            var array = Enum.GetValues(typeof(LoveType));
            return (LoveType) array.GetValue(Randy.randomInt(0, array.Length));
        }

        private static LikeGroup GetLikeGroup(string id)
        {
            return LikeTypes.First(type => type.Key.ID.Equals(id)).Key;
        }
        
        // checks for one actor based on a type, if you are checking for multiple types or multiple actors, then use the other methods
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, string type, bool sexual)
        {
            if (pActor == null || pTarget == null) return false;
            
            // preferences do not matter
            if (pActor.hasCultureTrait("orientationless"))
                return true;

            var list = GetActorLikes(pActor, type, sexual ? LoveType.Sexual : LoveType.Romantic).Select(like => like.ID).ToList();
            var targetType = GetActorTypeFromLikeGroup(pTarget, type);

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
            return BothActorsPreferenceMatch(actor1, actor2, false)
                   && BothActorsPreferenceMatch(actor1, actor2, true);
        }

        public static List<Like> GetRandomPreferences(Actor actor)
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

            var preferences = new List<Like>();

            if (preferredSets.Count <= 0)
            {
                if (Randy.randomChance(0.33f))
                    return new List<Like>();
                if (Randy.randomChance(0.33f))
                {
                    // for (var i = 0; i < Randy.randomInt(1, GetAllRegisteredPreferences().Count / 2); i++)
                    // {
                    var current = 0;
                    var max = Randy.randomInt(1, GetAllRegisteredPreferences().Count * 2);

                    while (current < max)
                    {
                        current++;
                        var like = GetLikeFromAsset(RandomLikeAsset());
                        
                        if(!preferences.Any(likeCompare => likeCompare.LikeAsset.Equals(like.LikeAsset) && likeCompare.LoveType.Equals(like.LoveType)))
                            preferences.Add(like);
                    }
                    
                    // var count = 0;
                    // Like like = null;
                    // while (!preferences.Any(likeCompare => like != null && likeCompare.LikeAsset.Equals(like.LikeAsset) && likeCompare.LoveType.Equals(like.LoveType)) 
                    //        && count < 6)
                    // {
                    //     var asset = RandomPreference(LoveType.Both);
                    //     like = new Like
                    //     {
                    //         LikeAsset = asset,
                    //         LoveType = Randy.randomBool() ? LoveType.Sexual : LoveType.Romantic
                    //     };
                    //     count++;
                    // }
                    //
                    // if (like != null)
                    //     preferences.Add(like);
                    // }
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
                    
                    var likeIdentitySexual = GetLikeFromAsset(sexualMatches ? 
                        GetAssetFromID(randomKey) : RandomLikeAssetFromType("identity", new[]{randomKey}), LoveType.Sexual);
                    var likeIdentityRomantic = GetLikeFromAsset(Randy.randomChance(0.95f) ?
                        likeIdentitySexual.LikeAsset : RandomLikeAssetFromType("identity", new[]{likeIdentitySexual.LikeAsset.ID}), LoveType.Romantic);

                    preferences.Add(likeIdentitySexual);
                    preferences.Add(likeIdentityRomantic);
                    
                    // multiple preferences chance
                    if (Randy.randomChance(0.4f) && sexualMatches)
                    {
                        var randomSexual =
                            GetLikeFromAsset(RandomLikeAssetFromType("identity", new []{likeIdentitySexual.LikeAsset.ID}), LoveType.Sexual);
                        preferences.Add(randomSexual);

                        var randomRomantic = GetLikeFromAsset(Randy.randomChance(0.95f) ? 
                            randomSexual.LikeAsset : RandomLikeAssetFromType("identity", new[]{likeIdentityRomantic.LikeAsset.ID}), LoveType.Romantic);
                        preferences.Add(randomRomantic);
                    }
                }
                
                if (TolUtil.IsTOIInstalled())
                {
                    randomKey = keys.GetRandom();
                    if (Randy.randomChance(0.2f))
                    {
                        var likeExpressionSexual = GetLikeFromAsset(Randy.randomChance(0.5f) ? 
                            GetAssetFromID(preferredSets[randomKey][0]) : RandomLikeAssetFromType("expression"), LoveType.Sexual);
                        var likeExpressionRomantic = GetLikeFromAsset(Randy.randomChance(0.5f) ?
                            likeExpressionSexual.LikeAsset : RandomLikeAssetFromType("expression"), LoveType.Romantic);   
                        preferences.Add(likeExpressionSexual);
                        preferences.Add(likeExpressionRomantic);
                    }

                    if (Randy.randomChance(0.2f))
                    {
                        randomKey = keys.GetRandom();
                        var likeGenital = GetLikeFromAsset(Randy.randomChance(0.8f) ? 
                            GetAssetFromID(preferredSets[randomKey][1]) : RandomLikeAssetFromType("genital"), LoveType.Sexual);
                    
                        preferences.Add(likeGenital);   
                    }
                }   
            }
            
            // if (Randy.randomChance(0.05f))
                // return List.Of(
                    // AllPreferences.Find(trait => trait.id.Equals("dislike_sex")), 
                    // AllPreferences.Find(trait => trait.id.Equals("dislike_romance")));
            
            return preferences;
        }

        public static bool HasLike(this Actor actor, string id, LoveType? forcedType)
        {
            return actor.HasLike(GetLikeFromID(id, forcedType));
        }
        
        public static bool HasLike(this Actor actor, Like like)
        {
            actor.data.get(like.ID, out var result, false);
            return result;
            // foreach (var list in PreferenceTypes.Values)
            // {
            //     var trait = list.Find(trait => trait.WithoutOrientationID.Equals(preference) && trait.IsSexual == sexual);
            //     if (trait != null)
            //         return actor.hasTrait(trait);
            // }
            // return false;
        }

        public static bool HasALike(this Actor actor)
        {
            return GetActorLikes(actor).Count > 0;
        }
        public static List<Like> GetActorLikes(this Actor actor, string groupType, LoveType loveType)
        {
            return GetActorLikes(actor, loveType).Where(like => like.LikeAsset.LikeGroup.ID.Equals(groupType)).ToList();
        }
        
        public static List<Like> GetActorLikes(this Actor actor, LoveType? loveType=null)
        {
            var likes = new List<Like>();
            foreach (var asset in AllLikeAssets)
            {
                // actor.data.get(preference.id + (sexual ? "_sexual" : "_romantic"), out var enabled, false);
                if (loveType.HasValue && !loveType.Value.Equals(LoveType.Both))
                {
                    var like = GetLikeFromAsset(asset, loveType);
                    if(actor.HasLike(like))
                        likes.Add(like);   
                }
                else
                {
                    var likeRomantic = GetLikeFromAsset(asset, LoveType.Romantic);
                    if(actor.HasLike(likeRomantic))
                        likes.Add(likeRomantic);
                    var likeSexual = GetLikeFromAsset(asset, LoveType.Sexual);
                    if(actor.HasLike(likeSexual))
                        likes.Add(likeSexual);
                }
            }

            return likes;
        }

        public static void TogglePreference(this Actor actor, Like like, bool? value=null)
        {
            actor.data.get(like.ID, out bool result);
            var opposite = !result;
            actor.data.set(like.ID, value.HasValue ? value.Value : opposite);
            
            Orientations.CreateOrientationBasedOnPrefChange(actor, like);
        }
        public static List<LikeAsset> GetRegisteredPreferencesFromType(string type)
        {
            return LikeTypes[GetLikeGroup(type)];
            // return LikeTypes[GetLikeType(type)].Where(
            //     trait => trait.ApplicableLoveType.Equals(loveType) || trait.ApplicableLoveType.Equals(LoveType.Both) || loveType.Equals(LoveType.Both)
            // ).ToList();
        }
        public static List<LikeAsset> GetAllRegisteredPreferences()
        {
            var list = new List<LikeAsset>();
            LikeTypes.Values.ForEach(list.AddRange);
            return list;
        }
        public static LikeAsset RandomLikeAssetFromType(string type, string[] exclude=null)
        {
            return GetRegisteredPreferencesFromType(type).Where(preference => exclude == null || !exclude.Contains(preference.ID)).ToList().GetRandom();
        }
        
        public static LikeAsset RandomLikeAsset()
        {
            var preferences = GetRegisteredPreferencesFromType(LikeTypes.Keys.ToList().GetRandom().ID);
            return preferences.Count > 0 ? preferences.GetRandom() : null;
        }

        public static Like GetLikeFromID(string id, LoveType? forcedType = null)
        {
            return GetLikeFromAsset(GetAssetFromID(id), forcedType);
        }

        // will return either romantic variant or sexual variant depending on forcedType
        public static Like GetLikeFromAsset(LikeAsset asset, LoveType? forcedType = null)
        {
            var loveType = asset.ApplicableLoveType.Equals(LoveType.Both)
                ? forcedType.HasValue ? forcedType.Value : Randy.randomBool() ? LoveType.Sexual : LoveType.Romantic
                : asset.ApplicableLoveType;
            if (forcedType.HasValue && !asset.ApplicableLoveType.Equals(LoveType.Both))
                throw new Exception("Tried using type " + forcedType.Value + " when " + asset.ID + "'s applicable love type does not allow for that!");
            CachedLikes.TryGetValue((asset, loveType), out var cached);
            if (cached != null)
                return cached;
            
            var like = new Like(asset, loveType);
            
            CachedLikes.Add((like.LikeAsset, like.LoveType), like);

            return like;
        }
        
        public static LikeAsset GetAssetFromID(string id)
        {
            var asset = AllLikeAssets.Find(asset => asset.ID.Equals(id));
            if (asset == null)
                throw new Exception("No asset found with ID: " + id);
            return asset;
        }

        public static string GetActorTypeFromLikeGroup(this Actor actor, string type)
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

        public static bool HasAnyLikesFor(this Actor actor, string groupType, LoveType loveType)
        {
            return actor.GetActorLikes(groupType, loveType).Count > 0;
        }
        // public static bool Dislikes(this Actor actor, bool sexual = false)
        // {
        //     return sexual ? actor.hasTrait("dislike_sex") : actor.hasTrait("dislike_romance");
        // }
    }
}