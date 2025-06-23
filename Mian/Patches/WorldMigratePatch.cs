using System.Collections.Generic;
using System.Linq;
using db;
using HarmonyLib;
using Topic_of_Love.Mian.CustomAssets;

namespace Topic_of_Love.Mian.Patches;

[HarmonyPatch(typeof(CanvasMain))]
public class WorldMigratePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CanvasMain.setMainUiEnabled))]
    static void OnWorldLoad(bool pEnabled)
    {
        // if (pEnabled)
        // {
        //     TolUtil.LogInfo("The world has loaded! Checking to see if old data needs migrating..");
        //     var db = DBManager.getSyncConnection();
        //
        //     var commands = new List<string>();
        //     foreach (var interval in HistoryMetaDataAssets.Intervals)
        //     {
        //         var intervalName = interval.Key;
        //         foreach (var metaAsset in HistoryMetaDataAssets.Collectors)
        //         {
        //             var asset = metaAsset.Key;
        //             var tableName = asset.Substring(0, 1).ToUpper() + asset.Substring(1) + "Yearly" + intervalName;
        //             var tableInfo = db.GetTableInfo(tableName);
        //             
        //             foreach (var missingColumn in metaAsset.Value.Keys.Where(name => !tableInfo.Any(c => c.Name.Equals(name))))
        //             {
        //                 commands.Add($"ALTER TABLE {tableName} ADD COLUMN {missingColumn} INT;");
        //             }
        //         }
        //     }
        //     
        //     if(commands.Count > 0)
        //         TolUtil.LogInfo("This world needs migration! Updating it..");
        //     
        //     db.RunInTransaction(() =>
        //     {
        //         foreach (var command in commands)
        //         {
        //             db.Execute(command);
        //         }
        //         TolUtil.LogInfo("Migrated world data :)");
        //     });
        // }
    }
}