using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Skills.Modifiers
{

    [CreateAssetMenu(fileName = "AOEAmplifierModifier", menuName = "Skill System/Modifiers/AOE Amplifier")]
    public class MultipleProject : SupportModifier
    {
        public int AdditionalProjectile = 1;

        public override void ModifyPayload(ref SkillPayload payload)
        {
            payload.ProjectileCount = payload.ProjectileCount + AdditionalProjectile;
        }
    }
}
