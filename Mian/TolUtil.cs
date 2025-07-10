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
        public static bool CouldReproduce(Actor pActor, Actor pTarget)
        {
            return pActor.subspecies.isPartnerSuitableForReproduction(pActor, pTarget);
        }

        public static bool ReproducesSexually(this Actor pActor)
        {
            return pActor.NeedDifferentSexTypeForReproduction() || pActor.NeedSameSexTypeForReproduction() ||
                   pActor.CanDoAnySexType();
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

        public static bool IsFaithful(Actor pActor)
        {
            return pActor.hasCultureTrait("committed") || pActor.hasTrait("faithful");
        }

        public static bool WillDoIntimacy(Actor pActor, Actor pTarget, SexType sexReason=SexType.None, bool isInit=false)
        {
            var withLover = pActor.hasLover() && pActor.lover == pTarget;
            
            pActor.data.get("intimacy_happiness", out float d);
            if (isInit)
                Debug(pActor.getName() + " is requesting to do intimacy. Sexual happiness: "+d + ". With lover: "+withLover);
            else
                Debug(pActor.getName() + " is being requested to do intimacy. Sexual happiness: "+d + ". With lover: "+withLover);
            Debug("\n"+sexReason);

            if (!sexReason.Equals(SexType.None) && !pActor.isAdult())
                return false;
            if (!sexReason.Equals(SexType.Reproduction) && !LikesManager.LikeMatches(pActor, pTarget, "identity",
                    !sexReason.Equals(SexType.None)))
                return false;
            
            if (!isInit)
            {
                if(pActor.hasTask() && !(pActor.ai.task.cancellable_by_reproduction ||
                                         pActor.ai.task.cancellable_by_socialize))
                {
                    Debug("Unable to do intimacy from this actor due to an uncancellable task");
                    return false;
                }
            }
            
            var allowedToHaveIntimacy = withLover || pActor.CanHaveIntimacyWithoutRepercussions(sexReason);
            var reduceChances = 0f;
            pActor.data.get("intimacy_happiness", out float intimacyHappiness);
            
            if (intimacyHappiness < 0)
            {
                var toReduce = intimacyHappiness / 300;
                reduceChances += toReduce;
            }

            if (!AffectedByIntimacy(pActor))
                reduceChances = 0f;
            
            reduceChances = Math.Max(-0.2f, reduceChances);

            if(!allowedToHaveIntimacy
               && Randy.randomChance(Math.Max(0, (pActor.hasTrait("unfaithful") ? 0.6f : .99f) + reduceChances)))
            {
                Debug("Not allowed to do intimacy because of lover and not low enough happiness");
                return false;
            }

            if (!allowedToHaveIntimacy && IsFaithful(pActor))
            {
                Debug("Not allowed to do intimacy because of lover and is faithful");
                return false;
            }
            
            reduceChances = 0.1f;
            if (intimacyHappiness > 0)
            {
                reduceChances += intimacyHappiness / 100f;
            }
            
            // person may choose to do sex even if really happy
            var doIntimacy = Randy.randomChance(Math.Max(0.05f, 1f - reduceChances));
            if (!doIntimacy && !sexReason.Equals(SexType.Reproduction))
            {
                Debug("Will not do intimacy since they are deemed to be happy enough");
                return false;
            }   

            if(!allowedToHaveIntimacy)
                Debug(pActor.getName() + " is cheating!");
            return true;
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
                actor1.lover.changeIntimacyHappiness(-25f);
            }

            if (actor2.hasLover() && actor2.lover != actor1)
            {
                actor2.lover.changeIntimacyHappiness(-25f);
            }
            
            if (actor1.lover != actor2)
            {
                actor1.data.get("sex_reason", out var sexReason, "reproduction");
                SexType.TryParse(sexReason, out SexType sexType);
                TolUtil.Debug("Sex Reason: "+sexReason);
                if (!actor1.CanHaveIntimacyWithoutRepercussions(sexType))
                {
                    PotentiallyCheatedWith(actor1, actor2);
                }

                if (!actor2.CanHaveIntimacyWithoutRepercussions(sexType))
                {
                    PotentiallyCheatedWith(actor2, actor1);
                }

                // did you really fucking enjoy it?
                if (opinion != null && opinion1 != null && 
                    sexReason.Equals("casual") && 
                    opinion.Equals("enjoyed_sex") && 
                    opinion1.Equals("enjoyed_sex")
                   )
                {
                    if (actor1.hasLover() && IsFaithful(actor1))
                        return;
                    if (actor2.hasLover() && IsFaithful(actor2))
                        return;
                    actor1.becomeLoversWith(actor2);
                }
            }
        }
        public static void NewLikes(Actor actor)
        {
            if (actor != null && CapableOfLove(actor))
            {
                var oldPreferences = actor.GetActorLikes();
                foreach (var preference in oldPreferences)
                {
                    actor.data.set(preference.IDWithLoveType, false);
                }
                
                var preferences =  LikesManager.GetRandomLikes(actor);
                foreach (var preference in preferences)
                {
                    actor.data.set(preference.IDWithLoveType, true);
                }
                Orientations.RollOrientationLabel(actor);
            }
        }

        public static bool CanHaveIntimacyWithoutRepercussions(this Actor actor, SexType sexType)
        {
            if (sexType == SexType.None)
            {
                return !actor.hasLover()
                       || (actor.hasLover() && !LikesManager.LikeMatches(actor, actor.lover)
                                            && actor.lover.hasCultureTrait("sexual_expectations"));
            }
            else
            {
                return !actor.hasLover()
                       || (actor.hasLover() && ((!LikesManager.LikeMatches(actor, actor.lover, true)
                                                 && actor.lover.hasCultureTrait("sexual_expectations"))
                                                || (actor.hasSubspeciesTrait("preservation") && IsDyingOut(actor) 
                                                    && sexType == SexType.Reproduction
                                                    && (!BabyHelper.canMakeBabies(actor.lover) || !CouldReproduce(actor, actor.lover)))));
            }
        }

        public static void PotentiallyCheatedWith(Actor actor, Actor actor2)
        {
            if (actor.hasLover() && actor.lover != actor2 && CanStopBeingLovers(actor))
            {
                var cheatedActor = actor.lover;
                // will they know :O
                // if (cheatedActor.isLying() || !cheatedActor.isOnSameIsland(actor))
                //     return;
                
                cheatedActor.addStatusEffect("cheated_on");
                
                if(actor.hasKingdom())
                    actor.kingdom.increaseCheated();
                if(actor.hasCity())
                    actor.city.increaseCheated();
                World.world.increaseCheated();
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
            return IsActorUndateable(actor, actor2) || IsActorUndateable(actor2, actor);
        }

        public static void BreakUp(Actor actor, bool actorIsSad=true)
        {
            if (!actor.hasLover())
                return;
            
            Debug(actor.getName() + " broke up with "+ actor.lover.getName());
            
            AddOrRemoveUndateableActor(actor, actor.lover);
            AddOrRemoveUndateableActor(actor.lover, actor);

            if(actor.hasKingdom())
                actor.kingdom.increaseBrokenUp();
            if(actor.hasCity())
                actor.city.increaseBrokenUp();
            World.world.increaseBrokenUp();
            
            actor.lover.addStatusEffect("broke_up");
            if(actorIsSad)
                actor.addStatusEffect("broke_up");
            
            RemoveLovers(actor);
        }

        public static bool CanStopBeingLovers(Actor actor)
        {
            actor.data.get("force_lover", out var isForced, false);
            return !isForced;
        }

        // can they fall in love in some way?
        public static bool CanFallInLove(Actor actor)
        {
            actor.data.get("just_lost_lover", out var justLostLover, false);
            actor.data.get("force_lover", out var isForced, false);
            return !justLostLover && !actor.hasStatus("broke_up") && (!isForced || !actor.hasLover()) && CapableOfLove(actor);
        }

        public static void RemoveLovers(Actor actor)
        {
            var lover = actor.lover;
            if (lover == null)
                return;
            lover.setLover(null);
            actor.setLover(null);
            actor.data.set("just_lost_lover", true);
            lover.data.set("just_lost_lover", true);
            lover.data.set("force_lover", false);
            actor.data.set("force_lover", false);
        }
        
        public static bool IsOrientationSystemEnabledFor(Actor pActor)
        {
            return !pActor.hasCultureTrait("orientationless");
        }

        public static bool IsDyingOut(Actor pActor)
        {
            if (!pActor.hasSubspecies() || pActor.hasReachedOffspringLimit()
                || (pActor.hasCity() && pActor.city.getUnitsTotal() >= pActor.city.getPopulationMaximum())) return false;
            var limit = (int)pActor.subspecies.base_stats_meta["limit_population"];
            return pActor.subspecies.countCurrentFamilies() <= 10 
                   || (pActor.hasCity() && (pActor.city.getAge() < 100 || (float) pActor.city.getUnitsTotal() / pActor.city.getPopulationMaximum() < 0.7))
        || (limit != 0 ? pActor.subspecies.countUnits() <= limit / 3 : pActor.subspecies.countUnits() <= 100);
        }
        
        public static bool WantsBaby(Actor pActor, bool reproductionPurposesIncluded=true)
        {
            if (!BabyHelper.canMakeBabies(pActor))
                return false;
            
            if (reproductionPurposesIncluded)
            {
                if (!pActor.isSapient() || IsDyingOut(pActor))
                {
                    Debug(pActor.getName() + " wants a baby because they are non-intelligent species or are dying out");
                    return true;
                }   
            }

            if (pActor.hasCity() && pActor.city.getUnitsTotal() >= pActor.city.getPopulationMaximum())
                return false;
            
            if (pActor.getHappiness() >= 50 & pActor.getIntimacy() > 10)
            {
                Debug(pActor.getName() + " wants a baby because they are happy enough");
                return true;
            }
            
            return false;
        }

        public static bool SocializedLoveCheck(BehaviourActionActor __instance, Actor pActor, Actor target, bool noBoringLove=false)
        {
            if (!pActor.canFallInLoveWith(target))
                return false;
            
            if (IsOrientationSystemEnabledFor(pActor) && IsOrientationSystemEnabledFor(target))
            {
                if (Randy.randomBool())
                {
                    if (pActor.lover != target)
                    {
                        if (WillDoIntimacy(pActor, target, SexType.None, true)
                            && WillDoIntimacy(target, pActor))
                        {
                            // does date instead
                            __instance.forceTask(pActor, "try_date", false);
                            return true;
                        }   
                    }
                    else if (WillDoIntimacy(pActor, target, SexType.Casual,  true)
                             && WillDoIntimacy(target, pActor, SexType.Casual))
                    {
                        pActor.cancelAllBeh();
                        target.cancelAllBeh();
                        pActor.beh_actor_target = target;
                        new BehGetPossibleTileForSex().execute(pActor);
                        return true;
                    }
                    else if(WillDoIntimacy(pActor, target, SexType.None, true) 
                            && WillDoIntimacy(target, pActor))
                    {
                        pActor.cancelAllBeh();
                        target.cancelAllBeh();
                        pActor.beh_actor_target = target;
                        __instance.forceTask(pActor, "try_kiss", false);
                        return true;
                    }
                } else if ((!noBoringLove || !pActor.isSapient()) && !pActor.hasLover() && !target.hasLover())
                {
                    pActor.becomeLoversWith(target);
                    return true;
                }
            }
            else
            {
                ActorTool.checkFallInLove(pActor, target);
            }

            return false;
        }

        public static bool IsActorUndateable(Actor pActor, Actor toCheck)
        {
            pActor.data.get("amount_undateable", out var length, 0);
            var id = toCheck.getID();
            for (var i = 0; i < length; i++)
            {
                pActor.data.get("undateable_" + i, out var idFromSave, 0L);
                if (id == idFromSave)
                    return true;
            }
        
            return false;
        }
        public static void AddOrRemoveUndateableActor(Actor pActor, Actor undateable)
        {
            pActor.data.get("amount_undateable", out var length, 0);
            
            var id = undateable.getID();
            var position = -1;
            
            for (var i = 0; i < length; i++)
            {
                pActor.data.get("undateable_" + i, out var idFromSave, 0L);
                if (idFromSave == id)
                {
                    position = i;
                    break;
                }
            }
        
            if (position == -1)
            {
                pActor.data.set("undateable_" + length, id);
                pActor.data.set("amount_undateable", length + 1);
            }
            else
            {
                pActor.data.removeLong("undateable_"+position);
                pActor.data.set("amount_undateable", length - 1);
                
                for (var i = position + 1; i < length; i++)
                {
                    pActor.data.get("undateable_" + i, out var idFromSave, 0L);
                    pActor.data.set("undateable_" + (i - 1), idFromSave);
                    pActor.data.removeLong("undateable_"+i);
                }
            }
        }

        public static void LogInfo(string message)
        {
            TopicOfLove.LogInfo(message);
        }

        public static void LogInfo(object message)
        {
            TopicOfLove.LogInfo(Convert.ToString(message));
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
            var slowOnLog = GetKeywords((string)config["Misc"]["SlowOnLog"].GetValue());
            var stackTrace = GetKeywords((string)config["Misc"]["StackTrace"].GetValue());
            var ignore = GetKeywords((string)config["Misc"]["Ignore"].GetValue());
            var debug = (bool)config["Misc"]["Debug"].GetValue();

            var stringMsg = message.ToString();

            if (!debug)
                return;
            if (ignore.Length > 0 && ignore.Any(keyword => stringMsg.Contains(keyword)))
                return;
            if(stackTrace.Length > 0 && stackTrace.Any(keyword => stringMsg.Contains(keyword)))
                stringMsg += "\nStackTrace: " + Environment.StackTrace;
            if(slowOnLog.Length > 0 && slowOnLog.Any(keyword => stringMsg.Contains(keyword)))
                Config.setWorldSpeed(AssetManager.time_scales.get("slow_mo"));
            LogInfo(stringMsg);
        }
        
        public static bool NeedSameSexTypeForReproduction(this Actor pActor)
        {
            return pActor.hasSubspeciesTrait("reproduction_same_sex");
        }
        public static bool CanDoAnySexType(this Actor pActor)
        {
            return pActor.hasSubspeciesTrait("reproduction_hermaphroditic");
        }
        
        public static bool NeedDifferentSexTypeForReproduction(this Actor pActor)
        {
            return pActor.hasSubspeciesTrait("reproduction_sexual");
        }

        // this is to typically catch types like boats
        public static bool CapableOfLove(Actor pActor)
        {
            return pActor.hasSubspecies() && !pActor.asset.is_boat;
        }

        public static bool HasNoOne(Actor pActor)
        {
            return !pActor.hasBestFriend() && !pActor.hasFamily() && !pActor.hasLover() && !pActor.getParents().Any();
        }

        public static bool AffectedByIntimacy(Actor pActor)
        {
            return !pActor.hasTrait("intimacy_averse") && !pActor.hasTrait("psychopath");
        }
        
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