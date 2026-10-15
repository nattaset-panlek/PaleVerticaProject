using CoreSystem.Attributes;
using UnityEngine;

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

    public static class CastTimeFormula
    {
        #region [Variables/Fields]
        public const float StatCap = 530f;
        #endregion

        #region [Public Methods]
        public static CastTimeInput BuildInput(AttributeSystem stats)
        {
            return new CastTimeInput
            {
                Dex = Read(stats, AttributeType.DEX),
                Int = Read(stats, AttributeType.INT),
                VariableFlat = Read(stats, AttributeType.VariableCastTimeReduc),
                GearVariableFlat = Read(stats, AttributeType.GearVariableCastTime),
                SkillVariableFlat = Read(stats, AttributeType.SkillVariableCastTime),
                VariablePercent = Read(stats, AttributeType.VariableCastTimeReducPercent),
                GearVariablePercent = Read(stats, AttributeType.GearVariableCastTimePercent),
                SkillVariablePercent = Read(stats, AttributeType.SkillVariableCastTimePercent),
                FixedFlat = Read(stats, AttributeType.FixedCastTimeReduc),
                FixedPercent = Read(stats, AttributeType.FixedCastTimeReducPercent)
            };
        }

        public static CastTime Compute(in CastTimeInput input, float baseVariable, float baseFixed)
        {
            float statTerm = Mathf.Clamp01((input.Dex * 2f + input.Int) / StatCap);
            float statReduction = Mathf.Sqrt(statTerm);

            float variableRemaining = baseVariable - input.VariableFlat - input.GearVariableFlat - input.SkillVariableFlat;
            float variable = variableRemaining
                             * (1f - statReduction)
                             * (1f - (input.GearVariablePercent / 100f))
                             * (1f - (input.SkillVariablePercent / 100f))
                             * (1f - (input.VariablePercent / 100f));

            float fixedTime = (baseFixed - input.FixedFlat) * (1f - (input.FixedPercent / 100f));

            return new CastTime
            {
                Variable = Mathf.Max(0f, variable),
                Fixed = Mathf.Max(0f, fixedTime)
            };
        }
        #endregion

        #region [Private Methods]
        private static float Read(AttributeSystem stats, AttributeType type)
        {
            return stats.GetAttribute(type)?.CurrentValue ?? 0;
        }
        #endregion
    }
}
