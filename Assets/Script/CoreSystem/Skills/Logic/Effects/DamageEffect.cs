using CoreSystem.Combat;
using CoreSystem.Entity;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class DamageEffect : EffectLogic
    {
        #region [Variables/Fields]
        public AttributeType TargetAttribute = AttributeType.HP;
        #endregion

        #region [Public Methods]
        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (payload == null) return;
            CombatCore.Execute(CombatAction.Damage(payload, TargetAttribute), payload.Caster, target);
        }
        #endregion
    }
}
