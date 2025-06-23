using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using db;
using db.tables;
using NCMS.Extensions;
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

    private static readonly Dictionary<string, Dictionary<string, Func<NanoObject, long?>>> VanillaCollectors = new()
    {
        ["kingdom"] =
        new (){
            ["population"] = obj => ((Kingdom) obj).countUnits(),
            ["adults"] = obj => ((Kingdom) obj).countAdults(),
            ["children"] = obj => ((Kingdom) obj).countChildren(),
            ["boats"] = obj => ((Kingdom) obj).countBoats(),
            ["army"] = obj => ((Kingdom) obj).countTotalWarriors(),
            ["sick"] = obj => ((Kingdom) obj).countSick(),
            ["hungry"] = obj => ((Kingdom) obj).countHungry(),
            ["starving"] = obj => ((Kingdom) obj).countStarving(),
            ["happy"] = obj => ((Kingdom)obj).countHappyUnits(),
            ["deaths"] = obj => ((Kingdom)obj).getTotalDeaths(),
            ["births"] = obj => ((Kingdom)obj).getTotalBirths(),
            ["kills"] = obj => ((Kingdom)obj).getTotalKills(),
            ["joined"] = obj => ((Kingdom)obj).getTotalJoined(),
            ["left"] = obj => ((Kingdom)obj).getTotalLeft(),
            ["moved"] = obj => ((Kingdom)obj).getTotalMoved(),
            ["migrated"] = obj => ((Kingdom)obj).getTotalMigrated(),
            ["territory"] = obj => ((Kingdom)obj).countZones(),
            ["buildings"] = obj => ((Kingdom)obj).countBuildings(),
            ["homeless"] = obj => ((Kingdom)obj).countHomeless(),
            ["housed"] = obj => ((Kingdom)obj).countHoused(),
            ["food"] = obj => ((Kingdom)obj).countTotalFood(),
            ["families"] = obj => ((Kingdom)obj).countFamilies(),
            ["males"] = obj => ((Kingdom)obj).countMales(),
            ["females"] = obj => ((Kingdom)obj).countFemales(),
            ["cities"] = obj => ((Kingdom)obj).countCities(),
            ["renown"] = obj => ((Kingdom)obj).getRenown(),
            ["money"] = obj => ((Kingdom)obj).countTotalMoney(),
            ["deaths_eaten"] = obj => ((Kingdom)obj).getDeaths(AttackType.Eaten),
            ["deaths_hunger"] = obj => ((Kingdom)obj).getDeaths(AttackType.Starvation),
            ["deaths_natural"] = obj => ((Kingdom)obj).getDeaths(AttackType.Age),
            ["deaths_plague"] = obj => ((Kingdom)obj).getDeaths(AttackType.Plague),
            ["deaths_poison"] = obj => ((Kingdom)obj).getDeaths(AttackType.Poison),
            ["deaths_infection"] = obj => ((Kingdom)obj).getDeaths(AttackType.Infection),
            ["deaths_tumor"] = obj => ((Kingdom)obj).getDeaths(AttackType.Tumor),
            ["deaths_acid"] = obj => ((Kingdom)obj).getDeaths(AttackType.Acid),
            ["deaths_fire"] = obj => ((Kingdom)obj).getDeaths(AttackType.Fire),
            ["deaths_divine"] = obj => ((Kingdom)obj).getDeaths(AttackType.Divine),
            ["deaths_weapon"] = obj => ((Kingdom)obj).getDeaths(AttackType.Weapon),
            ["deaths_gravity"] = obj => ((Kingdom)obj).getDeaths(AttackType.Gravity),
            ["deaths_drowning"] = obj => ((Kingdom)obj).getDeaths(AttackType.Drowning),
            ["deaths_water"] = obj => ((Kingdom)obj).getDeaths(AttackType.Water),
            ["deaths_explosion"] = obj => ((Kingdom)obj).getDeaths(AttackType.Explosion),
            ["deaths_other"] = obj => ((Kingdom)obj).getDeaths(AttackType.Other),
            ["metamorphosis"] = obj => ((Kingdom)obj).getDeaths(AttackType.Metamorphosis),
            ["evolutions"] = obj => ((Kingdom)obj).getEvolutions(),
        }
    };

    public static void Init()
    {
        
        AddStatisticsToMetaAsset("kingdom", new(){"lonely"}, nano =>
        {
            var kingdom = (Kingdom) nano;
            return new(){
                ["lonely"] = kingdom.countLonely(),
            };
        });
    }

    public static void AddStatisticsToMetaAsset(string id, List<string> keys, Func<NanoObject, Dictionary<string, long?>> collector)
    {

        var capitalized = id.Substring(0, 1).ToUpper() + id.Substring(1); 
        var asset = AssetManager.history_meta_data_library.get(id);

        var tableExtras = DynamicModule.DefineType(capitalized + "TableExtras",
            TypeAttributes.Public | TypeAttributes.Class,
            asset.table_type);
        // SQL's Column attribute
        var attrConstructor = typeof(ColumnAttribute).GetConstructor(new[]{typeof(string)});

        keys.AddRange(VanillaCollectors[id].Keys);
        
        foreach (var statistic in keys)
        {
            var field = tableExtras.DefineField("_" + statistic, typeof(long?), FieldAttributes.Private);
            var propBuilder = tableExtras.DefineProperty(statistic, PropertyAttributes.HasDefault, typeof(long?), null);
            var attrBuilder = new CustomAttributeBuilder(attrConstructor, new object[] { statistic });
            field.SetCustomAttribute(attrBuilder);

            var getMethod = tableExtras.DefineMethod("get_" + statistic,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(long?),
                Type.EmptyTypes);
            
            var generatorForGet = getMethod.GetILGenerator();
            generatorForGet.Emit(OpCodes.Ldarg_0);
            generatorForGet.Emit(OpCodes.Ldfld, field);
            generatorForGet.Emit(OpCodes.Ret);

            var setMethod = tableExtras.DefineMethod("set_" + statistic,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new[]{typeof(long?)});
            var generatorForSet = setMethod.GetILGenerator();
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
            dictionary.AddRange(VanillaCollectors[id].ToDictionary(pair => pair.Key, pair => pair.Value.Invoke(nano)));
            
            var instance = Activator.CreateInstance(asset.table_types[HistoryInterval.Yearly1]) as HistoryTable;
            
            foreach (var pair in dictionary)
            {
                tableExtrasType.GetProperty(pair.Key).SetValue(instance, pair.Value);
            }

            instance.id = nano.getID();
            return instance;
        };
    }
}