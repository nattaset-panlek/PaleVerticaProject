using CoreSystem.Shop;

namespace CoreSystem.UI.Shop
{
    public class ShopWarePayload
    {
        #region [Variables/Fields]
        public readonly ShopSession Session;
        public readonly int EntryIndex;
        public readonly int Row;
        #endregion

        #region [Public Methods]
        public ShopWarePayload(ShopSession session, int entryIndex, int row)
        {
            Session = session;
            EntryIndex = entryIndex;
            Row = row;
        }
        #endregion
    }
}
