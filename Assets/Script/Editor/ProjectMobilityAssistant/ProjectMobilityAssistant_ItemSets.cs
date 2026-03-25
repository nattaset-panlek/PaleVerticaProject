using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data.Items;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private void DrawItemSetEditor()
    {
        DrawEditor(_itemSetData.Sets, _itemSetSelectedIndex, (idx) => _itemSetSelectedIndex = idx, (entry) => {
            EditorGUILayout.BeginVertical(SectionStyle);
            entry.SetID = EditorGUILayout.TextField("Set ID (Technical)", entry.SetID);
            entry.SetName = EditorGUILayout.TextField("Display Name", entry.SetName);
            EditorGUILayout.EndVertical();
            
            // --- Visualization ---
            DrawSetProgression(entry);

            DrawSectionHeader("\uD83C\uDF81  Bonus Configuration");
            if (entry.BonusLevels == null) entry.BonusLevels = new List<SetBonusEntry>();
            
            for (int i = 0; i < entry.BonusLevels.Count; i++)
            {
                var bonus = entry.BonusLevels[i];
                EditorGUILayout.BeginVertical(SectionStyle);
                EditorGUILayout.BeginHorizontal();
                bonus.RequiredPieces = EditorGUILayout.IntField("Required Pieces", bonus.RequiredPieces);
                if (GUILayout.Button("Delete Level", GUILayout.Width(100))) { entry.BonusLevels.RemoveAt(i); break; }
                EditorGUILayout.EndHorizontal();
                
                // Effects for this level
                if (bonus.Effects == null) bonus.Effects = new List<EffectEntry>();
                
                for (int j = 0; j < bonus.Effects.Count; j++)
                {
                    var effect = bonus.Effects[j];
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();
                    
                    string[] effectTypes = { "StatBonus", "SkillUnlockEffect", "AutoCastEffect" };
                    int typeIdx = System.Array.IndexOf(effectTypes, effect.EffectType);
                    if (typeIdx == -1) typeIdx = 0;
                    effect.EffectType = effectTypes[EditorGUILayout.Popup(typeIdx, effectTypes, GUILayout.Width(180))];
                    
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("X", GUILayout.Width(20))) { bonus.Effects.RemoveAt(j); break; }
                    EditorGUILayout.EndHorizontal();
                    
                    DrawItemEffectFields(effect);
                    EditorGUILayout.EndVertical();
                }
                
                if (GUILayout.Button("\uFF0B Add Bonus Effect")) bonus.Effects.Add(new EffectEntry { EffectType = "StatBonus" });
                
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("\uFF0B Add New Bonus Level")) entry.BonusLevels.Add(new SetBonusEntry { RequiredPieces = 2, Effects = new List<EffectEntry>() });
            
            EditorGUILayout.Space(20);
            DrawSectionHeader("\uD83D\uDD0D  Items in this Set");
            var setMembers = _itemData.Items.Where(it => it.SetID == entry.SetID).ToList();
            if (setMembers.Count == 0) EditorGUILayout.HelpBox("No items are currently assigned to this set.", MessageType.Info);
            foreach(var member in setMembers)
            {
                EditorGUILayout.LabelField($"\u2022 [{member.ItemId}] {member.Name} ({member.EquipmentSlot})");
            }
            
        }, (entry) => string.IsNullOrEmpty(entry.SetName) ? $"[New Set]" : entry.SetName,
        () => new ItemSetEntry { SetID = "NewSet", SetName = "New Set", BonusLevels = new List<SetBonusEntry>() });
    }

    private void DrawSetProgression(ItemSetEntry entry)
    {
        EditorGUILayout.BeginVertical(SectionStyle);
        GUILayout.Label("Bonus Progression", EditorStyles.boldLabel);
        
        if (entry.BonusLevels == null || entry.BonusLevels.Count == 0)
        {
            EditorGUILayout.HelpBox("No bonus levels defined.", MessageType.None);
        }
        else
        {
            var sortedLevels = entry.BonusLevels.OrderBy(l => l.RequiredPieces).ToList();
            EditorGUILayout.BeginHorizontal();
            foreach (var level in sortedLevels)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(80));
                GUILayout.Label($"{level.RequiredPieces} Pcs", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });
                GUI.color = new Color(0.8f, 1f, 0.8f);
                GUILayout.Label($"\u2728 {level.Effects?.Count ?? 0} effects", EditorStyles.miniLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndVertical();
                if (level != sortedLevels.Last()) GUILayout.Label("\u2192", GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}
