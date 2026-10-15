using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
    [System.Serializable]
    public class SupportSkillJsonData
    {
        public int SkillID;
        public string ModifierName;
        public string Description;
        public string IconPath;
    }
}
