using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public abstract class EffectLogic
    {
        public virtual bool ApplyOncePerCast => false;
        public abstract void Apply(SkillPayload payload, IEntity target);
    }
}
