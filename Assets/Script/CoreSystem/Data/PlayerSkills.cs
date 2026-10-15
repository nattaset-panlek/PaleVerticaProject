using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using System;
using System.Collections.Generic;



namespace CoreSystem.Data
{
    public class PlayerSkills 
    {
        private SkillCaster _skillCaster;

        public void Bind(SkillCaster skillCaster)
        {
            _skillCaster = skillCaster;
        }

        // Add skill-related manager methods here
        // e.g., Equipping skills to slots, removing skills, verifying cooldowns
    }
}
