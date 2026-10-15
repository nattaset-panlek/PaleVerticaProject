using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
    [System.Serializable]
    public class SkillChainOverrideJsonData
    {
        public string RequiredBuffCode;
        public int ReplacementSkillID;
        public bool ConsumeBuffOnCast;
        public string HintColorHex;
    }
}
