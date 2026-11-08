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
        public int BaseExp;
        public int JobExp;
        public float ExpMultiplier = 1f;
        public int DropTableId;
        public int DialogueId;
        public bool UseDataStats;
        public int Level = 1;
        public float STR = 1f;
        public float AGI = 1f;
        public float VIT = 1f;
        public float INT = 1f;
        public float DEX = 1f;
        public float LUK = 1f;
        public float Defense;
        public float MDEF;
        public float WeaponATK;
        public float WeaponMATK;
        public float MoveSpeed;
        public ElementType Element;
        public SizeType Size = SizeType.Medium;
        public RaceType Race;
        public CoreSystem.Enums.AIBehaviorType Behavior = CoreSystem.Enums.AIBehaviorType.Aggressive;
        public CoreSystem.Enums.CombatStyle Combat;
        public float DetectionRadius = 10f;
        public float AttackRange = 3f;
        public bool CanRoam = true;
        public List<int> SkillIds = new List<int>();
        public string PrefabPath;
    }
}
