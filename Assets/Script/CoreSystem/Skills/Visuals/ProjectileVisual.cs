using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class ProjectileVisual : IVisualHandler
    {
        public void Execute(GameObject prefab, SkillPayload context)
        {
            if (prefab != null)
            {
                Quaternion rotation = Quaternion.identity;
                if (context.IsTargetPositionSet)
                {
                    Vector3 dir = (context.TargetPosition - context.OriginPosition).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                else if (context.Caster != null)
                {
                    rotation = context.Caster.transform.rotation;
                }

                int count = Mathf.Max(1, context.ProjectileCount);
                if (count == 1)
                {
                    GameObject proj = Object.Instantiate(prefab, context.OriginPosition, rotation);
                    InitializePayload(proj, context);
                }
                else
                {
                    float spreadAngle = Mathf.Min(180f, 45f * (count - 1));
                    float startAngle = -spreadAngle / 2f;
                    float angleStep = spreadAngle / (count - 1);

                    for (int i = 0; i < count; i++)
                    {
                        float currentAngle = startAngle + (angleStep * i);
                        Quaternion projRotation = rotation * Quaternion.Euler(0, 0, currentAngle);
                        GameObject proj = Object.Instantiate(prefab, context.OriginPosition, projRotation);
                        InitializePayload(proj, context);
                    }
                }
            }
        }

        private void InitializePayload(GameObject go, SkillPayload payload)
        {
            if (go == null || payload == null) return;
            var visual = go.GetComponent<ISkillVisual>();
            if (visual != null)
            {
                visual.Initialize(payload);
            }
            else
            {
                var visuals = go.GetComponentsInChildren<ISkillVisual>();
                foreach (var v in visuals)
                {
                    v.Initialize(payload);
                }
            }
        }
    }
}
