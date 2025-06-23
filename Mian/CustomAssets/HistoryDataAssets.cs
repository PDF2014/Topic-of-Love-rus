using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using db;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;
public class HistoryDataAssets
{
    private static readonly string[] Categories =
    {
        "kingdom"
        // "kingdom", "world", "alliance", "clan", "city", "culture",
        // "family", "army", "language", "religion",
        // "subspecies", "war"
    };

public static readonly List<CustomHistoryDataAsset> Assets = new();
    public class CustomHistoryDataAsset : HistoryDataAsset
    {
        private Dictionary<string, Func<NanoObject, long>> collectors = new();

        public void AddCollector(string category, Func<NanoObject, long> func)
        {
            collectors.Add(category, func);
        }

        public List<string> GetCategories()
        {
            return collectors.Keys.ToList();
        }
    }
    
    public static void Init()
    {
        var lonely = Add(new CustomHistoryDataAsset
        {
            id = "lonely",
            color_hex = "#FB8B8B",
            path_icon = "ui/Icons/status/broke_up",
            statistics_asset = "statistics_lonely"
        });
        
        // foreach (var category in Categories)
        // {
        //     if (!category.Equals("world"))
        //     {
        //         lonely.AddCollector(category, nanoObj => ((IMetaObject) nanoObj).countLonely());
        //     }
        //     else
        //     {
        //         lonely.AddCollector(category, nanoObj => World.world.world_object.countLonely());
        //     }
        // }
    }

    static CustomHistoryDataAsset Add(CustomHistoryDataAsset asset)
    {
        foreach (var assetCategory in Categories)
        {
            AssetManager.history_meta_data_library.get(assetCategory).categories.Add(asset);
        }
        asset.localized_key = "statistics_" + asset.id;
        asset.localized_key_description = "statistics_" + asset.id + "_description";
        
        // NML loads it as a sprite so we gotta do this instead
        byte[] data = File.ReadAllBytes(TopicOfLove.Mod.GetDeclaration().FolderPath+"/GameResources/"+asset.path_icon+".png");
        Texture2D tex = new Texture2D(28, 28, TextureFormat.RGBA32, false);
        tex.LoadImage(data, true); // preserves data
        
        asset._material_point = HistoryDataAsset.cloneMaterial("materials/graph/graph_base_point");
        asset._material_point.SetTexture("_MainTex", tex);
        
        AssetManager.history_data_library.add(asset);
        Assets.Add(asset);
        return asset;
    }
}