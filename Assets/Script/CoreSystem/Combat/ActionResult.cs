using System.Collections.Generic;
using CoreSystem.Entity;

namespace CoreSystem.Combat
{
    public sealed class ActionResult
    {
        #region [Variables/Fields]
        private readonly List<Outcome> _outcomes = new List<Outcome>(1);
        private readonly List<ActionResult> _subResults = new List<ActionResult>();
        #endregion

        #region [Properties]
        public CombatAction Action { get; }
        public IEntity User { get; }
        public IEntity Target { get; }
        public int Depth { get; }
        public BlockReason BlockReason { get; internal set; }
        public bool IsBlocked => BlockReason != BlockReason.None;
        public IReadOnlyList<Outcome> Outcomes => _outcomes;
        public IReadOnlyList<ActionResult> SubResults => _subResults;
        #endregion

        #region [Public Methods]
        public ActionResult(CombatAction action, IEntity user, IEntity target, int depth)
        {
            Action = action;
            User = user;
            Target = target;
            Depth = depth;
        }

        public float Total(EffectKind kind)
        {
            float total = 0f;
            for (int i = 0; i < _outcomes.Count; i++)
            {
                if (!_outcomes[i].IsBlocked && _outcomes[i].Kind == kind) total += _outcomes[i].Amount;
            }
            return total;
        }

        public bool HasHit()
        {
            return Total(EffectKind.Damage) > 0f;
        }

        public bool HasCritical()
        {
            for (int i = 0; i < _outcomes.Count; i++)
            {
                if (!_outcomes[i].IsBlocked && _outcomes[i].IsCritical) return true;
            }
            return false;
        }

        public bool PredictsKill()
        {
            var hp = Target?.attributeSystem?.GetAttribute(AttributeType.HP);
            if (hp == null || hp.CurrentValue <= 0f) return false;

            float hpDamage = 0f;
            for (int i = 0; i < _outcomes.Count; i++)
            {
                var outcome = _outcomes[i];
                if (!outcome.IsBlocked && outcome.Kind == EffectKind.Damage && outcome.Attribute == AttributeType.HP) hpDamage += outcome.Amount;
            }
            return hpDamage >= hp.CurrentValue;
        }

        internal void AddOutcome(Outcome outcome)
        {
            if (outcome != null) _outcomes.Add(outcome);
        }

        internal void AddSubResult(ActionResult result)
        {
            if (result != null) _subResults.Add(result);
        }
        #endregion
    }
}
