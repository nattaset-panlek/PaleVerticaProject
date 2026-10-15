using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class InventoryItemRecord
    {
        public long InstanceId;
        public int ItemId;
        public int Quantity;
        public int Refine;
        public int Reforge;
        public List<int> SocketedFragmentIds = new List<int>();
        public int EquippedSlotIndex = -1;
        public int CostumeSlotIndex = -1;
    }
}
