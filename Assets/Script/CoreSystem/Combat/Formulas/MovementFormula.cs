using CoreSystem.Movement;
using UnityEngine;

namespace CoreSystem.Combat
{
    public static class MovementFormula
    {
        #region [Variables/Fields]
        public const float DefaultSprintShare = 0.4f;
        public const float DefaultRampShare = 0.1f;
        public const float DefaultJogShare = 0.3f;
        public const float DefaultJogRatio = 0.5f;
        #endregion

        #region [Public Methods]
        public static SprintPhase GetSprintPhase(float elapsed, float duration, float sprintShare = DefaultSprintShare, float rampShare = DefaultRampShare, float jogShare = DefaultJogShare)
        {
            if (duration <= 0f || elapsed >= duration) return SprintPhase.Walk;
            float t = Mathf.Max(0f, elapsed) / duration;
            float sprintEnd = sprintShare;
            float rampEnd = sprintEnd + rampShare;
            float jogEnd = rampEnd + jogShare;
            if (t < sprintEnd) return SprintPhase.Sprint;
            if (t < rampEnd) return SprintPhase.SprintToJog;
            if (t < jogEnd) return SprintPhase.Jog;
            return SprintPhase.JogToWalk;
        }

        public static float SprintBonus(float elapsed, float duration, float sprintBonus, float jogRatio = DefaultJogRatio, float sprintShare = DefaultSprintShare, float rampShare = DefaultRampShare, float jogShare = DefaultJogShare)
        {
            if (duration <= 0f || sprintBonus <= 0f) return 0f;
            float t = Mathf.Clamp01(Mathf.Max(0f, elapsed) / duration);
            float jogBonus = sprintBonus * Mathf.Clamp01(jogRatio);
            float sprintEnd = sprintShare;
            float rampEnd = sprintEnd + rampShare;
            float jogEnd = rampEnd + jogShare;

            if (t < sprintEnd) return sprintBonus;
            if (t < rampEnd) return Mathf.Lerp(sprintBonus, jogBonus, SmoothStep((t - sprintEnd) / Mathf.Max(0.0001f, rampShare)));
            if (t < jogEnd) return jogBonus;
            if (t >= 1f) return 0f;
            return Mathf.Lerp(jogBonus, 0f, SmoothStep((t - jogEnd) / Mathf.Max(0.0001f, 1f - jogEnd)));
        }
        #endregion

        #region [Private Methods]
        private static float SmoothStep(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
        #endregion
    }
}
