using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class SkillRecord
    {
        public int SkillId;
        public int EquippedSlotIndex = -1;
        public List<int> LinkedSupportSkillIds = new List<int>();
    }
}
