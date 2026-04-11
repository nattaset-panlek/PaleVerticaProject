using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.SkillTree.Models
{
    #region Node

    [System.Serializable]
    public class SkillTreeNode
    {
        public int    id;
        public string label;
        public string kind;
        public string role;
        public float  x;
        public float  y;
        public bool   isActive;
        public string linkStyle;
        public string shape;
        public int    polySides;
        public string iconPath;
        public List<AttributeBonus> extras;

        public Vector2 Position    => new Vector2(x, y);
        public bool    IsBaseClass => role == "base";
        public bool    HasStats    => extras != null && extras.Count > 0;
    }

    #endregion

    #region Edge

    [System.Serializable]
    public class SkillTreeEdge
    {
        public int    id;
        public int    from;
        public int    to;
        public string style;
    }

    #endregion

    #region Stats

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

    [System.Serializable]
    public class AttributeBonus
    {
        public int   id;
        public float value;
    }

    #endregion

    #region Root Wrapper

    [System.Serializable]
    public class SkillTreeData
    {
        public List<SkillTreeNode> nodes;
        public List<SkillTreeEdge> edges;
    }

    #endregion
}
