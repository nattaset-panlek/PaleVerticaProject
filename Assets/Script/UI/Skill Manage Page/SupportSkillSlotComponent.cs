using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CoreSystem.Skills;
using CoreSystem.UI.Utility;
using CoreSystem.UI.Inventory;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.UI.SkillManagePage
{
    public class SupportSkillSlotComponent : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        #region [Variables/Fields]
        [SerializeField] private Button _slotButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _activeSlot;
        [SerializeField] private Image _skillIcon;

        private int _slotIndex;
        private SupportModifier _linkedModifier;
        private System.Action<int> _onClickAction;
        #endregion

        #region [Properties]
        public int SlotIndex => _slotIndex;
        public SupportModifier LinkedModifier => _linkedModifier;
        public Button SlotButton => _slotButton;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public GameObject ActiveSlot => _activeSlot;
        public Image SkillIcon => _skillIcon;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.AddListener(OnSlotClicked);
            }
        }
        #endregion

        #region [Public Methods]
        public void Setup(int slotIndex, SupportModifier modifier, bool isUnlocked, System.Action<int> onClickAction = null)
        {
            _slotIndex = slotIndex;
            _linkedModifier = modifier;
            _onClickAction = onClickAction;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isUnlocked ? 1f : 0.5f;
                _canvasGroup.interactable = isUnlocked;
                _canvasGroup.blocksRaycasts = isUnlocked;
            }

            if (_slotButton != null)
            {
                _slotButton.interactable = isUnlocked;
            }

            if (_activeSlot != null)
            {
                _activeSlot.SetActive(modifier != null);
            }

            if (_skillIcon != null)
            {
                if (modifier != null && modifier.Icon != null)
                {
                    _skillIcon.sprite = modifier.Icon;
                    _skillIcon.enabled = true;
                }
                else
                {
                    _skillIcon.sprite = null;
                    _skillIcon.enabled = false;
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_canvasGroup != null && (!_canvasGroup.interactable || _canvasGroup.alpha < 1f)) return;

            var draggedItem = GlobalDragManager.Payload as ItemInstance ?? ItemDragHandler.DraggedItem;
            if (draggedItem == null || !(draggedItem.Item is SupportRune supportRune)) return;

            SupportModifier mod = supportRune.GetSupportModifier();
            if (mod != null)
            {
                SkillManageWindow window = GetComponentInParent<SkillManageWindow>();
                if (window != null)
                {
                    window.LinkSupportSkillToSelected(mod, _slotIndex, draggedItem);
                }

                GlobalDragManager.Instance.FinishDrag();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_linkedModifier == null || ContextMenuManager.Instance == null) return;

                List<ContextMenuItem> items = new List<ContextMenuItem>();

                items.Add(new ContextMenuItem("Info", () =>
                {
                    int itemId = 7000 + (_linkedModifier.ID % 1000);
                    var itemInstance = new ItemInstance(-1, itemId, 1);
                    if (ItemInformationManager.Instance != null)
                    {
                        ItemInformationManager.Instance.Open(itemInstance);
                    }
                }));

                items.Add(new ContextMenuItem("UnEquip", () =>
                {
                    SkillManageWindow window = GetComponentInParent<SkillManageWindow>();
                    if (window != null)
                    {
                        window.UnlinkSupportSkillFromSelected(_slotIndex);
                    }
                }));

                ContextMenuManager.Instance.Show(items, eventData.position);
            }
        }
        #endregion

        #region [Private Methods]
        private void OnSlotClicked()
        {
            _onClickAction?.Invoke(_slotIndex);
        }
        #endregion
    }
}
