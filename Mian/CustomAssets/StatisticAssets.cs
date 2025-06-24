using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.CustomAssets;

public class StatisticAssets
{
    public static void Init()
    {
        Add(new StatisticsAsset
        {
            id = "statistics_lonely",
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
            foreach (var orientation in Orientation.Orientations.Values)
            {
                Add(new StatisticsAsset
                {
                    id = "statistics_" + (isSexual ? orientation.OrientationType : orientation.OrientationType + "_romantic"),
                    path_icon = orientation.GetPathIcon(isSexual, true),
                    is_world_statistics = true,
                    list_window_meta_type = MetaType.Unit,
                    long_action = _ => World.world.world_object.countOrientation(orientation.OrientationType, isSexual),
                    world_stats_tabs = WorldStatsTabs.Noosphere
                });
            }
        }
    }

    static void Add(StatisticsAsset asset)
    {
        AssetManager.statistics_library.add(asset);
    }
}