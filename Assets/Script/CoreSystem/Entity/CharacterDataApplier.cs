using System.Collections.Generic;
using CoreSystem.Agent;
using CoreSystem.Attributes;
using CoreSystem.Data;

namespace CoreSystem.Entity
{
    public static class CharacterDataApplier
    {
        #region [Public Methods]
        public static void ApplyStats(AttributeSystem attributes, CharacterModel model)
        {
            Set(attributes, AttributeType.BaseLevel, model.Level);
            Set(attributes, AttributeType.STR, model.STR);
            Set(attributes, AttributeType.AGI, model.AGI);
            Set(attributes, AttributeType.VIT, model.VIT);
            Set(attributes, AttributeType.INT, model.INT);
            Set(attributes, AttributeType.DEX, model.DEX);
            Set(attributes, AttributeType.LUK, model.LUK);
            Set(attributes, AttributeType.MaxHP, model.BaseHP);
            Set(attributes, AttributeType.MaxMP, model.BaseMP);
            Set(attributes, AttributeType.Defense, model.Defense);
            Set(attributes, AttributeType.MDEF, model.MDEF);
            Set(attributes, AttributeType.WeaponATK, model.WeaponATK);
            Set(attributes, AttributeType.WeaponMATK, model.WeaponMATK);
            if (model.MoveSpeed > 0f) Set(attributes, AttributeType.MoveSpeed, model.MoveSpeed);
        }

        public static void FillVitals(AttributeSystem attributes)
        {
            Set(attributes, AttributeType.HP, attributes.GetAttribute(AttributeType.MaxHP).CurrentValue);
            Set(attributes, AttributeType.MP, attributes.GetAttribute(AttributeType.MaxMP).CurrentValue);
        }

        public static void ApplyBehavior(AgentProfile profile, CharacterModel model)
        {
            profile.Behavior = model.Behavior;
            profile.Combat = model.Combat;
            profile.DetectionRadius = model.DetectionRadius;
            profile.CanRoam = model.CanRoam;
        }

        public static List<AgentSkillEntry> BuildSkillPool(CharacterModel model)
        {
            var pool = new List<AgentSkillEntry>();
            if (model.SkillIds == null) return pool;

            foreach (int id in model.SkillIds)
            {
                var skill = Database.GetSynchronizedSkill(id);
                if (skill != null) pool.Add(new AgentSkillEntry { Skill = skill });
            }
            return pool;
        }
        #endregion

        #region [Private Methods]
        private static void Set(AttributeSystem attributes, AttributeType type, float value)
        {
            var attribute = attributes.GetAttribute(type);
            if (attribute == null) return;
            attribute.BaseValue = value;
            attribute.Recalculate();
        }
        #endregion
    }
}
