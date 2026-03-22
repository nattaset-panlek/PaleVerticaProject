using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public abstract class EffectLogic
    {
        public abstract void Apply(SkillPayload payload, IEntity target);
    }
}
