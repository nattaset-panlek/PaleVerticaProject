using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class ProjectileVisual : IVisualHandler
    {
        public void Execute(VisualPayload data, SkillPayload context)
        {
            if (data.Prefab != null)
            {
                Object.Instantiate(data.Prefab, context.OriginPosition, Quaternion.identity);
            }
        }
    }
}
