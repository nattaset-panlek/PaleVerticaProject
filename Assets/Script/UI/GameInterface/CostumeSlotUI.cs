using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Inventory;
using CoreSystem.UI.Utile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CostumeSlotUI : MonoBehaviour, IItemProvider, IDropHandler
{
    #region [Variables/Fields]

    private Button SlotButton;
    public CostumeInstance CostumeItem;

    public CostumeSlot TargetSlot;
    public Image Icon;
    public Image RarityFrame;

    #endregion

    #region [Properties]

    public ItemInstance GetItem() => CostumeItem;
    public Sprite GetIcon() => (Icon != null && Icon.gameObject.activeSelf) ? Icon.sprite : null;

    #endregion

    #region [Unity Methods]

    private void Awake()
    {
        if (GameData.Player?.CostumeManager != null)
            GameData.Player.CostumeManager.OnCostumeChanged += UpdateSlot;
    }

    private void OnDestroy()
    {
        if (GameData.Player?.CostumeManager != null)
            GameData.Player.CostumeManager.OnCostumeChanged -= UpdateSlot;
    }

    private void OnEnable()
    {
        UpdateSlot(null);
    }

    private void Start()
    {
        SlotButton = GetComponent<Button>();
        if (SlotButton != null)
        {
            SlotButton.onClick.AddListener(() =>
            {
                if (CostumeItem != null && ContextMenuManager.Instance != null)
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

    #endregion

    #region [Public Methods]

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = GlobalDragManager.Payload as ItemInstance ?? ItemDragHandler.DraggedItem;
        if (draggedItem == null)
        {
            if (GlobalDragManager.Payload != null)
            {
                Debug.Log($"[CostumeSlotUI] Incompatible drop payload type: {GlobalDragManager.Payload.GetType().Name}");
            }
            return;
        }

        if (draggedItem is CostumeInstance costumeInst && costumeInst.Item is Costume costume)
        {
            // ตรวจว่า costume ประเภทนี้สามารถใส่ใน slot ของ UI นี้ได้ไหม
            if (costume.Slot != TargetSlot)
            {
                Debug.Log($"[CostumeSlotUI] {costume.Name} ({costume.Slot}) ไม่สามารถใส่ใน {TargetSlot} slot ได้");
                return;
            }

            GameData.Player.CostumeManager.Equip(costumeInst);
        }
    }

    public void UpdateSlot(CostumeInstance changedInstance)
    {
        CostumeInstance currentCostume = GameData.Player.CostumeManager.GetCostume(TargetSlot);
        CostumeItem = currentCostume;

        var hoverableUI = GetComponent<HoverableUI>() ?? gameObject.AddComponent<HoverableUI>();
        if (currentCostume != null && currentCostume.Item != null)
        {
            hoverableUI.Setup(
                $"<color=#{ItemRarityExtensions.GetHexColor(currentCostume.Item.Rarity)}>{currentCostume.Name}</color>",
                currentCostume.Item.Description
            );
        }
        else
        {
            hoverableUI.Setup($"{TargetSlot} Costume Slot", $"Empty slot for {TargetSlot} costume.");
        }

        var ctx = GetComponent<ContextMenuUI>() ?? gameObject.AddComponent<ContextMenuUI>();

        if (currentCostume != null && currentCostume.Item is Costume costumeModel)
        {
            if (RarityFrame != null)
                RarityFrame.color = ItemRarityExtensions.GetColor(costumeModel.Rarity);

            if (Icon != null)
            {
                Icon.gameObject.SetActive(true);
                Icon.color = Color.white;
                Icon.sprite = costumeModel.Icon;
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
                Icon.sprite = null;
            }

            ctx.ClearMenu();
        }

        // Drag support
        if (GetComponent<ItemDragHandler>() == null)
            gameObject.AddComponent<ItemDragHandler>();
    }

    #endregion

    #region [Private Methods]

    private List<ContextMenuItem> GetContextMenuItems()
    {
        var items = new List<ContextMenuItem>();

        if (CostumeItem != null && CostumeItem.Item is Costume costume)
        {
            items.Add(new ContextMenuItem("Unequip", () =>
            {
                GameData.Player.CostumeManager.Unequip((int)TargetSlot);
            }));

            items.Add(new ContextMenuItem("Info", () => 
            {
                if (ItemInformationManager.Instance != null)
                {
                    ItemInformationManager.Instance.Open(CostumeItem);
                }
                else
                {
                    Debug.Log($"[Info] {CostumeItem.Name}: {CostumeItem.Item.Description}");
                }
            }));
        }

        return items;
    }

    #endregion

    #region [ContextMenu Helpers]

    [ContextMenu("SetNameGameObject")]
    public void SetObjectName()
    {
        gameObject.name = $"{TargetSlot} Costume Slot";
    }

    #endregion
}
