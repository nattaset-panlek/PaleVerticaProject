using CoreSystem.UI.Utility;
using UnityEngine.UI;
using UnityEngine;
using CoreSystem.Data.Items;
using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Inventory;
using CoreSystem;
using UnityEngine.EventSystems;
using CoreSystem.UI.GameInterface;

namespace CoreSystem.UI.Inventory.Utility
{
    public class ItemSlotGlobal : MonoBehaviour, IPointerClickHandler, IItemProvider, IDropHandler
    {
        public ItemInstance itemInstance;
        public bool ReadOnly;
        public Image ItemIcon;
        public Image RarityFrame;

        public ItemInstance GetItem() => itemInstance;
        public Sprite GetIcon() => ItemIcon != null ? ItemIcon.sprite : null;

        public void OnDrop(PointerEventData eventData)
        {
            if (ReadOnly) return;

            var draggedItem = GlobalDragManager.Payload as ItemInstance ?? ItemDragHandler.DraggedItem;
            var activeSource = GlobalDragManager.Source as IItemProvider ?? ItemDragHandler.ActiveSource;

            if (draggedItem == null)
            {
                if (GlobalDragManager.Payload != null)
                {
                    Debug.Log($"[ItemSlotGlobal] Incompatible drop payload type: {GlobalDragManager.Payload.GetType().Name}");
                }
                return;
            }

            // If we drop an EQUIPPED item into the inventory, unequip it
            if (draggedItem is EquipmentInstance equipInst && equipInst.IsEquipped)
            {
                if (activeSource is EquipmentSlotUI sourceSlot)
                {
                    int slotIdx = Database.GetDefaultSlotForType(sourceSlot.TargetSlot);
                    GameData.Player.Equipment.Equip(slotIdx, null);
                }
            }
        }


        public virtual void UpdateUI(ItemInstance item)
        {
            itemInstance = item;

            if (itemInstance != null && itemInstance.Item != null)
            {
                // ... (rest of old code will be matched by TargetContent)

                if (RarityFrame != null)
                {
                    RarityFrame.color = ItemRarityExtensions.GetColor(itemInstance.Item.Rarity);
                }
                if (ItemIcon != null)
                    ItemIcon.sprite = itemInstance.Item.Icon;

                // Hoverable Tooltip
                var hoverable = GetComponent<HoverableUI>() ?? gameObject.AddComponent<HoverableUI>();

                string ItemSocketInfo = "";
                if(item.Item is Equipment equipment)
                {
                    if (equipment.MaxSockets > 0)
                    {
                        ItemSocketInfo = $"[{equipment.MaxSockets}] ";
                    }
                }

                hoverable.Setup($"{ItemSocketInfo}<color=#{ItemRarityExtensions.GetHexColor(itemInstance.Item.Rarity)}>{itemInstance.Name}</color>",itemInstance.Item.Description);

                if (TryGetComponent(out ContextMenuUI legacyMenu)) legacyMenu.enabled = false;

                // Drag support
                if (!ReadOnly && GetComponent<ItemDragHandler>() == null)
                    gameObject.AddComponent<ItemDragHandler>();
            }
        }


        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (itemInstance == null || itemInstance.Item == null) return;

            if (ReadOnly)
            {
                if (eventData.button == PointerEventData.InputButton.Right && ItemInformationManager.Instance != null) ItemInformationManager.Instance.Open(itemInstance);
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ShowContextMenu(eventData.position);
            }
            else if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 2)
            {
                UsePrimaryAction();
            }
        }

        public void ShowContextMenu(Vector2 screenPosition)
        {
            if (ContextMenuManager.Instance == null) return;
            var items = GetDefaultContextMenuItems();
            if (items != null && items.Count > 0) ContextMenuManager.Instance.Show(items, screenPosition);
        }

        public bool UsePrimaryAction()
        {
            var captured = itemInstance;
            if (captured == null || captured.Item == null) return false;

            if (captured is EquipmentInstance equipInst && equipInst.Item is Equipment equipment)
            {
                if (equipInst.IsEquipped) GameData.Player.Equipment.Equip(Database.GetDefaultSlotForType(equipment.Slot), null);
                else GameData.Player.Equipment.SmartEquip(captured);
                return true;
            }

            if (captured is CostumeInstance costumeInst && costumeInst.Item is Costume costume)
            {
                if (costumeInst.IsEquipped) GameData.Player.Costumes.Unequip((int)costume.Slot);
                else GameData.Player.Costumes.Equip(captured);
                return true;
            }

            if (captured.Item is Consumable || captured.Item is SkillRune)
            {
                UseItem(captured);
                return true;
            }

            return false;
        }

        protected virtual List<ContextMenuItem> GetDefaultContextMenuItems()
        {
            var captured = itemInstance;
            var items = ItemInsertRegistry.BuildContextMenuItems(captured);

            if (captured is EquipmentInstance equipInst && equipInst.Item is Equipment equipment)
            {
                if (equipInst.IsEquipped)
                {
                    items.Add(new ContextMenuItem("Unequip", () =>
                    {
                        int targetSlot = Database.GetDefaultSlotForType(equipment.Slot);
                        GameData.Player.Equipment.Equip(targetSlot, null);
                    }));
                }
                else
                {
                    items.Add(new ContextMenuItem("Equip", () =>
                    {
                        // SmartEquip จะตัดสิน slot ที่ถูกต้องเองอัตโนมัติ
                        GameData.Player.Equipment.SmartEquip(captured);
                    }));
                }
            }
            else if (captured is CostumeInstance costumeInst && costumeInst.Item is Costume costume)
            {
                if (costumeInst.IsEquipped)
                {
                    items.Add(new ContextMenuItem("Take Off", () =>
                    {
                        int slotIdx = (int)costume.Slot;
                        GameData.Player.Costumes.Unequip(slotIdx);
                    }));
                }
                else
                {
                    items.Add(new ContextMenuItem("Wear", () =>
                    {
                        GameData.Player.Costumes.Equip(captured);
                    }));
                }
            }

            if (captured.Item is Consumable || captured.Item is SkillRune)
            {
                items.Add(new ContextMenuItem("Use", () => UseItem(captured)));
            }

            items.Add(new ContextMenuItem("Info", () =>
            {
                if (ItemInformationManager.Instance != null)
                {
                    ItemInformationManager.Instance.Open(captured);
                }
                else
                {
                    Debug.Log($"[Info] {captured.Name}: {captured.Item.Description}");
                }
            }));
            items.Add(new ContextMenuItem("Discard", () => Debug.Log($"[Discard] {captured.Name}")));

            return items;
        }

        private static void UseItem(ItemInstance instance)
        {
            instance.Item.OnUse();
            if (instance.Item is CoreSystem.Data.Items.Model.Consumable consumable && !consumable.LastUseConsumed) return;
            if (GameData.Player != null && GameData.Player.Inventory != null)
            {
                GameData.Player.Inventory.RemoveItemByInstance(instance.Id, 1);
            }
        }

        protected virtual void OnEnable()
        {
            if (GameData.Player != null)
            {
                if (GameData.Player.Equipment != null)
                    GameData.Player.Equipment.OnEquipmentChanged += OnEquipmentChanged;

                if (GameData.Player.Costumes != null)
                    GameData.Player.Costumes.OnCostumeChanged += OnCostumeChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (GameData.Player != null)
            {
                if (GameData.Player.Equipment != null)
                    GameData.Player.Equipment.OnEquipmentChanged -= OnEquipmentChanged;

                if (GameData.Player.Costumes != null)
                    GameData.Player.Costumes.OnCostumeChanged -= OnCostumeChanged;
            }
        }

        protected virtual void OnEquipmentChanged(EquipmentInstance equipmentInstance)
        {
            UpdateUI(itemInstance);
        }

        protected virtual void OnCostumeChanged(CostumeInstance costumeInstance)
        {
            UpdateUI(itemInstance);
        }
    }
}
