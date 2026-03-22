using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class LinePierceTargeting : TargetingLogic
    {
        public float LineWidth = 1f;

        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            List<IEntity> targets = new List<IEntity>();

            Vector2 direction = payload.Caster.transform.up;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(payload.OriginPosition, LineWidth, direction, payload.AOERadius);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null)
                {
                    IEntity entity = hits[i].collider.GetComponent<IEntity>();
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
            }

            return targets;
        }

#if UNITY_EDITOR
        public override void DrawGizmos(Vector3 center, Vector2 direction, SkillPayload payload = null)
        {
            if (payload == null) return;
            
            float distance = payload.AOERadius;
            float radius = LineWidth;
            Vector3 endPos = center + (Vector3)(direction * distance);
            
            // Draw a quick representation of the thick line 
            Vector3 dir3D = direction;
            Vector3 right = Vector3.Cross(dir3D, Vector3.forward).normalized * radius;
            Vector3[] points = new Vector3[] {
                center - right,
                center + right,
                endPos + right,
                endPos - right
            };
            UnityEditor.Handles.DrawSolidRectangleWithOutline(points, new Color(1f, 0f, 0f, 0.2f), Color.red);
        }
#endif
    }
}
