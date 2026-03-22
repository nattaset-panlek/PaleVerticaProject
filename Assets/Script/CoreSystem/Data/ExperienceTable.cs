using UnityEngine;

namespace CoreSystem.Data
{
    [CreateAssetMenu(fileName = "ExperienceTable", menuName = "CoreSystem/Experience Table")]
    public class ExperienceTable : ScriptableObject
    {
        [Header("Progression Curves")]
        [Tooltip("X = Level (0.0 to 1.0 mapping), Y = Exp Scale (0.0 to 1.0)")]
        public AnimationCurve BaseExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve JobExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Scaling Settings")]
        public int MaxLevel = 99;
        public long BaseExpAtMax = 1000000; // 1M exp at lvl 99
        public long JobExpAtMax = 500000;   // 500k exp at lvl 99

        public long GetBaseExp(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            float t = (float)currentLevel / MaxLevel;
            return (long)(BaseExpCurve.Evaluate(t) * BaseExpAtMax);
        }

        public long GetJobExp(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            float t = (float)currentLevel / MaxLevel;
            return (long)(JobExpCurve.Evaluate(t) * JobExpAtMax);
        }
    }
}
