using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.CustomAssets;

public class WorldStatisticAssets
{
    public static void Init()
    {
        Add(new StatisticsAsset
        {
            id = "world_statistics_lonely",
            path_icon = "ui/Icons/status/broke_up",
            is_world_statistics = true,
            list_window_meta_type = MetaType.Unit,
            long_action = _ => World.world.world_object.countLonely(),
            world_stats_tabs = WorldStatsTabs.Noosphere
        });

        // statistics for break up and cheating in da future?
        
        for (int i = 0; i <= 1; i++)
        {
            var isSexual = i == 0;
            foreach (var orientation in Orientation.Orientations)
            {
                Add(new StatisticsAsset
                {
                    id = "world_statistics_" + (isSexual ? orientation.OrientationType : orientation.OrientationType + "_romantic"),
                    localized_key = "statistics_" + (isSexual ? orientation.OrientationType : orientation.OrientationType +  "_romantic"),
                    localized_key_description = "statistics_" + orientation.OrientationType + "_description",
                    path_icon = "ui/Icons/" + (isSexual ? orientation.SexualPathIcon : orientation.RomanticPathIcon),
                    is_world_statistics = true,
                    list_window_meta_type = MetaType.Unit,
                    long_action = _ => World.world.world_object.countOrientation(orientation.OrientationType, isSexual),
                    world_stats_tabs = WorldStatsTabs.Noosphere
                }, false);
            }
        }
    }

    static void Add(StatisticsAsset asset, bool handleLocales=true)
    {
        if (handleLocales)
        {
            asset.localized_key = asset.id.Substring(asset.id.IndexOf("_") + 1);
            asset.localized_key_description = asset.id.Substring(asset.id.IndexOf("_") + 1) + "_description";   
        }
        AssetManager.statistics_library.add(asset);
    }
}