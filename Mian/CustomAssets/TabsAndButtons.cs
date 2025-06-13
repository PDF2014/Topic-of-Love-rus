using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;

public class TabsAndButtons
{
    private static PowersTab _modTab;
    public static void Init()
    { 
        _modTab = TabManager.CreateTab(
            "Tab_TOL", 
            "tab_title_tol", 
            "tab_desc_tol", 
            SpriteTextureLoader.getSprite("ui/Icons/tabs/tab_tol"));
        
        AddButton("forceLover");
        AddButton("forceBreakup");
        AddButton("forceSex");
        AddButton("forceKiss");
        AddButton("forceSexualIVF");
        AddButton("forceDate");
    }

    private static void AddButton(string id)
    {
        PowerButtonCreator.AddButtonToTab(
            PowerButtonCreator.CreateGodPowerButton(id, SpriteTextureLoader.getSprite("ui/Icons/"+AssetManager.powers.get(id).path_icon)),
                _modTab);
    }
}