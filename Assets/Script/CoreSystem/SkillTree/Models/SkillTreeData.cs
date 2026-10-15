using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.SkillTree.Models
{
    [System.Serializable]
    public class SkillTreeData
    {
        public List<SkillTreeNode> nodes;
        public List<SkillTreeEdge> edges;
    }
}
