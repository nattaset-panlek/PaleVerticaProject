using CoreSystem.Data;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class ItemInstance
    {
        public long Id;
        public int ItemId;
        public int Quantity;

        public ItemInstance(long id, int itemId, int quantity = 1)
        {
            this.Id = id;
            this.ItemId = itemId;
            this.Quantity = quantity;
        }

        public Item Item => Database.GetItem(ItemId);
        public string Name => Item?.Name ?? "Unknown";

        public override bool Equals(object obj)
        {
            if (obj is ItemInstance other)
            {
                // If both have valid unique IDs, compare IDs
                if (Id != -1 && other.Id != -1)
                    return Id == other.Id;
                
                // If either is temporary (-1), compare ItemId
                return ItemId == other.ItemId && Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + ItemId.GetHashCode();
                return hash;
            }
        }
    }
}

