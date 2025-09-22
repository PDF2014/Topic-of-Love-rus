using System.Collections.Generic;
using Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

namespace Topic_of_Love.Mian.CustomAssets.Custom.meta;

public class MetaTypes
{
    private static MetaTypeLibrary library = AssetManager.meta_type_library;
    
    public static void Init()
    {
        Add(new MetaTypeAsset
        {
            id = "orientation",
            window_name = "orientation",
            power_tab_id = "selected_orientation", // gotta add a power tab for this :(
            force_zone_when_selected = true,
            set_icon_for_cancel_button = true,
            icon_list = "iconOrientationList",
            icon_single_path = "ui/icons/iconOrientation",
            window_action_clear = (MetaTypeAction) (() => TOLSelectedMetas.selected_orientation = (Orientation) null),
            window_history_action_update = (MetaTypeHistoryAction) ((ref WindowHistoryData pHistoryData) => {}),
            window_history_action_restore = (MetaTypeHistoryAction) ((ref WindowHistoryData pHistoryData) => {}),
            get_list = (MetaTypeListAction) (() => (IEnumerable<NanoObject>) MetaManagers.orientations),
            has_any = (MetaTypeListHasAction) (() => MetaManagers.orientations.hasAny()),
            get_selected = (MetaSelectedGetter) (() => (NanoObject) TOLSelectedMetas.selected_orientation),
            set_selected = (MetaSelectedSetter) (pElement => TOLSelectedMetas.selected_orientation = pElement as Orientation),
            get = (MetaGetter) (pId => (NanoObject) MetaManagers.orientations.get(pId)),
            map_mode = Orientation.MetaType,
            option_id = "map_orientation_layer",
            power_option_zone_id = "orientation_layer",
            has_dynamic_zones = true,
            dynamic_zone_option = 0,
            click_action_zone = new MetaZoneClickAction(ActionLibrary.inspectArmy),
            selected_tab_action_meta = new MetaTypeActionAsset(library.defaultClickActionZone),
            check_unit_has_meta = (MetaCheckUnitWindowAction) (pActor => pActor.hasArmy()),
            set_unit_set_meta_for_meta_for_window = (MetaUnitSetMetaForWindow) (pActor => SelectedMetas.selected_army = pActor.army),
            // reports = new string[2]{ "happy", "unhappy" };
            draw_zones = (MetaZoneDrawAction) (pMetaTypeAsset =>
            {
                if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
                    library.drawDefaultFluid(pMetaTypeAsset);
                else
                    library.drawDefaultMeta(pMetaTypeAsset);
            })
        });
    }

    private static void Add(MetaTypeAsset asset)
    {
        AssetManager.meta_type_library.add(asset);
    }
}