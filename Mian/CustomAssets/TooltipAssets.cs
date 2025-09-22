using System.Drawing;
using NeoModLoader.General;
using Topic_of_Love.Mian.CustomAssets.Custom;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Topic_of_Love.Mian.CustomAssets;

public class TooltipAssets
{
    private static TooltipLibrary library = AssetManager.tooltips;
    public static void Init()
    {
        Add(new ()
        {
            id = "like_neuron",
            callback = ShowLikeNeuron,
            callback_text_animated = ShowLikeNeuron
        });
        
        Add(new ()
        {
            id = "orientation",
            prefab_id = "tooltips/tooltip_army",
            callback = new TooltipShowAction(library.showArmy)
        });
    }

    public static void Add(TooltipAsset asset)
    {
        AssetManager.tooltips.add(asset);
    }
    
    public static void ShowLikeNeuron(Tooltip pTooltip, string pType, TooltipData pData)
    {
        var extended = (ExtendedToolTipData) pData;
        
        LikeNeuronElement neuron = extended.like_neuron;
        Like like = neuron.like;
        Actor actor = neuron.actor;

        pTooltip.clearTextRows();   
        pTooltip.setTitle(like.Title, "neuron", like.LikeAsset.LikeGroup.HexCode);

        pTooltip.setDescription(like.Description);
        bool flag = actor.HasLike(like);
        pTooltip.addLineText("neuron_state", flag ? LocalizedTextManager.getText("neuron_active") : LocalizedTextManager.getText("neuron_silenced"), flag ? "#43FF43" : "#FB2C21");
        pTooltip.addLineBreak();
        pTooltip.addLineText("like_group", like.LikeAsset.LikeGroup.Title, like.LikeAsset.LikeGroup.HexCode);
        pTooltip.addLineText("intimate", LM.Get(like.LoveType.ToString().ToLower()), LikesManager.GetHexCodeForLoveType(like.LoveType));
        pTooltip.setBottomDescription(like.Description2);

        var species = pTooltip.transform.FindRecursive("IconSpecies");
        if (species == null)
        {
            var IconSpecies = new GameObject();
            IconSpecies.transform.SetParent(pTooltip._headline.transform);
            IconSpecies.name = "IconSpecies";
            IconSpecies.AddComponent<Image>();
            IconSpecies.transform.localPosition = new Vector3(-45, -11);
            IconSpecies.transform.localScale = new Vector3(0.2f, 0.2f);
        }
        else
        {
            species.transform.localPosition = new Vector3(-45, -11);
        }
        pTooltip.setSpeciesIcon(neuron.like.GetSprite());
    }

}