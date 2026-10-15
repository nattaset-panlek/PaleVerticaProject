using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Data.Items;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Player;

namespace CoreSystem.Data.Items.Model
{
    public class SkillRune : Item
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        public int LinkedSkillID { get; set; }
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public ActiveSkill GetActiveSkill()
        {
            return Database.GetSynchronizedSkill(LinkedSkillID);
        }

        public override void OnUse()
        {
            var player = PlayerCore.GetPlayer();
            if (player != null && player.skillCaster != null)
            {
                player.skillCaster.GrantSkill(LinkedSkillID);
                Debug.Log($"[SkillRune] Granted Skill ID {LinkedSkillID} to Player.");
            }
        }

        public override Item Clone()
        {
            return new SkillRune
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Description = this.Description,
                IsStackable = this.IsStackable,
                Type = this.Type,
                Rarity = this.Rarity,
                IconPath = this.IconPath,
                LinkedSkillID = this.LinkedSkillID
            };
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
