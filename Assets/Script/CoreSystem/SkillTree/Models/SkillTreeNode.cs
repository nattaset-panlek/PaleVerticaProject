using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.SkillTree.Models
{
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
}
