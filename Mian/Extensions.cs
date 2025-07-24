using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian;

public static class Extensions
{
    
    // this method may be a bit confusing but it's to determine if actors can get pregnant based on their genitalia and if they have eggs
    public static bool IsAbleToBecomePregnant(this Actor pActor)
    {
        if (TolUtil.IsTOIInstalled())
        {
            // TOI compatibility
        }
        
        // if (TolUtil.NeedSameSexTypeForReproduction(pActor) || TolUtil.CanDoAnySexType(pActor))
        //     return true;
        // if (TolUtil.NeedDifferentSexTypeForReproduction(pActor))
        //     return Preferences.HasVulva(pActor);
        return pActor.HasVulva(); // vulva required for pregnancy
    }

    public static float getIntimacy(this Actor actor)
    {
        actor.data.get("intimacy_happiness", out float intimacy);
        return intimacy;
    }

    public static bool isIntimacyHappinessEnough(this Actor actor, float happiness)
    {
        actor.data.get("intimacy_happiness", out float compare);
        return compare >= happiness;
    }
        
    public static void changeIntimacyHappiness(this Actor actor, float happiness)
    {
        actor.data.get("intimacy_happiness", out float init);
        actor.data.set("intimacy_happiness", Math.Max(-100, Math.Min(happiness + init, 100)));
    }

    public static void increaseCheated<TData>(this CoreSystemObject<TData> instance) where TData : BaseSystemData
    {
        instance.data.get("cheated_on_amount", out int amount);
        instance.data.set("cheated_on_amount", amount + 1);
    }
    
    public static void increaseBrokenUp<TData>(this CoreSystemObject<TData> instance) where TData : BaseSystemData
    {
        instance.data.get("broken_up_amount", out int amount);
        instance.data.set("broken_up_amount", amount + 1);
    }
    
    public static void increaseAdoptedBaby<TData>(this CoreSystemObject<TData> instance) where TData : BaseSystemData
    {
        instance.data.get("broken_up_amount", out int amount);
        instance.data.set("broken_up_amount", amount + 1);
    }
    
    public static void increaseCheated(this MapBox instance)
    {
        instance.map_stats.custom_data.get("cheated_on_amount", out int amount);
        instance.map_stats.custom_data.set("cheated_on_amount", amount + 1);
    }
    
    public static void increaseBrokenUp(this MapBox instance)
    {
        instance.map_stats.custom_data.get("broken_up_amount", out int amount);
        instance.map_stats.custom_data.set("broken_up_amount", amount + 1);
    }
    
    public static void increaseAdoptedBaby(this MapBox instance)
    {
        instance.map_stats.custom_data.get("broken_up_amount", out int amount);
        instance.map_stats.custom_data.set("broken_up_amount", amount + 1);
    }

    public static int countCheated<TData>(this CoreSystemObject<TData> instance) where TData : BaseSystemData
    {
        instance.data.get("cheated_on_amount", out int amount);
        return amount;
    }
    
    public static int countBrokenUp<TData>(this CoreSystemObject<TData> instance) where TData : BaseSystemData
    {
        instance.data.get("broken_up_amount", out int amount);
        return amount;
    }
    
    public static int countAdoptedBaby<TData>(this CoreSystemObject<TData> instance) where TData : BaseSystemData
    {
        instance.data.get("adopted_amount", out int amount);
        return amount;
    }
    
    public static int countCheated(this MapBox box)
    {
        box.map_stats.custom_data.get("cheated_on_amount", out int cheatedAmount);
        return cheatedAmount;
    }

    public static int countBrokenUp(this MapBox box)
    {
        box.map_stats.custom_data.get("broken_up_amount", out int brokenUpCount);
        return brokenUpCount;
    }
    
    public static int countAdoptedBaby(this MapBox box)
    {
        box.map_stats.custom_data.get("adopted_amount", out int adoptedCount);
        return adoptedCount;
    }

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
        return instance.getUnits().LongCount(unit => unit.getIntimacy() < 0 && TolUtil.AffectedByIntimacy(unit));
    }
    public static long countLonely(this WorldObject world)
    {
        return world.units.LongCount(unit => unit.getIntimacy() < 0 && TolUtil.AffectedByIntimacy(unit));
    }

    public static void showSplitPopulationByOrientation<TMetaObject, TData>(this WindowMetaGeneric<TMetaObject, TData> instance, ICollection<Actor> pListWithUnits, bool sexual)
    where TMetaObject : CoreSystemObject<TData>
    where TData : BaseSystemData
    {
        Dictionary<Orientation, int> dictionary = UnsafeCollectionPool<Dictionary<Orientation, int>, KeyValuePair<Orientation, int>>.Get();
        var unitsCount = pListWithUnits.Count;
        foreach (Actor pListWithUnit in pListWithUnits)
        {
            var orientation = Orientations.GetOrientationForActorBasedOnCriteria(pListWithUnit, sexual);
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