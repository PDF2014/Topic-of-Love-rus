namespace Topic_of_Love.Mian.CustomAssets;

public class BaseStatAssets
{
    public static void Init()
    {
        Add(new BaseStatAsset
        {
            id = "intimacy_happiness",
            actor_data_attribute = true,
            normalize = true,
            normalize_min = -100,
            normalize_max = 100,
        });
        Add(new BaseStatAsset
        {
            id = "multiplier_intimacy_happiness",
            show_as_percents = true,
            multiplier = true,
            main_stat_to_multiply = "intimacy_happiness",
            tooltip_multiply_for_visual_number = 100f,
            translation_key = "intimacy_happiness"
        });
    }

    public static void Add(BaseStatAsset asset)
    {
        AssetManager.base_stats_library.add(asset);
    }
}