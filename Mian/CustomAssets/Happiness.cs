using HarmonyLib;

namespace Topic_of_Love.Mian.CustomAssets
{
    public class Happiness
    {
        public static void Init()
        {
            // breakup
            Add(new HappinessAsset
            {
                id = "breakup",
                value = -15,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = true,
                pot_amount = 5,
            });
            
            // cheated on
            Add(new HappinessAsset
            {
                id = "cheated_on",
                value = -20,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = true,
                pot_amount = 5,
                show_change_happiness_effect = true
            });
            
            // true self
            Add(new HappinessAsset
            {
                id = "true_self",
                value = 20,
                pot_task_id = "singing",
                path_icon = "ui/Icons/iconAge",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true
            });
            
            Add(new HappinessAsset
            {
                id = "orientation_fits",
                value = 10,
                pot_task_id = "singing",
                path_icon = "ui/Icons/iconAge",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "orientation_does_not_fit",
                value = -10,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "insulted_for_orientation",
                value = -20,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "enjoyed_sex",
                value = 15,
                path_icon = "ui/Icons/status/enjoyed_sex",
                ignored_by_psychopaths = false,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "disliked_sex",
                value = -15,
                path_icon = "ui/Icons/status/disliked_sex",
                ignored_by_psychopaths = false,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "okay_sex",
                value = 5,
                path_icon = "ui/Icons/status/okay_sex",
                ignored_by_psychopaths = false,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "did_not_want_baby",
                value = -25,
                path_icon = "ui/Icons/status/cheated_on",
                pot_task_id = "madness_random_emotion",
                ignored_by_psychopaths = false,
                show_change_happiness_effect = true,
            });
            
            Add(new HappinessAsset
            {
                id = "adopted_baby",
                value = 30,
                pot_task_id = "singing",
                path_icon = "ui/Icons/status/adopted_baby",
                pot_amount = 1,
                ignored_by_psychopaths = true,
                show_change_happiness_effect = true,
            });
            
            Add(new HappinessAsset
            {
                id = "went_on_date",
                value = 15,
                pot_task_id = "singing",
                path_icon = "ui/Icons/status/went_on_date",
                pot_amount = 1,
                ignored_by_psychopaths = true,
                show_change_happiness_effect = true,
                dialogs_amount = 5
            });
        }

        private static void Add(HappinessAsset asset)
        {
            AssetManager.happiness_library.add(asset);
            asset.index = AssetManager.happiness_library.list.Count-1;
        }
    }
}