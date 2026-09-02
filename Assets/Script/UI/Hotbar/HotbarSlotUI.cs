using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.UI.Utile;
using CoreSystem.UI.Inventory;

namespace CoreSystem.UI.Hotbar
{
    public class HotbarSlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        #region [Variables/Fields]
        [Header("UI References")]
        [SerializeField] private Button _actionButton;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private GameObject _skillSelected;
        [SerializeField] private GameObject _overlay;
        [SerializeField] private Image _overlayImage;
        [SerializeField] private GameObject _onUse;
        [SerializeField] private GameObject _onEmpty;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _overlayValueText;
        [SerializeField] private TMP_Text _numHotSlotText;
        [SerializeField] private GameObject _comboHintObj;

        [Header("Slot Configuration")]
        [SerializeField] private int _rowIndex;
        [SerializeField] private int _slotIndex;
        private HotbarSlotData _slotData = new HotbarSlotData();

        [Header("Drag & Hold Settings")]
        [SerializeField] private float _holdThreshold = 0.15f;
        [SerializeField] private float _dragDistanceThreshold = 10f;

        private bool _isPointerDown = false;
        private bool _isDragging = false;
        private float _pointerDownTime;
        private Vector2 _pressPosition;
        private Canvas _parentCanvas;
        #endregion

        #region [Properties]
        public Button ActionButton => _actionButton;
        public Image SkillIcon => _skillIcon;
        public GameObject SkillSelected => _skillSelected;
        public GameObject Overlay => _overlay;
        public Image OverlayImage => _overlayImage;
        public GameObject OnUse => _onUse;
        public GameObject OnEmpty => _onEmpty;
        public TMP_Text CostText => _costText;
        public TMP_Text OverlayValueText => _overlayValueText;
        public TMP_Text NumHotSlotText => _numHotSlotText;

        public int RowIndex => _rowIndex;
        public int SlotIndex => _slotIndex;
        public HotbarSlotData SlotData => _slotData;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            UpdateTimersAndSelection();
            CheckHoldToDrag();
        }

        private void OnDisable()
        {
            EndDragVisual();
        }

        private void OnDestroy()
        {
            EndDragVisual();
        }
        #endregion

        #region [Event System Interfaces]
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            _isPointerDown = true;
            _isDragging = false;
            _pointerDownTime = Time.unscaledTime;
            _pressPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            bool wasDragging = _isDragging;
            if (_isPointerDown && !wasDragging)
            {
                ExecuteAction();
            }

            _isPointerDown = false;
            if (!eventData.dragging)
            {
                EndDragVisual();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ShowContextMenu(eventData.position);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_slotData == null || _slotData.IsEmpty) return;

            if (!_isDragging)
            {
                Vector2 delta = eventData.position - _pressPosition;
                if (delta.magnitude >= _dragDistanceThreshold || (Time.unscaledTime - _pointerDownTime) >= _holdThreshold)
                {
                    StartDrag(eventData);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (!_isDragging && _isPointerDown && _slotData != null && !_slotData.IsEmpty)
            {
                Vector2 delta = eventData.position - _pressPosition;
                if (delta.magnitude >= _dragDistanceThreshold)
                {
                    StartDrag(eventData);
                }
            }

            if (_isDragging)
            {
                MoveDragVisual(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragVisual();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (GlobalDragManager.Payload is HotbarSlotUI draggedSlot && draggedSlot != this)
            {
                SwapSlots(draggedSlot);
                return;
            }

            var draggedSkill = GlobalDragManager.Payload as SkillInstance;
            if (draggedSkill != null && draggedSkill.SkillData != null)
            {
                AssignSkill(draggedSkill.SkillData.Id);
                return;
            }

            var draggedItem = GlobalDragManager.Payload as ItemInstance;
            if (draggedItem != null && draggedItem.Item != null)
            {
                // [FUTURE SUBCATEGORY]: In the future, consumable subcategories will be defined and filtered here.
                // Currently allowing all consumable items from inventory to be dragged and assigned to hotbar slots.
                if (draggedItem.Item is Consumable)
                {
                    AssignItem(draggedItem.Item.Id, draggedItem.Id);
                }
                else
                {
                    Debug.Log($"[HotbarSlotUI] Cannot assign item '{draggedItem.Item.Name}' - Only Consumables can be assigned to Hotbar.");
                }
                return;
            }

            if (GlobalDragManager.Payload != null)
            {
                Debug.Log($"[HotbarSlotUI] Unsupported drop payload type: {GlobalDragManager.Payload.GetType().Name}");
            }
        }
        #endregion

        #region [Public Methods]
        public void SetupSlot(int rowIndex, int slotIndex, string hotkeyText, HotbarSlotData data = null)
        {
            _rowIndex = rowIndex;
            _slotIndex = slotIndex;
            
            if (data != null)
            {
                _slotData = data;
            }
            else if (_slotData == null)
            {
                _slotData = new HotbarSlotData();
            }

            _slotData.HotkeyText = hotkeyText;

            if (_numHotSlotText != null)
            {
                _numHotSlotText.text = hotkeyText;
            }

            RefreshUI();
        }

        public void AssignSkill(int skillId)
        {
            _slotData.SetSkill(skillId);
            RefreshUI();
            SyncWithSkillCaster();
        }

        public void AssignItem(int itemId, long instanceId = -1)
        {
            _slotData.SetItem(itemId, instanceId);
            RefreshUI();
            SyncWithSkillCaster();
        }

        public void ClearSlot()
        {
            EndDragVisual();
            _slotData.Clear();
            RefreshUI();
            SyncWithSkillCaster();
        }

        public void SwapSlots(HotbarSlotUI otherSlot)
        {
            if (otherSlot == null || otherSlot == this) return;

            var temp = _slotData;
            _slotData = otherSlot.SlotData;
            otherSlot._slotData = temp;

            RefreshUI();
            otherSlot.RefreshUI();

            SyncWithSkillCaster();
            otherSlot.SyncWithSkillCaster();
        }

        private bool _isSyncingWithCaster = false;

        public void SyncWithSkillCaster()
        {
            if (_isSyncingWithCaster) return;
            _isSyncingWithCaster = true;

            try
            {
                var player = GameData.Player.PlayerCharacterModel;
                if (player == null || player.skillCaster == null) return;

                if (_rowIndex == 0)
                {
                    if (_slotData != null && _slotData.ActionType == HotbarActionType.Skill)
                    {
                        ActiveSkill skill = Database.GetSynchronizedSkill(_slotData.SkillId);
                        player.skillCaster.EquipSkill(skill, _slotIndex);
                    }
                    else
                    {
                        player.skillCaster.EquipSkill((ActiveSkill)null, _slotIndex);
                    }
                }
            }
            finally
            {
                _isSyncingWithCaster = false;
            }
        }

        public void ExecuteAction()
        {
            if (_slotData == null || _slotData.IsEmpty) return;

            var player = GameData.Player.PlayerCharacterModel;
            if (player == null) return;

            switch (_slotData.ActionType)
            {
                case HotbarActionType.Skill:
                    if (player.skillCaster != null)
                    {
                        var grantedSkill = player.skillCaster.GrantedSkills.Find(s => s.SkillData != null && s.SkillData.Id == _slotData.SkillId);
                        if (grantedSkill != null && CoreSystem.Controllers.PlayerController.Instance != null)
                        {
                            var controller = CoreSystem.Controllers.PlayerController.Instance;
                            controller.TryEnterAimingMode(grantedSkill, controller.GetMouseWorldPosition());
                        }
                        else if (grantedSkill != null)
                        {
                            player.skillCaster.CastSkill(grantedSkill);
                        }
                    }
                    break;

                case HotbarActionType.Item:
                    ItemInstance itemInst = null;
                    if (_slotData.ItemInstanceId >= 0)
                    {
                        itemInst = GameData.Player.Inventory.GetByInstanceId(_slotData.ItemInstanceId);
                    }
                    if (itemInst == null && _slotData.ItemId >= 0)
                    {
                        itemInst = GameData.Player.Inventory.GetFirstByModelId(_slotData.ItemId);
                    }

                    if (itemInst != null && itemInst.Item is Consumable consumable)
                    {
                        consumable.OnUse();
                        GameData.Player.Inventory.RemoveItemByInstance(itemInst.Id, 1);

                        ItemInstance remainingInst = null;
                        if (_slotData.ItemInstanceId >= 0)
                        {
                            remainingInst = GameData.Player.Inventory.GetByInstanceId(_slotData.ItemInstanceId);
                        }
                        if (remainingInst == null && _slotData.ItemId >= 0)
                        {
                            remainingInst = GameData.Player.Inventory.GetFirstByModelId(_slotData.ItemId);
                        }

                        if (remainingInst == null || remainingInst.Quantity <= 0)
                        {
                            ClearSlot();
                        }
                        else
                        {
                            RefreshUI();
                        }
                    }
                    break;
            }
        }

        public void RefreshUI()
        {
            bool hasAction = _slotData != null && !_slotData.IsEmpty;

            if (_onUse != null) _onUse.SetActive(hasAction);
            if (_onEmpty != null) _onEmpty.SetActive(!hasAction);

            if (!hasAction)
            {
                if (_skillIcon != null) _skillIcon.gameObject.SetActive(false);
                if (_skillSelected != null) _skillSelected.SetActive(false);
                if (_overlay != null) _overlay.SetActive(false);
                if (_costText != null) _costText.text = string.Empty;
                if (_overlayValueText != null) _overlayValueText.text = string.Empty;
                return;
            }

            if (_skillIcon != null) _skillIcon.gameObject.SetActive(true);

            switch (_slotData.ActionType)
            {
                case HotbarActionType.Skill:
                    ActiveSkill skillData = Database.GetSynchronizedSkill(_slotData.SkillId);
                    if (skillData != null)
                    {
                        var player = GameData.Player?.PlayerCharacterModel;
                        var overrideData = ActiveSkill.GetActiveChainOverride(player, skillData);
                        var effectiveSkill = (overrideData != null && overrideData.ReplacementSkill != null) ? overrideData.ReplacementSkill : skillData;

                        if (_skillIcon != null && effectiveSkill.Icon != null)
                        {
                            _skillIcon.sprite = effectiveSkill.Icon;
                        }
                        if (_costText != null)
                        {
                            _costText.text = $"{effectiveSkill.BaseManaCost:F0} MP";
                        }

                        if (_comboHintObj != null)
                        {
                            _comboHintObj.SetActive(overrideData != null);
                        }
                    }
                    break;

                case HotbarActionType.Item:
                    Item itemData = Database.GetItem(_slotData.ItemId);
                    if (itemData != null)
                    {
                        ItemInstance inst = null;
                        if (_slotData.ItemInstanceId >= 0)
                        {
                            inst = GameData.Player.Inventory.GetByInstanceId(_slotData.ItemInstanceId);
                        }
                        if (inst == null && _slotData.ItemId >= 0)
                        {
                            inst = GameData.Player.Inventory.GetFirstByModelId(_slotData.ItemId);
                        }

                        if (inst == null || inst.Quantity <= 0)
                        {
                            ClearSlot();
                            return;
                        }

                        if (_skillIcon != null && itemData.Icon != null)
                        {
                            _skillIcon.sprite = itemData.Icon;
                        }
                        if (_costText != null)
                        {
                            _costText.text = "Instant";
                        }
                    }
                    break;
            }
        }
        #endregion

        #region [Private Methods]
        private void ShowContextMenu(Vector2 screenPos)
        {
            if (_slotData == null || _slotData.IsEmpty || ContextMenuManager.Instance == null) return;

            List<ContextMenuItem> items = new List<ContextMenuItem>();

            items.Add(new ContextMenuItem("Execute", () =>
            {
                ExecuteAction();
            }));

            items.Add(new ContextMenuItem("Remove Slot", () =>
            {
                ClearSlot();
            }));

            if (_slotData.ActionType == HotbarActionType.Item)
            {
                ItemInstance itemInst = null;
                if (_slotData.ItemInstanceId >= 0)
                {
                    itemInst = GameData.Player.Inventory.GetByInstanceId(_slotData.ItemInstanceId);
                }
                if (itemInst == null && _slotData.ItemId >= 0)
                {
                    itemInst = GameData.Player.Inventory.GetFirstByModelId(_slotData.ItemId);
                }

                if (itemInst != null)
                {
                    items.Add(new ContextMenuItem("Info", () =>
                    {
                        if (ItemInformationManager.Instance != null)
                        {
                            ItemInformationManager.Instance.Open(itemInst);
                        }
                        else
                        {
                            Debug.Log($"[Info] {itemInst.Name}: {itemInst.Item.Description}");
                        }
                    }));
                }
            }

            ContextMenuManager.Instance.Show(items, screenPos);
        }

        private void CheckHoldToDrag()
        {
            if (_isPointerDown && !_isDragging && _slotData != null && !_slotData.IsEmpty)
            {
                if ((Time.unscaledTime - _pointerDownTime) >= _holdThreshold)
                {
                    PointerEventData ped = new PointerEventData(EventSystem.current)
                    {
                        position = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : _pressPosition
                    };
                    StartDrag(ped);
                }
            }
        }

        private void StartDrag(PointerEventData eventData)
        {
            _isDragging = true;

            Sprite icon = _skillIcon != null ? _skillIcon.sprite : null;
            RectTransform myRect = GetComponent<RectTransform>();
            Vector2 size = myRect != null ? myRect.sizeDelta : new Vector2(50, 50);

            Color? rarityColor = null;
            if (_slotData != null && !_slotData.IsEmpty && _slotData.ActionType == HotbarActionType.Item)
            {
                Item itemData = Database.GetItem(_slotData.ItemId);
                if (itemData != null)
                {
                    rarityColor = CoreSystem.ItemRarityExtensions.GetColor(itemData.Rarity);
                }
            }

            GlobalDragManager.Instance.StartDrag(
                source: this,
                payload: this,
                icon: icon,
                screenPos: eventData.position,
                customSize: size,
                raritySprite: null,
                rarityColor: rarityColor
            );
        }

        private void MoveDragVisual(Vector2 screenPos)
        {
            if (_isDragging)
            {
                GlobalDragManager.Instance.UpdatePosition(screenPos);
            }
        }

        private void EndDragVisual()
        {
            if (_isDragging)
            {
                GlobalDragManager.Instance.FinishDrag();
            }

            _isDragging = false;
            _isPointerDown = false;
        }

        private void UpdateTimersAndSelection()
        {
            if (_slotData == null || _slotData.IsEmpty) return;

            var player = GameData.Player.PlayerCharacterModel;
            if (player == null) return;

            if (_slotData.ActionType == HotbarActionType.Skill)
            {
                if (player.skillCaster == null) return;

                var grantedSkill = player.skillCaster.GrantedSkills.Find(s => s.SkillData != null && s.SkillData.Id == _slotData.SkillId);
                
                // 1. Skill Selected / Aiming / Active / Toggle State
                bool isSelected = false;
                if (grantedSkill != null)
                {
                    // A. Aiming Mode
                    if (player.skillCaster.AimedSkillInstance == grantedSkill ||
                        (player.skillCaster.AimedSkillSlot >= 0 &&
                         player.skillCaster.AimedSkillSlot < player.skillCaster.EquippedSkills.Count &&
                         player.skillCaster.EquippedSkills[player.skillCaster.AimedSkillSlot] == grantedSkill))
                    {
                        isSelected = true;
                    }

                    // B. Active Cast State
                    if (player.skillCaster.ActiveSkillInstance == grantedSkill)
                    {
                        isSelected = true;
                    }

                    // C. Active Toggle Skill State
                    if (grantedSkill.IsToggledOn)
                    {
                        isSelected = true;
                    }
                }

                if (_skillSelected != null)
                {
                    _skillSelected.SetActive(isSelected);
                }

                // Skill Chain / Combo Override Dynamic Update
                ActiveSkill baseSkill = Database.GetSynchronizedSkill(_slotData.SkillId);
                if (baseSkill != null)
                {
                    var overrideData = ActiveSkill.GetActiveChainOverride(player, baseSkill);
                    if (overrideData != null && overrideData.ReplacementSkill != null)
                    {
                        if (_skillIcon != null && overrideData.ReplacementSkill.Icon != null)
                        {
                            _skillIcon.sprite = overrideData.ReplacementSkill.Icon;
                        }
                        if (_costText != null)
                        {
                            _costText.text = $"{overrideData.ReplacementSkill.BaseManaCost:F0} MP";
                        }
                        if (_comboHintObj != null && !_comboHintObj.activeSelf)
                        {
                            _comboHintObj.SetActive(true);
                        }
                    }
                    else
                    {
                        if (_skillIcon != null && baseSkill.Icon != null)
                        {
                            _skillIcon.sprite = baseSkill.Icon;
                        }
                        if (_costText != null)
                        {
                            _costText.text = $"{baseSkill.BaseManaCost:F0} MP";
                        }
                        if (_comboHintObj != null && _comboHintObj.activeSelf)
                        {
                            _comboHintObj.SetActive(false);
                        }
                    }
                }

                // 2. Cooldown / VCT / Delay Timer Overlay
                float activeTimer = 0f;
                float totalTimer = 1f;

                if (grantedSkill != null && !grantedSkill.IsReady())
                {
                    activeTimer = grantedSkill.CurrentCooldown;
                    totalTimer = grantedSkill.MaxCooldown > 0f ? grantedSkill.MaxCooldown : 1f;
                }
                else if (player.skillCaster.CurrentState == SkillCasterState.Casting && player.skillCaster.ActiveSkillInstance == grantedSkill)
                {
                    activeTimer = player.skillCaster.CurrentCastTimeTotal - player.skillCaster.CurrentCastTimer;
                    totalTimer = player.skillCaster.CurrentCastTimeTotal > 0f ? player.skillCaster.CurrentCastTimeTotal : 1f;
                }
                else if (player.skillCaster.GlobalCastDelayTimer > 0f)
                {
                    activeTimer = player.skillCaster.GlobalCastDelayTimer;
                    totalTimer = 0.5f;
                }

                if (activeTimer > 0f)
                {
                    if (_overlay != null) _overlay.SetActive(true);
                    if (_overlayImage != null) _overlayImage.fillAmount = Mathf.Clamp01(activeTimer / totalTimer);
                    if (_overlayValueText != null) _overlayValueText.text = activeTimer.ToString("0.00");
                }
                else
                {
                    if (_overlay != null) _overlay.SetActive(false);
                    if (_overlayValueText != null) _overlayValueText.text = string.Empty;
                }
            }
            else
            {
                if (_skillSelected != null) _skillSelected.SetActive(false);
                if (_overlay != null) _overlay.SetActive(false);
                if (_overlayValueText != null) _overlayValueText.text = string.Empty;
            }
        }
        #endregion
    }
}
