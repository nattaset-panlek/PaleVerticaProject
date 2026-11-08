using System;
using System.Collections.Generic;

namespace CoreSystem.Loot
{
    [Serializable]
    public class DropEntry
    {
        public int ItemId;
        public float Chance = 1f;
        public int MinQty = 1;
        public int MaxQty = 1;
    }

    [Serializable]
    public class DropTableModel
    {
        public int Id;
        public string Name;
        public List<DropEntry> Entries = new List<DropEntry>();
    }

    [Serializable]
    public class DropTableDataList
    {
        public float GroundLifetimeSeconds = 60f;
        public float OwnerLockSeconds = 10f;
        public List<DropTableModel> Tables = new List<DropTableModel>();
    }

    public readonly struct LootRoll
    {
        public readonly int ItemId;
        public readonly int Quantity;

        public LootRoll(int itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}
