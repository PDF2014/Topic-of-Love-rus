namespace Topic_of_Love.Mian.CustomAssets;

public class BaseStatAssets
{
    public static void Init()
    {
        Add(new BaseStatAsset
        {
            id = "intimacy_happiness",
            icon = "ui/Icons/god_powers/force_lover",
            actor_data_attribute = true,
            normalize = true,
            normalize_min = -100,
            normalize_max = 100,
        });
    }

    public static void Add(BaseStatAsset asset)
    {
        AssetManager.base_stats_library.add(asset);
    }
}