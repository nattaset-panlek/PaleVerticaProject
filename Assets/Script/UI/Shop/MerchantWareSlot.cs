using System;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Shop;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.Utility;
using CoreSystem.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI.Shop
{
    [DisallowMultipleComponent]
    public class MerchantWareSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region [Variables/Fields]
        public const string SoldOutLabel = "Sold out";
        private static readonly Vector2 DragSize = new Vector2(64f, 64f);

        [SerializeField] private ItemCardView _card;

        private ShopSession _session;
        private int _entryIndex = -1;
        private int _row = -1;

        public static event Action<ShopSession, int> OnClicked;
        public static event Action<ShopSession, int> OnDoubleClicked;
        public static event Action<ShopSession, int> OnInfoRequested;
        #endregion

        #region [Properties]
        public ItemCardView Card => _card;
        public int EntryIndex => _entryIndex;
        #endregion

        #region [Public Methods]
        public void Bind(ShopSession session, int entryIndex, int row, bool available)
        {
            _session = session;
            _entryIndex = entryIndex;
            _row = row;
            if (_card == null) return;
            _card.Show(BuildDisplay(session, entryIndex));
            _card.SetInteractable(available && session != null && !session.IsSoldOut(entryIndex));
        }

        public void SetFocused(bool focused)
        {
            if (_card != null) _card.SetFocused(focused);
        }

        public static ItemCardDisplay BuildDisplay(ShopSession session, int entryIndex)
        {
            if (session == null || entryIndex < 0 || entryIndex >= session.Entries.Count) return default;
            ShopEntry entry = session.Entries[entryIndex];
            Item item = entry.Item;
            if (item == null) return default;

            var template = item is Equipment ? new EquipmentInstance(-1, item.Id) : new ItemInstance(-1, item.Id, 1);
            ItemCardDisplay display = ItemCardDisplay.From(template);
            display.Badge = null;
            display.Label = session.IsSoldOut(entryIndex) ? SoldOutLabel : NumberFormatter.FormatWithCommas(entry.Price);
            if (!entry.IsUnlimited && !(item is Equipment) && entry.Stock > 1) display.Badge = "x" + NumberFormatter.Format(session.Remaining(entryIndex));
            return display;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_session == null || _entryIndex < 0 || eventData.dragging) return;
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnInfoRequested?.Invoke(_session, _entryIndex);
                return;
            }
            if (eventData.button != PointerEventData.InputButton.Left || _card == null || !_card.Interactable) return;
            if (eventData.clickCount >= 2) OnDoubleClicked?.Invoke(_session, _row);
            else OnClicked?.Invoke(_session, _row);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_session == null || _entryIndex < 0 || _card == null || !_card.Interactable) return;
            if (eventData.button != PointerEventData.InputButton.Left || GlobalDragManager.Instance == null) return;
            Item item = _session.Entries[_entryIndex].Item;
            if (item == null) return;
            GlobalDragManager.Instance.StartDrag(this, new ShopWarePayload(_session, _entryIndex, _row), item.Icon, eventData.position, DragSize);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (GlobalDragManager.Source == this && GlobalDragManager.Instance != null) GlobalDragManager.Instance.UpdatePosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (GlobalDragManager.Source == this && GlobalDragManager.Instance != null) GlobalDragManager.Instance.FinishDrag();
        }
        #endregion
    }
}
