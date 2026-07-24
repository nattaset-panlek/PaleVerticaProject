using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class GroundEruptionVisual : IVisualHandler
    {
        public void Execute(GameObject prefab, SkillPayload context)
        {
            if (prefab != null)
            {
                Vector3 spawnPos = context.IsTargetPositionSet ? context.TargetPosition : context.OriginPosition;
                GameObject aoeEffect = Object.Instantiate(prefab, spawnPos, Quaternion.identity);

                if (context.AOERadius > 0)
                {
                    aoeEffect.transform.localScale = new Vector3(context.AOERadius * 2f, context.AOERadius * 2f, 1f);
                }

                InitializePayload(aoeEffect, context);
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
