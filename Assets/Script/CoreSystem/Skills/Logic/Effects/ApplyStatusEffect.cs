using CoreSystem.Combat;
using CoreSystem.Entity;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class ApplyStatusEffect : EffectLogic
    {
        #region [Variables/Fields]
        public StatusKind Status = StatusKind.Slow;
        public float Duration = 2f;
        public float TickDamage;
        public float TickInterval = 0.5f;
        public float SlowPercent = 30f;
        #endregion

        #region [Public Methods]
        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (target == null) return;
            var spec = new StatusSpec
            {
                Status = Status,
                Duration = SupportHitFormula.Duration(Duration, payload.DurationMultiplier),
                TickDamage = TickDamage,
                TickInterval = TickInterval,
                SlowPercent = SlowPercent,
                CreditTo = payload.Caster
            };
            StatusHandler.Apply(payload.Caster, target, spec, ProcSkillIds.StatusApply);
        }
        #endregion
    }
}
