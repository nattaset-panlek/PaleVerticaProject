using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    /// <summary>
    /// Targeting where the caster selects a point on the ground.
    /// Has a 'CastRange' (Fixed) and an 'AOERadius' (Area of effect at point).
    /// </summary>
    [System.Serializable]
    public class TargetLocationTargeting : TargetingLogic
    {
        public float CastRange = 7f;

        public Vector3 ClampTargetPosition(Vector3 origin, Vector3 target)
        {
            if (CastRange <= 0) return target;

            float dist = Vector2.Distance(origin, target);
            if (dist > CastRange)
            {
                Vector3 dir = (target - origin).normalized;
                return origin + dir * CastRange;
            }
            return target;
        }

        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            List<IEntity> targets = new List<IEntity>();

            // Use payload's AOE radius
            float effectiveAOE = payload.AOERadius;
            
            // The 'OriginPosition' is assumed to be the caster's position.
            // 'TargetPosition' is the destination point provided by the controller.
            Vector3 finalPoint = payload.TargetPosition;
            
            // Re-validate against current position if requested, otherwise trust payload
            // This prevents "Point A followings caster" if the payload was already clamped at start.
            if (!payload.IsPositionLocked)
            {
                finalPoint = ClampTargetPosition(payload.OriginPosition, finalPoint);
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(finalPoint, effectiveAOE);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                IEntity entity = hitColliders[i].GetComponent<IEntity>();
                if (entity == null) entity = hitColliders[i].GetComponentInParent<IEntity>();
                if (entity == null) entity = hitColliders[i].GetComponentInChildren<IEntity>();

                if (entity != null)
                {
                    if (entity == payload.Caster as IEntity && !IncludeSelf) continue;

                    if (IsValidTarget(payload.Caster as IEntity, entity))
                    {
                        if (!targets.Contains(entity))
                        {
                            targets.Add(entity);
                        }
                    }
                }
            }

            return targets;
        }

#if UNITY_EDITOR
        public override void DrawGizmos(Vector3 center, Vector2 direction, SkillPayload payload = null)
        {
            float effectiveRange = CastRange > 0 ? CastRange : 7f;

            // 1. Draw Cast Range circle (The Fixed area)
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.05f);
            UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, effectiveRange);
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.3f);
            UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, effectiveRange);

            // 2. Draw AOE circle at the target location (usually payload.TargetPosition)
            float aoe = (payload != null) ? payload.AOERadius : 2f;
            Vector3 targetPos = center + (Vector3)direction.normalized * (CastRange * 0.7f); 
            
            if (payload != null && payload.IsTargetPositionSet)
            {
                targetPos = payload.TargetPosition;
                // Clamp to CastRange for visual accuracy in indicators
                float dist = Vector2.Distance(center, targetPos);
                if (dist > CastRange)
                {
                    Vector3 dir = (targetPos - center).normalized;
                    targetPos = center + dir * CastRange;
                }
            }

            UnityEditor.Handles.color = new Color(0f, 0.7f, 1f, 0.25f);
            UnityEditor.Handles.DrawSolidDisc(targetPos, Vector3.forward, aoe);
            UnityEditor.Handles.color = new Color(0f, 0.7f, 1f, 0.8f);
            UnityEditor.Handles.DrawWireDisc(targetPos, Vector3.forward, aoe);
            
            UnityEditor.Handles.Label(targetPos + new Vector3(5, 15, 0), "Target Point", UnityEditor.EditorStyles.miniLabel);
        }
#endif
    }
}
