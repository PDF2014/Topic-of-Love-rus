using System.Collections;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

public class OrientationStatsElement : OrientationElement, IStatsElement, IRefreshElement
{
    public void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
    {
        this._stats_icons.setIconValue(pName, pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
    }

    // Token: 0x060033D0 RID: 13264 RVA: 0x001840A2 File Offset: 0x001822A2
    public override void Awake()
    {
        this._stats_icons = base.gameObject.AddOrGetComponent<StatsIconContainer>();
        base.Awake();
    }

    // Token: 0x060033D1 RID: 13265 RVA: 0x001840BB File Offset: 0x001822BB
    public override IEnumerator showContent()
    {
        if (base.orientation == null)
        {
            yield break;
        }
        if (!base.orientation.isAlive())
        {
            yield break;
        }
        this._stats_icons.showGeneralIcons<Orientation, OrientationData>(base.orientation);
        // this.setIconValue("i_army_size", (float)base.army.countUnits(), null, "", false, "", '/');
        // this.setIconValue("i_kills", (float)base.army.getTotalKills(), null, "", false, "", '/');
        // this.setIconValue("i_melee", (float)base.army.countMelee(), null, "", false, "", '/');
        // this.setIconValue("i_range", (float)base.army.countRange(), null, "", false, "", '/');
        yield break;
    }

    // Token: 0x060033D3 RID: 13267 RVA: 0x001840D2 File Offset: 0x001822D2
    private GameObject gameObject => base.gameObject;

    // Token: 0x0400272D RID: 10029
    private StatsIconContainer _stats_icons;
}