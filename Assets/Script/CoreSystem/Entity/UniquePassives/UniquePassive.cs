using UnityEngine;
using CoreSystem.Combat;
using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    public abstract class UniquePassive
    {
        #region [Properties]
        public string Id { get; protected set; }
        public Character Owner { get; protected set; }
        public float Duration { get; set; } = -1f;
        public float RemainingDuration { get; set; } = -1f;
        #endregion

        #region [Public Methods]
        public virtual void Initialize(string id, Character owner)
        {
            Id = id;
            Owner = owner;
        }

        public virtual void OnEquip() {}
        public virtual void OnUnequip() {}

        public virtual void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload) {}
        public virtual void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload) {}
        public virtual void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload) {}
        public virtual void TriggerOnBasicAttack(IEntity target) {}
        public virtual void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload) {}
        public virtual void ModifyCoreAttributes(ref float str, ref float agi, ref float vit, ref float @int, ref float dex, ref float luk) {}
        public virtual void ModifyManaCost(ref float manaCost, ref float hpCost, ref bool isHpCost) {}
        public virtual void Tick(float dt) {}
        #endregion

        #region [Private Methods]
        protected void DealProcDamage(IEntity target, float amount, DamageType damageType, int procSkillId)
        {
            var payload = new SkillPayload
            {
                Caster = Owner,
                SkillID = procSkillId,
                SkillLevel = 1,
                SkillElement = ElementType.Neutral,
                DamageType = damageType,
                BaseDamage = amount
            };
            CombatCore.Execute(CombatAction.Damage(payload), Owner, target);
        }

        protected void RestoreResource(AttributeType resource, AttributeType maxResource, float amount)
        {
            var current = Owner.attributeSystem.GetAttribute(resource);
            var max = Owner.attributeSystem.GetAttribute(maxResource);
            if (current == null || max == null) return;

            current.BaseValue = Mathf.Min(current.CurrentValue + amount, max.CurrentValue);
            current.Recalculate();
        }
        #endregion
    }
}
