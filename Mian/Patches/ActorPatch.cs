using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using Topic_of_Love.Mian.CustomManagers.Dateable;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.services;
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
            Preferences.CreateOrientationBasedOnPrefChange(__instance, preferenceTrait);
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
            Preferences.CreateOrientationBasedOnPrefChange(__instance, preferenceTrait);
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
                TOLUtil.Debug(lover.getName() + "'s lover was attacked! They are going to defend them.");
                lover.startFightingWith(__instance.attackedBy);
            }
        }
    }
    
    [HarmonyPatch(typeof(Actor), nameof(Actor.create))]
    class ActorCreatePatch
    {
        static void Postfix(Actor __instance)
        {
            __instance.asset.addDecision("find_lover");
            __instance.data.set("intimacy_happiness", 10f);
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.updateAge))]
    class CalcAgeStatesPatch
    {
        static void Postfix(Actor __instance)
        {
            // maybe we can reintroduce fluid preferences in the future?
            
            if (__instance.isAdult()) // fluid sexuality
            {
                // if (!Preferences.HasQueerTraits(__instance)){
                    // Orientations.GiveQueerTraits(__instance, false, true);
                    // __instance.changeHappiness("true_self");
                // }
                // else
                // {
                    // bool changed = false;
                    // var list = Orientations.GetQueerTraits(__instance);
                    // list = Orientations.RandomizeQueerTraits(__instance, true, list);
                    // if (__instance.hasTrait("abroromantic") && Randy.randomChance(0.1f))
                    // {
                    //     Orientations.CleanQueerTraits(__instance, false);
                    //     __instance.addTrait(list[1]);
                    //     changed = true;
                    // }
                    // if (__instance.hasTrait("abrosexual") && Randy.randomChance(0.1f))
                    // {
                    //     Orientations.CleanQueerTraits(__instance, true);
                    //     __instance.addTrait(list[0]);
                    //     changed = true;
                    // }
                    // if(changed)
                    //     __instance.changeHappiness("true_self");
                // }
                
                // maybe rework so that aromantic/asexual ppl still experience intimacy happiness in some way?
                if(!Preferences.Dislikes(__instance, true) && TOLUtil.IsOrientationSystemEnabledFor(__instance))
                    TOLUtil.ChangeIntimacyHappinessBy(__instance.a, -Randy.randomFloat(5, 10f));
                else
                    __instance.data.set("intimacy_happiness", 100f);
            } else if (!__instance.isAdult() && Randy.randomChance(0.1f)) // random chance younger kid finds their orientations
            {
                TOLUtil.GivePreferences(__instance);
                __instance.changeHappiness("true_self");
            }
            
            // List<Actor> undateables = DateableManager.Manager.GetUndateablesFor(__instance);
            // if (undateables != null)
            // {
            //     foreach (var actor in undateables)
            //     {
            //         if (Randy.randomChance(0.2f))
            //         {
            //             Util.Debug(__instance.getName() + " has forgived " + actor.getName());
            //             DateableManager.Manager.AddOrRemoveUndateable(__instance, actor); 
            //         }
            //     }   
            // }
            
            __instance.data.get("amount_undateable", out var length, 0);
            for(var i = 0; i < length; i++)
            {
                __instance.data.get("undateable_" + i, out var id, 0L);
                var actor = World.world.units.get(id);
                if (actor != null)
                {
                    if (Randy.randomChance(0.05f))
                    {
                        TOLUtil.Debug(__instance.getName() + " has forgived " + actor.getName());
                        TOLUtil.AddOrRemoveUndateableActor(__instance, actor); 
                    }   
                }
            }
            
            if (Randy.randomChance(0.05f))
            {
                __instance.data.set("just_lost_lover", false);
            } 

            // Randomize breaking up (1% if preferences match. 25% if preferences do not match.) 
            
            // break up is too common rn, let's implement a system in the future to get lovers back together
            if (__instance.hasLover() && TOLUtil.CanStopBeingLovers(__instance) &&
                ((TOLUtil.IsOrientationSystemEnabledFor(__instance) 
                  && Randy.randomChance(!Preferences.PreferenceMatches(__instance, __instance.lover, false) ? 0.25f : 0.01f)) 
                 || (!TOLUtil.IsOrientationSystemEnabledFor(__instance) && !TOLUtil.CanReproduce(__instance, __instance.lover))))
            {
                if (!__instance.hasCultureTrait("committed") || !__instance.lover.hasCultureTrait("committed"))
                {
                    TOLUtil.BreakUp(__instance);   
                }
            }
        } 
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.becomeLoversWith))]
    class BecomeLoversWithPatch
    {
        static void Postfix(Actor pTarget, Actor __instance)
        {
            BehaviourActionBase<Actor>.world.families.newFamily(__instance, __instance.current_tile, pTarget);
        }
    }
    
    // This is where we handle the beef of our code for having cross species and non-same reproduction method ppl fall in love
    [HarmonyPatch(typeof(Actor), nameof(Actor.canFallInLoveWith))]
    class CanFallInLoveWithPatch
    {
        private static bool WithinOfAge(Actor pActor, Actor pTarget)
        { 
            int higherAge = Math.Max(pActor.age, pTarget.age);
            int lowerAge = Math.Min(pActor.age, pTarget.age);
            int minimumAge = higherAge / 2 + 7;
            return lowerAge >= minimumAge || (!pActor.hasCultureTrait("mature_dating") && !pTarget.hasCultureTrait("mature_dating"));
        }
        static bool Prefix(Actor pTarget, ref bool __result, Actor __instance)
        {
            // LogService.LogInfo($"Can {__instance.getName()} fall in love with {pTarget.getName()}?");
            var config = TopicOfLove.Mod.GetConfig();
            var allowCrossSpeciesLove = (bool)config["CrossSpecies"]["AllowCrossSpeciesLove"].GetValue();
            var mustBeSmart = (bool)config["CrossSpecies"]["MustBeSmart"].GetValue();
            var mustBeXenophile = (bool)config["CrossSpecies"]["MustBeXenophile"].GetValue();
            
            if (
                // DateableManager.Manager.IsActorUndateable(pTarget, __instance)
                TOLUtil.CannotDate(pTarget, __instance)
                ||
                 (!Preferences.PreferenceMatches(__instance, pTarget, false) && TOLUtil.IsOrientationSystemEnabledFor(__instance))
                 || (!Preferences.PreferenceMatches(pTarget, __instance, false) && TOLUtil.IsOrientationSystemEnabledFor(pTarget))
                
                || __instance.hasLover()
                || pTarget.hasLover()

                || !WithinOfAge(__instance, pTarget)
                
                || __instance.areFoes(pTarget)
                
                || (!(__instance.isSameSpecies(pTarget) || __instance.isSameSubspecies(pTarget.subspecies))
                                                       && !((__instance.hasXenophiles() || !mustBeXenophile)
                                                             && (__instance.isSapient() && pTarget.isSapient() || !mustBeSmart)
                                                             && !pTarget.hasXenophobic() || !allowCrossSpeciesLove)) // subspecies stuff!
                
                || !TOLUtil.CanFallInLove(pTarget)
                || !TOLUtil.CanFallInLove(__instance)
                
                // if queer but culture trait says they do not matter
                || ((!TOLUtil.IsOrientationSystemEnabledFor(__instance) || !TOLUtil.IsOrientationSystemEnabledFor(pTarget))
                    && !TOLUtil.CanReproduce(__instance, pTarget)))
            {
                __result = false;
                return false;
            }

            if (__instance.isRelatedTo(pTarget) && (!__instance.hasCultureTrait("incest") || !pTarget.hasCultureTrait("incest")))
            {
                __result = false;
                return false;
            }
            
            __result = true;

            // LogService.LogInfo($"Success! They in love :D");
            return false;
        }
    }
}