namespace CoreSystem.Shop
{
    public class ShopCartLine
    {
        #region [Variables/Fields]
        public readonly int EntryIndex;
        public readonly ShopEntry Entry;
        public int Quantity;
        #endregion

        #region [Properties]
        public long Subtotal => (long)Entry.Price * Quantity;
        #endregion

        #region [Public Methods]
        public ShopCartLine(int entryIndex, ShopEntry entry, int quantity)
        {
            EntryIndex = entryIndex;
            Entry = entry;
            Quantity = quantity;
        }
        #endregion
    }
}
