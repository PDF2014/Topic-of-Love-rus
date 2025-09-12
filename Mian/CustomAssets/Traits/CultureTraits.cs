﻿
using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General.Game.extensions;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class CultureTraits : BaseTraits<CultureTrait, CultureTraitLibrary>
    {
        public void Init()
        {
            Init("culture");
            
            Add(new CultureTrait
            {
                id = "orientationless",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("angle", "snowman")).addOpposites(new List<string>{"heterophobic", "homophobic"});

            var homo = Add(new CultureTrait
            {
                id = "homophobic",
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true,
            }, List.Of("orc", "demon"));
            
            homo.addOpposite("heterophobic");

            var hetero = Add(new CultureTrait
            {
                id = "heterophobic",
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true,
            }, List.Of("flower_bud", "garl"));
            
            hetero.addOpposite("homophobic");

            // incest doesnt really have any gameplay value to it :shrug:
            // Add(new CultureTrait
            // {
            //     id = "incest",
            //     group_id = "miscellaneous",
            //     needs_to_be_explored = true,
            //     rarity = Rarity.R1_Rare,
            //     can_be_in_book = true,
            //     can_be_removed = true,
            //     can_be_given = true
            // }, List.Of("orc", "demon"), List.Of("biome_infernal", "biome_corrupted"));
            //
            // Add(new CultureTrait
            // {
            //     id = "scar_of_incest",
            //     group_id = "miscellaneous",
            //     rarity = Rarity.R1_Rare,
            //     can_be_given = true,
            //     can_be_in_book = false,
            //     can_be_removed = true,
            // }, List.Of("orc", "demon"), List.Of("biome_infernal", "biome_corrupted"));
            
            Add(new CultureTrait
            {
                id = "committed",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("elf", "coolbeak"), List.Of("biome_celestial", "biome_flower"));
            
            Add(new CultureTrait
            {
                id = "mature_dating",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("human", "elf", "dwarf"), List.Of("biome_grass", "biome_maple"));

            Add(new CultureTrait
            {
                id = "sexual_expectations",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("elf"), List.Of("biome_maple"));
            
            Add(new CultureTrait
            {
                id="sex_for_reproduction",
                group_id = "miscellaneous",
                rarity = Rarity.R0_Normal,
                needs_to_be_explored = true,
                can_be_given = true,
                can_be_in_book = true,
                can_be_removed = true
            }, List.Of("elf"));
            
            Add(new CultureTrait
            {
                id="sex_for_reproduction_only",
                group_id = "miscellaneous",
                rarity = Rarity.R0_Normal,
                needs_to_be_explored = true,
                can_be_given = true,
                can_be_in_book = true,
                can_be_removed = true
            }, List.Of("orc"));
            
            Finish();
        }
    }
}