namespace Topic_of_Love.Mian.CustomAssets;

public class WorldLawAssets
{
    public static void Init()
    {
        AssetManager.world_laws_library.add(new WorldLawAsset
        {
            id = "world_law_fluid_sexuality",
            group_id = "units",
            icon_path = "ui/Icons/actor_traits/unfluid",
            default_state = true
        });
    }

}