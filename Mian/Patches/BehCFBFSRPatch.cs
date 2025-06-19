using System;
using ai.behaviours;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction))]
public class BehCFBFSRPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(BehCheckForBabiesFromSexualReproduction.execute))]
    // code is running twice??
    static bool SexPatch(Actor pActor, ref BehResult __result, BehCheckForBabiesFromSexualReproduction __instance)
    {
        var target = pActor.beh_actor_target != null ? pActor.beh_actor_target.a : pActor.lover;
        if (target == null)
        { 
            TolUtil.Debug(pActor.getName()+": Cant do sex because target is null");
            __result = BehResult.Stop;
            return false;
        }
            
        pActor.subspecies.counter_reproduction_acts?.registerEvent();
        if(target.subspecies != pActor.subspecies)
            target.subspecies.counter_reproduction_acts?.registerEvent();
        __instance.checkForBabies(pActor, target);
        TolUtil.JustHadSex(pActor, target);
        __result = BehResult.Continue;
        return false;
    }
    
    // simple patch fix that actually utilizes pLover in all parts of checkFamily
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehCheckForBabiesFromSexualReproduction.checkFamily))]
    static bool CheckFamilyFix(Actor pActor, Actor pLover)
    {
        var flag = false;
        TolUtil.Debug($"Checking if {pActor.getName()} needs a new family..");
        if (pActor.hasFamily())
        {
            TolUtil.Debug( pActor.family.isMainFounder(pActor));
            if ((pLover != null && pActor.family != pLover.family) || !pActor.family.isMainFounder(pActor))
            {
                TolUtil.Debug($"{pActor.getName()} needs a new family.");
                flag = true;
            }
        }
        else
            flag = true;
        if (!flag)
            return false;
        TolUtil.Debug($"Creating family for {pActor.getName()}!");
        BehaviourActionBase<Actor>.world.families.newFamily(pActor, pActor.current_tile, pLover);
        return false;
    }
    
    // this patch handles who the mother is when it comes to sexual reproduction
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction),
        nameof(BehCheckForBabiesFromSexualReproduction.checkForBabies))]
    [HarmonyAfter("netdot.mian.topicofidentity")]
    // runs after TOI because we need to check for genitalia there
        static bool CheckForBabiesPrefix(Actor pParentA, Actor pParentB, BehCheckForBabiesFromSexualReproduction __instance)
        {
            TolUtil.Debug($"\nAble to make a baby?\n{pParentA.getName()}: "+(BabyHelper.canMakeBabies(pParentA)+$"\n${pParentB.getName()}: "+(BabyHelper.canMakeBabies(pParentB))));

            if (!BabyHelper.canMakeBabies(pParentA) || !BabyHelper.canMakeBabies(pParentB) || !TolUtil.CouldReproduce(pParentA, pParentB))
                return false;

            // ensures that both subspecies HAVE not reached population limit
            if (pParentA.subspecies.hasReachedPopulationLimit() || pParentB.subspecies.hasReachedPopulationLimit())
                return false;

            var aCanBePregnant = TolUtil.IsAbleToBecomePregnant(pParentA);
            var bCanBePregnant = TolUtil.IsAbleToBecomePregnant(pParentB);
            Actor pregnantActor;
            if (aCanBePregnant && bCanBePregnant)
                pregnantActor = Randy.randomBool() ? pParentA : pParentB;
            else
                pregnantActor = aCanBePregnant ? pParentA : bCanBePregnant ? pParentB : null; 
            
            if (pregnantActor == null)
                return false;
            
            Actor nonPregnantActor;
            nonPregnantActor = pregnantActor == pParentA ? pParentB : pParentA;
            
            var maturationTimeSeconds = pregnantActor.getMaturationTimeSeconds();
            
            pParentA.data.get("sex_reason", out var sexReason, "");
            pParentB.data.get("sex_reason", out var sexReason1, "");

            var aWantsBaby = TolUtil.WantsBaby(pParentA);
            var bWantsBaby = TolUtil.WantsBaby(pParentB);
            bool success = sexReason1.Equals("casual") || sexReason.Equals("casual") ? 
                aWantsBaby && bWantsBaby : true;
            
            if(!success)
            {
                success = Randy.randomChance(0.2f);
                if (success)
                {
                    if (!aWantsBaby)
                    {
                        pParentA.changeHappiness("did_not_want_baby");
                    }
                    if (!bWantsBaby)
                    {
                        pParentA.changeHappiness("did_not_want_baby");
                    }
                }
            }
            
            // bool success = sexReason1.Equals("casual") || sexReason.Equals("casual") ? Randy.randomChance(0.2F) : true;
            // Util.Debug($"\nDo parents want a baby?\n{pParentA.getName()}: {Util.WantsBaby(pParentA)}\n{pParentB.getName()}: {Util.WantsBaby(pParentB)}\nSex Reason: ${sexReason}, ${sexReason1}");
            if (success)
            {
                if(pParentA.lover == pParentB || (!pParentB.hasLover() && !pParentA.hasLover()))
                    __instance.checkFamily(pParentA, pParentB);
                ReproductiveStrategy reproductionStrategy = pregnantActor.subspecies.getReproductionStrategy();
                switch (reproductionStrategy)
                {
                    case ReproductiveStrategy.Egg:
                    case ReproductiveStrategy.SpawnUnitImmediate:
                        BabyMaker.makeBabiesViaSexual(pregnantActor, pregnantActor, pParentB);
                        pregnantActor.subspecies.counterReproduction();
                        break;
                    case ReproductiveStrategy.Pregnancy:
                        if (pregnantActor.hasStatus("pregnant"))
                            return false;
                        
                        pregnantActor.data.set("otherParent", nonPregnantActor.getID());

                        BabyHelper.babyMakingStart(pregnantActor);
                        pregnantActor.addStatusEffect("pregnant", maturationTimeSeconds);
                        pregnantActor.subspecies.counterReproduction();
                        break;
                }   
            }
            return false;
        }
}