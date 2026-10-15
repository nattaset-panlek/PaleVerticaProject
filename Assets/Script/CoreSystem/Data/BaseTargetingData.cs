using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
    [System.Serializable]
    public class BaseTargetingData
    {
        public string Type;
        public float Radius;
        public float CastRange = 5f;
        
        public float Angle = 60f;
        public UnityEngine.Vector2 BoxSize = new UnityEngine.Vector2(2f, 5f);
        public UnityEngine.Vector2 Offset = UnityEngine.Vector2.zero;
        public float LineWidth = 1f;

        public string Alignment;
        public bool IncludeSelf;
    }
}
