using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class HotbarSlotRecord
    {
        public int ActionType;
        public int SkillId = -1;
        public int ItemId = -1;
        public long ItemInstanceId = -1;
        public string CustomActionId = "";
        public string HotkeyText = "";
    }
}
