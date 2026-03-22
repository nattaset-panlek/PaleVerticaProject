using CoreSystem.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class SelfTargeting : TargetingLogic
    {
        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            List<IEntity> targets = new List<IEntity>();
            
            if (payload.Caster != null && payload.Caster is IEntity entity)
            {
                targets.Add(entity);
            }
            
            return targets;
        }
    }
}
