using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class AuraBuffVisual : IVisualHandler
    {
        public void Execute(VisualPayload data, SkillPayload context)
        {
            if (data.Prefab != null)
            {
                for (int i = 0; i < context.Targets.Count; i++)
                {
                    if (context.Targets[i] != null && context.Targets[i].transform != null)
                    {
                        Object.Instantiate(data.Prefab, context.Targets[i].transform.position, Quaternion.identity, context.Targets[i].transform);
                    }
                }
            }
        }
    }
}
