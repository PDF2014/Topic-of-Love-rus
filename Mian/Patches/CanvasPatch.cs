using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(CanvasMain))]
public class CanvasPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CanvasMain.setMainUiEnabled))]
    public static void WorldLoaded(bool pEnabled)
    {
        if (pEnabled)
        {
            TolUtil.LogInfo("The new world has loaded! Time to sneak in our data...");
        }
    }
}