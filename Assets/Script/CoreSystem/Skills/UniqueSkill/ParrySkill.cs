using CoreSystem.Combat;
using CoreSystem.Entity.UniquePassives;
using UnityEngine;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "ParrySkill", menuName = "Skill System/Unique/Parry Skill")]
    public class ParrySkill : ActiveSkill
    {
        #region [Variables/Fields]
        public float ParryWindow = 0.5f;
        public float CounterDamageMult = 2.0f;
        public GameObject CounterVfx;
        #endregion

        #region [Public Methods]
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            var character = caster.CharacterRef;
            if (character == null || character.UniquePassives == null) return;

            float window = SupportHitFormula.Duration(ParryWindow, payload.DurationMultiplier);
            var stance = new ParryStanceUniquePassive(payload, CounterDamageMult, CounterVfx);
            stance.Initialize(ParryStanceUniquePassive.StanceId, character);
            stance.Duration = window;
            stance.RemainingDuration = window;
            character.UniquePassives.AddPassive(stance);
            character.UniquePassives.TriggerOnSkillExecuted(this, payload);
        }
        #endregion
    }
}
