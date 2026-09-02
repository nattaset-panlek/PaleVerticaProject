using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class SingleUnitTargeting : TargetingLogic
    {
        #region [Variables/Fields]
        public float CastRange = 8f;
        public float TargetToleranceRadius = 1.5f;
        #endregion

        #region [Properties]
        public override bool RequiresSelection => true;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public override List<IEntity> GetTargets(SkillPayload payload)
        {
            if (payload.IsPositionLocked && payload.Targets != null && payload.Targets.Count > 0)
            {
                // Return the explicitly locked target
                return new List<IEntity>(payload.Targets);
            }

            List<IEntity> targets = new List<IEntity>();
            Vector3 center = payload.IsTargetPositionSet ? payload.TargetPosition : payload.OriginPosition;
            float maxRange = payload.CastRange > 0 ? payload.CastRange : CastRange;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, TargetToleranceRadius);
            IEntity closest = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < colliders.Length; i++)
            {
                IEntity entity = colliders[i].GetComponent<IEntity>();
                if (entity == null) entity = colliders[i].GetComponentInParent<IEntity>();
                if (entity == null) entity = colliders[i].GetComponentInChildren<IEntity>();

                if (entity != null)
                {
                    if (entity == payload.Caster as IEntity && !IncludeSelf) continue;
                    if (IsValidTarget(payload.Caster as IEntity, entity))
                    {
                        float distFromCaster = Vector2.Distance(payload.OriginPosition, entity.transform.position);
                        if (distFromCaster <= maxRange + TargetToleranceRadius)
                        {
                            float distFromCursor = Vector2.Distance(center, entity.transform.position);
                            if (distFromCursor < minDistance)
                            {
                                minDistance = distFromCursor;
                                closest = entity;
                            }
                        }
                    }
                }
            }

            if (closest != null)
            {
                targets.Add(closest);
            }

            return targets;
        }

#if UNITY_EDITOR
        public override void DrawGizmos(Vector3 center, Vector2 direction, SkillPayload payload = null)
        {
            float effectiveRange = (payload != null && payload.CastRange > 0) ? payload.CastRange : (CastRange > 0 ? CastRange : 8f);

            UnityEditor.Handles.color = new Color(0.8f, 0.2f, 0.8f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, effectiveRange);
            UnityEditor.Handles.color = new Color(0.8f, 0.2f, 0.8f, 0.4f);
            UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, effectiveRange);

            if (payload != null && payload.IsTargetPositionSet)
            {
                Vector3 clickPos = payload.TargetPosition;
                UnityEditor.Handles.color = new Color(0.9f, 0.3f, 0.9f, 0.3f);
                UnityEditor.Handles.DrawSolidDisc(clickPos, Vector3.forward, TargetToleranceRadius);
                UnityEditor.Handles.color = new Color(0.9f, 0.3f, 0.9f, 0.9f);
                UnityEditor.Handles.DrawWireDisc(clickPos, Vector3.forward, TargetToleranceRadius);
            }
        }
#endif
        #endregion

        #region [Private Methods]
        #endregion
    }
}
