using ai.behaviours;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction))]
public class BehCFBFSRPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehCheckForBabiesFromSexualReproduction.execute))]
    // code is running twice??
    static bool SexPatch(Actor pActor, ref BehResult __result, BehCheckForBabiesFromSexualReproduction __instance)
    {
        var target = pActor.beh_actor_target != null ? pActor.beh_actor_target.a : pActor.lover;
        if (target == null)
        { 
            Util.Debug(pActor.getName()+": Cant do sex because target is null");
            __result = BehResult.Stop;
            return false;
        }
            
        pActor.subspecies.counter_reproduction_acts?.registerEvent();
        if(target.subspecies != pActor.subspecies)
            target.subspecies.counter_reproduction_acts?.registerEvent();
        __instance.checkForBabies(pActor, target);
        Util.JustHadSex(pActor, target);
        __result = BehResult.Continue;
        return false;
    }

    // simple patch fix that actually utilizes pLover in all parts of checkFamily
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehCheckForBabiesFromSexualReproduction.checkFamily))]
    static bool CheckFamilyFix(Actor pActor, Actor pLover)
    {
        bool flag = false;
        if (pActor.hasFamily())
        {
            if (pActor.family != pLover.family)
                flag = true;
        }
        else
            flag = true;
        if (!flag)
            return false;
        BehaviourActionBase<Actor>.world.families.newFamily(pActor, pActor.current_tile, pLover);
        return false;
    }
    
    // this patch handles who the mother is when it comes to sexual reproduction
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BehCheckForBabiesFromSexualReproduction.checkForBabies))]
        static bool CheckForBabiesPatch(Actor pParentA, Actor pParentB, BehCheckForBabiesFromSexualReproduction __instance)
        {
            Util.Debug($"\nAble to make a baby?\n{pParentA.getName()}: "+(Util.CanMakeBabies(pParentA)+$"\n${pParentB.getName()}: "+(Util.CanMakeBabies(pParentB))));

            if (!Util.CanMakeBabies(pParentA) || !Util.CanMakeBabies(pParentB))
                return false;

            // ensures that both subspecies HAVE not reached population limit
            if (pParentA.subspecies.hasReachedPopulationLimit() || pParentB.subspecies.hasReachedPopulationLimit())
                return false;

            Actor pregnantActor = null;
            Actor nonPregnantActor;
            
            if (Util.NeedDifferentSexTypeForReproduction(pParentA) && Util.NeedDifferentSexTypeForReproduction(pParentB))
            {
                if (pParentA.data.sex == pParentB.data.sex) return false;
                
                if (pParentA.isSexFemale())
                    pregnantActor = pParentA;
                else if (pParentB.isSexFemale())
                    pregnantActor = pParentB;
            }
            else if(Util.NeedSameSexTypeForReproduction(pParentA) && Util.NeedSameSexTypeForReproduction(pParentB))
            {
                if (pParentA.data.sex != pParentB.data.sex) return false;
                pregnantActor = !Randy.randomBool() ? pParentB : pParentA;
            } else if (Util.CanDoAnySexType(pParentA) || Util.CanDoAnySexType(pParentB))
            {
                if(Util.CanDoAnySexType(pParentA) && Util.CanDoAnySexType(pParentB))
                    pregnantActor = !Randy.randomBool() ? pParentB : pParentA;
                else if (Util.CanDoAnySexType(pParentA))
                {
                    pregnantActor = pParentA;
                }
                else
                {
                    pregnantActor = pParentB;
                }
            }

            if (pregnantActor == null)
                return false;

            nonPregnantActor = pregnantActor == pParentA ? pParentB : pParentA;
            
            var maturationTimeSeconds = pregnantActor.getMaturationTimeSeconds();
            
            pParentA.data.get("sex_reason", out var sexReason, "");
            pParentB.data.get("sex_reason", out var sexReason1, "");

            var aWantsBaby = Util.WantsBaby(pParentA);
            var bWantsBaby = Util.WantsBaby(pParentB);
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