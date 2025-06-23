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
using Topic_of_Love.Mian.CustomAssets.Custom;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;

public class HistoryMetaDataAssets
{
    public static readonly Dictionary<string, HistoryInterval> Intervals = new()
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

    public static readonly Dictionary<string, Dictionary<string, Func<NanoObject, long?>>> Collectors = new()
    {
        ["world"] = new()
        {
            ["alliances"] = _ => StatsHelper.getStat("alliances"),
            ["alliances_dissolved"] = _ => StatsHelper.getStat("world_statistics_alliances_dissolved"),
            ["alliances_made"] = _ => StatsHelper.getStat("world_statistics_alliances_made"),
            ["books"] = _ => StatsHelper.getStat("books"),
            ["books_burnt"] = _ => StatsHelper.getStat("world_statistics_books_burnt"),
            ["books_read"] = _ => StatsHelper.getStat("world_statistics_books_read"),
            ["books_written"] = _ => StatsHelper.getStat("world_statistics_books_written"),
            ["cities"] = _ => StatsHelper.getStat("villages"),
            ["cities_rebelled"] = _ => StatsHelper.getStat("world_statistics_cities_rebelled"),
            ["cities_conquered"] = _ => StatsHelper.getStat("world_statistics_cities_conquered"),
            ["cities_created"] = _ => StatsHelper.getStat("world_statistics_cities_created"),
            ["cities_destroyed"] = _ => StatsHelper.getStat("world_statistics_cities_destroyed"),
            ["clans"] = _ => StatsHelper.getStat("clans"),
            ["clans_created"] = _ => StatsHelper.getStat("world_statistics_clans_created"),
            ["clans_destroyed"] = _ => StatsHelper.getStat("world_statistics_clans_destroyed"),
            ["creatures_born"] = _ => StatsHelper.getStat("world_statistics_creatures_born"),
            ["creatures_created"] = _ => StatsHelper.getStat("world_statistics_creatures_created"),
            ["cultures"] = _ => StatsHelper.getStat("cultures"),
            ["cultures_created"] = _ => StatsHelper.getStat("world_statistics_cultures_created"),
            ["cultures_forgotten"] = _ => StatsHelper.getStat("world_statistics_cultures_forgotten"),
            ["deaths_eaten"] = _ => StatsHelper.getStat("world_statistics_deaths_eaten"),
            ["deaths_hunger"] = _ => StatsHelper.getStat("world_statistics_deaths_hunger"),
            ["deaths_natural"] = _ => StatsHelper.getStat("world_statistics_deaths_natural"),
            ["deaths_poison"] = _ => StatsHelper.getStat("world_statistics_deaths_poison"),
            ["deaths_infection"] = _ => StatsHelper.getStat("world_statistics_deaths_infection"),
            ["deaths_tumor"] = _ => StatsHelper.getStat("world_statistics_deaths_tumor"),
            ["deaths_acid"] = _ => StatsHelper.getStat("world_statistics_deaths_acid"),
            ["deaths_fire"] = _ => StatsHelper.getStat("world_statistics_deaths_fire"),
            ["deaths_divine"] = _ => StatsHelper.getStat("world_statistics_deaths_divine"),
            ["deaths_weapon"] = _ => StatsHelper.getStat("world_statistics_deaths_weapon"),
            ["deaths_gravity"] = _ => StatsHelper.getStat("world_statistics_deaths_gravity"),
            ["deaths_drowning"] = _ => StatsHelper.getStat("world_statistics_deaths_drowning"),
            ["deaths_water"] = _ => StatsHelper.getStat("world_statistics_deaths_water"),
            ["deaths_explosion"] = _ => StatsHelper.getStat("world_statistics_deaths_explosion"),
            ["metamorphosis"] = _ => StatsHelper.getStat("world_statistics_metamorphosis"),
            ["evolutions"] = _ => StatsHelper.getStat("world_statistics_evolutions"),
            ["deaths_other"] = _ => StatsHelper.getStat("world_statistics_deaths_other"),
            ["deaths_plague"] = _ => StatsHelper.getStat("world_statistics_deaths_plague"),
            ["deaths_total"] = _ => StatsHelper.getStat("world_statistics_deaths_total"),
            ["families"] = _ => StatsHelper.getStat("families"),
            ["families_created"] = _ => StatsHelper.getStat("world_statistics_families_created"),
            ["families_destroyed"] = _ => StatsHelper.getStat("world_statistics_families_destroyed"),
            ["houses"] = _ => StatsHelper.getStat("world_statistics_houses"),
            ["houses_built"] = _ => StatsHelper.getStat("world_statistics_houses_built"),
            ["houses_destroyed"] = _ => StatsHelper.getStat("world_statistics_houses_destroyed"),
            ["infected"] = _ => StatsHelper.getStat("world_statistics_infected"),
            ["islands"] = _ => StatsHelper.getStat("world_statistics_islands"),
            ["kingdoms"] = _ => StatsHelper.getStat("kingdoms"),
            ["kingdoms_created"] = _ => StatsHelper.getStat("world_statistics_kingdoms_created"),
            ["kingdoms_destroyed"] = _ => StatsHelper.getStat("world_statistics_kingdoms_destroyed"),
            ["languages"] = _ => StatsHelper.getStat("languages"),
            ["languages_created"] = _ => StatsHelper.getStat("world_statistics_languages_created"),
            ["languages_forgotten"] = _ => StatsHelper.getStat("world_statistics_languages_forgotten"),
            ["peaces_made"] = _ => StatsHelper.getStat("world_statistics_peaces_made"),
            ["plots"] = _ => StatsHelper.getStat("plots"),
            ["plots_forgotten"] = _ => StatsHelper.getStat("world_statistics_plots_forgotten"),
            ["plots_started"] = _ => StatsHelper.getStat("world_statistics_plots_started"),
            ["plots_succeeded"] = _ => StatsHelper.getStat("world_statistics_plots_succeeded"),
            ["population_beasts"] = _ => StatsHelper.getStat("world_statistics_beasts"),
            ["population_civ"] = _ => StatsHelper.getStat("world_statistics_population"),
            ["religions"] = _ => StatsHelper.getStat("religions"),
            ["religions_created"] = _ => StatsHelper.getStat("world_statistics_religions_created"),
            ["religions_forgotten"] = _ => StatsHelper.getStat("world_statistics_religions_forgotten"),
            ["subspecies"] = _ => StatsHelper.getStat("subspecies"),
            ["subspecies_created"] = _ => StatsHelper.getStat("world_statistics_subspecies_created"),
            ["subspecies_extinct"] = _ => StatsHelper.getStat("world_statistics_subspecies_extinct"),
            ["trees"] = _ => StatsHelper.getStat("world_statistics_trees"),
            ["vegetation"] = _ => StatsHelper.getStat("world_statistics_vegetation"),
            ["wars"] = _ => StatsHelper.getStat("wars"),
            ["wars_started"] = _ => StatsHelper.getStat("world_statistics_wars_started"),
            ["grass"] = _ => StatsHelper.getStat("world_statistics_grass"),
            ["savanna"] = _ => StatsHelper.getStat("world_statistics_savanna"),
            ["jungle"] = _ => StatsHelper.getStat("world_statistics_jungle"),
            ["desert"] = _ => StatsHelper.getStat("world_statistics_desert"),
            ["lemon"] = _ => StatsHelper.getStat("world_statistics_lemon"),
            ["permafrost"] = _ => StatsHelper.getStat("world_statistics_permafrost"),
            ["swamp"] = _ => StatsHelper.getStat("world_statistics_swamp"),
            ["crystal"] = _ => StatsHelper.getStat("world_statistics_crystal"),
            ["enchanted"] = _ => StatsHelper.getStat("world_statistics_enchanted"),
            ["corruption"] = _ => StatsHelper.getStat("world_statistics_corruption"),
            ["infernal"] = _ => StatsHelper.getStat("world_statistics_infernal"),
            ["candy"] = _ => StatsHelper.getStat("world_statistics_candy"),
            ["mushroom"] = _ => StatsHelper.getStat("world_statistics_mushroom"),
            ["wasteland"] = _ => StatsHelper.getStat("world_statistics_wasteland"),
            ["birch"] = _ => StatsHelper.getStat("world_statistics_birch"),
            ["maple"] = _ => StatsHelper.getStat("world_statistics_maple"),
            ["rocklands"] = _ => StatsHelper.getStat("world_statistics_rocklands"),
            ["garlic"] = _ => StatsHelper.getStat("world_statistics_garlic"),
            ["flower"] = _ => StatsHelper.getStat("world_statistics_flower"),
            ["celestial"] = _ => StatsHelper.getStat("world_statistics_celestial"),
            ["clover"] = _ => StatsHelper.getStat("world_statistics_clover"),
            ["singularity"] = _ => StatsHelper.getStat("world_statistics_singularity"),
            ["paradox"] = _ => StatsHelper.getStat("world_statistics_paradox"),
            ["sand"] = _ => StatsHelper.getStat("world_statistics_sand"),
            ["biomass"] = _ => StatsHelper.getStat("world_statistics_biomass"),
            ["cybertile"] = _ => StatsHelper.getStat("world_statistics_cybertile"),
            ["pumpkin"] = _ => StatsHelper.getStat("world_statistics_pumpkin"),
            ["tumor"] = _ => StatsHelper.getStat("world_statistics_tumor"),
            ["water"] = _ => StatsHelper.getStat("world_statistics_water"),
            ["soil"] = _ => StatsHelper.getStat("world_statistics_soil"),
            ["summit"] = _ => StatsHelper.getStat("world_statistics_summit"),
            ["mountains"] = _ => StatsHelper.getStat("world_statistics_mountains"),
            ["hills"] = _ => StatsHelper.getStat("world_statistics_hills"),
            ["lava"] = _ => StatsHelper.getStat("world_statistics_lava"),
            ["pit"] = _ => StatsHelper.getStat("world_statistics_pit"),
            ["field"] = _ => StatsHelper.getStat("world_statistics_field"),
            ["fireworks"] = _ => StatsHelper.getStat("world_statistics_fireworks"),
            ["frozen"] = _ => StatsHelper.getStat("world_statistics_frozen"),
            ["fuse"] = _ => StatsHelper.getStat("world_statistics_fuse"),
            ["ice"] = _ => StatsHelper.getStat("world_statistics_ice"),
            ["landmine"] = _ => StatsHelper.getStat("world_statistics_landmine"),
            ["road"] = _ => StatsHelper.getStat("world_statistics_road"),
            ["snow"] = _ => StatsHelper.getStat("world_statistics_snow"),
            ["tnt"] = _ => StatsHelper.getStat("world_statistics_tnt"),
            ["wall"] = _ => StatsHelper.getStat("world_statistics_wall"),
            ["water_bomb"] = _ => StatsHelper.getStat("world_statistics_water_bomb"),
            ["grey_goo"] = _ => StatsHelper.getStat("world_statistics_grey_goo")
        },
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
        },
        ["alliance"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Alliance)obj).countPopulation(),
            ["adults"] = obj => ((Alliance)obj).countAdults(),
            ["children"] = obj => ((Alliance)obj).countChildren(),
            ["army"] = obj => ((Alliance)obj).countWarriors(),
            ["sick"] = obj => ((Alliance)obj).countSick(),
            ["hungry"] = obj => ((Alliance)obj).countHungry(),
            ["starving"] = obj => ((Alliance)obj).countStarving(),
            ["happy"] = obj => ((Alliance)obj).countHappyUnits(),
            ["deaths"] = obj => ((Alliance)obj).getTotalDeaths(),
            ["kills"] = obj => ((Alliance)obj).getTotalKills(),
            ["births"] = obj => ((Alliance)obj).getTotalBirths(),
            ["territory"] = obj => ((Alliance)obj).countZones(),
            ["buildings"] = obj => ((Alliance)obj).countBuildings(),
            ["homeless"] = obj => ((Alliance)obj).countHomeless(),
            ["housed"] = obj => ((Alliance)obj).countHoused(),
            ["families"] = obj => ((Alliance)obj).countFamilies(),
            ["males"] = obj => ((Alliance)obj).countMales(),
            ["females"] = obj => ((Alliance)obj).countFemales(),
            ["kingdoms"] = obj => ((Alliance)obj).countKingdoms(),
            ["cities"] = obj => ((Alliance)obj).countCities(),
            ["renown"] = obj => ((Alliance)obj).getRenown(),
            ["money"] = obj => ((Alliance)obj).countTotalMoney()
        },
        ["clan"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Clan)obj).countUnits(),
            ["adults"] = obj => ((Clan)obj).countAdults(),
            ["children"] = obj => ((Clan)obj).countChildren(),
            ["births"] = obj => ((Clan)obj).getTotalBirths(),
            ["deaths"] = obj => ((Clan)obj).getTotalDeaths(),
            ["kills"] = obj => ((Clan)obj).getTotalKills(),
            ["kings"] = obj => ((Clan)obj).countKings(),
            ["leaders"] = obj => ((Clan)obj).countLeaders(),
            ["renown"] = obj => ((Clan)obj).getRenown(),
            ["money"] = obj => ((Clan)obj).countTotalMoney(),
            ["deaths_eaten"] = obj => ((Clan)obj).getDeaths(AttackType.Eaten),
            ["deaths_hunger"] = obj => ((Clan)obj).getDeaths(AttackType.Starvation),
            ["deaths_natural"] = obj => ((Clan)obj).getDeaths(AttackType.Age),
            ["deaths_plague"] = obj => ((Clan)obj).getDeaths(AttackType.Plague),
            ["deaths_poison"] = obj => ((Clan)obj).getDeaths(AttackType.Poison),
            ["deaths_infection"] = obj => ((Clan)obj).getDeaths(AttackType.Infection),
            ["deaths_tumor"] = obj => ((Clan)obj).getDeaths(AttackType.Tumor),
            ["deaths_acid"] = obj => ((Clan)obj).getDeaths(AttackType.Acid),
            ["deaths_fire"] = obj => ((Clan)obj).getDeaths(AttackType.Fire),
            ["deaths_divine"] = obj => ((Clan)obj).getDeaths(AttackType.Divine),
            ["deaths_weapon"] = obj => ((Clan)obj).getDeaths(AttackType.Weapon),
            ["deaths_gravity"] = obj => ((Clan)obj).getDeaths(AttackType.Gravity),
            ["deaths_drowning"] = obj => ((Clan)obj).getDeaths(AttackType.Drowning),
            ["deaths_water"] = obj => ((Clan)obj).getDeaths(AttackType.Water),
            ["deaths_explosion"] = obj => ((Clan)obj).getDeaths(AttackType.Explosion),
            ["deaths_other"] = obj => ((Clan)obj).getDeaths(AttackType.Other),
            ["metamorphosis"] = obj => ((Clan)obj).getDeaths(AttackType.Metamorphosis),
            ["evolutions"] = obj => ((Clan)obj).getEvolutions()
        },
        ["city"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((City)obj).countUnits(),
            ["adults"] = obj => ((City)obj).countAdults(),
            ["children"] = obj => ((City)obj).countChildren(),
            ["boats"] = obj => ((City)obj).countBoats(),
            ["army"] = obj => ((City)obj).countWarriors(),
            ["families"] = obj => ((City)obj).countFamilies(),
            ["males"] = obj => ((City)obj).countMales(),
            ["females"] = obj => ((City)obj).countFemales(),
            ["sick"] = obj => ((City)obj).countSick(),
            ["loyalty"] = obj => ((City)obj).getLoyalty(),
            ["hungry"] = obj => ((City)obj).countHungry(),
            ["starving"] = obj => ((City)obj).countStarving(),
            ["happy"] = obj => ((City)obj).countHappyUnits(),
            ["deaths"] = obj => ((City)obj).getTotalDeaths(),
            ["births"] = obj => ((City)obj).getTotalBirths(),
            ["joined"] = obj => ((City)obj).getTotalJoined(),
            ["left"] = obj => ((City)obj).getTotalLeft(),
            ["moved"] = obj => ((City)obj).getTotalMoved(),
            ["migrated"] = obj => ((City)obj).getTotalMigrated(),
            ["territory"] = obj => ((City)obj).countZones(),
            ["buildings"] = obj => ((City)obj).countBuildings(),
            ["homeless"] = obj => ((City)obj).countHomeless(),
            ["housed"] = obj => ((City)obj).countHoused(),
            ["renown"] = obj => ((City)obj).getRenown(),
            ["money"] = obj => ((City)obj).countTotalMoney(),
            ["food"] = obj => ((City)obj).getTotalFood(),
            ["gold"] = obj => ((City)obj).getResourcesAmount("gold"),
            ["wood"] = obj => ((City)obj).getResourcesAmount("wood"),
            ["stone"] = obj => ((City)obj).getResourcesAmount("stone"),
            ["common_metals"] = obj => ((City)obj).getResourcesAmount("common_metals"),
            ["items"] = obj => ((City)obj).data.equipment.countItems(),
            ["deaths_eaten"] = obj => ((City)obj).getDeaths(AttackType.Eaten),
            ["deaths_hunger"] = obj => ((City)obj).getDeaths(AttackType.Starvation),
            ["deaths_natural"] = obj => ((City)obj).getDeaths(AttackType.Age),
            ["deaths_plague"] = obj => ((City)obj).getDeaths(AttackType.Plague),
            ["deaths_poison"] = obj => ((City)obj).getDeaths(AttackType.Poison),
            ["deaths_infection"] = obj => ((City)obj).getDeaths(AttackType.Infection),
            ["deaths_tumor"] = obj => ((City)obj).getDeaths(AttackType.Tumor),
            ["deaths_acid"] = obj => ((City)obj).getDeaths(AttackType.Acid),
            ["deaths_fire"] = obj => ((City)obj).getDeaths(AttackType.Fire),
            ["deaths_divine"] = obj => ((City)obj).getDeaths(AttackType.Divine),
            ["deaths_weapon"] = obj => ((City)obj).getDeaths(AttackType.Weapon),
            ["deaths_gravity"] = obj => ((City)obj).getDeaths(AttackType.Gravity),
            ["deaths_drowning"] = obj => ((City)obj).getDeaths(AttackType.Drowning),
            ["deaths_water"] = obj => ((City)obj).getDeaths(AttackType.Water),
            ["deaths_explosion"] = obj => ((City)obj).getDeaths(AttackType.Explosion),
            ["deaths_other"] = obj => ((City)obj).getDeaths(AttackType.Other),
            ["metamorphosis"] = obj => ((City)obj).getDeaths(AttackType.Metamorphosis),
            ["evolutions"] = obj => ((City)obj).getEvolutions()
        },
        ["culture"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Culture)obj).countUnits(),
            ["cities"] = obj => ((Culture)obj).countCities(),
            ["kingdoms"] = obj => ((Culture)obj).countKingdoms(),
            ["births"] = obj => ((Culture)obj).getTotalBirths(),
            ["deaths"] = obj => ((Culture)obj).getTotalDeaths(),
            ["kills"] = obj => ((Culture)obj).getTotalKills(),
            ["adults"] = obj => ((Culture)obj).countAdults(),
            ["children"] = obj => ((Culture)obj).countChildren(),
            ["kings"] = obj => ((Culture)obj).countKings(),
            ["leaders"] = obj => ((Culture)obj).countLeaders(),
            ["renown"] = obj => ((Culture)obj).getRenown(),
            ["money"] = obj => ((Culture)obj).countTotalMoney()
        },
        ["family"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Family)obj).countUnits(),
            ["adults"] = obj => ((Family)obj).countAdults(),
            ["children"] = obj => ((Family)obj).countChildren(),
            ["births"] = obj => ((Family)obj).getTotalBirths(),
            ["deaths"] = obj => ((Family)obj).getTotalDeaths(),
            ["kills"] = obj => ((Family)obj).getTotalKills(),
            ["money"] = obj => ((Family)obj).countTotalMoney()
        },
        ["army"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Army)obj).countUnits(),
            ["deaths"] = obj => ((Army)obj).getTotalDeaths(),
            ["kills"] = obj => ((Army)obj).getTotalKills()
        },
        ["language"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Language)obj).countUnits(),
            ["adults"] = obj => ((Language)obj).countAdults(),
            ["children"] = obj => ((Language)obj).countChildren(),
            ["kingdoms"] = obj => ((Language)obj).countKingdoms(),
            ["cities"] = obj => ((Language)obj).countCities(),
            ["books"] = obj => ((Language)obj).books.count(),
            ["books_written"] = obj => ((Language)obj).countWrittenBooks(),
            ["speakers_new"] = obj => ((Language)obj).getSpeakersNew(),
            ["speakers_lost"] = obj => ((Language)obj).getSpeakersLost(),
            ["speakers_converted"] = obj => ((Language)obj).getSpeakersConverted(),
            ["deaths"] = obj => ((Language)obj).getTotalDeaths(),
            ["kills"] = obj => ((Language)obj).getTotalKills(),
            ["renown"] = obj => ((Language)obj).getRenown(),
            ["money"] = obj => ((Language)obj).countTotalMoney()
        },
        ["religion"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Religion)obj).countUnits(),
            ["kingdoms"] = obj => ((Religion)obj).countKingdoms(),
            ["cities"] = obj => ((Religion)obj).countCities(),
            ["sick"] = obj => ((Religion)obj).countSick(),
            ["happy"] = obj => ((Religion)obj).countHappyUnits(),
            ["hungry"] = obj => ((Religion)obj).countHungry(),
            ["starving"] = obj => ((Religion)obj).countStarving(),
            ["deaths"] = obj => ((Religion)obj).getTotalDeaths(),
            ["births"] = obj => ((Religion)obj).getTotalBirths(),
            ["kills"] = obj => ((Religion)obj).getTotalKills(),
            ["adults"] = obj => ((Religion)obj).countAdults(),
            ["children"] = obj => ((Religion)obj).countChildren(),
            ["males"] = obj => ((Religion)obj).countMales(),
            ["females"] = obj => ((Religion)obj).countFemales(),
            ["homeless"] = obj => ((Religion)obj).countHomeless(),
            ["housed"] = obj => ((Religion)obj).countHoused(),
            ["kings"] = obj => ((Religion)obj).countKings(),
            ["leaders"] = obj => ((Religion)obj).countLeaders(),
            ["renown"] = obj => ((Religion)obj).getRenown(),
            ["money"] = obj => ((Religion)obj).countTotalMoney(),
            ["evolutions"] = obj => ((Religion)obj).getEvolutions()
        },
        ["subspecies"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((Subspecies)obj).countUnits(),
            ["adults"] = obj => ((Subspecies)obj).countAdults(),
            ["children"] = obj => ((Subspecies)obj).countChildren(),
            ["deaths"] = obj => ((Subspecies)obj).getTotalDeaths(),
            ["births"] = obj => ((Subspecies)obj).getTotalBirths(),
            ["kills"] = obj => ((Subspecies)obj).getTotalKills(),
            ["renown"] = obj => ((Subspecies)obj).getRenown(),
            ["money"] = obj => ((Subspecies)obj).countTotalMoney(),
            ["deaths_eaten"] = obj => ((Subspecies)obj).getDeaths(AttackType.Eaten),
            ["deaths_hunger"] = obj => ((Subspecies)obj).getDeaths(AttackType.Starvation),
            ["deaths_natural"] = obj => ((Subspecies)obj).getDeaths(AttackType.Age),
            ["deaths_plague"] = obj => ((Subspecies)obj).getDeaths(AttackType.Plague),
            ["deaths_poison"] = obj => ((Subspecies)obj).getDeaths(AttackType.Poison),
            ["deaths_infection"] = obj => ((Subspecies)obj).getDeaths(AttackType.Infection),
            ["deaths_tumor"] = obj => ((Subspecies)obj).getDeaths(AttackType.Tumor),
            ["deaths_acid"] = obj => ((Subspecies)obj).getDeaths(AttackType.Acid),
            ["deaths_fire"] = obj => ((Subspecies)obj).getDeaths(AttackType.Fire),
            ["deaths_divine"] = obj => ((Subspecies)obj).getDeaths(AttackType.Divine),
            ["deaths_weapon"] = obj => ((Subspecies)obj).getDeaths(AttackType.Weapon),
            ["deaths_gravity"] = obj => ((Subspecies)obj).getDeaths(AttackType.Gravity),
            ["deaths_drowning"] = obj => ((Subspecies)obj).getDeaths(AttackType.Drowning),
            ["deaths_water"] = obj => ((Subspecies)obj).getDeaths(AttackType.Water),
            ["deaths_explosion"] = obj => ((Subspecies)obj).getDeaths(AttackType.Explosion),
            ["deaths_other"] = obj => ((Subspecies)obj).getDeaths(AttackType.Other),
            ["metamorphosis"] = obj => ((Subspecies)obj).getDeaths(AttackType.Metamorphosis),
            ["evolutions"] = obj => ((Subspecies)obj).getEvolutions()
        },
        ["war"] = new Dictionary<string, Func<NanoObject, long?>>()
        {
            ["population"] = obj => ((War)obj).countTotalPopulation(),
            ["army"] = obj => ((War)obj).countTotalArmy(),
            ["renown"] = obj => ((War)obj).getRenown(),
            ["kingdoms"] = obj => ((War)obj).countKingdoms(),
            ["cities"] = obj => ((War)obj).countCities(),
            ["deaths"] = obj => ((War)obj).getTotalDeaths(),
            ["population_attackers"] = obj => ((War)obj).countAttackersPopulation(),
            ["population_defenders"] = obj => ((War)obj).countDefendersPopulation(),
            ["army_attackers"] = obj => ((War)obj).countAttackersWarriors(),
            ["army_defenders"] = obj => ((War)obj).countDefendersWarriors(),
            ["deaths_attackers"] = obj => ((War)obj).getDeadAttackers(),
            ["deaths_defenders"] = obj => ((War)obj).getDeadDefenders(),
            ["money_attackers"] = obj => ((War)obj).countAttackersMoney(),
            ["money_defenders"] = obj => ((War)obj).countDefendersMoney()
        }
    };

    public static void Init()
    {
        Dictionary<KeyValuePair<string, Func<NanoObject, long?>>, List<string>> toAdd = new();
        toAdd.Add(new ("lonely", nano =>
        {
            if (nano.getMetaType().Equals(MetaType.World))
            {
                return StatsHelper.getStat("statistics_lonely");
            }

            return ((IMetaObject)nano).countLonely();
        }), new(){"kingdom", "city", "world"});
        

        Orientation.Orientations.ForEach(orientation =>
        {
            var id = orientation.OrientationType;
            toAdd.Add(new(id, nano =>
            {
                if (nano.getMetaType().Equals(MetaType.World))
                {
                    return StatsHelper.getStat("statistics_" + id);
                }

                return ((IMetaObject)nano).countOrientation(orientation.OrientationType, true);
            }), new(){"kingdom", "city", "alliance", "world", "culture"});
        });
        
        var toUse = new Dictionary<string, Dictionary<string, Func<NanoObject, long?>>>();
        
        foreach (var pair in toAdd)
        {
            var statisticInfo = pair.Key;
            var statId = statisticInfo.Key;
            var statCollector = statisticInfo.Value;

            foreach (var metaAsset in pair.Value)
            {
                if(!toUse.ContainsKey(metaAsset))
                    toUse.Add(metaAsset, new());
                
                toUse[metaAsset][statId] = statCollector;
            }
        }

        foreach (var asset in toUse)
        {
            AddStatisticsToMetaAsset(asset.Key, asset.Value);
        }
        
        Finish();
    }

    public static void AddStatisticsToMetaAsset(string id, Dictionary<string, Func<NanoObject, long?>> collector)
    {
        if(!Collectors.ContainsKey(id))
            Collectors.Add(id, new());
        Collectors[id].AddRange(collector);
    }

    public static void Finish()
    {
        foreach (var collectorPair in Collectors)
        {
            var id = collectorPair.Key;
            var mergedCollector = collectorPair.Value;
            var capitalized = id.Substring(0, 1).ToUpper() + id.Substring(1); 
            var asset = AssetManager.history_meta_data_library.get(id);

            var tableExtras = DynamicModule.DefineType(capitalized + "TableExtras",
                TypeAttributes.Public | TypeAttributes.Class,
                asset.table_type);
            // SQL's Column attribute
            var attrConstructor = typeof(ColumnAttribute).GetConstructor(new[]{typeof(string)});
            
            foreach (var statistic in mergedCollector.Keys.ToList())
            {
                var historyDataAsset = AssetManager.history_data_library.get(statistic);
                if(!asset.categories.Contains(historyDataAsset))
                    asset.categories.Add(historyDataAsset);
                
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
                var dictionary = mergedCollector.ToDictionary(pair => pair.Key, pair => pair.Value.Invoke(nano));
                
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
}