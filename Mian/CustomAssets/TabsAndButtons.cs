using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;

public class TabsAndButtons
{
    private static PowersTab _modTab;

    private static float _nextX = 120f;
    private static float _nextY = 18f;
    private static int _timesAddedButton;
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
        _timesAddedButton++;
        PowerButtonCreator.AddButtonToTab(
            PowerButtonCreator.CreateGodPowerButton(id, SpriteTextureLoader.getSprite("ui/Icons/"+AssetManager.powers.get(id).path_icon)),
                _modTab, new Vector2(_nextX, _nextY));
        _nextY = _nextY == 18f ? -18f : 18f;

        if (_timesAddedButton % 2 == 0)
            _nextX += 36f;
    }
}