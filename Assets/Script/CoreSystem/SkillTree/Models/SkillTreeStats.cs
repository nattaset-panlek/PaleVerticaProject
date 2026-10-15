using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.SkillTree.Models
{
    [System.Serializable]
    public class SkillTreeStats
    {
        public float STR;
        public float AGI;
        public float INT;
        public float VIT;
        public float DEX;
        public float LUK;

        public Dictionary<int, float> RawStats = new Dictionary<int, float>();

        public override string ToString()
            => $"STR:{STR} AGI:{AGI} INT:{INT} VIT:{VIT} DEX:{DEX} LUK:{LUK}";
    }
}
