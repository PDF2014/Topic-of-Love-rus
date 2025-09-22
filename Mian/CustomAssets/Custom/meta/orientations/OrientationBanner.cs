using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

public class OrientationBanner : BannerGeneric<Orientation, OrientationData>
{
    public override MetaType meta_type => Orientation.MetaType;

    public override string tooltip_id => "orientation";

    public override void setupBanner()
    {
        base.setupBanner();
        Culture culture = this.meta_object.getCulture();
        this.part_background.sprite = culture.getDecorSprite();
        this.part_icon.sprite = culture.getElementSprite();
        ColorAsset color = culture.getColor();
        Color tColorMain2 = color.getColorMainSecond();
        Color tColorIcon = color.getColorBanner();
        tColorMain2 = Color.Lerp(tColorMain2, Color.black, 0.05f);
        tColorIcon = Color.Lerp(tColorIcon, Color.black, 0.05f);
        this.part_background.color = tColorMain2;
        this.part_icon.color = tColorIcon;
    }

    public override TooltipData getTooltipData()
    {
        ExtendedToolTipData data = new ExtendedToolTipData()
        {
            custom_data_bool = new CustomDataContainer<bool>()
            {
                ["tab_banner"] = this.enable_tab_show_click
            }
        };
        data.orientation = this.meta_object;
        return data;
    }
}