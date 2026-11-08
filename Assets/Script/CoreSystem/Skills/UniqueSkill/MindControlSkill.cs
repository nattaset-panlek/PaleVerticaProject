using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "MindControlSkill", menuName = "Skill System/Unique/Mind Control")]
    public class MindControlSkill : ActiveSkill
    {
        #region [Variables/Fields]
        public float Duration = 4f;
        #endregion

        #region [Public Methods]
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            Character owner = caster != null ? caster.CharacterRef : null;
            if (owner == null || BaseTargeting == null) return;

            var targets = BaseTargeting.GetTargets(payload);
            if (targets == null || targets.Count == 0 || !(targets[0] is Character target)) return;
            if (target.IsDead || (target.Tags & CharacterTag.ImmuneToControl) != 0) return;

            float duration = SupportHitFormula.Duration(Duration, payload.DurationMultiplier);
            MindControlledUnit unit = target.GetComponent<MindControlledUnit>();
            if (unit == null) unit = target.gameObject.AddComponent<MindControlledUnit>();
            unit.Begin(owner, target, duration);

            if (owner.UniquePassives != null) owner.UniquePassives.TriggerOnSkillExecuted(this, payload);
        }
        #endregion
    }
}
