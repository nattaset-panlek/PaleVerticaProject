using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills
{
    [System.Serializable]
    public class SkillChainOverride
    {
        [Tooltip("Name of the Buff required to trigger this combo/mutation")]
        public string RequiredBuffCode;
        
        [Tooltip("The skill that replaces this one")]
        public ActiveSkill ReplacementSkill;
        
        [Tooltip("If true, casting this skill consumes the combo buff immediately")]
        public bool ConsumeBuffOnCast = true;
        
        [Tooltip("Color of the hint overlay shown on the UI (optional)")]
        public Color HintColor = Color.white;
    }
}
