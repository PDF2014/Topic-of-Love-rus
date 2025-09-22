using Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

namespace Topic_of_Love.Mian.CustomAssets.Custom.meta;

public class MetaManagers
{
    public static OrientationManager orientations;

    public static void Init()
    {
        MapBox.instance._list_meta_main_managers.Add(orientations = new OrientationManager());
        MapBox.instance.list_all_sim_managers.Add(orientations);
    }
}