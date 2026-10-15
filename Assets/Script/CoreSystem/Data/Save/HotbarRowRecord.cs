using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class HotbarRowRecord
    {
        public int RowIndex;
        public bool IsLocked;
        public Vector2 AnchoredPosition;
        public int SlotCount;
        public List<HotbarSlotRecord> Slots = new List<HotbarSlotRecord>();
    }
}
