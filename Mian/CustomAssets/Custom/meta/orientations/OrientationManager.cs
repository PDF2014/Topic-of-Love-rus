namespace Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

public class OrientationManager : MetaSystemManager<Orientation, OrientationData>
{
    public OrientationManager()
    {
        this.type_id = Orientation.OrientationMetaType.AsString();
    }

    public Orientation newOrientation(Culture pCulture)
    {
        Orientation orientation = base.newObject();
        orientation.setCulture(pCulture);
        return orientation;
    }

    public override void update(float pElapsed)
    {
        base.update(pElapsed);
    }

    public override void updateDirtyUnits()
    {
        World.world.units.units_only_alive.ForEach((actor) =>
        {
            
        });
    }
}