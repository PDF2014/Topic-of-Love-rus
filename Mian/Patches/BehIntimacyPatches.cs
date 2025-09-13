using ai.behaviours;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch]
public class BehIntimacyPatches
{
    [HarmonyPatch(typeof(BehChildFindRandomFamilyParent), nameof(BehChildFindRandomFamilyParent.execute))]
    static class ChildFindRandomFamilyParentPatch
    {
        static void Postfix(Actor pBabyActor, BehResult __result)
        {
            if(__result == BehResult.Continue)
                pBabyActor.changeIntimacyHappinessBy(10);
        }
    }

    [HarmonyPatch(typeof(BehFamilyFollowAlpha), nameof(BehFamilyFollowAlpha.execute))]
    static class FamilyFollowAlphaPatch
    {
        static void Postfix(Actor pActor, BehResult __result)
        {
            if(__result == BehResult.Continue)
                pActor.changeIntimacyHappinessBy(10);
        }
    }
    
    [HarmonyPatch(typeof(BehFamilyGroupJoin), nameof(BehFamilyGroupJoin.execute))]
    static class FamilyGroupJoinPatch
    {
        static void Postfix(Actor pActor, BehResult __result)
        {
            if(__result == BehResult.Continue && pActor.hasFamily())
                pActor.changeIntimacyHappinessBy(25);
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
                    lover.changeIntimacyHappinessBy(-30);
                var bestFriend = __instance.getBestFriend();
                if(bestFriend != null)
                    bestFriend.changeIntimacyHappinessBy(-15);
                var parents = __instance.getParents();
                foreach(var parent in parents)
                {
                    parent.changeIntimacyHappinessBy(-10);
                }

                var family = __instance.family;
                if (family != null)
                {
                    foreach(var unit in family.getUnits())
                        unit.changeIntimacyHappinessBy(-5);
                }
            }
        }
    }
}