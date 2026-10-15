using System.Collections.Generic;
using UnityEngine;
using CoreSystem;

namespace CoreSystem.Attributes
{
    [System.Serializable]
    public class AttributeEntry
    {
        public int    Id;
        public string Icon;
        public string Name;
        public string FullName;
        public string Description;
        public bool   IsPercent;
        public string Mapping;
    }
}
