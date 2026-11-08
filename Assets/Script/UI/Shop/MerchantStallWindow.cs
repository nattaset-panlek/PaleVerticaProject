using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Shop;
using CoreSystem.UI.Inventory;
using CoreSystem.UI.Visualizer;
using CoreSystem.Utility;
using GlobalVisualizer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Shop
{
    public class MerchantStallWindow : UIWindow
    {
        #region [Variables/Fields]
        public const int QuantityCap = 9999;

        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _waresContent;
        [SerializeField] private GameObject _wareItemPrefab;
        [SerializeField] private CartSlotView[] _cartSlots = new CartSlotView[0];
        [SerializeField] private TMP_Text _walletAmount;
        [SerializeField] private TMP_Text _total;
        [SerializeField] private Button _buyAllButton;
        [SerializeField] private QuantityModalView _quantityModal;

        private ShopSession _session;
        private PlayerInventory _inventory;
        private GlobalVisualizerView _waresView;
        private readonly List<int> _wareOrder = new List<int>();
        private int _focusedRow = -1;
        #endregion

        #region [Properties]
        public ShopSession Session => _session;
        public QuantityModalView QuantityModal => _quantityModal;
        public IReadOnlyList<CartSlotView> CartSlots => _cartSlots;
        public IReadOnlyList<int> WareOrder => _wareOrder;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseSelf);
            if (_buyAllButton != null) _buyAllButton.onClick.AddListener(BuyAll);
            if (_quantityModal != null) _quantityModal.gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            MerchantWareSlot.OnClicked += HandleWareClicked;
            MerchantWareSlot.OnDoubleClicked += HandleWareDoubleClicked;
            MerchantWareSlot.OnInfoRequested += HandleWareInfo;
            CartSlotView.OnWareDropped += HandleWareDropped;
            CartSlotView.OnRemoveRequested += HandleRemove;
            CartSlotView.OnInfoRequested += HandleCartInfo;
        }

        private void OnDisable()
        {
            MerchantWareSlot.OnClicked -= HandleWareClicked;
            MerchantWareSlot.OnDoubleClicked -= HandleWareDoubleClicked;
            MerchantWareSlot.OnInfoRequested -= HandleWareInfo;
            CartSlotView.OnWareDropped -= HandleWareDropped;
            CartSlotView.OnRemoveRequested -= HandleRemove;
            CartSlotView.OnInfoRequested -= HandleCartInfo;
            if (_quantityModal != null) _quantityModal.Close();
            Bind(null, null);
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            Bind(data as ShopSession, GameData.Player != null ? GameData.Player.Inventory : null);
        }

        public void Bind(ShopSession session, PlayerInventory inventory)
        {
            if (_session != null)
            {
                _session.OnCartChanged -= RefreshCart;
                _session.OnStockChanged -= HandleStockChanged;
            }
            if (_inventory != null) _inventory.OnInventoryChanged -= RefreshWallet;

            _session = session;
            _inventory = inventory;
            _focusedRow = -1;

            if (_session != null)
            {
                _session.OnCartChanged += RefreshCart;
                _session.OnStockChanged += HandleStockChanged;
            }
            if (_inventory != null) _inventory.OnInventoryChanged += RefreshWallet;
            if (_session == null)
            {
                ClearCartSlots();
                return;
            }

            BuildOrder();
            RefreshWares();
            RefreshCart();
            RefreshWallet();
        }

        public void FocusWare(int row)
        {
            _focusedRow = row;
            if (_waresView != null) _waresView.Refresh(true);
        }

        public void RequestQuantity(int entryIndex)
        {
            if (_session == null || _quantityModal == null) return;
            int max = MaxBuyable(entryIndex);
            if (max <= 0) return;
            if (_session.Entries[entryIndex].IsEquipment)
            {
                _session.AddToCart(entryIndex, 1);
                return;
            }
            Item item = _session.Entries[entryIndex].Item;
            int initial = Mathf.Max(1, max / 2);
            _quantityModal.Open(item != null ? item.Name : string.Empty, max, initial, quantity => _session.AddToCart(entryIndex, quantity));
        }

        public int MaxBuyable(int entryIndex)
        {
            if (_session == null) return 0;
            long max = Mathf.Min(_session.MaxAddable(entryIndex), QuantityCap);
            int price = _session.Entries[entryIndex].Price;
            if (price > 0 && _inventory != null)
            {
                long left = _inventory.GetCreditAmount() - _session.Total;
                max = System.Math.Min(max, System.Math.Max(0, left / price));
            }
            return (int)System.Math.Max(0, max);
        }

        public void BuyAll()
        {
            if (_session == null) return;
            _session.Checkout(_inventory);
        }
        #endregion

        #region [Private Methods]
        private void BuildOrder()
        {
            _wareOrder.Clear();
            for (int i = 0; i < _session.Entries.Count; i++)
            {
                if (_session.Entries[i].Item != null) _wareOrder.Add(i);
            }
            _wareOrder.Sort((a, b) =>
            {
                int rarity = RarityRank(_session.Entries[b].Item.Rarity).CompareTo(RarityRank(_session.Entries[a].Item.Rarity));
                return rarity != 0 ? rarity : a.CompareTo(b);
            });
            ExpandEquipmentRows();
        }

        private void ExpandEquipmentRows()
        {
            for (int row = _wareOrder.Count - 1; row >= 0; row--)
            {
                int entryIndex = _wareOrder[row];
                ShopEntry entry = _session.Entries[entryIndex];
                if (!entry.IsEquipment || entry.IsUnlimited) continue;
                int extra = _session.Remaining(entryIndex) - 1;
                for (int i = 0; i < extra; i++) _wareOrder.Insert(row, entryIndex);
            }
        }

        private static int RarityRank(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Legendary: return 6;
                case ItemRarity.Unique: return 5;
                case ItemRarity.Epic: return 4;
                case ItemRarity.Rare: return 3;
                case ItemRarity.Uncommon: return 2;
                case ItemRarity.Crafting: return 1;
                default: return 0;
            }
        }

        private void RefreshWares()
        {
            if (_waresContent == null || _wareItemPrefab == null) return;
            if (_waresView == null)
            {
                _waresView = VisualizerViewFactory.AttachTo(_waresContent, (RectTransform)_wareItemPrefab.transform);
                if (_waresView != null) _waresView.SetItems(_wareOrder, BindWare);
                return;
            }
            _waresView.NotifyReset();
        }

        private void BindWare(GameObject view, int entryIndex, int index)
        {
            MerchantWareSlot slot = view.GetComponent<MerchantWareSlot>();
            if (slot == null) return;
            slot.Bind(_session, entryIndex, index, IsRowAvailable(entryIndex, index));
            slot.SetFocused(index == _focusedRow);
        }

        private bool IsRowAvailable(int entryIndex, int row)
        {
            ShopEntry entry = _session.Entries[entryIndex];
            if (!entry.IsEquipment || entry.IsUnlimited) return true;
            int unit = row - _wareOrder.IndexOf(entryIndex);
            return unit < _session.Remaining(entryIndex) - _session.InCart(entryIndex);
        }

        private void HandleStockChanged()
        {
            _focusedRow = -1;
            BuildOrder();
            RefreshWares();
        }

        private void RefreshCart()
        {
            if (_session == null) return;
            if (_waresView != null) _waresView.Refresh(true);
            IReadOnlyList<ShopCartLine> cart = _session.Cart;
            for (int i = 0; i < _cartSlots.Length; i++)
            {
                if (_cartSlots[i] != null) _cartSlots[i].Bind(i < cart.Count ? cart[i] : null, i);
            }
            if (_total != null) _total.text = NumberFormatter.FormatWithCommas(_session.Total);
            RefreshBuyButton();
        }

        private void ClearCartSlots()
        {
            for (int i = 0; i < _cartSlots.Length; i++)
            {
                if (_cartSlots[i] != null) _cartSlots[i].Bind(null, i);
            }
        }

        private void RefreshWallet()
        {
            if (_walletAmount != null) _walletAmount.text = NumberFormatter.FormatWithCommas(_inventory != null ? _inventory.GetCreditAmount() : 0);
            RefreshBuyButton();
        }

        private void RefreshBuyButton()
        {
            if (_buyAllButton == null) return;
            _buyAllButton.interactable = _session != null && _session.Cart.Count > 0 && _session.CanAfford(_inventory);
        }

        private void HandleWareClicked(ShopSession session, int row)
        {
            if (session != _session) return;
            FocusWare(row);
        }

        private void HandleWareDoubleClicked(ShopSession session, int row)
        {
            if (session != _session || row < 0 || row >= _wareOrder.Count) return;
            FocusWare(row);
            int entryIndex = _wareOrder[row];
            if (MaxBuyable(entryIndex) > 0) _session.AddToCart(entryIndex, 1);
        }

        private void HandleWareDropped(ShopWarePayload payload)
        {
            if (payload.Session != _session) return;
            FocusWare(payload.Row);
            RequestQuantity(payload.EntryIndex);
        }

        private void HandleWareInfo(ShopSession session, int entryIndex)
        {
            if (session != _session) return;
            ShowInfo(_session.Entries[entryIndex].Item);
        }

        private void HandleCartInfo(int cartIndex)
        {
            if (_session == null || cartIndex < 0 || cartIndex >= _session.Cart.Count) return;
            ShowInfo(_session.Cart[cartIndex].Entry.Item);
        }

        private void HandleRemove(int cartIndex)
        {
            if (_session != null) _session.RemoveLine(cartIndex);
        }

        private static void ShowInfo(Item item)
        {
            if (item == null || ItemInformationManager.Instance == null) return;
            ItemInstance preview = item is Equipment ? new EquipmentInstance(-1, item.Id) : new ItemInstance(-1, item.Id, 1);
            ItemInformationManager.Instance.Open(preview);
        }
        #endregion
    }
}
