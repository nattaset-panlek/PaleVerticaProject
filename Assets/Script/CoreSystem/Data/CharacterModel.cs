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
    public class CharacterModel
    {
        public int Id;
        public string Name;
        public int Faction;
        public int BaseHP;
        public int BaseMP;
        public string PrefabPath;
    }
}
