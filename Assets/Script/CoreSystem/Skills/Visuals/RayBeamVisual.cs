using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class RayBeamVisual : IVisualHandler
    {
        public void Execute(VisualPayload data, SkillPayload context)
        {
            if (data.Prefab != null && context.Targets.Count > 0)
            {
                for (int i = 0; i < context.Targets.Count; i++)
                {
                    if (context.Targets[i] != null && context.Targets[i].transform != null)
                    {
                        GameObject beam = Object.Instantiate(data.Prefab, context.OriginPosition, Quaternion.identity);

                        LineRenderer lineRenderer = beam.GetComponent<LineRenderer>();
                        if (lineRenderer != null)
                        {
                            lineRenderer.SetPosition(0, context.OriginPosition);
                            lineRenderer.SetPosition(1, context.Targets[i].transform.position);
                        }
                    }
                }
            }
        }
    }
}
