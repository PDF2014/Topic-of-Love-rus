using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using db;
using NCMS.Extensions;
using Topic_of_Love.Mian.CustomAssets.Custom;
using UnityEngine;

namespace Topic_of_Love.Mian.CustomAssets;
public class HistoryDataAssets
{
    public static void Init()
    {
        Add(new HistoryDataAsset
        {
            id = "lonely",
            color_hex = "#FB8B8B",
            path_icon = "ui/Icons/status/broke_up",
            statistics_asset = "statistics_lonely",
            category_group = GraphCategoryGroup.Noosphere
        });

        for (int i = 0; i <= 1; i++)
        {
            var isSexual = i == 0;
            Orientation.Orientations.Values.ForEach(orientation =>
            {
                Add(new HistoryDataAsset
                {
                    id = isSexual ? orientation.OrientationType : orientation.OrientationType + "_romantic",
                    path_icon = orientation.GetPathIcon(isSexual, true),
                    color_hex = orientation.HexCode,
                    statistics_asset = "statistics_" + (isSexual ? orientation.OrientationType : orientation.OrientationType + "_romantic"),
                    category_group = GraphCategoryGroup.Noosphere
                });
            });
        }
    }

    static void Add(HistoryDataAsset asset)
    {
        asset.localized_key = "statistics_" + asset.id;
        asset.localized_key_description = "statistics_" + asset.id + "_description";
        
        // NML loads it as a sprite so we gotta do this instead
        byte[] data = File.ReadAllBytes(TopicOfLove.Mod.GetDeclaration().FolderPath+"/GameResources/"+asset.path_icon+".png");
        Texture2D tex = new Texture2D(28, 28, TextureFormat.RGBA32, false);
        tex.LoadImage(data, true); // preserves data
        
        asset._material_point = HistoryDataAsset.cloneMaterial("materials/graph/graph_base_point");
        asset._material_point.SetTexture("_MainTex", tex);
        
        AssetManager.history_data_library.add(asset);
    }
}