namespace Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

public class Orientation : MetaObject<OrientationData>
{
    public const MetaType OrientationMetaType = (MetaType) 295;
    public override MetaType meta_type => OrientationMetaType;

    private Culture _culture;

    public override void loadData(OrientationData pData)
    {
        base.loadData(pData);
        _culture = World.world.cultures.get(pData.id_culture);
    }

    public override void save()
    {
        base.save();
        data.id_culture = _culture.data.id;
    }

    public void setCulture(Culture culture)
    {
        _culture = culture;
    }

    public override void Dispose()
    {
        base.Dispose();
        _culture = null;
    }

    public bool hasCulture()
    {
        return !_culture.isRekt();
    }

    public Culture getCulture()
    {
        return _culture;
    }
}