using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    public enum TargetAlignment
    {
        Enemy,
        Ally,
        All
    }

    [System.Serializable]
    public abstract class TargetingLogic
    {
        public TargetAlignment AlignmentAction = TargetAlignment.Enemy;
        public bool IncludeSelf = false;
        public virtual bool RequiresSelection => true;

        public abstract List<IEntity> GetTargets(SkillPayload payload);

        protected bool IsValidTarget(IEntity caster, IEntity target)
        {
            if (caster == null || target == null) return false;

            if (AlignmentAction == TargetAlignment.All) return true;
            if (AlignmentAction == TargetAlignment.Ally && caster.Faction == target.Faction) return true;
            if (AlignmentAction == TargetAlignment.Enemy && caster.Faction != target.Faction) return true;

            return false;
        }

#if UNITY_EDITOR
        public virtual void DrawGizmos(Vector3 center, Vector2 direction, SkillPayload payload = null)
        {
        }
#endif
    }
}
