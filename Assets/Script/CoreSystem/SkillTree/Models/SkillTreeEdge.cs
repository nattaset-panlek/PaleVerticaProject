using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.SkillTree.Models
{
    [System.Serializable]
    public class SkillTreeEdge
    {
        public int    id;
        public int    from;
        public int    to;
        public string style;
    }
}
