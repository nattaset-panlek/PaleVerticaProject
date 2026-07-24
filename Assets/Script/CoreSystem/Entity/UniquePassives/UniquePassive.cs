using UnityEngine;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.UI;

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
    }

    public class VampireUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && payload.DamageType == DamageType.Physical && damage > 0f)
            {
                float healAmount = damage * 0.3f;
                var hpAttr = Owner.attributeSystem.GetAttribute(AttributeType.HP);
                var maxHp = Owner.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (hpAttr != null && maxHp != null)
                {
                    hpAttr.BaseValue = Mathf.Min(hpAttr.CurrentValue + healAmount, maxHp.CurrentValue);
                    hpAttr.Recalculate();
                }
            }
        }
        #endregion
    }

    public class TimeCollectorUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnBasicAttack(IEntity target)
        {
            if (Owner.skillCaster != null)
            {
                foreach (var instance in Owner.skillCaster.EquippedSkills)
                {
                    if (instance != null && instance.CurrentCooldown > 0f)
                    {
                        instance.ReduceCooldown(0.1f);
                    }
                }
                foreach (var instance in Owner.skillCaster.GrantedSkills)
                {
                    if (instance != null && instance.CurrentCooldown > 0f)
                    {
                        instance.ReduceCooldown(0.1f);
                    }
                }
            }
        }
        #endregion
    }

    public class DepressionChaosUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            if (payload.Caster == Owner && skill.Tags.Contains(SkillTag.AreaOfEffect))
            {
                bool isDamaging = skill.Effects.Exists(e => e is DamageEffect);
                if (!isDamaging && payload.Targets != null && payload.Targets.Count > 0)
                {
                    float intVal = Owner.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                    float dmg = intVal * 2f;
                    foreach (var target in payload.Targets)
                    {
                        var dmgEffect = new DamageEffect();
                        var dmgPayload = new SkillPayload
                        {
                            Caster = Owner,
                            SkillID = -998,
                            SkillLevel = 1,
                            SkillElement = ElementType.Neutral,
                            DamageType = DamageType.Magic,
                            BaseDamage = dmg
                        };
                        dmgEffect.Apply(dmgPayload, target);
                    }
                }
            }
        }
        #endregion
    }

    public class ThornUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (attacker != null && attacker != Owner && payload.SkillID != -999 && payload.SkillID != -998 && payload.SkillID != -997 && damage > 0f)
            {
                float str = Owner.attributeSystem.GetAttribute(AttributeType.STR).CurrentValue;
                float reflectDmg = damage * 0.1f * (str * 0.01f);
                if (reflectDmg > 0f)
                {
                    var dmgEffect = new DamageEffect();
                    var dmgPayload = new SkillPayload
                    {
                        Caster = Owner,
                        SkillID = -999,
                        SkillLevel = 1,
                        SkillElement = ElementType.Neutral,
                        DamageType = DamageType.Physical,
                        BaseDamage = reflectDmg
                    };
                    dmgEffect.Apply(dmgPayload, attacker);
                }
            }
        }
        #endregion
    }

    public class ManaLeechUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && target != null && Random.Range(0f, 100f) < 30f)
            {
                float mpRecPercent = Owner.attributeSystem.GetAttribute(AttributeType.MPRecoveryPercent).CurrentValue;
                float leechAmount = 5f * (1f + (mpRecPercent / 100f));
                var mpAttr = Owner.attributeSystem.GetAttribute(AttributeType.MP);
                var maxMp = Owner.attributeSystem.GetAttribute(AttributeType.MaxMP);
                if (mpAttr != null && maxMp != null)
                {
                    mpAttr.BaseValue = Mathf.Min(mpAttr.CurrentValue + leechAmount, maxMp.CurrentValue);
                    mpAttr.Recalculate();
                }
            }
        }
        #endregion
    }

    public class ExecutionUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && target != null && target != Owner)
            {
                var targetHp = target.attributeSystem.GetAttribute(AttributeType.HP);
                var targetMaxHp = target.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (targetHp != null && targetMaxHp != null && targetHp.CurrentValue > 0f)
                {
                    if (targetHp.CurrentValue <= targetMaxHp.CurrentValue * 0.05f)
                    {
                        targetHp.BaseValue = 0f;
                        targetHp.Recalculate();
                        if (DamageTextManager.Instance != null)
                        {
                            DamageTextManager.Instance.ShowDamage(target.transform.position, 999999f, DamageType.Status, ElementType.Ghost, true);
                        }
                    }
                }
            }
        }
        #endregion
    }

    public class BloodBleedingUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnPostHit(IEntity target, float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner && target != null && target != Owner && Random.Range(0f, 100f) < 20f)
            {
                var targetCharacter = target as Character;
                if (targetCharacter != null && targetCharacter.UniquePassives != null)
                {
                    float casterInt = Owner.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                    float casterDex = Owner.attributeSystem.GetAttribute(AttributeType.DEX).CurrentValue;
                    
                    var dotBuff = new BloodBleedingDOTUniquePassive
                    {
                        CasterInt = casterInt,
                        CasterDex = casterDex,
                        Duration = 5f,
                        RemainingDuration = 5f
                    };
                    dotBuff.Initialize("BloodBleedingDOT", targetCharacter);
                    targetCharacter.UniquePassives.AddPassive(dotBuff);
                }
            }
        }
        #endregion
    }

    public class BleedingUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private float tickTimer = 0f;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            tickTimer += dt;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                var maxHpAttr = Owner.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (maxHpAttr != null)
                {
                    float dmg = maxHpAttr.CurrentValue * 0.01f;
                    if (dmg > 0f)
                    {
                        var dmgEffect = new DamageEffect();
                        var dmgPayload = new SkillPayload
                        {
                            Caster = Owner,
                            SkillID = -997,
                            SkillLevel = 1,
                            SkillElement = ElementType.Neutral,
                            DamageType = DamageType.Status,
                            BaseDamage = dmg
                        };
                        dmgEffect.Apply(dmgPayload, Owner);
                    }
                }
            }
        }
        #endregion
    }

    public class BloodBleedingDOTUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        public float CasterInt { get; set; }
        public float CasterDex { get; set; }
        private float tickTimer = 0f;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            tickTimer += dt;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                var maxHpAttr = Owner.attributeSystem.GetAttribute(AttributeType.MaxHP);
                if (maxHpAttr != null)
                {
                    float dmg = (maxHpAttr.CurrentValue * 0.01f) + ((CasterInt + CasterDex) * 0.01f);
                    if (dmg > 0f)
                    {
                        var dmgEffect = new DamageEffect();
                        var dmgPayload = new SkillPayload
                        {
                            Caster = Owner,
                            SkillID = -997,
                            SkillLevel = 1,
                            SkillElement = ElementType.Neutral,
                            DamageType = DamageType.Status,
                            BaseDamage = dmg
                        };
                        dmgEffect.Apply(dmgPayload, Owner);
                    }
                }
            }
        }
        #endregion
    }

    public class CurseOfKanixUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyCoreAttributes(ref float str, ref float agi, ref float vit, ref float @int, ref float dex, ref float luk)
        {
            float tempInt = @int;
            @int = str;
            str = tempInt;

            float tempDex = dex;
            dex = vit;
            vit = tempDex;

            float tempLuk = luk;
            luk = agi;
            agi = tempLuk;
        }
        #endregion
    }

    public class BlessingOfMagearnaUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (damage > 0f)
            {
                float absorbable = damage * 0.4f;
                var mpAttr = Owner.attributeSystem.GetAttribute(AttributeType.MP);
                if (mpAttr != null && mpAttr.CurrentValue > 0f)
                {
                    float absorbed = Mathf.Min(absorbable, mpAttr.CurrentValue);
                    mpAttr.BaseValue = mpAttr.CurrentValue - absorbed;
                    mpAttr.Recalculate();
                    damage -= absorbed;
                }
            }
        }
        #endregion
    }

    public class BloodMageUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyManaCost(ref float manaCost, ref float hpCost, ref bool isHpCost)
        {
            hpCost = manaCost;
            manaCost = 0f;
            isHpCost = true;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (payload.SkillID > 0)
            {
                damage *= 1.3f;
            }
        }
        #endregion
    }

    public class EldritchFaithUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyCoreAttributes(ref float str, ref float agi, ref float vit, ref float @int, ref float dex, ref float luk)
        {
            @int *= 0.5f;
        }

        public override void TriggerOnHit(IEntity target, ref float damage, SkillPayload payload)
        {
            if (payload.Caster == Owner)
            {
                float currentInt = Owner.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                damage += currentInt;
            }
        }
        #endregion
    }

    public class RockUniquePassive : UniquePassive
    {
    }
}
