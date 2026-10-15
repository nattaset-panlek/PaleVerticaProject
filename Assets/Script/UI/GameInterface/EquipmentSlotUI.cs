using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Inventory;
using CoreSystem.UI.Utility;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IItemProvider, IDropHandler
{
    private Button SlotButton;
    public EquipmentInstance EquipmentItem;

    public EquipmentSlot TargetSlot;
    public Image Icon;
    public Image RarityFrame;

    public ItemInstance GetItem() => EquipmentItem;
    public Sprite GetIcon() => (Icon != null && Icon.gameObject.activeSelf) ? Icon.sprite : null;

    [ContextMenu("SetNameGameObject")]
    public void SetObjectName()
    {
        gameObject.name = $"{TargetSlot} Slot";
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = GlobalDragManager.Payload as ItemInstance ?? ItemDragHandler.DraggedItem;
        if (draggedItem == null)
        {
            if (GlobalDragManager.Payload != null)
            {
                Debug.Log($"[EquipmentSlotUI] Incompatible drop payload type: {GlobalDragManager.Payload.GetType().Name}");
            }
            return;
        }

        if (draggedItem is EquipmentInstance equipInst && equipInst.Item is Equipment equipment)
        {
            // ── Type-compatibility guard ───────────────────────────────────
            // ตรวจว่า item ประเภทนี้สามารถ equip ใน slot ของ UI นี้ได้ไหม
            if (!CanEquipIn(equipment.Slot, TargetSlot))
            {
                Debug.Log($"[EquipmentSlotUI] {equipment.Name} ({equipment.Slot}) ไม่สามารถใส่ใน {TargetSlot} slot ได้");
                return;
            }

            // ResolveSmartSlot จะตัดสินว่า item ชิ้นนี้ควรลงที่ slot ไหน
            int requestedSlot = Database.GetDefaultSlotForType(TargetSlot);
            int resolvedSlot  = GameData.Player.Equipment.ResolveSmartSlot(equipment, requestedSlot);

            if (resolvedSlot >= 0)
                GameData.Player.Equipment.Equip(resolvedSlot, equipInst);
        }
    }

    /// <summary>
    /// ตรวจว่า item ที่มี slot type นั้นๆ สามารถวางลง UI TargetSlot นี้ได้ไหม
    /// </summary>
    private static bool CanEquipIn(EquipmentSlot itemSlot, EquipmentSlot targetSlot)
    {
        switch (itemSlot)
        {
            // ─── อุปกรณ์ทั่วไป: ต้องตรงกัน 1:1 ──────────────────────────────
            case EquipmentSlot.Helm:
                return targetSlot == EquipmentSlot.Helm;

            case EquipmentSlot.Armor:
                return targetSlot == EquipmentSlot.Armor;

            case EquipmentSlot.Glove:
                return targetSlot == EquipmentSlot.Glove;

            case EquipmentSlot.Talisman:
                return targetSlot == EquipmentSlot.Talisman;

            // ─── Weapon: TwoHand → ใส่ได้ทั้ง Left และ Right weapon slot ──
            case EquipmentSlot.WeaponTwoHand:
                return targetSlot == EquipmentSlot.WeaponOneHandLeft
                    || targetSlot == EquipmentSlot.WeaponOneHandRight;

            // ─── Weapon: OneHandLeft → ใส่ได้ที่ Left หรือ Right (redirect left) ──
            case EquipmentSlot.WeaponOneHandLeft:
                return targetSlot == EquipmentSlot.WeaponOneHandLeft
                    || targetSlot == EquipmentSlot.WeaponOneHandRight;

            // ─── Weapon: OneHandRight → ใส่ได้ที่ Right หรือ Left (redirect right) ─
            case EquipmentSlot.WeaponOneHandRight:
                return targetSlot == EquipmentSlot.WeaponOneHandLeft
                    || targetSlot == EquipmentSlot.WeaponOneHandRight;

            // ─── Accessory (ทั่วไป) → ใส่ได้ทั้ง AccessoryLeft/Right slot ──
            case EquipmentSlot.Accessory:
                return targetSlot == EquipmentSlot.AccessoryLeft
                    || targetSlot == EquipmentSlot.AccessoryRight;

            // ─── AccessoryLeft → เฉพาะ AccessoryLeft slot ──────────────────
            case EquipmentSlot.AccessoryLeft:
                return targetSlot == EquipmentSlot.AccessoryLeft;

            // ─── AccessoryRight → เฉพาะ AccessoryRight slot ─────────────────
            case EquipmentSlot.AccessoryRight:
                return targetSlot == EquipmentSlot.AccessoryRight;

            default:
                return false;
        }
    }


    private void Awake()
    {
        if (GameData.Player?.Equipment != null)
            GameData.Player.Equipment.OnEquipmentChanged += UpdateSlot;
    }

    private void OnDestroy()
    {
        if (GameData.Player?.Equipment != null)
            GameData.Player.Equipment.OnEquipmentChanged -= UpdateSlot;
    }

    private void OnEnable()
    {
        UpdateSlot(null);
    }

    public void UpdateSlot(EquipmentInstance changedInstance)
    {
        EquipmentInstance currentEquip = GameData.Player.Equipment.GetEquipment(TargetSlot);
        EquipmentItem = currentEquip;

        var hoverableUI = GetComponent<HoverableUI>() ?? gameObject.AddComponent<HoverableUI>();
        if (currentEquip != null && currentEquip.Item != null)
        {
            hoverableUI.Setup(
                $"<color=#{ItemRarityExtensions.GetHexColor(currentEquip.Item.Rarity)}>{currentEquip.Name}</color>",
                currentEquip.Item.Description
            );
        }
        else
        {
            hoverableUI.Setup($"{TargetSlot} Empty Slot", $"This slot is for {TargetSlot}.");
        }

        var ctx = GetComponent<ContextMenuUI>() ?? gameObject.AddComponent<ContextMenuUI>();

        if (currentEquip != null && currentEquip.Item is Equipment equipModel)
        {
            if (RarityFrame != null)
                RarityFrame.color = ItemRarityExtensions.GetColor(equipModel.Rarity);

            if (Icon != null)
            {
                Icon.gameObject.SetActive(true);
                Icon.color = Color.white;
                Icon.sprite = equipModel.Icon; // โหลด icon จาก Resources ผ่าน ItemIconLoader
            }

            ctx.SetupMenu(GetContextMenuItems());
        }
        else
        {
            if (RarityFrame != null)
                RarityFrame.color = Color.white;

            if (Icon != null)
            {
                Icon.gameObject.SetActive(false);
                Icon.sprite = null; // clear เมื่อไม่มี equipment
            }

            ctx.ClearMenu();
        }

        // Drag support
        if (GetComponent<ItemDragHandler>() == null)
            gameObject.AddComponent<ItemDragHandler>();
    }


    private List<ContextMenuItem> GetContextMenuItems()
    {
        var items = new List<ContextMenuItem>();

        if (EquipmentItem != null && EquipmentItem.Item is Equipment equipment)
        {
            items.Add(new ContextMenuItem("Unequip", () =>
            {
                int targetSlotIdx = Database.GetDefaultSlotForType(TargetSlot);
                GameData.Player.Equipment.Equip(targetSlotIdx, null);
            }));

            items.Add(new ContextMenuItem("Info", () => 
            {
                if (ItemInformationManager.Instance != null)
                {
                    ItemInformationManager.Instance.Open(EquipmentItem);
                }
                else
                {
                    Debug.Log($"[Info] {EquipmentItem.Name}: {EquipmentItem.Item.Description}");
                }
            }));
        }

        return items;
    }

    private void Start()
    {
        SlotButton = GetComponent<Button>();
        if (SlotButton != null)
        {
            SlotButton.onClick.AddListener(() =>
            {
                if (EquipmentItem != null && ContextMenuManager.Instance != null)
                {
                    var items = GetContextMenuItems();
                    if (items != null && items.Count > 0)
                    {
                        ContextMenuManager.Instance.Show(items, UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero);
                    }
                }
            });
        }
    }
}
