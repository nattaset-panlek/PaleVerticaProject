using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utility;
using CoreSystem.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CoreSystem.UI.Inventory.Utility
{
    public class ItemSlotSquare : ItemSlotGlobal
    {
        public GameObject QuanityLable;
        public TMP_Text QuanityValue;
        public GameObject EquipIcon;
        public GameObject ModifiedItem;
        public GameObject HasSocket;

        public Item Item;
        public ItemInstance ItemInstance;

        public override void UpdateUI(ItemInstance item)
        {
            base.UpdateUI(item);

            this.Item = item.Item;
            this.ItemInstance = item;

            HasSocket.SetActive(false);

            if (itemInstance is EquipmentInstance equipInst)
            {
                // ── Equipment Icon ─────────────────────────────────────────────
                if (EquipIcon != null)
                    EquipIcon.SetActive(equipInst.IsEquipped);

                // ── Modified Item Indicator (Socket/Advancement) ───────────────
                // แสดง icon พิเศษถ้ามีการตีบวก (Advancement) หรือใส่ Fragment (Socket)
                if (ModifiedItem != null)
                {
                    bool isModified = equipInst.Reforge > 0 || (equipInst.SocketedFragments != null && equipInst.SocketedFragments.Count > 0);
                    ModifiedItem.SetActive(isModified);
                }

                if(HasSocket != null)
                {
                    HasSocket.SetActive(((Equipment)equipInst.Item).MaxSockets > 0);
                }
            }
            else if (itemInstance is CostumeInstance costumeInst)
            {
                if (EquipIcon != null) EquipIcon.SetActive(costumeInst.IsEquipped);
                if (ModifiedItem != null) ModifiedItem.SetActive(false);
                if (HasSocket != null) HasSocket.SetActive(false);
            }
            else
            {
                if (EquipIcon != null) EquipIcon.SetActive(false);
                if (ModifiedItem != null) ModifiedItem.SetActive(false);
                if (HasSocket != null) HasSocket.SetActive(false);
            }

            if (itemInstance != null && itemInstance.Item != null)
            {
                QuanityLable.SetActive(false);
                if (itemInstance is EquipmentInstance equipment)
                {
                    QuanityValue.text = NumberFormatter.Format(equipment.Refine);
                    QuanityLable.SetActive(equipment.Refine > 0);
                }
                else
                {
                    QuanityValue.text = NumberFormatter.Format(itemInstance.Quantity);
                    QuanityLable.SetActive(itemInstance.Quantity > 1);
                }
            }
        }
    }
}
