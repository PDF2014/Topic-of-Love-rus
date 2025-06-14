using System;
using EpPathFinding.cs;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.services;
using Topic_of_Love.Mian.CustomAssets.Custom;
using UnityEngine.Rendering;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(Actor))]
public class ActorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.removeTrait), typeof(ActorTrait))]
    static void ClearTraitPatch(ActorTrait pTrait, Actor __instance)
    {
        if (pTrait is PreferenceTrait preferenceTrait)
        {
            Orientations.CreateOrientationBasedOnPrefChange(__instance, preferenceTrait);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.addTrait), typeof(ActorTrait), typeof(bool))]
    static void AddTraitPatch(ActorTrait pTrait, Actor __instance)
    {
        // removes preference traits if they are all added for some reason 
        foreach (var type in Preferences.PreferenceTypes.Keys)
        {
            // Romantic
            
            var list = Preferences.GetActorPreferencesFromType(__instance, type);
            var toCompare = Preferences.GetPreferencesFromType(type);

            if (list.Count == toCompare.Count)
                __instance.removeTraits(list);
            
            // Sexual
            
            list = Preferences.GetActorPreferencesFromType(__instance, type, true);
            toCompare = Preferences.GetPreferencesFromType(type, true);

            if (list.Count == toCompare.Count)
                __instance.removeTraits(list);
        }

        if (pTrait is PreferenceTrait preferenceTrait)
        {
            Orientations.CreateOrientationBasedOnPrefChange(__instance, preferenceTrait);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.buildCityAndStartCivilization))]
    static void BuildCityAndStartCivPatch(Actor __instance)
    {
        if (__instance.hasLover() && (!__instance.lover.hasKingdom() || __instance.lover.kingdom.wild))
        {
            __instance.lover.setForcedKingdom(__instance.kingdom);
            __instance.lover.setCity(__instance.city);
            __instance.lover.setCulture(__instance.culture);
            __instance.kingdom.data.original_actor_asset = __instance.getActorAsset().id;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.getHit))]
    static void OnHitPatch(Actor __instance)
    {
        if (__instance.hasLover())
        {
            var lover = __instance.lover;
            if ((!lover.has_attack_target || (lover.has_attack_target && lover.attackedBy != lover.attack_target)) 
                && __instance.attackedBy != null && !lover.isLying()  && !lover.shouldIgnoreTarget(__instance.attackedBy)
                && lover.distanceToObjectTarget(__instance.attackedBy) < 40)
            {
                TolUtil.Debug(lover.getName() + "'s lover was attacked! They are going to defend them.");
                lover.startFightingWith(__instance.attackedBy);
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.create))]
        static void ActorCreatePatch(Actor __instance)
        {
            __instance.asset.addDecision("find_lover");
            __instance.data.set("intimacy_happiness", 10f);
        }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.updateAge))]
        static void CalcAgeStages(Actor __instance)
        {
            // maybe we can reintroduce fluid preferences in the future?
            
            if (__instance.isAdult()) // fluid sexuality
            {
                if (!__instance.hasTrait("unfluid") && MapBox.instance.world_laws.isEnabled("world_law_fluid_sexuality"))
                {
                    // preference for gender
                    if (Randy.randomChance(0.005f))
                    {
                        if (Randy.randomBool())
                        {
                            __instance.addTrait(Preferences.RandomPreferenceFromType("identity", Randy.randomBool()));
                        }
                        else
                        {
                            var preferences = Preferences.GetActorPreferencesFromType(__instance, "identity", Randy.randomBool());
                            if (preferences.Count > 0)
                            {
                                __instance.removeTrait(preferences.GetRandom());
                            }
                        }
                    }

                    if (TolUtil.IsTOIInstalled())
                    {
                        // preference for masculinity/femininity
                        if (Randy.randomChance(0.01f))
                        {
                            if (Randy.randomBool())
                            {
                                __instance.addTrait(Preferences.RandomPreferenceFromType("expression", Randy.randomBool()));
                            }
                            else
                            {
                                var preferences = Preferences.GetActorPreferencesFromType(__instance, "expression", Randy.randomBool());
                                if (preferences.Count > 0)
                                {
                                    __instance.removeTrait(preferences.GetRandom());
                                }
                            }
                        }
                
                        // preference for genitals
                        if (Randy.randomChance(0.005f))
                        {
                            if (Randy.randomBool())
                            {
                                __instance.addTrait(Preferences.RandomPreferenceFromType("genital", true));
                            }
                            else
                            {
                                var preferences = Preferences.GetActorPreferencesFromType(__instance, "genital", true);
                                if (preferences.Count > 0)
                                {
                                    __instance.removeTrait(preferences.GetRandom());
                                }
                            }
                        }   
                    }
                }
                
                // maybe rework so that aromantic/asexual ppl still experience intimacy happiness in some way?
                if(!Preferences.Dislikes(__instance, true) && TolUtil.IsOrientationSystemEnabledFor(__instance))
                    TolUtil.ChangeIntimacyHappinessBy(__instance.a, -Randy.randomFloat(5, 10f));
                else
                    __instance.data.set("intimacy_happiness", 100f);
            } else if (!__instance.isAdult() && Randy.randomChance(0.1f) && !Preferences.HasAPreference(__instance))
            {
                TolUtil.NewPreferences(__instance);
                __instance.changeHappiness("true_self");
            }
            
            __instance.data.get("amount_undateable", out var length, 0);
            for(var i = 0; i < length; i++)
            {
                __instance.data.get("undateable_" + i, out var id, 0L);
                var actor = World.world.units.get(id);
                if (actor != null)
                {
                    if (Randy.randomChance(0.05f))
                    {
                        TolUtil.Debug(__instance.getName() + " has forgived " + actor.getName());
                        TolUtil.AddOrRemoveUndateableActor(__instance, actor); 
                    }   
                }
            }
            
            if (Randy.randomChance(0.05f))
            {
                __instance.data.set("just_lost_lover", false);
            } 

            if (__instance.hasLover())
            {
                var breakingUpChance = 0.005f;
                if (!Preferences.PreferenceMatches(__instance, __instance.lover, "identity", false))
                    breakingUpChance += 0.2f;
                if (!Preferences.PreferenceMatches(__instance, __instance.lover, "expression", false))
                    breakingUpChance += 0.05f;
                var wantsToBreakUp = Randy.randomChance(breakingUpChance);
                                
                if(TolUtil.CanStopBeingLovers(__instance) &&
                    ( (TolUtil.IsOrientationSystemEnabledFor(__instance) && wantsToBreakUp) 
                     || (!TolUtil.IsOrientationSystemEnabledFor(__instance) && !TolUtil.CouldReproduce(__instance, __instance.lover))))
                {
                    if (!__instance.hasCultureTrait("committed") || !__instance.lover.hasCultureTrait("committed"))
                    {
                        TolUtil.BreakUp(__instance);   
                    }   
                }
            }
        }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Actor.becomeLoversWith))]
    static void BecomeLoversWith(Actor pTarget, Actor __instance)
    {
        // if they become new lovers with someone, the others were cheated on
        TolUtil.PotentiallyCheatedWith(__instance, pTarget);
        TolUtil.PotentiallyCheatedWith(pTarget, __instance);
        
        TolUtil.Debug($"{__instance.getName()} fell in love {pTarget.getName()}!");
    }
        
    // This is where we handle the beef of our code for having cross species and non-same reproduction method ppl fall in love
    // important to note this should check for both actors
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Actor.canFallInLoveWith))]
        static bool CanFallInLoveWithPatch(Actor pTarget, ref bool __result, Actor __instance)
        {
            // TolUtil.Debug($"Can {__instance.getName()} fall in love with {pTarget.getName()}?");
            var config = TopicOfLove.Mod.GetConfig();

            // we will add traits for these in the future
            // var allowCrossSpeciesLove = (bool)config["CrossSpecies"]["AllowCrossSpeciesLove"].GetValue();
            // var mustBeSmart = (bool)config["CrossSpecies"]["MustBeSmart"].GetValue();
            // var mustBeXenophile = (bool)config["CrossSpecies"]["MustBeXenophile"].GetValue();
            
            if (TolUtil.CannotDate(pTarget, __instance))
            {
                __result = false;
                return false;
            }

            // both actors must have the same orientation system otherwise we will run into issues tbh
            if (TolUtil.IsOrientationSystemEnabledFor(__instance) != TolUtil.IsOrientationSystemEnabledFor(pTarget))
            {
                __result = false;
                return false;
            }

            // there is no cheating when the orientation system is disabled
            var orientationSystemInvolved = TolUtil.IsOrientationSystemEnabledFor(__instance);
            if (!orientationSystemInvolved && (__instance.hasLover() || pTarget.hasLover()))
            {
                __result = false;
                return false;
            }

            if (orientationSystemInvolved)
            {
                if (!Preferences.BothActorsPreferenceMatch(__instance, pTarget, false))
                {
                    __result = false;
                    return false;
                }
            }
            else if (!TolUtil.CouldReproduce(pTarget, __instance))
            {
                __result = false;
                return false;
            }

            // makes sure they are both within age of dating (mature dating trait involved)
            if (!TolUtil.WithinOfAge(__instance, pTarget))
            {
                __result = false;
                return false;
            }

            // they literally hate each other (maybe implement toxic relationships in the future?)
            if (__instance.areFoes(pTarget))
            {
                __result = false;
                return false;
            }

            // both must exhibit the same sapientness
            var bothAreSapient = __instance.isSapient();
            if (__instance.isSapient() != pTarget.isSapient())
            {
                __result = false;
                return false;
            }

            if (!__instance.isSameSpecies(pTarget) && (__instance.hasXenophobic() || pTarget.hasXenophobic() || !bothAreSapient))
            {
                __result = false;
                return false;
            }
            
            if (!TolUtil.CanFallInLove(pTarget) || !TolUtil.CanFallInLove(__instance))
            {
                __result = false;
                return false;
            }

            // no more incest for now
            // if (__instance.isRelatedTo(pTarget) && (!__instance.hasCultureTrait("incest") || !pTarget.hasCultureTrait("incest")))
            // {
                // __result = false;
                // return false;
            // }


            if (__instance.isRelatedTo(pTarget))
            {
                __result = false;
                return false;
            }
            
            __result = true;
            return false;
        }
}