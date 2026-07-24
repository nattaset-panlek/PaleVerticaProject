using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class AuraBuffVisual : IVisualHandler
    {
        public void Execute(GameObject prefab, SkillPayload context)
        {
            if (prefab != null)
            {
                for (int i = 0; i < context.Targets.Count; i++)
                {
                    if (context.Targets[i] != null && context.Targets[i].transform != null)
                    {
                        GameObject auraObj = Object.Instantiate(prefab, context.Targets[i].transform.position, Quaternion.identity, context.Targets[i].transform);
                        InitializePayload(auraObj, context);
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
