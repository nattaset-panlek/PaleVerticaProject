using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class BoxTargeting : TargetingLogic
    {
        public Vector2 BoxSize = new Vector2(2f, 5f);
        public Vector2 Offset = new Vector2(0f, 2.5f);

        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            List<IEntity> targets = new List<IEntity>();

            // Calculate rotation angle
            Vector2 forward = payload.Caster.transform.up;
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f;

            // Apply rotation to offset
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, angle) * Offset;
            Vector2 boxCenter = (Vector2)payload.OriginPosition + rotatedOffset;

            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(boxCenter, BoxSize, angle);

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
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, angle) * Offset;
            Vector3 boxCenter = center + (Vector3)rotatedOffset;

            // Draw a rotated box requires matrix transformation
            Matrix4x4 oldMatrix = UnityEditor.Handles.matrix;
            Matrix4x4 newMatrix = Matrix4x4.TRS(boxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);
            UnityEditor.Handles.matrix = newMatrix;
            
            Vector3[] points = new Vector3[]
            {
                new Vector3(-BoxSize.x/2, -BoxSize.y/2, 0),
                new Vector3(BoxSize.x/2, -BoxSize.y/2, 0),
                new Vector3(BoxSize.x/2, BoxSize.y/2, 0),
                new Vector3(-BoxSize.x/2, BoxSize.y/2, 0)
            };

            UnityEditor.Handles.color = new Color(0f, 0.5f, 1f, 0.2f);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(points, new Color(0f, 0.5f, 1f, 0.2f), new Color(0f, 0.5f, 1f, 1f));
            
            UnityEditor.Handles.matrix = oldMatrix;
        }
#endif
    }
}
