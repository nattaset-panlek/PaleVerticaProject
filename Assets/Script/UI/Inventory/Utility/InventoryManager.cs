using CoreSystem.Data.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;

    public GameObject ItemSlotWidePrefab;
    public GameObject ItemSlotSquarePrefab;

    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<InventoryManager>();

                if (_instance == null)
                {
                    var prefab = Resources.Load<GameObject>("Prefab/InventoryManager");
                    if (prefab != null)
                    {
                        var go = Instantiate(prefab);
                        go.name = "InventoryManager";
                        _instance = go.GetComponent<InventoryManager>();
                    }
                    else
                    {
                        var go = new GameObject("InventoryManager");
                        _instance = go.AddComponent<InventoryManager>();
                        Debug.LogWarning("Database prefab not found in Resources/Prefab/. A temporary GameObject was created.");
                    }
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
            return _instance;
        }
    }


    public GameObject CreateItemSlotWideOnParent(ItemInstance item,Transform parent)
    {
        var itemSlotObj = Instantiate(ItemSlotWidePrefab, parent);
        var itemSlot = itemSlotObj.GetComponent<ItemSlotWide>();
        itemSlot.UpdateUI(item);
        return itemSlotObj;
    }

    public GameObject CreateItemSlotSquareOnParent(ItemInstance item, Transform parent)
    {
        var itemSlotObj = Instantiate(ItemSlotSquarePrefab, parent);
        var itemSlot = itemSlotObj.GetComponent<ItemSlotSquare>();
        itemSlot.UpdateUI(item);
        return itemSlotObj;
    }

    public GameObject CreateItemSlotSquareOnParent(Item item, Transform parent, int quanity = 1)
    {
        var itemInstance = new ItemInstance(-1, item.Id, quanity);
        return CreateItemSlotSquareOnParent(itemInstance, parent);
    }
}
