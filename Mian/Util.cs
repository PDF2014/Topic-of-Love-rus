using System;
using System.Collections.Generic;
using Topic_of_Love.Mian;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using Topic_of_Love.Mian.CustomManagers.Dateable;
using NeoModLoader.services;

namespace Topic_of_Love
{
    public class Util
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

        public static bool CanReproduce(Actor pActor, Actor pTarget)
        {
            return pActor.subspecies.isPartnerSuitableForReproduction(pActor, pTarget);
        }

        public static bool IsIntimacyHappinessEnough(Actor actor, float happiness)
        {
            actor.data.get("intimacy_happiness", out float compare);
            return compare >= happiness;
        }
        
        public static void ChangeIntimacyHappinessBy(Actor actor, float happiness)
        {
            actor.data.get("intimacy_happiness", out float init);
            actor.data.set("intimacy_happiness", Math.Max(-100, Math.Min(happiness + init, 100)));
        }

        private static void OpinionOnSex(Actor actor1, Actor actor2)
        {
            if (actor1.hasSubspeciesTrait("amygdala"))
            {
                actor1.data.get("sex_reason", out var sexReason, "");
                
                // bug spotted? some actors were lovers but one of them disliked the sex for some reason
                if ((QueerTraits.PreferenceMatches(actor1, actor2, true) || (actor1.lover == actor2 && Randy.randomChance(0.5f)))
                    && (Randy.randomChance(sexReason.Equals("reproduction") ? 0.5f : 1f) || actor1.lover == actor2))
                {
                    var normal = 0.3f;
                    if (actor1.lover == actor2)
                        normal += 0.5f;

                    actor1.a.data.get("intimacy_happiness", out float happiness);
                    if (happiness < 0)
                    {
                        normal += Math.Abs((happiness / 100) / 2);
                    }

                    if (!QueerTraits.PreferenceMatches(actor1, actor2, true))
                        normal -= 0.2f;
                    
                    var type = Randy.randomChance(Math.Min(1, normal)) ? "enjoyed_sex" : "okay_sex"; 
                    actor1.addStatusEffect(type);
                }
                else
                    actor1.addStatusEffect("disliked_sex");   
            }
        }

        public static bool IsFaithful(Actor pActor)
        {
            return pActor.hasCultureTrait("committed") || pActor.hasTrait("faithful");
        }

        public static bool WillDoIntimacy(Actor pActor, string sexReason=null, bool withLover=true, bool isInit=false)
        {
            pActor.data.get("intimacy_happiness", out float d);
            if (isInit)
                Debug(pActor.getName() + " is requesting to do intimacy. Sexual happiness: "+d + ". With lover: "+withLover);
            else
                Debug(pActor.getName() + " is being requested to do intimacy. Sexual happiness: "+d + ". With lover: "+withLover);
            if(sexReason != null)
                Debug("\n"+sexReason);

            if (sexReason == null && !pActor.isAdult())
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
            
            var allowedToHaveIntimacy = withLover || (sexReason != null ? CanHaveSexWithoutRepercussionsWithSomeoneElse(pActor, sexReason) : CanHaveRomanceWithoutRepercussionsWithSomeoneElse(pActor));
            var reduceChances = 0f;
            pActor.data.get("intimacy_happiness", out float intimacyHappiness);
            
            if (intimacyHappiness < 0)
            {
                var toReduce = intimacyHappiness / 100;
                reduceChances += toReduce;
            }

            if (pActor.hasTrait("sex_indifferent") && sexReason != null)
                reduceChances = 0f;
            
            reduceChances = Math.Max(-0.2f, reduceChances);

            if(!allowedToHaveIntimacy
               && Randy.randomChance(Math.Max(0, (pActor.hasTrait("unfaithful") ? 0.6f : 0.95f) + reduceChances)))
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
            if (!doIntimacy && (sexReason == null || !sexReason.Equals("reproduction")))
            {
                Debug("Will not do intimacy since they are deemed to be happy enough");
                return false;
            }   

            if(!allowedToHaveIntimacy)
                Debug(pActor.getName() + " is cheating!");
            return true;
        }

        public static void ActorsInteracted(Actor actor1, Actor actor2)
        {
            actor1.data.set("last_had_interaction_with", actor2.getID());
            actor2.data.set("last_had_interaction_with", actor1.getID());
        }
        
        // handles the actors' happiness and mood towards their sex depending on their preferences
        // handle cheating here too
        public static void JustHadSex(Actor actor1, Actor actor2)
        {
            Util.Debug(actor1.getName() + " had sex with "+actor2.getName()+". They are lovers: "+(actor1.lover==actor2));
            ActorsInteracted(actor1, actor2);
            
            actor1.addAfterglowStatus();
            actor2.addAfterglowStatus();   
            
            if (Randy.randomChance(actor1.lover == actor2 ? 1f : QueerTraits.BothActorsPreferencesMatch(actor1, actor2, true) ? 0.25f : 0f))
            {
                actor1.addStatusEffect("just_kissed");
                actor2.addStatusEffect("just_kissed");
            }
            
            OpinionOnSex(actor1, actor2);
            OpinionOnSex(actor2, actor1);

            if (actor1.lover != actor2)
            {
                actor1.data.get("sex_reason", out var sexReason, "reproduction");
                Util.Debug("Sex Reason: "+sexReason);
                if (!CanHaveSexWithoutRepercussionsWithSomeoneElse(actor1, sexReason))
                {
                    PotentiallyCheatedWith(actor1, actor2);
                }

                if (!CanHaveSexWithoutRepercussionsWithSomeoneElse(actor2, sexReason))
                {
                    PotentiallyCheatedWith(actor2, actor1);
                }   
            }

            if (actor1.hasLover() && actor1.lover != actor2)
            {
                ChangeIntimacyHappinessBy(actor1.lover, -25f);
            }

            if (actor2.hasLover() && actor2.lover != actor1)
            {
                ChangeIntimacyHappinessBy(actor2.lover, -25f);
            }
        }

        public static bool CanHaveSexWithoutRepercussionsWithSomeoneElse(Actor actor, string sexReason)
        {
            return !actor.hasLover()
                   || (actor.hasLover() && ((!QueerTraits.PreferenceMatches(actor, actor.lover, true)
                                                              && actor.lover.hasCultureTrait("sexual_expectations"))
                                                              || (actor.hasSubspeciesTrait("preservation") && IsDyingOut(actor) 
                                                                  && sexReason.Equals("reproduction")
                                                                  && (!CanMakeBabies(actor.lover) || !CanReproduce(actor, actor.lover)))));
        }
        
        public static bool CanHaveRomanceWithoutRepercussionsWithSomeoneElse(Actor actor)
        {
            return !actor.hasLover()
                   || (actor.hasLover() && !QueerTraits.BothPreferencesMatch(actor, actor.lover)
                                             && actor.lover.hasCultureTrait("sexual_expectations"));
        }


        public static void PotentiallyCheatedWith(Actor actor, Actor actor2)
        {
            if (actor.hasLover() && actor.lover != actor2 && CanStopBeingLovers(actor))
            {
                var cheatedActor = actor.lover;
                if (cheatedActor.isLying() || !cheatedActor.isOnSameIsland(actor))
                    return;
                
                HandleFamilyRemoval(actor);

                cheatedActor.addStatusEffect("cheated_on");
            }
        }

        public static bool CannotDate(Actor actor, Actor actor2)
        {
            return IsActorUndateable(actor, actor2) || IsActorUndateable(actor2, actor);
        }

        public static void BreakUp(Actor actor)
        {
            if (!actor.hasLover())
                return;
            
            Debug(actor.getName() + " broke up with "+ actor.lover.getName());
            
            HandleFamilyRemoval(actor);
            
            // DateableManager.Manager.AddOrRemoveUndateable(actor, actor.lover);
            // DateableManager.Manager.AddOrRemoveUndateable(actor.lover, actor);
            
            AddOrRemoveUndateableActor(actor, actor.lover);
            AddOrRemoveUndateableActor(actor.lover, actor);
            
            actor.lover.changeHappiness("breakup");
            actor.changeHappiness("breakup");
            
            RemoveLovers(actor);
        }

        public static bool CanStopBeingLovers(Actor actor)
        {
            actor.data.get("force_lover", out var isForced, false);
            return !isForced;
        }

        public static void RemoveLovers(Actor actor)
        {
            var lover = actor.lover;
            lover.setLover(null);
            actor.setLover(null);
            lover.data.set("force_lover", false);
            actor.data.set("force_lover", false);
        }

        public static void HandleFamilyRemoval(Actor actor)
        {
            if (actor.hasFamily() && actor.hasLover())
            {
                var family = actor.family;
                var lover = actor.lover;
                if (family.isMainFounder(actor) && family.isMainFounder(lover))
                {
                    if (family.countUnits() <= 2)
                    {
                        actor.setFamily(null);
                        lover.setFamily(null);
                        return;
                    }
                    
                    var actor1Units = new List<Actor>();
                    var actor2Units = new List<Actor>();
                    
                    foreach (var unit in family.getUnits())
                    {
                        if (unit == actor || unit == lover)
                            continue;
                        
                        var sameAsActor1 = unit.isSameSubspecies(actor.subspecies);
                        var sameAsActor2 = unit.isSameSubspecies(lover.subspecies);
                        if ((sameAsActor1 && sameAsActor2) || (!sameAsActor1 && !sameAsActor2))
                        {
                            if(Randy.randomChance(0.5f))
                                actor1Units.Add(unit);
                            else
                                actor2Units.Add(unit);
                            continue;
                        }
                        
                        if(sameAsActor1)
                            actor1Units.Add(unit);
                        if(sameAsActor2)
                            actor2Units.Add(unit);
                    }
                    
                    var family1 = BehaviourActionBase<Actor>.world.families.newFamily(actor, actor.current_tile, null);
                    var family2 = BehaviourActionBase<Actor>.world.families.newFamily(lover, lover.current_tile, null);
                    
                    foreach (var unit in actor1Units)
                    {
                        unit.setFamily(family1);
                    }
                    
                    foreach (var unit in actor2Units)
                    {
                        unit.setFamily(family2);
                    }
                }
            }
        }

        public static bool IsOrientationSystemEnabledFor(Actor pActor)
        {
            return !pActor.hasCultureTrait("orientationless");
        }

        public static bool CanMakeBabies(Actor pActor)
        {
            return pActor.canBreed() &&
                   pActor.isAdult() && !pActor.hasReachedOffspringLimit() &&
                   !pActor.subspecies.hasReachedPopulationLimit() && (!pActor.hasCity() || !pActor.city.hasReachedWorldLawLimit()
            && ((pActor.subspecies.isReproductionSexual() || pActor.subspecies.hasTraitReproductionSexualHermaphroditic() 
                                                          || pActor.hasSubspeciesTrait("reproduction_same_sex"))
            && pActor.current_children_count == 0 || pActor.city.hasFreeHouseSlots()));
        }

        public static bool IsDyingOut(Actor pActor)
        {
            if (!pActor.hasSubspecies()) return false;
            var limit = (int)pActor.subspecies.base_stats_meta["limit_population"];
            return pActor.subspecies.countCurrentFamilies() <= 10
                   || (limit != 0 ? pActor.subspecies.countUnits() <= limit / 3 : pActor.subspecies.countUnits() <= 50);
        }
        
        public static bool WantsBaby(Actor pActor, bool reproductionPurposesIncluded=true)
        {
            if (!CanMakeBabies(pActor))
                return false;
            
            if (reproductionPurposesIncluded)
            {
                if (!pActor.isSapient() || IsDyingOut(pActor))
                {
                    Debug(pActor.getName() + " wants a baby because they are non-intelligent species or are dying out");
                    return true;
                }   
            }
            
            if (pActor.hasHouse() && pActor.getHappiness() >= 75)
            {
                Debug(pActor.getName() + " wants a baby because they have a house and are happy enough");
                return true;
            }
            
            // Debug(pActor.getName() + " does not want a baby.");
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

        public static void LogWithId(string message)
        {
            LogService.LogInfo($"[{TopicOfLove.Mod.GetDeclaration().Name}]: "+message);
        }
        
        public static void Debug(string message)
        {
            var config = TopicOfLove.Mod.GetConfig();
            var slowOnLog = (bool)config["Misc"]["SlowOnLog"].GetValue();
            var debug = (bool)config["Misc"]["Debug"].GetValue();

            if (!debug)
                return;
            if(slowOnLog)
                Config.setWorldSpeed(AssetManager.time_scales.get("slow_mo"));
            LogWithId(message);
        }
        
        public static bool NeedSameSexTypeForReproduction(Actor pActor)
        {
            return pActor.hasSubspeciesTrait("reproduction_same_sex");
        }
        public static bool CanDoAnySexType(Actor pActor)
        {
            return pActor.hasSubspeciesTrait("reproduction_hermaphroditic");
        }
        
        public static bool NeedDifferentSexTypeForReproduction(Actor pActor)
        {
            return pActor.hasSubspeciesTrait("reproduction_sexual");
        }
    }
}