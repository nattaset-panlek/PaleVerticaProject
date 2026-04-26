using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Visuals
{
    public interface ISkillIndicator
    {
        void Show(SkillPayload payload, TargetingLogic logic);
        void UpdateState(Vector3 origin, Vector3 target, Vector2 direction);
        void SetProgress(float progress);
        void StartExecution(SkillPayload payload);
        void Hide();
    }


    public enum IndicatorType
    {
        Circle,
        Line,
        Cone,
        Vector
    }
}
