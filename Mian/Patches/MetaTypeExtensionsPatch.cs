using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

namespace Topic_of_Love.Mian.Patches;

public class MetaTypeExtensionsPatch
{
    [HarmonyPatch(typeof(MetaTypeExtensions), nameof(MetaTypeExtensions.AsString))]
    [HarmonyPrefix]
    public static bool AsString(MetaType pType, ref string __result)
    {
        if (pType == Orientation.OrientationMetaType)
        {
            __result = "orientation";
            return false;
        }

        return true;
    }
}