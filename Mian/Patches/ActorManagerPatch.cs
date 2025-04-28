using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class ActorManagerPatch
{
    [HarmonyPatch(typeof(ActorManager), nameof(ActorManager.createNewUnit))]
    class ActorFinalizePatch
    {
        static void Postfix(Actor __result)
        {
            if (__result != null)
            {
                var preferences =  PreferenceTraits.GetRandomPreferences(__result);
                foreach (var trait in preferences)
                {
                    __result.addTrait(trait);
                }
                PreferenceTraits.CreateOrientations(__result);   
            }
        }
    }
}