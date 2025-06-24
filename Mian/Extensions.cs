using System;
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

    public static void showSplitPopulationByOrientation<TMetaObject, TData>(this WindowMetaGeneric<TMetaObject, TData> instance, ICollection<Actor> pListWithUnits, bool sexual)
    where TMetaObject : CoreSystemObject<TData>
    where TData : BaseSystemData
    {
        Dictionary<Orientation, int> dictionary = UnsafeCollectionPool<Dictionary<Orientation, int>, KeyValuePair<Orientation, int>>.Get();
        var unitsCount = pListWithUnits.Count;
        foreach (Actor pListWithUnit in pListWithUnits)
        {
            var orientation = Orientations.GetOrientationFromActor(pListWithUnit, sexual);
            if (!dictionary.ContainsKey(orientation))
                dictionary.Add(orientation, 0);
            dictionary[orientation]++;
        }
        foreach (KeyValuePair<Orientation, int> keyValuePair in dictionary.OrderByDescending((kv => kv.Value)))
        {
            var key = keyValuePair.Key;
            int num2 = keyValuePair.Value;
            float pFloat = unitsCount > 0 ? (float) ((double) num2 / unitsCount * 100.0) : 0.0f;
            if (unitsCount == num2)
                pFloat = 100f;
            string pValue = $"[{num2}] {pFloat.ToText()}%";
            instance.showStatRow(
                sexual ? key.SexualPathLocale : key.RomanticPathLocale, 
                pValue, 
                key.HexCode, 
                MetaType.None, 
                -1, 
                true, 
                key.GetPathIcon(sexual, false));
        }
        UnsafeCollectionPool<Dictionary<Orientation, int>, KeyValuePair<Orientation, int>>.Release(dictionary);
    }
    
    public static Orientation getMainOrientation(this Kingdom kingdom, bool sexual)
    {
        if (kingdom.hasKing())
            return Orientation.GetOrientation(kingdom.king, sexual);
        return kingdom.units.Count == 0 ? null : Orientation.GetOrientation(kingdom.units[0], sexual);
    }
    
    public static Orientation getMainOrientation(this City city, bool sexual)
    {
        if (city.hasLeader())
            return Orientation.GetOrientation(city.leader, sexual);
        return city.getPopulationPeople() == 0 ? null : Orientation.GetOrientation(city.units[0], sexual);
    }
    
    public static Orientation getFounderOrientation(this Kingdom kingdom)
    {
        kingdom.data.get("founder_orientation", out string id);
        return Orientation.GetOrientation(id);
    }
    
    public static Orientation getFounderOrientation(this City city)
    {
        city.data.get("founder_orientation", out string id);
        return Orientation.GetOrientation(id);
    }
}