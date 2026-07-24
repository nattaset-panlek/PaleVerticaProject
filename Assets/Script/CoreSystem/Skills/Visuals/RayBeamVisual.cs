using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class RayBeamVisual : IVisualHandler
    {
        public void Execute(GameObject prefab, SkillPayload context)
        {
            if (prefab != null && context.Targets.Count > 0)
            {
                for (int i = 0; i < context.Targets.Count; i++)
                {
                    if (context.Targets[i] != null && context.Targets[i].transform != null)
                    {
                        GameObject beam = Object.Instantiate(prefab, context.OriginPosition, Quaternion.identity);

                        LineRenderer lineRenderer = beam.GetComponent<LineRenderer>();
                        if (lineRenderer != null)
                        {
                            lineRenderer.SetPosition(0, context.OriginPosition);
                            lineRenderer.SetPosition(1, context.Targets[i].transform.position);
                        }

                        InitializePayload(beam, context);
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
