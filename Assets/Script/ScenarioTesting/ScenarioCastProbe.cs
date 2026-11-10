#if UNITY_EDITOR || PV_SCENARIO_TESTS
using CoreSystem.Entity;
using CoreSystem.Entity.UniquePassives;
using CoreSystem.Skills;

namespace ScenarioTesting
{
    public sealed class ScenarioCastProbe : UniquePassive
    {
        #region [Variables/Fields]
        public const string ProbeId = "ScenarioCastProbe";
        #endregion

        #region [Properties]
        public int Executions { get; private set; }
        #endregion

        #region [Public Methods]
        public static ScenarioCastProbe Attach(Character owner)
        {
            if (owner == null || owner.UniquePassives == null) return null;
            owner.UniquePassives.RemovePassive(ProbeId);
            var probe = new ScenarioCastProbe();
            probe.Initialize(ProbeId, owner);
            owner.UniquePassives.AddPassive(probe);
            return probe;
        }

        public void Detach()
        {
            if (Owner != null && Owner.UniquePassives != null) Owner.UniquePassives.RemovePassive(ProbeId);
        }

        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            Executions++;
        }
        #endregion
    }
}
#endif
