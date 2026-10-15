using System;

namespace CoreSystem.UI.Hotbar
{
    [Serializable]
    public class HotbarSlotData
    {
        #region [Variables/Fields]
        public HotbarActionType ActionType = HotbarActionType.Empty;
        public int SkillId = -1;
        public int ItemId = -1;
        public long ItemInstanceId = -1;
        public string CustomActionId = string.Empty;
        public string HotkeyText = string.Empty;
        #endregion

        #region [Public Methods]
        public bool IsEmpty => ActionType == HotbarActionType.Empty;

        public void Clear()
        {
            ActionType = HotbarActionType.Empty;
            SkillId = -1;
            ItemId = -1;
            ItemInstanceId = -1;
            CustomActionId = string.Empty;
        }

        public void SetSkill(int skillId)
        {
            ActionType = HotbarActionType.Skill;
            SkillId = skillId;
            ItemId = -1;
            ItemInstanceId = -1;
            CustomActionId = string.Empty;
        }

        public void SetItem(int itemId, long instanceId = -1)
        {
            ActionType = HotbarActionType.Item;
            ItemId = itemId;
            ItemInstanceId = instanceId;
            SkillId = -1;
            CustomActionId = string.Empty;
        }
        #endregion
    }
}
