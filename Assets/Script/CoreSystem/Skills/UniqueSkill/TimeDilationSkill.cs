using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "TimeDilationSkill", menuName = "Skill System/Unique/Time Dilation")]
    public class TimeDilationSkill : ActiveSkill
    {
        public float TimeScale = 0.5f;

        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            // Custom Behavior: Change the tick rate of a Continuous Area 
            // OR apply a custom "Slow" effect to everyone in the area.
            
            Debug.Log("[TimeDilation] Slowing down time in the area!");
            base.Execute(payload, instance, caster);
        }
    }
}
