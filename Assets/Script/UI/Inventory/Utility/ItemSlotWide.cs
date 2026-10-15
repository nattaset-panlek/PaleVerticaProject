using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Inventory.Utility
{
    public class ItemSlotWide : ItemSlotGlobal
    {
        public TMP_Text ItemName;
        public GameObject EquipIcon;
        public Button OnClickSlotButton;

        public override void UpdateUI(ItemInstance item)
        {
            base.UpdateUI(item);

            if (itemInstance != null && itemInstance.Item != null)
            {
                ItemName.text = itemInstance.Name;

                if (EquipIcon != null)
                {
                    if (itemInstance is EquipmentInstance equipInst)
                        EquipIcon.SetActive(equipInst.IsEquipped);
                    else
                        EquipIcon.SetActive(false);
                }

                if (OnClickSlotButton != null)
                {
                    OnClickSlotButton.onClick.RemoveAllListeners();
                    OnClickSlotButton.onClick.AddListener(() =>
                    {
                        if (itemInstance != null && itemInstance.Item != null && ContextMenuManager.Instance != null)
                        {
                            var items = GetDefaultContextMenuItems();
                            if (items != null && items.Count > 0)
                            {
                                ContextMenuManager.Instance.Show(items, UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero);
                            }
                        }
                    });
                }
            }
        }

    }
}
