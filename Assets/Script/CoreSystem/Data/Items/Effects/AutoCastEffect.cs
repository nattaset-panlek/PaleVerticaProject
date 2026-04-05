using CoreSystem.Entity;
using CoreSystem;
using CoreSystem.Skills;
using UnityEngine;

namespace CoreSystem.Data.Items.Effects
{
    public enum AutoCastTrigger
    {
        OnHit,
        OnTakeDamage
    }

    [System.Serializable]
    public class AutoCastEffect : EquipmentEffect
    {
        public AutoCastTrigger TriggerType;
        public ConditionLogic Condition; // Optional condition before triggering

        [Range(0f, 100f)]
        public float TriggerChance = 10f;
        
        public int SkillID;
        
        private Character _boundPlayer;

        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (player == null) return;
            _boundPlayer = player;

            if (TriggerType == AutoCastTrigger.OnHit)
                player.OnHitTarget += HandleTrigger;
            else if (TriggerType == AutoCastTrigger.OnTakeDamage)
                player.OnTakeDamage += HandleTrigger;
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (player == null) return;

            if (TriggerType == AutoCastTrigger.OnHit)
                player.OnHitTarget -= HandleTrigger;
            else if (TriggerType == AutoCastTrigger.OnTakeDamage)
                player.OnTakeDamage -= HandleTrigger;
                
            _boundPlayer = null;
        }

        private void HandleTrigger(IEntity other, float value)
        {
            if (_boundPlayer == null || _boundPlayer.skillCaster == null) return;

            if (UnityEngine.Random.Range(0f, 100f) <= TriggerChance)
            {
                if (Condition != null && !Condition.Evaluate(_boundPlayer)) return;
                
                var skill = CoreSystem.Data.Database.GetSynchronizedSkill(SkillID);
                if (skill != null)
                {
                    // Bypass normal slot restrictions, automatically cast the granted skill
                    _boundPlayer.skillCaster.CastSkillDirectly(skill);
                }
            }
        }

        public override string GetDescription()
        {
            var skill = CoreSystem.Data.Database.GetSynchronizedSkill(SkillID);
            string skillName = skill != null ? skill.Name : $"Skill {SkillID}";
            string trigger = TriggerType == AutoCastTrigger.OnHit ? "hitting a target" : "taking damage";
            return $"{TriggerChance:F1}% chance to cast {skillName} when {trigger}";
        }
    }
}
