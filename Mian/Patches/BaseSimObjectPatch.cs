using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(BaseSimObject))]
public class BaseSimObjectPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(BaseSimObject.canAttackTarget))]
    static void CanAttackTarget(BaseSimObject pTarget, ref bool __result, BaseSimObject __instance)
    {
        if (__instance.isActor() && __instance.a.lover == pTarget)
        {
            __result = false;
        }
    }
}