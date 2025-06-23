using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using db;
using db.tables;
using NeoModLoader.General.Game.extensions;
using SQLite;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;

public class HistoryMetaDataAssets
{
    private static readonly Dictionary<string, HistoryInterval> Intervals = new()
    {
        ["1"] = HistoryInterval.Yearly1,
        ["5"] = HistoryInterval.Yearly5,
        ["10"] = HistoryInterval.Yearly10,
        ["50"] = HistoryInterval.Yearly50,
        ["100"] = HistoryInterval.Yearly100,
        ["500"] = HistoryInterval.Yearly500,
        ["1000"] = HistoryInterval.Yearly1000,
        ["5000"] = HistoryInterval.Yearly5000,
        ["10000"] = HistoryInterval.Yearly10000
    };
    
    private static readonly ModuleBuilder DynamicModule =  
        AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicTypesAssembly"), AssemblyBuilderAccess.Run)
        .DefineDynamicModule("MainModule");
    // public class KingdomTableExtras : KingdomTable
    // {
    //     [Column("lonely")] public long? lonely { get; set; }
    // }
    //
    // public class KingdomYearly1 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly5 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly10 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly100 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly500 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly1000 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly5000 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly10000 : KingdomTableExtras
    // {
    // }
    //
    // public class KingdomYearly50 : KingdomTableExtras
    // {
    // }

    public static void Init()
    {
        
        AddFields("kingdom", nano =>
        {
            var kingdom = (Kingdom) nano;
            return new(){
                ["lonely"] = ((Kingdom)nano).countLonely(),
                ["population"] = new long?((long)kingdom.countUnits()),
                adults = new long?((long)kingdom.countAdults()),
                children = new long?((long)kingdom.countChildren()),
                boats = new long?((long)kingdom.countBoats()),
                army = new long?((long)kingdom.countTotalWarriors()),
                sick = new long?((long)kingdom.countSick()),
                hungry = new long?((long)kingdom.countHungry()),
                starving = new long?((long)kingdom.countStarving()),
                happy = new long?((long)kingdom.countHappyUnits()),
                deaths = new long?(kingdom.getTotalDeaths()),
                births = new long?(kingdom.getTotalBirths()),
                kills = new long?(kingdom.getTotalKills()),
                joined = new long?(kingdom.getTotalJoined()),
                left = new long?(kingdom.getTotalLeft()),
                moved = new long?(kingdom.getTotalMoved()),
                migrated = new long?(kingdom.getTotalMigrated()),
                territory = new long?((long)kingdom.countZones()),
                buildings = new long?((long)kingdom.countBuildings()),
                homeless = new long?((long)kingdom.countHomeless()),
                housed = new long?((long)kingdom.countHoused()),
                food = new long?((long)kingdom.countTotalFood()),
                families = new long?((long)kingdom.countFamilies()),
                males = new long?((long)kingdom.countMales()),
                females = new long?((long)kingdom.countFemales()),
                cities = new long?((long)kingdom.countCities()),
                renown = new long?((long)kingdom.getRenown()),
                money = new long?((long)kingdom.countTotalMoney()),
                deaths_eaten = new long?(kingdom.getDeaths(AttackType.Eaten)),
                deaths_hunger = new long?(kingdom.getDeaths(AttackType.Starvation)),
                deaths_natural = new long?(kingdom.getDeaths(AttackType.Age)),
                deaths_plague = new long?(kingdom.getDeaths(AttackType.Plague)),
                deaths_poison = new long?(kingdom.getDeaths(AttackType.Poison)),
                deaths_infection = new long?(kingdom.getDeaths(AttackType.Infection)),
                deaths_tumor = new long?(kingdom.getDeaths(AttackType.Tumor)),
                deaths_acid = new long?(kingdom.getDeaths(AttackType.Acid)),
                deaths_fire = new long?(kingdom.getDeaths(AttackType.Fire)),
                deaths_divine = new long?(kingdom.getDeaths(AttackType.Divine)),
                deaths_weapon = new long?(kingdom.getDeaths(AttackType.Weapon)),
                deaths_gravity = new long?(kingdom.getDeaths(AttackType.Gravity)),
                deaths_drowning = new long?(kingdom.getDeaths(AttackType.Drowning)),
                deaths_water = new long?(kingdom.getDeaths(AttackType.Water)),
                deaths_explosion = new long?(kingdom.getDeaths(AttackType.Explosion)),
                deaths_other = new long?(kingdom.getDeaths(AttackType.Other)),
                metamorphosis = new long?(kingdom.getDeaths(AttackType.Metamorphosis)),
                evolutions = new long?(kingdom.getEvolutions())
            };
        });
        
        // var kingdom = AssetManager.history_meta_data_library.get("kingdom");
        // kingdom.table_type = typeof(KingdomTableExtras);
        // kingdom.table_types = new Dictionary<HistoryInterval, Type>()
        // {
        //     {
        //         HistoryInterval.Yearly1,
        //         typeof(KingdomYearly1)
        //     },
        //     {
        //         HistoryInterval.Yearly5,
        //         typeof(KingdomYearly5)
        //     },
        //     {
        //         HistoryInterval.Yearly10,
        //         typeof(KingdomYearly10)
        //     },
        //     {
        //         HistoryInterval.Yearly50,
        //         typeof(KingdomYearly50)
        //     },
        //     {
        //         HistoryInterval.Yearly100,
        //         typeof(KingdomYearly100)
        //     },
        //     {
        //         HistoryInterval.Yearly500,
        //         typeof(KingdomYearly500)
        //     },
        //     {
        //         HistoryInterval.Yearly1000,
        //         typeof(KingdomYearly1000)
        //     },
        //     {
        //         HistoryInterval.Yearly5000,
        //         typeof(KingdomYearly5000)
        //     },
        //     {
        //         HistoryInterval.Yearly10000,
        //         typeof(KingdomYearly10000)
        //     },
        // };
        // kingdom.collector = (HistoryDataCollector)(pNanoObject =>
        // {
        //     Kingdom kingdom = (Kingdom)pNanoObject;
        //     return (HistoryTable)new KingdomYearly1()
        //     {
        //         id = kingdom.getID(),
        //         lonely = new long?((long)kingdom.countLonely()),
        //         population = new long?((long)kingdom.countUnits()),
        //         adults = new long?((long)kingdom.countAdults()),
        //         children = new long?((long)kingdom.countChildren()),
        //         boats = new long?((long)kingdom.countBoats()),
        //         army = new long?((long)kingdom.countTotalWarriors()),
        //         sick = new long?((long)kingdom.countSick()),
        //         hungry = new long?((long)kingdom.countHungry()),
        //         starving = new long?((long)kingdom.countStarving()),
        //         happy = new long?((long)kingdom.countHappyUnits()),
        //         deaths = new long?(kingdom.getTotalDeaths()),
        //         births = new long?(kingdom.getTotalBirths()),
        //         kills = new long?(kingdom.getTotalKills()),
        //         joined = new long?(kingdom.getTotalJoined()),
        //         left = new long?(kingdom.getTotalLeft()),
        //         moved = new long?(kingdom.getTotalMoved()),
        //         migrated = new long?(kingdom.getTotalMigrated()),
        //         territory = new long?((long)kingdom.countZones()),
        //         buildings = new long?((long)kingdom.countBuildings()),
        //         homeless = new long?((long)kingdom.countHomeless()),
        //         housed = new long?((long)kingdom.countHoused()),
        //         food = new long?((long)kingdom.countTotalFood()),
        //         families = new long?((long)kingdom.countFamilies()),
        //         males = new long?((long)kingdom.countMales()),
        //         females = new long?((long)kingdom.countFemales()),
        //         cities = new long?((long)kingdom.countCities()),
        //         renown = new long?((long)kingdom.getRenown()),
        //         money = new long?((long)kingdom.countTotalMoney()),
        //         deaths_eaten = new long?(kingdom.getDeaths(AttackType.Eaten)),
        //         deaths_hunger = new long?(kingdom.getDeaths(AttackType.Starvation)),
        //         deaths_natural = new long?(kingdom.getDeaths(AttackType.Age)),
        //         deaths_plague = new long?(kingdom.getDeaths(AttackType.Plague)),
        //         deaths_poison = new long?(kingdom.getDeaths(AttackType.Poison)),
        //         deaths_infection = new long?(kingdom.getDeaths(AttackType.Infection)),
        //         deaths_tumor = new long?(kingdom.getDeaths(AttackType.Tumor)),
        //         deaths_acid = new long?(kingdom.getDeaths(AttackType.Acid)),
        //         deaths_fire = new long?(kingdom.getDeaths(AttackType.Fire)),
        //         deaths_divine = new long?(kingdom.getDeaths(AttackType.Divine)),
        //         deaths_weapon = new long?(kingdom.getDeaths(AttackType.Weapon)),
        //         deaths_gravity = new long?(kingdom.getDeaths(AttackType.Gravity)),
        //         deaths_drowning = new long?(kingdom.getDeaths(AttackType.Drowning)),
        //         deaths_water = new long?(kingdom.getDeaths(AttackType.Water)),
        //         deaths_explosion = new long?(kingdom.getDeaths(AttackType.Explosion)),
        //         deaths_other = new long?(kingdom.getDeaths(AttackType.Other)),
        //         metamorphosis = new long?(kingdom.getDeaths(AttackType.Metamorphosis)),
        //         evolutions = new long?(kingdom.getEvolutions())
        //     };
        // });
    }

    // gotta add known collectors
    // define table type and table types
    public static void AddFields(string id, string[] keys, Func<NanoObject, Dictionary<string, long>> collector)
    {
        var capitalized = id.Substring(0, 1).ToUpper() + id.Substring(1); 
        var asset = AssetManager.history_meta_data_library.get(id);

        var tableExtras = DynamicModule.DefineType(capitalized + "TableExtras",
            TypeAttributes.Public | TypeAttributes.Class,
            asset.table_type);
        
        var attrConstructor = typeof(ColumnAttribute).GetConstructor(new[]{typeof(string)});

        foreach (var key in keys)
        {
            var field = tableExtras.DefineField("_" + key, typeof(long?), FieldAttributes.Private);
            var propBuilder = tableExtras.DefineProperty(key, PropertyAttributes.HasDefault, typeof(long?), null);
            var attrBuilder = new CustomAttributeBuilder(attrConstructor, new object[] { key });
            field.SetCustomAttribute(attrBuilder);

            var getMethod = tableExtras.DefineMethod("get_" + key,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(long?),
                Type.EmptyTypes);
            
            var generatorForGet = getMethod.GetILGenerator();
            generatorForGet.Emit(OpCodes.Ldarg_0);
            generatorForGet.Emit(OpCodes.Ldfld, field);
            generatorForGet.Emit(OpCodes.Ret);

            var setMethod = tableExtras.DefineMethod("set_" + key,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new[]{typeof(long?)});
            var generatorForSet = getMethod.GetILGenerator();
            generatorForSet.Emit(OpCodes.Ldarg_0);
            generatorForSet.Emit(OpCodes.Ldarg_1);
            generatorForSet.Emit(OpCodes.Stfld, field);
            generatorForSet.Emit(OpCodes.Ret);
            
            propBuilder.SetGetMethod(getMethod);
            propBuilder.SetSetMethod(setMethod);
        }
        var tableExtrasType = tableExtras.CreateType();
        
        asset.table_type = tableExtrasType;
        asset.table_types = Intervals.Select(pair => new KeyValuePair<HistoryInterval, Type>(
            pair.Value,
            DynamicModule.DefineType(capitalized + "Yearly" + pair.Key,
                TypeAttributes.Public | TypeAttributes.Class,
                tableExtrasType).CreateType()
        )).ToDictionary(pair => pair.Key, pair => pair.Value);
        
        asset.collector = nano =>
        {
            var dictionary = collector.Invoke(nano);
            var instance = Activator.CreateInstance(tableExtrasType) as HistoryTable;
            
            foreach (var pair in dictionary)
            {
                tableExtrasType.GetProperty(pair.Key).SetValue(instance, pair.Value);
            }
        };
    }
}