using CoreSystem.Entity;
using CoreSystem;
using CoreSystem.Skills;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class SkillUnlockEffect : EquipmentEffect
    {
        public int SkillID;

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null || player.skillCaster == null) return;
            player.skillCaster.GrantSkill(SkillID);
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player == null || player.skillCaster == null) return;
            player.skillCaster.RevokeSkill(SkillID);
        }

        public override string GetDescription()
        {
            var skill = CoreSystem.Data.Database.GetSynchronizedSkill(SkillID);
            return skill != null ? skill.Name : $"Unknown Skill ({SkillID})";
        }
    }
}
