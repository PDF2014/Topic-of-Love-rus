using Topic_of_Love.Mian.CustomAssets.Custom;
using Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;
using UnityEngine;
using UnityEngine.UI;

public class OrientationListElement : WindowListElementBase<Orientation, OrientationData>
{
	public override void show(Orientation orientation)
	{
		base.show(orientation);
		this._text_name.text = orientation.name;
		Color tColor = orientation.getColor().getColorText();
		this._text_name.color = tColor;
		// this._amount.setValue(orientation.countUnits(), "");
		// this._age.setValue(orientation.getAge(), "");
		// this._renown.setValue(orientation.getRenown(), "");
		// this._kills.setValue((int)orientation.getTotalKills(), "");
		// this._deaths.setValue((int)orientation.getTotalDeaths(), "");
	}

	// Token: 0x060033B8 RID: 13240 RVA: 0x00183D8D File Offset: 0x00181F8D
	public override void initMonoFields()
	{
	}

	// Token: 0x060033B9 RID: 13241 RVA: 0x00183D8F File Offset: 0x00181F8F
	public override void loadBanner()
	{
		this._orientation_banner.load(this.meta_object);
	}

	// Token: 0x060033BA RID: 13242 RVA: 0x00183DA2 File Offset: 0x00181FA2
	public override void tooltipAction()
	{
		Tooltip.show(this, "orientation", new ExtendedToolTipData()
		{
			orientation = this.meta_object
		});
	}

	// Token: 0x060033BB RID: 13243 RVA: 0x00183DC0 File Offset: 0x00181FC0
	public override ActorAsset getActorAsset()
	{
		return this.meta_object.getActorAsset();
	}

	public Text _text_name;

	private OrientationBanner _orientation_banner;
}
