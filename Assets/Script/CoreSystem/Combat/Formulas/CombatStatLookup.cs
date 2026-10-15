using CoreSystem.Attributes;

namespace CoreSystem.Combat
{
    public static class CombatStatLookup
    {
        #region [Public Methods]
        public static float ElementDamageBonus(AttributeSystem caster, ElementType element)
        {
            switch (element)
            {
                case ElementType.Neutral: return caster.GetAttribute(AttributeType.NeutralDamage);
                case ElementType.Water: return caster.GetAttribute(AttributeType.WaterDamage);
                case ElementType.Earth: return caster.GetAttribute(AttributeType.EarthDamage);
                case ElementType.Fire: return caster.GetAttribute(AttributeType.FireDamage);
                case ElementType.Electric: return caster.GetAttribute(AttributeType.ElectricDamage);
                case ElementType.Ghost: return caster.GetAttribute(AttributeType.GhostDamage);
                case ElementType.Holy: return caster.GetAttribute(AttributeType.HolyDamage);
                default: return 0f;
            }
        }

        public static float ElementResistance(AttributeSystem target, ElementType element)
        {
            switch (element)
            {
                case ElementType.Neutral: return target.GetAttribute(AttributeType.NeutralResist);
                case ElementType.Water: return target.GetAttribute(AttributeType.WaterResist);
                case ElementType.Earth: return target.GetAttribute(AttributeType.EarthResist);
                case ElementType.Fire: return target.GetAttribute(AttributeType.FireResist);
                case ElementType.Electric: return target.GetAttribute(AttributeType.ElectricResist);
                case ElementType.Ghost: return target.GetAttribute(AttributeType.GhostResist);
                case ElementType.Holy: return target.GetAttribute(AttributeType.HolyResist);
                default: return 0f;
            }
        }

        public static float SizeDamageBonus(AttributeSystem caster, SizeType targetSize)
        {
            switch (targetSize)
            {
                case SizeType.Small: return caster.GetAttribute(AttributeType.SmallDamage);
                case SizeType.Medium: return caster.GetAttribute(AttributeType.MediumDamage);
                case SizeType.Large: return caster.GetAttribute(AttributeType.LargeDamage);
                default: return 0f;
            }
        }

        public static float SizeResistance(AttributeSystem target, SizeType attackerSize)
        {
            switch (attackerSize)
            {
                case SizeType.Small: return target.GetAttribute(AttributeType.SmallResist);
                case SizeType.Medium: return target.GetAttribute(AttributeType.MediumResist);
                case SizeType.Large: return target.GetAttribute(AttributeType.LargeResist);
                default: return 0f;
            }
        }

        public static float RaceDamageBonus(AttributeSystem caster, RaceType targetRace)
        {
            switch (targetRace)
            {
                case RaceType.Formless: return caster.GetAttribute(AttributeType.FormlessDamage);
                case RaceType.Undead: return caster.GetAttribute(AttributeType.UndeadDamage);
                case RaceType.Brute: return caster.GetAttribute(AttributeType.BruteDamage);
                case RaceType.Plant: return caster.GetAttribute(AttributeType.PlantDamage);
                case RaceType.Insect: return caster.GetAttribute(AttributeType.InsectDamage);
                case RaceType.Fish: return caster.GetAttribute(AttributeType.FishDamage);
                case RaceType.Demon: return caster.GetAttribute(AttributeType.DemonDamage);
                case RaceType.DemiHuman: return caster.GetAttribute(AttributeType.DemiHumanDamage);
                case RaceType.Angel: return caster.GetAttribute(AttributeType.AngelDamage);
                case RaceType.Dragon: return caster.GetAttribute(AttributeType.DragonDamage);
                default: return 0f;
            }
        }

        public static float RaceResistance(AttributeSystem target, RaceType attackerRace)
        {
            switch (attackerRace)
            {
                case RaceType.Formless: return target.GetAttribute(AttributeType.FormlessResist);
                case RaceType.Undead: return target.GetAttribute(AttributeType.UndeadResist);
                case RaceType.Brute: return target.GetAttribute(AttributeType.BruteResist);
                case RaceType.Plant: return target.GetAttribute(AttributeType.PlantResist);
                case RaceType.Insect: return target.GetAttribute(AttributeType.InsectResist);
                case RaceType.Fish: return target.GetAttribute(AttributeType.FishResist);
                case RaceType.Demon: return target.GetAttribute(AttributeType.DemonResist);
                case RaceType.DemiHuman: return target.GetAttribute(AttributeType.DemiHumanResist);
                case RaceType.Angel: return target.GetAttribute(AttributeType.AngelResist);
                case RaceType.Dragon: return target.GetAttribute(AttributeType.DragonResist);
                default: return 0f;
            }
        }
        #endregion
    }
}
