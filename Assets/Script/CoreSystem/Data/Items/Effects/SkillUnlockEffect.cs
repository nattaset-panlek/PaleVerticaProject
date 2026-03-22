using CoreSystem.Entity;
using CoreSystem;
using CoreSystem.Skills;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class SkillUnlockEffect : EquipmentEffect
    {
        public int SkillID;

        public override void Apply(Character player)
        {
            if (player == null || player.skillCaster == null) return;
        }

        public override void Remove(Character player)
        {
            if (player == null || player.skillCaster == null) return;
        }

        public override string GetDescription()
        {
            var skill = CoreSystem.Data.Database.GetSynchronizedSkill(SkillID);
            string skillName = skill != null ? skill.Name : $"Skill {SkillID}";
            return $"Unlocks Skill: {skillName}";
        }
    }
}
