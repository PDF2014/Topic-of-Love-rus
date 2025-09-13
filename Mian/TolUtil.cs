using System;
using System.Collections.Generic;
using System.Linq;
using ai;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;
using Topic_of_Love.Mian.CustomAssets.Custom;

#if TOPICOFIDENTITY
using Topic_of_Identity;
#endif

namespace Topic_of_Love.Mian
{
    public static class TolUtil
    {
        public static void ShowWhisperTipWithTime(string pText, float time=6f)
        {
            string pText1 = LocalizedTextManager.getText(pText);
            if (Config.whisper_A != null)
                pText1 = pText1.Replace("$kingdom_A$", Config.whisper_A.name);
            if (Config.whisper_B != null)
                pText1 = pText1.Replace("$kingdom_B$", Config.whisper_B.name);
            WorldTip.showNow(pText1, false, "top", time);
        }
        
        // Returns the parent that has a population limit not REACHED yet
        public static Actor EnsurePopulationFromParent(List<Actor> parents)
        {
            var canMake = new List<Actor>();

            foreach (var parent in parents)
            {
                if (!parent.subspecies.hasReachedPopulationLimit())
                    canMake.Add(parent);
            }

            if (canMake.Count <= 0) return null;

            return canMake.GetRandom();
        }
        
        private static string OpinionOnSex(Actor actor1, Actor actor2)
        {
            if (actor1.hasSubspeciesTrait("amygdala"))
            {
                actor1.data.get("sex_reason", out var sexReason, "");
                SexType.TryParse(sexReason, out SexType sexType);
                
                // bug spotted? some actors were lovers but one of them disliked the sex for some reason
                if ((LikesManager.LikeMatches(actor1, actor2, true) || (actor1.lover == actor2 && Randy.randomChance(0.5f)))
                    && (Randy.randomChance(sexType == SexType.Reproduction ? 0.5f : 1f) || actor1.lover == actor2))
                {
                    var normal = 0.3f;
                    if (actor1.lover == actor2)
                        normal += 0.5f;

                    actor1.a.data.get("intimacy_happiness", out float happiness);
                    if (happiness < 0)
                    {
                        normal += Math.Abs((happiness / 100) / 2);
                    }

                    if (!LikesManager.LikeMatches(actor1, actor2, true))
                        normal -= 0.2f;
                    
                    var type = Randy.randomChance(Math.Min(1, normal)) ? "enjoyed_sex" : "okay_sex"; 
                    actor1.addStatusEffect(type);
                    return type;
                }
                else
                {
                    actor1.addStatusEffect("disliked_sex");
                    return "disliked_sex";
                }
            }

            return null;
        }

        public static void ActorsInteractedIntimately(Actor actor1, Actor actor2)
        {
            actor1.data.set("last_had_intimate_interaction_with", actor2.getID());
            actor2.data.set("last_had_intimate_interaction_with", actor1.getID());
        }
        
        // handles the actors' happiness and mood towards their sex depending on their preferences
        // handle cheating here too
        public static void JustHadSex(Actor actor1, Actor actor2)
        {
            TolUtil.Debug(actor1.getName() + " had sex with "+actor2.getName()+". They are lovers: "+(actor1.lover==actor2));
            ActorsInteractedIntimately(actor1, actor2);
            
            actor1.addAfterglowStatus();
            actor2.addAfterglowStatus();   
            
            if (Randy.randomChance(actor1.lover == actor2 ? 1f : LikesManager.BothActorsLikesMatch(actor1, actor2, true) ? 0.25f : 0f))
            {
                actor1.addStatusEffect("just_kissed");
                actor2.addStatusEffect("just_kissed");
            }
            
            var opinion = OpinionOnSex(actor1, actor2);
            var opinion1 = OpinionOnSex(actor2, actor1);

            if (actor1.hasLover() && actor1.lover != actor2)
            {
                actor1.lover.changeIntimacyHappinessBy(-25f);
            }

            if (actor2.hasLover() && actor2.lover != actor1)
            {
                actor2.lover.changeIntimacyHappinessBy(-25f);
            }
            
            if (actor1.lover != actor2)
            {
                actor1.data.get("sex_reason", out var sexReason, "reproduction");
                SexType.TryParse(sexReason, true, out SexType sexType);
                TolUtil.Debug("Sex Reason: "+sexType);
                
                if (sexType != SexType.Reproduction)
                {
                    if (!actor1.CanHaveIntimacyWithoutRepercussions(sexType))
                    {
                        actor1.PotentiallyCheatedWith(actor2);
                    }

                    if (!actor2.CanHaveIntimacyWithoutRepercussions(sexType))
                    {
                        actor2.PotentiallyCheatedWith(actor1);
                    }   
                }

                // did you really fucking enjoy it?
                if (opinion != null && opinion1 != null && 
                    sexReason.Equals("casual") && 
                    opinion.Equals("enjoyed_sex") && 
                    opinion1.Equals("enjoyed_sex")
                   )
                {
                    if (actor1.hasLover() && actor1.IsFaithful())
                        return;
                    if (actor2.hasLover() && actor2.IsFaithful())
                        return;
                    actor1.becomeLoversWith(actor2);
                }
            }
        }
        public static bool WithinOfAge(Actor pActor, Actor pTarget)
        { 
            var higherAge = Math.Max(pActor.age, pTarget.age);
            var lowerAge = Math.Min(pActor.age, pTarget.age);
            var minimumAge = Math.Min(higherAge, higherAge / 2 + 7);
            return lowerAge >= minimumAge || (!pActor.hasCultureTrait("mature_dating") && !pTarget.hasCultureTrait("mature_dating"));
        }
        
        // can this actor date the other actor?
        public static bool CannotDate(Actor actor, Actor actor2)
        {
            return actor.IsActorUndateable(actor2) || actor2.IsActorUndateable(actor) || actor.IsOrientationSystemEnabled() != actor2.IsOrientationSystemEnabled()
                || !actor.hasSubspeciesTrait("advanced_hippocampus") || !actor2.hasSubspeciesTrait("advanced_hippocampus");
        }

        // public static bool SocializedLoveCheck(BehaviourActionActor __instance, Actor pActor, Actor target, bool noBoringLove=false)
        // {
        //     if (pActor.IsOrientationSystemEnabled() && target.IsOrientationSystemEnabled())
        //     {
        //         if (Randy.randomBool())
        //         {
        //             if (pActor.lover != target)
        //             {
        //                 if (pActor.WillDoIntimacy(target, SexType.None, true)
        //                      && target.WillDoIntimacy(pActor))
        //                 {
        //                     // does date instead
        //                     __instance.forceTask(pActor, "try_date", false);
        //                     return true;
        //                 }   
        //             }
        //             else if (pActor.WillDoIntimacy(target, SexType.Casual,  true)
        //                      && target.WillDoIntimacy(pActor, SexType.Casual))
        //             {
        //                 pActor.cancelAllBeh();
        //                 target.cancelAllBeh();
        //                 pActor.beh_actor_target = target;
        //                 new BehGetPossibleTileForSex().execute(pActor);
        //                 return true;
        //             }
        //             else if(pActor.WillDoIntimacy(target, SexType.None, true) 
        //                     && target.WillDoIntimacy(pActor))
        //             {
        //                 pActor.cancelAllBeh();
        //                 target.cancelAllBeh();
        //                 pActor.beh_actor_target = target;
        //                 __instance.forceTask(pActor, "try_kiss", false);
        //                 return true;
        //             }
        //         } else if ((!noBoringLove || !pActor.isSapient()) && !pActor.hasLover() && !target.hasLover())
        //         {
        //             pActor.becomeLoversWith(target);
        //             return true;
        //         }
        //     }
        //     else
        //     {
        //         ActorTool.checkFallInLove(pActor, target);
        //     }
        //
        //     return false;
        // }

        public static void LogInfo(string message)
        {
            TopicOfLove.LogInfo(message);
        }

        public static string[] GetKeywords(string word)
        {
            return word.Split(',').Where(keyword => !string.IsNullOrEmpty(keyword)).ToArray();
        }
        
        public static void Debug(object message)
        {
            if (message == null)
                return;
            var config = TopicOfLove.Mod.GetConfig();
            var debug = (bool)config["Misc"]["Debug"].GetValue();
            
            var stringMsg = message.ToString();
            
            if (!debug)
                return;
            var slowOnLog = GetKeywords((string)config["Misc"]["SlowOnLog"].GetValue());
            var stackTrace = GetKeywords((string)config["Misc"]["StackTrace"].GetValue());
            var ignore = GetKeywords((string)config["Misc"]["Ignore"].GetValue());
            if (ignore.Length > 0 && ignore.Any(keyword => stringMsg.Contains(keyword)))
                return;
            if(stackTrace.Length > 0 && stackTrace.Any(keyword => stringMsg.Contains(keyword)))
                stringMsg += "\nStackTrace: " + Environment.StackTrace;
            if(slowOnLog.Length > 0 && slowOnLog.Any(keyword => stringMsg.Contains(keyword)))
                Config.setWorldSpeed(AssetManager.time_scales.get("slow_mo"));
            LogInfo(stringMsg);
        }
        
        // public static bool NeedSameSexTypeForReproduction(this Actor pActor)
        // {
        //     return pActor.hasSubspeciesTrait("reproduction_same_sex");
        // }
        
        // orientations earlier in the list are prioritized
        public static int SortUnitsByOrientations(Actor pActor1, Actor pActor2, List<Orientation> orientations, bool sexual)
        {
            var orientation1 = Orientations.GetOrientationForActorBasedOnCriteria(pActor1, sexual);
            var orientation2 = Orientations.GetOrientationForActorBasedOnCriteria(pActor2, sexual);

            if (orientations.Contains(orientation1) && orientations.Contains(orientation2))
            {
                // if orientation1 index is lower, then return -1, else return 1
                return orientations.IndexOf(orientation1) < orientations.IndexOf(orientation2) ? -1 : 1;
            }
        
            if (orientation1.OrientationType.Equals(orientation2.OrientationType) 
                || (!orientations.Contains(orientation2) && !orientations.Contains(orientation1)))
                return 0;
            return orientations.Contains(orientation1) ? -1 : 1;
        }

        public static bool IsTOIInstalled()
        {
            #if TOPICOFIDENTITY
            return true;
            #else
            return false;
            #endif
        }
    }
}