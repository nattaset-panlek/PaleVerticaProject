using UnityEngine;

namespace CoreSystem.Skills
{
    public enum ChromaType
    {
        Physical,
        Magic,
        Soul,
        Cosmic,
        Dimensional,
        Origin
    }

    public enum MineralType
    {
        None,
        Hematite,
        Obsidian,
        Fluorite,
        Labradorite,
        Celestite,
        Moldavite,
        Tektite,
        Pallasite,
        Alexandrite,
        Ulexite,
        Zircon,
        Amber
    }

    public abstract class SupportModifier : ISupportModifier
    {
        public int ID;
        public ChromaType Chroma;
        public MineralType Mineral;

        public string Name;
        public string Description;
        public Sprite Icon;
        public string IconPath;

        public abstract void ModifyPayload(ref SkillPayload payload);

        public virtual bool IsCompatible(ActiveSkill activeSkill)
        {
            return true;
        }

        public virtual string GetDescription(string template)
        {
            return template;
        }

        public static SupportModifier Create(int id)
        {
            switch (id)
            {
                case 1001: return new Modifiers.MultiShotSupportModifier();
                case 1002: return new Modifiers.EchoSupportModifier();
                case 1003: return new Modifiers.PierceSupportModifier();
                case 1004: return new Modifiers.ChainSupportModifier();
                case 1005: return new Modifiers.AreaIncreaseSupportModifier();
                case 1006: return new Modifiers.DamageUpSupportModifier();
                case 1007: return new Modifiers.CooldownReductionSupportModifier();
                case 1008: return new Modifiers.CostReductionSupportModifier();
                case 1009: return new Modifiers.DurationUpSupportModifier();
                case 1010: return new Modifiers.StatusChanceUpSupportModifier();
                case 1011: return new Modifiers.BurnChanceSupportModifier();
                case 1012: return new Modifiers.FreezeChanceSupportModifier();
                case 1013: return new Modifiers.KnockbackSupportModifier();
                case 1014: return new Modifiers.CriticalPowerSupportModifier();
                case 1015: return new Modifiers.ManaLeechSupportModifier();
                case 1016: return new Modifiers.LifeLeechSupportModifier();
                case 1017: return new Modifiers.ProjectileSpeedSupportModifier();
                case 1018: return new Modifiers.SplitProjectileSupportModifier();
                case 1019: return new Modifiers.HomingSupportModifier();
                case 1020: return new Modifiers.EmpoweredCastSupportModifier();
                case 1021: return new Modifiers.EchoSpellSupportModifier();
                case 1022: return new Modifiers.SpellCascadeSupportModifier();
                default: return null;
            }
        }
    }
}
