namespace CoreSystem.Combat
{
    public static class DerivedStatFormula
    {
        #region [Variables/Fields]
        public const float MpRecoveryBonusThreshold = 120f;
        #endregion

        #region [Public Methods]
        public static DerivedStats Compute(in CoreStats stats)
        {
            float str = stats.Str;
            float agi = stats.Agi;
            float vit = stats.Vit;
            float @int = stats.Int;
            float dex = stats.Dex;
            float luk = stats.Luk;

            var derived = new DerivedStats
            {
                AtkFlat = str + (dex / 5f) + (luk / 3f),
                WeightLimitFlat = str * 30f,

                FleeFlat = agi + (luk / 5f),
                AspdFlat = (agi * 0.5f) + (dex * 0.15f),

                MaxHpPercent = vit * 0.01f,
                HealingItemsPercent = vit * 0.02f,
                HpRecoveryFlat = vit / 5f,

                MatkFlat = (@int * 1.5f) + (dex / 5f) + (luk / 3f),
                MaxMpPercent = @int * 0.01f,

                HitFlat = dex + (luk / 3f),

                StatusDmgFlat = (luk * 0.5f) + (@int / 4f),
                CriticalRateFlat = luk * 0.3f,
                CritResistanceFlat = luk / 5f,
                PerfectDodgeFlat = luk / 10f
            };

            float statusDef = (vit / 2f) + (agi / 5f) + (luk / 5f);
            statusDef += (vit / 5f) + (dex / 5f);
            statusDef += @int;
            derived.StatusDefFlat = statusDef;

            float mpRecovery = @int / 6f;
            if (@int >= MpRecoveryBonusThreshold) mpRecovery += (@int - MpRecoveryBonusThreshold) / 2f;
            derived.MpRecoveryFlat = mpRecovery;

            return derived;
        }
        #endregion
    }
}
