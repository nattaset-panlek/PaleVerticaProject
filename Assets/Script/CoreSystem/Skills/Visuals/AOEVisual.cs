using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class AOEVisual : IVisualHandler
    {
        public void Execute(VisualPayload data, SkillPayload context)
        {
            if (data.Prefab != null)
            {
                GameObject aoeEffect = Object.Instantiate(data.Prefab, context.OriginPosition, Quaternion.identity);
                aoeEffect.transform.localScale = new Vector3(context.AOERadius * 2f, context.AOERadius * 2f, 1f);
            }
        }
    }
}
