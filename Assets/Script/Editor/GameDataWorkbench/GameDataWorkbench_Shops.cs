using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Shop;
using UnityEditor;
using UnityEngine;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    [Serializable]
    private class ShopFile
    {
        public List<ShopDefinition> Shops = new List<ShopDefinition>();
    }

    private string _shopJsonPath = "Assets/Resources/Data/Shop.json";
    private ShopFile _shopData;
    private int _shopSelectedIndex = -1;
    #endregion

    #region [Private Methods]
    private void DrawShopEditor()
    {
        DrawEditor(_shopData.Shops, _shopSelectedIndex, idx => _shopSelectedIndex = idx, DrawShopFields, DescribeShop, CreateShop);
    }

    private void DrawShopFields(ShopDefinition entry)
    {
        if (entry.Items == null) entry.Items = new List<ShopEntry>();
        foreach (string problem in ShopDataValidator.Validate(entry, _shopData.Shops, ItemExists))
        {
            EditorGUILayout.HelpBox(problem, MessageType.Warning);
        }

        BeginInspectorColumns();
        if (BeginGroup("Shop", ThemeAccent))
        {
            entry.Id = EditorGUILayout.TextField("Shop Id", entry.Id);
            entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            EditorGUILayout.LabelField("Put this Shop Id on the NPC's Merchant Shop System.", EditorStyles.wordWrappedMiniLabel);
        }
        EndGroup();

        NextInspectorColumn();
        if (BeginGroup("Items For Sale", ThemeAccent, entry.Items.Count.ToString())) DrawShopItems(entry);
        EndGroup();
        EndInspectorColumns();
    }

    private void DrawShopItems(ShopDefinition entry)
    {
        int removeIndex = -1;
        for (int i = 0; i < entry.Items.Count; i++)
        {
            ShopEntry item = entry.Items[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawItemPicker("Item", item.ItemId, id => item.ItemId = id);
            item.Price = EditorGUILayout.IntField("Price", item.Price);

            bool unlimited = EditorGUILayout.Toggle("Unlimited Stock", item.Stock < 0);
            if (unlimited) item.Stock = ShopEntry.Unlimited;
            else item.Stock = EditorGUILayout.IntField("Stock", Mathf.Max(item.Stock, 1));

            if (GUILayout.Button("Remove Item", EditorStyles.miniButton)) removeIndex = i;
            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
        {
            entry.Items.RemoveAt(removeIndex);
            MarkDirty();
        }

        if (GUILayout.Button("+ Item"))
        {
            entry.Items.Add(new ShopEntry());
            MarkDirty();
        }
    }

    private EntryView DescribeShop(ShopDefinition entry)
    {
        int problems = ShopDataValidator.Validate(entry, _shopData.Shops, ItemExists).Count;
        return new EntryView
        {
            Id = entry.Id,
            Name = entry.Name,
            Meta = problems > 0 ? $"{problems} issue(s)" : string.Empty,
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = $"{entry.Items?.Count ?? 0} item(s)"
        };
    }

    private ShopDefinition CreateShop()
    {
        return new ShopDefinition { Id = $"shop_{_shopData.Shops.Count + 1}", Name = "New Shop" };
    }
    #endregion
}
