using CoreSystem.Attributes;
using CoreSystem.Entity;
using CoreSystem.Skills;
using UnityEngine;

namespace CoreSystem.Combat
{
    public struct DamageInput
    {
        #region [Variables/Fields]
        public float BaseDamage;
        public bool HasCasterStats;
        public float CasterPower;
        public float CasterWeaponPower;
        public float ElementAdvantage;
        public float ElementBonus;
        public float ElementResist;
        public float SizeBonus;
        public float SizeResist;
        public float RaceBonus;
        public float RaceResist;
        public float CritRate;
        public float CritResist;
        public float CritDamage;
        public float TargetDefense;
        #endregion
    }

    public struct DamageOffense
    {
        #region [Variables/Fields]
        public float RawDamage;
        public bool IsCritical;
        #endregion
    }

    public struct DamageResult
    {
        #region [Variables/Fields]
        public float Damage;
        public bool IsCritical;
        #endregion
    }

    public static class DamageFormula
    {
        #region [Variables/Fields]
        public const float BaseCriticalMultiplier = 1.5f;
        public const float MinimumDamage = 1f;
        #endregion

        #region [Public Methods]
        public static DamageInput BuildInput(SkillPayload payload, IEntity user, IEntity target)
        {
            var input = new DamageInput
            {
                BaseDamage = payload.BaseDamage,
                ElementAdvantage = ElementAdvantageTable.GetMultiplier(payload.SkillElement, target.Element)
            };

            AttributeSystem targetStats = target.attributeSystem;
            AttributeType defenseType = payload.DamageType == DamageType.Physical ? AttributeType.Defense : AttributeType.StatusDEF;
            input.TargetDefense = targetStats.GetAttribute(defenseType);

            if (user == null || user.attributeSystem == null) return input;

            AttributeSystem userStats = user.attributeSystem;
            input.HasCasterStats = true;

            if (payload.DamageType == DamageType.Physical)
            {
                input.CasterPower = userStats.GetAttribute(AttributeType.ATK).CurrentValue;
                input.CasterWeaponPower = userStats.GetAttribute(AttributeType.WeaponATK).CurrentValue;
            }
            else
            {
                input.CasterPower = userStats.GetAttribute(AttributeType.MATK).CurrentValue;
                input.CasterWeaponPower = userStats.GetAttribute(AttributeType.WeaponMATK).CurrentValue;
            }

            input.ElementBonus = CombatStatLookup.ElementDamageBonus(userStats, payload.SkillElement);
            input.ElementResist = CombatStatLookup.ElementResistance(targetStats, payload.SkillElement);
            input.SizeBonus = CombatStatLookup.SizeDamageBonus(userStats, target.Size);
            input.SizeResist = CombatStatLookup.SizeResistance(targetStats, user.Size);
            input.RaceBonus = CombatStatLookup.RaceDamageBonus(userStats, target.Race);
            input.RaceResist = CombatStatLookup.RaceResistance(targetStats, user.Race);
            input.CritRate = userStats.GetAttribute(AttributeType.CriticalRate);
            input.CritResist = targetStats.GetAttribute(AttributeType.CritResistance);
            input.CritDamage = userStats.GetAttribute(AttributeType.CriticalDamage);
            return input;
        }

        public static DamageOffense Offense(in DamageInput input, float critRoll)
        {
            float damage = input.BaseDamage;
            if (input.HasCasterStats)
            {
                damage += input.CasterPower;
                damage += input.CasterWeaponPower;
            }

            damage *= input.ElementAdvantage;

            bool isCritical = false;
            if (input.HasCasterStats)
            {
                damage *= Mathf.Max(0f, 1f + (input.ElementBonus - input.ElementResist));
                damage *= Mathf.Max(0f, 1f + (input.SizeBonus - input.SizeResist));
                damage *= Mathf.Max(0f, 1f + (input.RaceBonus - input.RaceResist));

                float critChance = Mathf.Max(0f, input.CritRate - input.CritResist);
                if (critRoll < critChance)
                {
                    isCritical = true;
                    damage *= BaseCriticalMultiplier + input.CritDamage;
                }
            }

            return new DamageOffense { RawDamage = damage, IsCritical = isCritical };
        }

        public static float Mitigate(float rawDamage, float targetDefense)
        {
            return Mathf.Max(MinimumDamage, rawDamage - targetDefense);
        }

        public static DamageResult Calculate(in DamageInput input, float critRoll)
        {
            DamageOffense offense = Offense(input, critRoll);
            return new DamageResult
            {
                Damage = Mitigate(offense.RawDamage, input.TargetDefense),
                IsCritical = offense.IsCritical
            };
        }
        #endregion
    }
}
