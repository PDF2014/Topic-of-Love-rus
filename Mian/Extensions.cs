using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian;

public static class Extensions
{
    public static int countOrientation(this WorldObject _, string id, bool sexual)
    {
        return World.world.units
            .Count(actor => actor.isAlive() && Orientations.HasOrientation(actor, id, sexual));
    }
    public static int countOrientation<T>(this MetaObject<T> instance, string id, bool sexual) where T : MetaObjectData
    {
        return instance.getUnits().Count(unit => unit.isAlive() && Orientations.HasOrientation(unit, id, sexual));
    }

    public static int countOrientation(this Alliance instance, string id, bool sexual)
    {
        return instance.kingdoms_list.Sum(kingdom => kingdom.countOrientation(id, sexual));
    }
}