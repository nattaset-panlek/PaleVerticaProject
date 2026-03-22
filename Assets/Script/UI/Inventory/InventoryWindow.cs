using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Utile;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryWindow : MonoBehaviour
{
    public Transform EquipmentSlotParent;
    public Transform InventorySlotParent;
    public TMP_Text InventoryCountText;

    public void Awake()
    {
        GameData.Player.Inventory.OnInventoryChanged += LoadItemSlotWideEquipmentInventory;
        GameData.Player.Inventory.OnInventoryChanged += LoadItemSlotAllInventory;
        GameData.Player.Inventory.OnInventoryChanged += UpdateInventoryCount;
    }

    public void OnDestroy()
    {
        GameData.Player.Inventory.OnInventoryChanged -= LoadItemSlotWideEquipmentInventory;
        GameData.Player.Inventory.OnInventoryChanged -= LoadItemSlotAllInventory;
        GameData.Player.Inventory.OnInventoryChanged -= UpdateInventoryCount;
    }

    public void LoadItemSlotAllInventory()
    {

        foreach (Transform child in InventorySlotParent)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemInstance item in GameData.Player.Inventory.GetAllItems())
        {
            InventoryManager.Instance.CreateItemSlotSquareOnParent(item, InventorySlotParent);
        }
    }

    public void LoadItemSlotWideEquipmentInventory()
    {

        foreach (Transform child in EquipmentSlotParent)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemInstance item in GameData.Player.Inventory.GetAllItems())
        {
            if (item.Item is Equipment)
            {
                InventoryManager.Instance.CreateItemSlotWideOnParent(item, EquipmentSlotParent);
            }
        }
    }

    public void UpdateInventoryCount()
    {
        int count = GameData.Player.Inventory.GetAllItems().Count;
        InventoryCountText.text = NumberFormatter.Format(count);
    }
}
