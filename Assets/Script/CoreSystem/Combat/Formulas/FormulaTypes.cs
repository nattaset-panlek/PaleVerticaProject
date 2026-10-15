namespace CoreSystem.Combat
{
    public struct CastTimeInput
    {
        #region [Variables/Fields]
        public float Dex;
        public float Int;
        public float VariableFlat;
        public float GearVariableFlat;
        public float SkillVariableFlat;
        public float VariablePercent;
        public float GearVariablePercent;
        public float SkillVariablePercent;
        public float FixedFlat;
        public float FixedPercent;
        #endregion
    }

    public struct CastTime
    {
        #region [Variables/Fields]
        public float Variable;
        public float Fixed;
        #endregion

        #region [Properties]
        public float Total => Variable + Fixed;
        #endregion
    }

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

    public struct CoreStats
    {
        #region [Variables/Fields]
        public float Str;
        public float Agi;
        public float Vit;
        public float Int;
        public float Dex;
        public float Luk;
        #endregion
    }

    public struct DerivedStats
    {
        #region [Variables/Fields]
        public float AtkFlat;
        public float MatkFlat;
        public float WeightLimitFlat;
        public float FleeFlat;
        public float HitFlat;
        public float StatusDefFlat;
        public float AspdFlat;
        public float MaxHpPercent;
        public float MaxMpPercent;
        public float HealingItemsPercent;
        public float HpRecoveryFlat;
        public float MpRecoveryFlat;
        public float StatusDmgFlat;
        public float CriticalRateFlat;
        public float CritResistanceFlat;
        public float PerfectDodgeFlat;
        #endregion
    }

    public struct ResourceCost
    {
        #region [Variables/Fields]
        public float Mana;
        public float Hp;
        public bool IsHpCost;
        #endregion
    }

    public struct StatResult
    {
        #region [Variables/Fields]
        public float FlatSum;
        public float PercentAddSum;
        public float Value;
        #endregion
    }
}
