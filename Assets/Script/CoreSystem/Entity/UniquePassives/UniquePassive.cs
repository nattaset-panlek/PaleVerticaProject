using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;

namespace CoreSystem.Entity.UniquePassives
{
    public abstract class UniquePassive
    {
        #region [Properties]
        private string PercentTag => $"{Id}%";
        public string Id { get; protected set; }
        public Character Owner { get; protected set; }
        public float Duration { get; set; } = -1f;
        public float RemainingDuration { get; set; } = -1f;
        public virtual int CoreAttributeOrder => 0;
        public virtual bool UsesFacing => false;
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
        public virtual bool CanCast(SkillPayload payload) => true;
        public virtual void ModifyCast(SkillPayload payload) {}
        public virtual bool CanBasicAttack() => true;
        public virtual bool ForcesCritical(IEntity target, SkillPayload payload) => false;
        public virtual void ModifyBasicAttack(SkillPayload payload) {}
        public virtual void ModifyOutgoingHeal(IEntity target, ref float amount, SkillPayload payload) {}
        public virtual void TriggerOnHealed(IEntity target, float amount, SkillPayload payload) {}
        public virtual bool CanUseConsumable() => true;
        public virtual bool ShouldKeepConsumable() => false;
        public virtual void TriggerOnConsumableUsed(Consumable consumable) {}
        public virtual bool CanEquip(Equipment equipment) => true;
        #endregion

        #region [Private Methods]
        protected void DealProcDamage(IEntity target, float amount, DamageType damageType, int procSkillId, ElementType element = ElementType.Neutral)
        {
            var payload = new SkillPayload
            {
                Caster = Owner,
                SkillID = procSkillId,
                SkillLevel = 1,
                SkillElement = element,
                DamageType = damageType,
                BaseDamage = amount
            };
            CombatCore.Execute(CombatAction.Damage(payload), Owner, target);
        }

        protected bool IsOwnHit(SkillPayload payload)
        {
            return payload != null && payload.Caster == Owner && !ProcSkillIds.IsProc(payload.SkillID);
        }

        protected static bool IsBasicAttack(SkillPayload payload)
        {
            return payload.SkillID == 0;
        }

        protected float Stat(AttributeType type)
        {
            var attribute = Owner.attributeSystem.GetAttribute(type);
            return attribute != null ? attribute.CurrentValue : 0f;
        }

        protected float HpRatio()
        {
            float max = Stat(AttributeType.MaxHP);
            return max > 0f ? Mathf.Clamp01(Stat(AttributeType.HP) / max) : 0f;
        }

        protected float MpRatio()
        {
            float max = Stat(AttributeType.MaxMP);
            return max > 0f ? Mathf.Clamp01(Stat(AttributeType.MP) / max) : 0f;
        }

        protected void SetBonus(AttributeType type, float value)
        {
            ClearBonus(type);
            if (value == 0f) return;
            Owner.attributeSystem.AddAdditional(type, 2, AdditionalOp.Add, value, Id);
            if (TryPercentTarget(type, out AttributeType target)) Owner.attributeSystem.AddAdditional(target, 2, AdditionalOp.Multiply, 1f + value / 100f, PercentTag);
        }

        protected void ClearBonus(AttributeType type)
        {
            Owner.attributeSystem.RemoveAdditionalByTag(type, Id);
            if (TryPercentTarget(type, out AttributeType target)) Owner.attributeSystem.RemoveAdditionalByTag(target, PercentTag);
        }

        protected List<IEntity> EnemiesAround(Vector3 center, float radius)
        {
            var found = new List<IEntity>();
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
            for (int i = 0; i < colliders.Length; i++)
            {
                IEntity entity = colliders[i].GetComponentInParent<IEntity>();
                if (entity == null || found.Contains(entity) || !TeamRegistry.IsHostile(Owner, entity)) continue;
                if (entity is Character character && character.IsDead) continue;
                found.Add(entity);
            }
            return found;
        }

        protected IEntity NearestEnemy(Vector3 center, float radius, IEntity exclude)
        {
            IEntity best = null;
            float bestDistance = float.MaxValue;
            foreach (IEntity entity in EnemiesAround(center, radius))
            {
                if (ReferenceEquals(entity, exclude)) continue;
                float distance = (entity.transform.position - center).sqrMagnitude;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = entity;
            }
            return best;
        }

        protected void ApplyBurn(IEntity target, float duration, float tickDamage)
        {
            if (target == null) return;
            var spec = new StatusSpec { Status = StatusKind.Burn, Duration = duration, TickDamage = tickDamage, CreditTo = Owner };
            StatusHandler.Apply(Owner, target, spec, ProcSkillIds.StatusApply);
        }

        protected List<SummonedUnit> OwnedSummons()
        {
            var owned = new List<SummonedUnit>();
            foreach (SummonedUnit unit in SummonRegistry.Units)
            {
                if (unit != null && unit.IsReady && unit.Owner == Owner) owned.Add(unit);
            }
            return owned;
        }

        private static bool TryPercentTarget(AttributeType type, out AttributeType target)
        {
            target = type;
            string name = type.ToString();
            const string suffix = "Percent";
            return name.EndsWith(suffix) && System.Enum.TryParse(name.Substring(0, name.Length - suffix.Length), out target);
        }

        protected void RestoreResource(AttributeType resource, AttributeType maxResource, float amount)
        {
            var current = Owner.attributeSystem.GetAttribute(resource);
            var max = Owner.attributeSystem.GetAttribute(maxResource);
            if (current == null || max == null || current.CurrentValue >= max.CurrentValue) return;

            current.BaseValue = Mathf.Min(current.CurrentValue + amount, max.CurrentValue);
            current.Recalculate();
        }
        #endregion
    }
}
