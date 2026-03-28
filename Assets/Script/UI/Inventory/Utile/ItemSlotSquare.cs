using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utile;
using CoreSystem.Utile;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        else
        {
            if (EquipIcon != null) EquipIcon.SetActive(false);
            if (ModifiedItem != null) ModifiedItem.SetActive(false);
        }

        if (itemInstance != null && itemInstance.Item != null)
        {
            QuanityValue.text = NumberFormatter.Format(itemInstance.Quantity);
            QuanityLable.SetActive(itemInstance.Quantity > 1);
        }
    }
}

