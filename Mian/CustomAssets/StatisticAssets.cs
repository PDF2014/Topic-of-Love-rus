using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.CustomAssets;

public class StatisticAssets
{
    public static void Init()
    {
        Add(new StatisticsAsset
        {
            id = "statistics_lonely",
            path_icon = "ui/Icons/status/okay_sex",
            is_world_statistics = true,
            long_action = _ => World.world.world_object.countLonely(),
            world_stats_tabs = WorldStatsTabs.Noosphere
        });
        
        Add(new StatisticsAsset
        {
            id = "statistics_broke_up",
            path_icon = "ui/Icons/status/broke_up",
            is_world_statistics = true,
            long_action = _ => World.world.countBrokenUp(),
            world_stats_tabs = WorldStatsTabs.Noosphere
        });
        
        Add(new StatisticsAsset
        {
            id = "statistics_cheated_on",
            path_icon = "ui/Icons/status/cheated_on",
            is_world_statistics = true,
            long_action = _ => World.world.countCheated(),
            world_stats_tabs = WorldStatsTabs.Noosphere
        });
        
        Add(new StatisticsAsset
        {
            id = "statistics_adopted_baby",
            path_icon = "ui/Icons/status/adopted_baby",
            is_world_statistics = true,
            long_action = _ => World.world.countAdoptedBaby(),
            world_stats_tabs = WorldStatsTabs.Noosphere
        });


        // statistics for break up and cheating in da future?
        
        for (int i = 0; i <= 1; i++)
        {
            var isSexual = i == 0;
            foreach (var orientation in _Orientation.RegisteredOrientations.Values)
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