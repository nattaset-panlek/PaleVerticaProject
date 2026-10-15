using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills;

namespace CoreSystem.Data.Skills
{
    [System.Serializable]
    public class SkillDataList
    {
        public List<ActiveSkillJsonData> ActiveSkills;
        public List<SupportSkillJsonData> SupportSkills;
    }
}
