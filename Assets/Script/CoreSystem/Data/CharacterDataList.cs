using UnityEngine;
using CoreSystem;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Attributes;
using System.Collections.Generic;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Modifiers;
using CoreSystem.SkillTree;

namespace CoreSystem.Data
{
    [System.Serializable]
    public class CharacterDataList
    {
        public List<CharacterModel> Characters;
    }
}
