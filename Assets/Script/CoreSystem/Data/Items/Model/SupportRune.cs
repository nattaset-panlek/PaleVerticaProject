using System.Collections.Generic;
using CoreSystem.Data.Items;
using CoreSystem;
using CoreSystem.Skills;
using CoreSystem.Data;

namespace CoreSystem.Data.Items.Model
{
    public class SupportRune : Item
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        public int SupportSkillID { get; set; }
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public SupportModifier GetSupportModifier()
        {
            return Database.GetSynchronizedSupport(SupportSkillID);
        }

        public override void OnUse()
        {
            UnityEngine.Debug.LogWarning($"[SupportRune] '{Name}' cannot be consumed directly. Must be used via SkillManagePage / SupportSkillSlotComponent!");
        }

        public override Item Clone()
        {
            return new SupportRune
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity,
                IconPath = this.IconPath,
                SupportSkillID = this.SupportSkillID
            };
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
