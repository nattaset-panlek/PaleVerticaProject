using System.Collections.Generic;
using CoreSystem.Skills;

namespace CoreSystem.Combat
{
    public abstract class EffectSpec
    {
        #region [Properties]
        public abstract EffectKind Kind { get; }
        #endregion
    }

    public sealed class CombatAction
    {
        #region [Variables/Fields]
        private readonly List<EffectSpec> _effects = new List<EffectSpec>(1);
        private readonly List<ActionRider> _riders = new List<ActionRider>();
        #endregion

        #region [Properties]
        public SkillPayload Payload { get; }
        public IReadOnlyList<EffectSpec> Effects => _effects;
        public IReadOnlyList<ActionRider> Riders => _riders;
        public bool IsProc { get; set; }
        #endregion

        #region [Public Methods]
        public CombatAction(SkillPayload payload)
        {
            Payload = payload;
            IsProc = payload != null && ProcSkillIds.IsProc(payload.SkillID);
        }

        public static CombatAction Damage(SkillPayload payload, AttributeType targetAttribute = AttributeType.HP)
        {
            return new CombatAction(payload).WithEffect(new DamageSpec { TargetAttribute = targetAttribute });
        }

        public CombatAction WithEffect(EffectSpec effect)
        {
            if (effect != null) _effects.Add(effect);
            return this;
        }

        public CombatAction WithRider(ActionRider rider)
        {
            if (rider != null) _riders.Add(rider);
            return this;
        }
        #endregion
    }
}
