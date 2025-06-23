using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian;

public static class Extensions
{
    public static long countOrientation(this WorldObject world, string id, bool sexual)
    {
        return world.units
            .LongCount(actor => actor.isAlive() && Orientations.HasOrientation(actor, id, sexual));
    }
    public static long countOrientation(this IMetaObject instance, string id, bool sexual)
    {
        return instance.getUnits().LongCount(unit => unit.isAlive() && Orientations.HasOrientation(unit, id, sexual));
    }

    public static long countOrientation(this Alliance instance, string id, bool sexual)
    {
        return instance.kingdoms_list.Sum(kingdom => kingdom.countOrientation(id, sexual));
    }
    public static long countLonely(this IMetaObject instance)
    {
        return instance.getUnits().LongCount(unit => TolUtil.GetIntimacy(unit) < 0 && TolUtil.AffectedByIntimacy(unit));
    }
    public static long countLonely(this WorldObject world)
    {
        return world.units.LongCount(unit => TolUtil.GetIntimacy(unit) < 0 && TolUtil.AffectedByIntimacy(unit));
    }
}