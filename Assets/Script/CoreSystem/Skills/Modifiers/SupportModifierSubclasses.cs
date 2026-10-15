using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1001)]
    public class MultiShotSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int extraProjectiles = 1;
        #endregion

        #region [Public Methods]
        public MultiShotSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Hematite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ProjectileCount += extraProjectiles;
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, extraProjectiles);
        }
        #endregion
    }

    [SupportModifierId(1002)]
    public class EchoSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float damageMultiplier = 0.8f;
        public float cooldownMultiplier = 1.25f;
        #endregion

        #region [Public Methods]
        public EchoSupportModifier()
        {
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Fluorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.BaseDamage = (payload.BaseDamage * damageMultiplier) + (payload.Cooldown * cooldownMultiplier);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, damageMultiplier * 100f, cooldownMultiplier);
        }
        #endregion
    }

    [SupportModifierId(1003)]
    public class PierceSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public PierceSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Obsidian;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return template;
        }
        #endregion
    }

    [SupportModifierId(1004)]
    public class ChainSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public ChainSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Obsidian;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return template;
        }
        #endregion
    }

    [SupportModifierId(1005)]
    public class AreaIncreaseSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float aoePercentBonus = 4f;
        public float vctPercentBonus = 2f;
        #endregion

        #region [Public Methods]
        public AreaIncreaseSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.AOERadius += (payload.AOERadius * aoePercentBonus / 100f);
            payload.VCT += (payload.VCT * vctPercentBonus / 100f);
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.AreaOfEffect);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, aoePercentBonus, vctPercentBonus);
        }
        #endregion
    }

    [SupportModifierId(1006)]
    public class DamageUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float damageMultiplier = 1.2f;
        #endregion

        #region [Public Methods]
        public DamageUpSupportModifier()
        {
            Chroma = ChromaType.Origin;
            Mineral = MineralType.Zircon;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.BaseDamage *= damageMultiplier;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (damageMultiplier - 1f) * 100f);
        }
        #endregion
    }

    [SupportModifierId(1007)]
    public class CooldownReductionSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float cdrPercent = 10f;
        #endregion

        #region [Public Methods]
        public CooldownReductionSupportModifier()
        {
            Chroma = ChromaType.Dimensional;
            Mineral = MineralType.Alexandrite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.Cooldown *= (1f - (cdrPercent / 100f));
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, cdrPercent);
        }
        #endregion
    }

    [SupportModifierId(1008)]
    public class CostReductionSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float costMultiplier = 0.5f;
        #endregion

        #region [Public Methods]
        public CostReductionSupportModifier()
        {
            Chroma = ChromaType.Dimensional;
            Mineral = MineralType.Ulexite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ManaCost *= costMultiplier;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (1f - costMultiplier) * 100f);
        }
        #endregion
    }

    [SupportModifierId(1009)]
    public class DurationUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float durationPercent = 20f;
        #endregion

        #region [Public Methods]
        public DurationUpSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Tektite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, durationPercent);
        }
        #endregion
    }

    [SupportModifierId(1010)]
    public class StatusChanceUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float statusChancePercent = 15f;
        #endregion

        #region [Public Methods]
        public StatusChanceUpSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Celestite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, statusChancePercent);
        }
        #endregion
    }

    [SupportModifierId(1011)]
    public class BurnChanceSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float burnChancePercent = 20f;
        #endregion

        #region [Public Methods]
        public BurnChanceSupportModifier()
        {
            Chroma = ChromaType.Origin;
            Mineral = MineralType.Amber;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillElement == ElementType.Fire;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, burnChancePercent);
        }
        #endregion
    }

    [SupportModifierId(1012)]
    public class FreezeChanceSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float freezeChancePercent = 20f;
        #endregion

        #region [Public Methods]
        public FreezeChanceSupportModifier()
        {
            Chroma = ChromaType.Origin;
            Mineral = MineralType.Amber;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillElement == ElementType.Water;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, freezeChancePercent);
        }
        #endregion
    }

    [SupportModifierId(1013)]
    public class KnockbackSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float knockbackDistance = 2f;
        #endregion

        #region [Public Methods]
        public KnockbackSupportModifier()
        {
            Chroma = ChromaType.Physical;
            Mineral = MineralType.Hematite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, knockbackDistance);
        }
        #endregion
    }

    [SupportModifierId(1014)]
    public class CriticalPowerSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float critDamageMult = 1.5f;
        public float lukScalingMult = 0.05f;
        #endregion

        #region [Public Methods]
        public CriticalPowerSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Moldavite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            float bonus = 0f;
            if (payload.Caster != null && payload.Caster.attributeSystem != null)
            {
                bonus = payload.Caster.attributeSystem.GetAttribute(AttributeType.LUK).CurrentValue * lukScalingMult;
            }
            payload.BaseDamage *= (critDamageMult + bonus);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (critDamageMult - 1f) * 100f, lukScalingMult * 100f);
        }
        #endregion
    }

    [SupportModifierId(1015)]
    public class ManaLeechSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float leechPercent = 10f;
        #endregion

        #region [Public Methods]
        public ManaLeechSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Celestite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, leechPercent);
        }
        #endregion
    }

    [SupportModifierId(1016)]
    public class LifeLeechSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float leechPercent = 10f;
        #endregion

        #region [Public Methods]
        public LifeLeechSupportModifier()
        {
            Chroma = ChromaType.Soul;
            Mineral = MineralType.Moldavite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillDamageType == DamageType.Physical;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, leechPercent);
        }
        #endregion
    }

    [SupportModifierId(1017)]
    public class ProjectileSpeedSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float speedPercent = 30f;
        #endregion

        #region [Public Methods]
        public ProjectileSpeedSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Tektite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, speedPercent);
        }
        #endregion
    }

    [SupportModifierId(1018)]
    public class SplitProjectileSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int extraProjectiles = 2;
        #endregion

        #region [Public Methods]
        public SplitProjectileSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ProjectileCount += extraProjectiles;
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, extraProjectiles);
        }
        #endregion
    }

    [SupportModifierId(1019)]
    public class HomingSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public HomingSupportModifier()
        {
            Chroma = ChromaType.Dimensional;
            Mineral = MineralType.Alexandrite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.Tags.Contains(SkillTag.Projectile);
        }

        public override string GetDescription(string template)
        {
            return template;
        }
        #endregion
    }

    [SupportModifierId(1020)]
    public class EmpoweredCastSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float baseEmpowerDamage = 50f;
        public float intScalingMult = 1f;
        #endregion

        #region [Public Methods]
        public EmpoweredCastSupportModifier()
        {
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Labradorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
            float bonus = 0f;
            if (payload.Caster != null && payload.Caster.attributeSystem != null)
            {
                bonus = payload.Caster.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue * intScalingMult;
            }
            payload.BaseDamage += (baseEmpowerDamage + bonus);
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.SkillDamageType == DamageType.Magic;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, baseEmpowerDamage, intScalingMult * 100f);
        }
        #endregion
    }

    [SupportModifierId(1021)]
    public class EchoSpellSupportModifier : SupportModifier, ICastRepeater
    {
        #region [Variables/Fields]
        public float aoeMultiplier = 1.3f;
        public float damageMultiplier = 0.6f;
        public float repeatDelay = 0.15f;
        #endregion

        #region [Properties]
        public float RepeatDelay => repeatDelay;
        #endregion

        #region [Public Methods]
        public EchoSpellSupportModifier()
        {
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Fluorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public SkillPayload CreateRepeatPayload(SkillPayload original)
        {
            var echoPayload = original.Clone();
            echoPayload.AOERadius *= aoeMultiplier;
            echoPayload.BaseDamage *= damageMultiplier;
            return echoPayload;
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (aoeMultiplier - 1f) * 100f, (1f - damageMultiplier) * 100f);
        }
        #endregion
    }

    [SupportModifierId(1022)]
    public class SpellCascadeSupportModifier : SupportModifier, IPayloadSplitter
    {
        #region [Variables/Fields]
        public float sideDamageMultiplier = 0.5f;
        public float mainDamageMultiplier = 0.75f;
        public float offsetMargin = 5f;
        #endregion

        #region [Public Methods]
        public SpellCascadeSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public bool CanSplit(ActiveSkill skill)
        {
            return skill != null && skill.BaseTargeting is TargetLocationTargeting;
        }

        public IReadOnlyList<SkillPayload> Split(SkillPayload payload, Vector3 casterPosition)
        {
            Vector3 targetPos = payload.TargetPosition;

            Vector3 direction = (targetPos - casterPosition).normalized;
            if (direction == Vector3.zero) direction = Vector3.up;

            Vector3 leftDir = new Vector3(-direction.y, direction.x, 0f);
            Vector3 rightDir = new Vector3(direction.y, -direction.x, 0f);

            var mainPayload = payload.Clone();
            mainPayload.BaseDamage *= mainDamageMultiplier;

            var leftPayload = payload.Clone();
            leftPayload.TargetPosition = targetPos + leftDir * offsetMargin;
            leftPayload.BaseDamage *= sideDamageMultiplier;

            var rightPayload = payload.Clone();
            rightPayload.TargetPosition = targetPos + rightDir * offsetMargin;
            rightPayload.BaseDamage *= sideDamageMultiplier;

            return new[] { leftPayload, mainPayload, rightPayload };
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.BaseTargeting is TargetLocationTargeting && activeSkill.Tags.Contains(SkillTag.AreaOfEffect);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, sideDamageMultiplier * 100f, mainDamageMultiplier * 100f);
        }
        #endregion
    }
}
