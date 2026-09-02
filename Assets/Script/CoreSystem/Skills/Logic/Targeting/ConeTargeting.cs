using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class ConeTargeting : TargetingLogic
    {
        [Tooltip("The total angle of the cone in degrees.")]
        public float ConeAngle = 90f;

        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            List<IEntity> targets = new List<IEntity>();

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(payload.OriginPosition, payload.AOERadius);
            Vector2 forward = payload.GetAimDirection();

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
                        Vector2 dirToTarget = (hitColliders[i].transform.position - payload.OriginPosition).normalized;
                        float angle = Vector2.Angle(forward, dirToTarget);
                        if (angle <= ConeAngle / 2f)
                        {
                            if (!targets.Contains(entity))
                            {
                                targets.Add(entity);
                            }
                        }
                    }
                }
            }

            return targets;
        }

#if UNITY_EDITOR
        public override void DrawGizmos(Vector3 center, Vector2 direction, SkillPayload payload = null)
        {
            if (payload == null) return;
            
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.2f);
            
            // Draw Solid Arc
            UnityEditor.Handles.DrawSolidArc(center, Vector3.forward, direction, ConeAngle / 2f, payload.AOERadius);
            UnityEditor.Handles.DrawSolidArc(center, Vector3.forward, direction, -ConeAngle / 2f, payload.AOERadius);
            
            // Draw Wire Arc
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 1f);
            UnityEditor.Handles.DrawWireArc(center, Vector3.forward, direction, ConeAngle / 2f, payload.AOERadius);
            UnityEditor.Handles.DrawWireArc(center, Vector3.forward, direction, -ConeAngle / 2f, payload.AOERadius);
            
            // Draw lines to connect center to arc ends
            Vector3 rightDir = Quaternion.Euler(0, 0, -ConeAngle / 2f) * direction;
            Vector3 leftDir = Quaternion.Euler(0, 0, ConeAngle / 2f) * direction;
            UnityEditor.Handles.DrawLine(center, center + rightDir * payload.AOERadius);
            UnityEditor.Handles.DrawLine(center, center + leftDir * payload.AOERadius);
        }
#endif
    }
}

