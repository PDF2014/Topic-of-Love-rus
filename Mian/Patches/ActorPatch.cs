using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using Topic_of_Love.Mian.CustomManagers.Dateable;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(Actor))]
public class ActorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Actor.buildCityAndStartCivilization))]
    static void Postfix(Actor __instance)
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
    static void Postfix1(Actor __instance)
    {
        if (__instance.hasLover())
        {
            var lover = __instance.lover;
            if ((!lover.has_attack_target || (lover.has_attack_target && lover.attackedBy != lover.attack_target)) 
                && __instance.attackedBy != null && !lover.isLying()  && !lover.shouldIgnoreTarget(__instance.attackedBy)
                && lover.distanceToObjectTarget(__instance.attackedBy) < 40)
            {
                Util.Debug(lover.getName() + "'s lover was attacked! They are going to defend them.");
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
            if (__instance.isAdult()) // fluid sexuality
            {
                if (!QueerTraits.HasQueerTraits(__instance)){
                    QueerTraits.GiveQueerTraits(__instance, false, true);
                    __instance.changeHappiness("true_self");
                }
                else
                {
                    bool changed = false;
                    var list = QueerTraits.GetQueerTraits(__instance);
                    list = QueerTraits.RandomizeQueerTraits(__instance, true, list);
                    if (__instance.hasTrait("abroromantic") && Randy.randomChance(0.1f))
                    {
                        QueerTraits.CleanQueerTraits(__instance, false);
                        __instance.addTrait(list[1]);
                        changed = true;
                    }
                    if (__instance.hasTrait("abrosexual") && Randy.randomChance(0.1f))
                    {
                        QueerTraits.CleanQueerTraits(__instance, true);
                        __instance.addTrait(list[0]);
                        changed = true;
                    }
                    if(changed)
                        __instance.changeHappiness("true_self");
                }
                if(QueerTraits.GetPreferenceFromActor(__instance, true) != Preference.Neither && Util.IsOrientationSystemEnabledFor(__instance))
                    Util.ChangeIntimacyHappinessBy(__instance.a, -Randy.randomFloat(5, 10f));
                else
                    __instance.data.set("intimacy_happiness", 100f);
            } else if (!__instance.isAdult() && Randy.randomChance(0.1f)) // random chance younger kid finds their orientations
            {
                QueerTraits.GiveQueerTraits(__instance, false, true);
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
                        Util.Debug(__instance.getName() + " has forgived " + actor.getName());
                        Util.AddOrRemoveUndateableActor(__instance, actor); 
                    }   
                }
            }   

            // Randomize breaking up (1% if preferences match. 25% if preferences do not match.) 
            
            // break up is too common rn, let's implement a system in the future to get lovers back together
            if (__instance.hasLover() && Util.CanStopBeingLovers(__instance) &&
                ((Util.IsOrientationSystemEnabledFor(__instance) 
                  && Randy.randomChance(!QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.25f : 0.01f)) 
                 || (!Util.IsOrientationSystemEnabledFor(__instance) && !Util.CanReproduce(__instance, __instance.lover))))
            {
                if (!__instance.hasCultureTrait("committed") || !__instance.lover.hasCultureTrait("committed"))
                {
                    Util.BreakUp(__instance);   
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
                Util.CannotDate(pTarget, __instance)
                ||
                 (!QueerTraits.PreferenceMatches(__instance, pTarget, false) && Util.IsOrientationSystemEnabledFor(__instance))
                 || (!QueerTraits.PreferenceMatches(pTarget, __instance, false) && Util.IsOrientationSystemEnabledFor(pTarget))
                
                || __instance.hasLover()
                || pTarget.hasLover()

                || !WithinOfAge(__instance, pTarget)
                
                || __instance.areFoes(pTarget)
                
                || (!(__instance.isSameSpecies(pTarget) || __instance.isSameSubspecies(pTarget.subspecies))
                                                       && !((__instance.hasXenophiles() || !mustBeXenophile)
                                                             && (__instance.isSapient() && pTarget.isSapient() || !mustBeSmart)
                                                             && !pTarget.hasXenophobic() || !allowCrossSpeciesLove)) // subspecies stuff!
                
                // if queer but culture trait says they do not matter
                || ((!Util.IsOrientationSystemEnabledFor(__instance) || !Util.IsOrientationSystemEnabledFor(pTarget))
                    && !Util.CanReproduce(__instance, pTarget)))
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