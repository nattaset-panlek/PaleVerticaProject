using UnityEngine;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "BlinkStrikeSkill", menuName = "Skill System/Unique/Blink Strike")]
    public class BlinkStrikeSkill : ActiveSkill
    {
        #region [Variables/Fields]
        public float StrikeDamageMult = 1.5f;
        public float BehindDistance = 1f;
        #endregion

        #region [Public Methods]
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            if (!payload.IsTargetPositionSet) return;

            caster.CharacterRef.transform.position = payload.TargetPosition - payload.GetAimDirection() * BehindDistance;

            SkillPayload strike = payload.Clone();
            strike.DamageMultiplier *= StrikeDamageMult;
            base.Execute(strike, instance, caster);
        }
        #endregion
    }
}
