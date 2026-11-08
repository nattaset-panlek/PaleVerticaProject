using CoreSystem.Data.Items;

namespace CoreSystem.UI.Utility
{
    public interface IItemInsertTarget
    {
        bool CanInsert(ItemInstance item, int slotIndex);
        bool TryInsert(ItemInstance item, int slotIndex);
        void ClearInsert(int slotIndex);
        string GetInsertLabel(ItemInstance item);
    }
}
