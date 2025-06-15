using ai.behaviours;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class BehIntimacyPatches
{
    [HarmonyPatch(typeof(BehChildFindRandomFamilyParent), nameof(BehChildFindRandomFamilyParent.execute))]
    static class ChildFindRandomFamilyParentPatch
    {
        static void Postfix(Actor pBabyActor, BehResult __result)
        {
            if(__result == BehResult.Continue)
                TolUtil.ChangeIntimacyHappinessBy(pBabyActor, 10);
        }
    }

    [HarmonyPatch(typeof(BehFamilyFollowAlpha), nameof(BehFamilyFollowAlpha.execute))]
    static class FamilyFollowAlphaPatch
    {
        static void Postfix(Actor pActor, BehResult __result)
        {
            if(__result == BehResult.Continue)
                TolUtil.ChangeIntimacyHappinessBy(pActor, 10);
        }
    }
    
    [HarmonyPatch(typeof(BehFamilyGroupJoin), nameof(BehFamilyGroupJoin.execute))]
    static class FamilyGroupJoinPatch
    {
        static void Postfix(Actor pActor, BehResult __result)
        {
            if(__result == BehResult.Continue && pActor.hasFamily())
                TolUtil.ChangeIntimacyHappinessBy(pActor, 50);
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.die))]
    static class ActorDiePatch
    {
        static void Prefix(Actor __instance)
        {
            if (__instance.isAlive())
            {
                var lover = __instance.lover;
                if(lover != null)
                    TolUtil.ChangeIntimacyHappinessBy(lover, -40);
                var bestFriend = __instance.getBestFriend();
                if(bestFriend != null)
                    TolUtil.ChangeIntimacyHappinessBy(bestFriend, -20);
                var parents = __instance.getParents();
                foreach(var parent in parents)
                {
                    TolUtil.ChangeIntimacyHappinessBy(parent, -15);
                }

                var family = __instance.family;
                if (family != null)
                {
                    foreach(var unit in family.getUnits())
                        TolUtil.ChangeIntimacyHappinessBy(unit, -5);
                }
            }
        }
    }
}