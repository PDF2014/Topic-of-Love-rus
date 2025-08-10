using ai;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(ActorTool))]
public class ActorToolPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)] // we're overwriting the method so let's try to let others do what they want
    [HarmonyPatch(nameof(ActorTool.checkFallInLove))]
    public static bool FallInLovePatch(Actor pActor, Actor pTarget)
    {
        if (pActor.lover == pTarget)
            return false;
        if (!pActor.canFallInLoveWith(pTarget))
            return false;
        if(pActor.hasLover() && Randy.randomBool())
            pActor.BreakUp(false);
        if(pTarget.hasLover() && Randy.randomBool())
            pTarget.BreakUp(false);
        pActor.becomeLoversWith(pTarget);
        return false;
    }
}