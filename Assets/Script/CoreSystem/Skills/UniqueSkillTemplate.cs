using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills
{
    [CreateAssetMenu(fileName = "NewUniqueSkill", menuName = "Skill System/Unique Skill")]
    public class UniqueSkillTemplate : ActiveSkill
    {
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            // 1. You can still call the base logic if you want standard targeting/effects
            // base.Execute(payload, instance, caster);

            // 2. Or implement completely custom logic
            Debug.Log($"Executing Unique Skill: {Name}");
            
            // Example: Do something directly to the caster
            caster.CharacterRef.gameObject.transform.localScale *= 1.1f;
            
            // Example: Custom targeting
            // var targets = MyCustomTargetingFunction();
        }
    }
}
