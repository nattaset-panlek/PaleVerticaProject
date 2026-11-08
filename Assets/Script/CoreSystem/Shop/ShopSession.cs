using System;
using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;

namespace CoreSystem.Shop
{
    public class ShopSession
    {
        #region [Variables/Fields]
        public const int CartCapacity = 36;

        private readonly ShopDefinition _shop;
        private readonly int[] _sold;
        private readonly List<ShopCartLine> _cart = new List<ShopCartLine>();

        public event Action OnCartChanged;
        public event Action OnStockChanged;
        public event Action<ShopPurchaseResult> OnPurchased;
        #endregion

        #region [Properties]
        public ShopDefinition Shop => _shop;
        public IReadOnlyList<ShopEntry> Entries => _shop.Items;
        public IReadOnlyList<ShopCartLine> Cart => _cart;
        public bool IsCartFull => _cart.Count >= CartCapacity;
        public long Total
        {
            get
            {
                long total = 0;
                for (int i = 0; i < _cart.Count; i++) total += _cart[i].Subtotal;
                return total;
            }
        }
        #endregion

        #region [Public Methods]
        public ShopSession(ShopDefinition shop)
        {
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _sold = new int[_shop.Items.Count];
        }

        public int Remaining(int entryIndex)
        {
            if (!IsValid(entryIndex)) return 0;
            ShopEntry entry = _shop.Items[entryIndex];
            if (entry.IsUnlimited) return int.MaxValue;
            return Math.Max(0, entry.Stock - _sold[entryIndex]);
        }

        public bool IsSoldOut(int entryIndex) => Remaining(entryIndex) <= 0;

        public int InCart(int entryIndex)
        {
            int total = 0;
            for (int i = 0; i < _cart.Count; i++)
            {
                if (_cart[i].EntryIndex == entryIndex) total += _cart[i].Quantity;
            }
            return total;
        }

        public int MaxAddable(int entryIndex)
        {
            if (!IsValid(entryIndex)) return 0;
            bool separate = _shop.Items[entryIndex].IsEquipment;
            if (!separate && FindLine(entryIndex) == null && IsCartFull) return 0;
            long room = (long)Remaining(entryIndex) - InCart(entryIndex);
            if (separate) room = Math.Min(room, CartCapacity - _cart.Count);
            return (int)Math.Max(0, Math.Min(room, int.MaxValue));
        }

        public int AddToCart(int entryIndex, int quantity)
        {
            int added = Math.Min(Math.Max(0, quantity), MaxAddable(entryIndex));
            if (added <= 0) return 0;
            if (_shop.Items[entryIndex].IsEquipment)
            {
                for (int i = 0; i < added; i++) _cart.Add(new ShopCartLine(entryIndex, _shop.Items[entryIndex], 1));
                OnCartChanged?.Invoke();
                return added;
            }
            ShopCartLine line = FindLine(entryIndex);
            if (line == null) _cart.Add(new ShopCartLine(entryIndex, _shop.Items[entryIndex], added));
            else line.Quantity += added;
            OnCartChanged?.Invoke();
            return added;
        }

        public void SetCartQuantity(int entryIndex, int quantity)
        {
            if (IsValid(entryIndex) && _shop.Items[entryIndex].IsEquipment)
            {
                _cart.RemoveAll(existing => existing.EntryIndex == entryIndex);
                if (AddToCart(entryIndex, quantity) <= 0) OnCartChanged?.Invoke();
                return;
            }

            ShopCartLine line = FindLine(entryIndex);
            if (line == null)
            {
                AddToCart(entryIndex, quantity);
                return;
            }
            int clamped = Math.Min(Math.Max(0, quantity), (int)Math.Min(Remaining(entryIndex), int.MaxValue));
            if (clamped == 0) _cart.Remove(line);
            else line.Quantity = clamped;
            OnCartChanged?.Invoke();
        }

        public void RemoveLine(int cartIndex)
        {
            if (cartIndex < 0 || cartIndex >= _cart.Count) return;
            _cart.RemoveAt(cartIndex);
            OnCartChanged?.Invoke();
        }

        public void ClearCart()
        {
            if (_cart.Count == 0) return;
            _cart.Clear();
            OnCartChanged?.Invoke();
        }

        public bool CanAfford(PlayerInventory inventory)
        {
            if (inventory == null) return false;
            long total = Total;
            return total <= int.MaxValue && inventory.HasCredit((int)total);
        }

        public ShopPurchaseResult Checkout(PlayerInventory inventory)
        {
            ShopPurchaseResult result = TryCheckout(inventory);
            OnPurchased?.Invoke(result);
            return result;
        }
        #endregion

        #region [Private Methods]
        private ShopPurchaseResult TryCheckout(PlayerInventory inventory)
        {
            if (inventory == null) return ShopPurchaseResult.NoInventory;
            if (_cart.Count == 0) return ShopPurchaseResult.EmptyCart;
            if (!CanAfford(inventory)) return ShopPurchaseResult.NotEnoughCredit;

            inventory.BeginBatch();
            try
            {
                if (!inventory.SpendCredit((int)Total)) return ShopPurchaseResult.NotEnoughCredit;
                foreach (ShopCartLine line in _cart)
                {
                    Item item = line.Entry.Item;
                    if (item != null) inventory.AddItem(item, line.Quantity);
                    _sold[line.EntryIndex] += line.Quantity;
                }
            }
            finally
            {
                inventory.EndBatch();
            }

            _cart.Clear();
            OnCartChanged?.Invoke();
            OnStockChanged?.Invoke();
            return ShopPurchaseResult.Success;
        }

        private ShopCartLine FindLine(int entryIndex)
        {
            for (int i = 0; i < _cart.Count; i++)
            {
                if (_cart[i].EntryIndex == entryIndex) return _cart[i];
            }
            return null;
        }

        private bool IsValid(int entryIndex) => entryIndex >= 0 && entryIndex < _shop.Items.Count;
        #endregion
    }
}
