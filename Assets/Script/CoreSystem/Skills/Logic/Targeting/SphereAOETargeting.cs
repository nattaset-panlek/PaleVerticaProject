using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class SphereAOETargeting : TargetingLogic
    {
        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            List<IEntity> targets = new List<IEntity>();

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(payload.OriginPosition, payload.AOERadius);

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
            if (payload == null) return;
            
            UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.2f);
            UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, payload.AOERadius);
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, payload.AOERadius);
        }
#endif
    }
}
