namespace Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

public class OrientationWindow : WindowMetaGeneric<Orientation, OrientationData>
{
    public override MetaType meta_type => Orientation.MetaType;

    public override Orientation meta_object => TOLSelectedMetas.selected_orientation;

    public override void showTopPartInformation()
    {
        base.showTopPartInformation();
        if (meta_object == null)
            return;
        
    }
}