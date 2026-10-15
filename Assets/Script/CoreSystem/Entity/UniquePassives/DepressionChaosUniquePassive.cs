using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("DepressionChaos")]
    public class DepressionChaosUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            if (payload.Caster == Owner && skill.Tags.Contains(SkillTag.AreaOfEffect))
            {
                bool isDamaging = skill.Effects.Exists(e => e is DamageEffect);
                if (!isDamaging && payload.Targets != null && payload.Targets.Count > 0)
                {
                    float intVal = Owner.attributeSystem.GetAttribute(AttributeType.INT).CurrentValue;
                    float dmg = intVal * 2f;
                    foreach (var target in payload.Targets)
                    {
                        DealProcDamage(target, dmg, DamageType.Magic, ProcSkillIds.ChaosBurst);
                    }
                }
            }
        }
        #endregion
    }
}
