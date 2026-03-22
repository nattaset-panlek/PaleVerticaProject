using System.Collections.Generic;
using CoreSystem;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Attributes.Editor
{
    [CustomEditor(typeof(Entity.Character), true)]
    public class AttributeSystemEditor : UnityEditor.Editor
    {
        private bool showBasePrimary = true;
        private bool showOffensivePhys = false;
        private bool showOffensiveMag = false;
        private bool showDefensive = false;
        private bool showCritical = false;
        private bool showElementalDmg = false;
        private bool showElementalRes = false;
        private bool showSize = false;
        private bool showRace = false;
        private bool showUtility = false;

        private static readonly AttributeType[] basePrimaryTypes = {
            AttributeType.HP, AttributeType.MaxHP, AttributeType.MP, AttributeType.MaxMP,
            AttributeType.STR, AttributeType.AGI, AttributeType.VIT, AttributeType.INT, AttributeType.DEX, AttributeType.LUK
        };
        private static readonly AttributeType[] offensivePhysTypes = {
            AttributeType.ATK, AttributeType.StatusDMG, AttributeType.WeaponATK, AttributeType.ASPD, AttributeType.Hit, AttributeType.CriticalRate
        };
        private static readonly AttributeType[] offensiveMagTypes = {
            AttributeType.MATK, AttributeType.CastTime
        };
        private static readonly AttributeType[] defensiveTypes = {
            AttributeType.Defense, AttributeType.StatusDEF, AttributeType.StatusMDEF, AttributeType.Flee, AttributeType.PerfectDodge
        };
        private static readonly AttributeType[] criticalTypes = {
            AttributeType.CriticalDamage, AttributeType.CritResistance
        };
        private static readonly AttributeType[] elementalDmgTypes = {
            AttributeType.NeutralDamage, AttributeType.WaterDamage, AttributeType.EarthDamage, AttributeType.FireDamage,
            AttributeType.ElectricDamage, AttributeType.GhostDamage, AttributeType.HolyDamage
        };
        private static readonly AttributeType[] elementalResTypes = {
            AttributeType.NeutralResist, AttributeType.WaterResist, AttributeType.EarthResist, AttributeType.FireResist,
            AttributeType.ElectricResist, AttributeType.GhostResist, AttributeType.HolyResist
        };
        private static readonly AttributeType[] sizeTypes = {
            AttributeType.SmallDamage, AttributeType.MediumDamage, AttributeType.LargeDamage,
            AttributeType.SmallResist, AttributeType.MediumResist, AttributeType.LargeResist
        };
        private static readonly AttributeType[] raceTypes = {
            AttributeType.FormlessDamage, AttributeType.UndeadDamage, AttributeType.BruteDamage, AttributeType.PlantDamage, AttributeType.InsectDamage,
            AttributeType.FishDamage, AttributeType.DemonDamage, AttributeType.DemiHumanDamage, AttributeType.AngelDamage, AttributeType.DragonDamage,
            AttributeType.FormlessResist, AttributeType.UndeadResist, AttributeType.BruteResist, AttributeType.PlantResist, AttributeType.InsectResist,
            AttributeType.FishResist, AttributeType.DemonResist, AttributeType.DemiHumanResist, AttributeType.AngelResist, AttributeType.DragonResist
        };
        private static readonly AttributeType[] utilityTypes = {
            AttributeType.MoveSpeed, AttributeType.WeightLimit, AttributeType.HPRecovery, AttributeType.MPRecovery, AttributeType.HealingItems
        };

        public override void OnInspectorGUI()
        {
            // Draw default inspector for initial attributes array
            DrawDefaultInspector();

            Entity.Character character = (Entity.Character)target;
            AttributeSystem system = character.attributeSystem;

            if (system == null) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Attributes (Read-Only)", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Real-time attributes are only visible during Play Mode.", MessageType.Info);
                return;
            }

            if (system.ActiveAttributes == null || system.ActiveAttributes.Count == 0)
            {
                EditorGUILayout.LabelField("No attributes initialized yet.");
                return;
            }

            EditorGUILayout.BeginVertical("box");

            DrawGroup("Base & Primary Stats", ref showBasePrimary, basePrimaryTypes, system);
            DrawGroup("Offensive (Physical)", ref showOffensivePhys, offensivePhysTypes, system);
            DrawGroup("Offensive (Magical)", ref showOffensiveMag, offensiveMagTypes, system);
            DrawGroup("Defensive", ref showDefensive, defensiveTypes, system);
            DrawGroup("Critical Modifiers", ref showCritical, criticalTypes, system);
            DrawGroup("Elemental Damage", ref showElementalDmg, elementalDmgTypes, system);
            DrawGroup("Elemental Resistances", ref showElementalRes, elementalResTypes, system);
            DrawGroup("Size Modifiers", ref showSize, sizeTypes, system);
            DrawGroup("Race Modifiers", ref showRace, raceTypes, system);
            DrawGroup("Utility & Recovery", ref showUtility, utilityTypes, system);

            EditorGUILayout.EndVertical();

            // Repaint continuously to show real-time changes
            Repaint();
        }

        private void DrawGroup(string title, ref bool foldout, AttributeType[] types, AttributeSystem system)
        {
            GUI.enabled = true; // Foldout should be clickable
            foldout = EditorGUILayout.Foldout(foldout, title, true);
            
            if (foldout)
            {
                GUI.enabled = false; // Make values read-only
                EditorGUI.indentLevel++;
                foreach (var type in types)
                {
                    if (system.ActiveAttributes.TryGetValue(type, out var attr))
                    {
                        string label = type.ToString();
                        string val = $"{attr.BaseValue:F1} \u2192 {attr.CurrentValue:F1}";

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(label, GUILayout.Width(180));
                        EditorGUILayout.LabelField(val);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;
            }
            GUI.enabled = true; // Restoring GUI enabled for the next foldout
        }
    }
}
