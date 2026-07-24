using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public interface IVisualHandler
    {
        void Execute(GameObject prefab, SkillPayload context);
    }
}
