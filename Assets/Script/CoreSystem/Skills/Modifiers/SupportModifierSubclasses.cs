using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    public class MultiShotSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int extraProjectiles = 1;
        #endregion

        #region [Public Methods]
        public MultiShotSupportModifier()
        {
            ID = 1001;
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

    public class EchoSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float damageMultiplier = 0.8f;
        public float cooldownMultiplier = 1.25f;
        #endregion

        #region [Public Methods]
        public EchoSupportModifier()
        {
            ID = 1002;
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

    public class PierceSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public PierceSupportModifier()
        {
            ID = 1003;
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

    public class ChainSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public ChainSupportModifier()
        {
            ID = 1004;
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

    public class AreaIncreaseSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float aoePercentBonus = 4f;
        public float vctPercentBonus = 2f;
        #endregion

        #region [Public Methods]
        public AreaIncreaseSupportModifier()
        {
            ID = 1005;
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

    public class DamageUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float damageMultiplier = 1.2f;
        #endregion

        #region [Public Methods]
        public DamageUpSupportModifier()
        {
            ID = 1006;
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

    public class CooldownReductionSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float cdrPercent = 10f;
        #endregion

        #region [Public Methods]
        public CooldownReductionSupportModifier()
        {
            ID = 1007;
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

    public class CostReductionSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float costMultiplier = 0.5f;
        #endregion

        #region [Public Methods]
        public CostReductionSupportModifier()
        {
            ID = 1008;
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

    public class DurationUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float durationPercent = 20f;
        #endregion

        #region [Public Methods]
        public DurationUpSupportModifier()
        {
            ID = 1009;
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

    public class StatusChanceUpSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float statusChancePercent = 15f;
        #endregion

        #region [Public Methods]
        public StatusChanceUpSupportModifier()
        {
            ID = 1010;
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

    public class BurnChanceSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float burnChancePercent = 20f;
        #endregion

        #region [Public Methods]
        public BurnChanceSupportModifier()
        {
            ID = 1011;
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

    public class FreezeChanceSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float freezeChancePercent = 20f;
        #endregion

        #region [Public Methods]
        public FreezeChanceSupportModifier()
        {
            ID = 1012;
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

    public class KnockbackSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float knockbackDistance = 2f;
        #endregion

        #region [Public Methods]
        public KnockbackSupportModifier()
        {
            ID = 1013;
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

    public class CriticalPowerSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float critDamageMult = 1.5f;
        public float lukScalingMult = 0.05f;
        #endregion

        #region [Public Methods]
        public CriticalPowerSupportModifier()
        {
            ID = 1014;
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

    public class ManaLeechSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float leechPercent = 10f;
        #endregion

        #region [Public Methods]
        public ManaLeechSupportModifier()
        {
            ID = 1015;
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

    public class LifeLeechSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float leechPercent = 10f;
        #endregion

        #region [Public Methods]
        public LifeLeechSupportModifier()
        {
            ID = 1016;
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

    public class ProjectileSpeedSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float speedPercent = 30f;
        #endregion

        #region [Public Methods]
        public ProjectileSpeedSupportModifier()
        {
            ID = 1017;
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

    public class SplitProjectileSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public int extraProjectiles = 2;
        #endregion

        #region [Public Methods]
        public SplitProjectileSupportModifier()
        {
            ID = 1018;
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

    public class HomingSupportModifier : SupportModifier
    {
        #region [Public Methods]
        public HomingSupportModifier()
        {
            ID = 1019;
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

    public class EmpoweredCastSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float baseEmpowerDamage = 50f;
        public float intScalingMult = 1f;
        #endregion

        #region [Public Methods]
        public EmpoweredCastSupportModifier()
        {
            ID = 1020;
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

    public class EchoSpellSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float aoeMultiplier = 1.3f;
        public float damageMultiplier = 0.6f;
        #endregion

        #region [Public Methods]
        public EchoSpellSupportModifier()
        {
            ID = 1021;
            Chroma = ChromaType.Magic;
            Mineral = MineralType.Fluorite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, (aoeMultiplier - 1f) * 100f, (1f - damageMultiplier) * 100f);
        }
        #endregion
    }

    public class SpellCascadeSupportModifier : SupportModifier
    {
        #region [Variables/Fields]
        public float sideDamageMultiplier = 0.5f;
        public float mainDamageMultiplier = 0.75f;
        public float offsetMargin = 5f;
        #endregion

        #region [Public Methods]
        public SpellCascadeSupportModifier()
        {
            ID = 1022;
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
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
