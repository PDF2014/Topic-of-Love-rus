using NeoModLoader.General;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian.CustomAssets;

public class TooltipAssets
{
    public static void Init()
    {
        Add(new ()
        {
            id = "like_neuron",
            callback = ShowLikeNeuron,
            callback_text_animated = ShowLikeNeuron
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
        pTooltip.setBottomDescription(like.Description2);
        bool flag = actor.HasLike(like);
        pTooltip.addLineText("neuron_state", flag ? LocalizedTextManager.getText("neuron_active") : LocalizedTextManager.getText("neuron_silenced"), flag ? "#43FF43" : "#FB2C21");
        pTooltip.addLineBreak();
        pTooltip.addLineText("like_group", like.LikeAsset.LikeGroup.Title, like.LikeAsset.LikeGroup.HexCode);
        pTooltip.addLineText("intimate", LM.Get(like.LoveType.ToString().ToLower()));
    }

}