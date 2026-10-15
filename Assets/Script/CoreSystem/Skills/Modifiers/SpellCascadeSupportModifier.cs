using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Modifiers
{
    [SupportModifierId(1022)]
    public class SpellCascadeSupportModifier : SupportModifier, IPayloadSplitter
    {
        #region [Variables/Fields]
        public float sideDamageMultiplier = 0.5f;
        public float mainDamageMultiplier = 0.75f;
        public float offsetMargin = 5f;
        #endregion

        #region [Public Methods]
        public SpellCascadeSupportModifier()
        {
            Chroma = ChromaType.Cosmic;
            Mineral = MineralType.Pallasite;
        }

        public override void ModifyPayload(ref SkillPayload payload)
        {
        }

        public bool CanSplit(ActiveSkill skill)
        {
            return skill != null && skill.BaseTargeting is TargetLocationTargeting;
        }

        public IReadOnlyList<SkillPayload> Split(SkillPayload payload, Vector3 casterPosition)
        {
            Vector3 targetPos = payload.TargetPosition;

            Vector3 direction = (targetPos - casterPosition).normalized;
            if (direction == Vector3.zero) direction = Vector3.up;

            Vector3 leftDir = new Vector3(-direction.y, direction.x, 0f);
            Vector3 rightDir = new Vector3(direction.y, -direction.x, 0f);

            var mainPayload = payload.Clone();
            mainPayload.BaseDamage *= mainDamageMultiplier;

            var leftPayload = payload.Clone();
            leftPayload.TargetPosition = targetPos + leftDir * offsetMargin;
            leftPayload.BaseDamage *= sideDamageMultiplier;

            var rightPayload = payload.Clone();
            rightPayload.TargetPosition = targetPos + rightDir * offsetMargin;
            rightPayload.BaseDamage *= sideDamageMultiplier;

            return new[] { leftPayload, mainPayload, rightPayload };
        }

        public override bool IsCompatible(ActiveSkill activeSkill)
        {
            return activeSkill.BaseTargeting is TargetLocationTargeting && activeSkill.Tags.Contains(SkillTag.AreaOfEffect);
        }

        public override string GetDescription(string template)
        {
            return string.Format(template, sideDamageMultiplier * 100f, mainDamageMultiplier * 100f);
        }
        #endregion
    }
}
