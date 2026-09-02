using CoreSystem.Entity;
using UnityEngine;


namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public abstract class EffectLogic
    {
        [field: SerializeField] public virtual bool ApplyToCaster { get; set; } = false;
        [field: SerializeField] public virtual bool ApplyOncePerCast { get; set; } = false;
        public abstract void Apply(SkillPayload payload, IEntity target);
    }
}
