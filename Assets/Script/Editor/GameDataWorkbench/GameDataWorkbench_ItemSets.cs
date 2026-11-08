using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data.Items;
using CoreSystem.Data.Skills;

public partial class GameDataWorkbench : EditorWindow
{
    #region [Private Methods]
    private void DrawItemSetEditor()
    {
        DrawEditor(_itemSetData.Sets, _itemSetSelectedIndex, (idx) => _itemSetSelectedIndex = idx, (entry) => {
            if (entry.BonusLevels == null) entry.BonusLevels = new List<SetBonusEntry>();
            var setMembers = _itemData.Items.Where(it => it.SetID == entry.SetID).ToList();

            BeginInspectorColumns();
            if (BeginGroup("Basic Set Info", ThemeAccent))
            {
                entry.SetID = EditorGUILayout.TextField("Set ID (Technical)", entry.SetID);
                entry.SetName = EditorGUILayout.TextField("Display Name", entry.SetName);
            }
            EndGroup();

            if (BeginGroup("Bonus Progression", ThemeAccent, $"{entry.BonusLevels.Count} levels"))
                DrawSetProgression(entry);
            EndGroup();

            if (BeginGroup("Items in this Set", ThemeAccent, $"{setMembers.Count} pieces"))
            {
                if (setMembers.Count == 0) EditorGUILayout.HelpBox("No items are currently assigned to this set.", MessageType.Info);
                foreach (var member in setMembers)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{member.ItemId}", MutedStyle, GUILayout.Width(44));
                    GUILayout.Label(member.Name, GUILayout.ExpandWidth(true));
                    GUILayout.Label(member.EquipmentSlot, MutedStyle, GUILayout.Width(130));
                    if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(44)))
                    {
                        _itemSelectedIndex = _itemData.Items.IndexOf(member);
                        SelectTab(ToolTab.Items);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EndGroup();

            NextInspectorColumn();
            if (BeginGroup("Bonus Configuration", ThemeAccent))
            {
                for (int i = 0; i < entry.BonusLevels.Count; i++)
                {
                    var bonus = entry.BonusLevels[i];
                    EditorGUILayout.BeginVertical(SectionStyle);
                    EditorGUILayout.BeginHorizontal();
                    bonus.RequiredPieces = EditorGUILayout.IntField("Required Pieces", bonus.RequiredPieces);
                    if (GUILayout.Button("Delete Level", GUILayout.Width(100)))
                    {
                        entry.BonusLevels.RemoveAt(i);
                        MarkDirty();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

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
                        bool removed = GUILayout.Button("X", GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                        if (removed)
                        {
                            bonus.Effects.RemoveAt(j);
                            MarkDirty();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                        DrawItemEffectFields(effect);
                        EditorGUILayout.EndVertical();
                    }

                    if (GUILayout.Button("Add Bonus Effect", EditorStyles.miniButton)) bonus.Effects.Add(new EffectEntry { EffectType = "StatBonus" });
                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("Add New Bonus Level")) entry.BonusLevels.Add(new SetBonusEntry { RequiredPieces = 2, Effects = new List<EffectEntry>() });
            }
            EndGroup();
            EndInspectorColumns();
        }, (entry) => new EntryView {
            Id = entry.SetID,
            Name = string.IsNullOrEmpty(entry.SetName) ? "New Set" : entry.SetName,
            Meta = $"{_itemData.Items.Count(it => it.SetID == entry.SetID)} pcs",
            MetaColor = ThemeMuted,
            Accent = ThemeAccent,
            Subtitle = "Item Set",
            Chips = new[] { new EntryChip($"{entry.BonusLevels?.Count ?? 0} bonus levels", ThemeAccent) }
        },
        () => new ItemSetEntry { SetID = "NewSet", SetName = "New Set", BonusLevels = new List<SetBonusEntry>() },
        SetFilter, DrawSetFilters, sorts: GetSetSorts());
    }

    private void DrawSetProgression(ItemSetEntry entry)
    {
        if (entry.BonusLevels == null || entry.BonusLevels.Count == 0)
        {
            EditorGUILayout.HelpBox("No bonus levels defined.", MessageType.None);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Required Pieces", MutedStyle, GUILayout.Width(100));
        GUILayout.Label("Effect", MutedStyle, GUILayout.Width(120));
        GUILayout.Label("Target", MutedStyle);
        GUILayout.Label("Value", MutedStyle, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        foreach (var level in entry.BonusLevels.OrderBy(l => l.RequiredPieces))
        {
            if (level.Effects == null || level.Effects.Count == 0)
            {
                Rect empty = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.DrawRect(empty, ThemeField);
                GUI.Label(new Rect(empty.x + 6, empty.y, empty.width, empty.height), $"{level.RequiredPieces} pieces   no effects", MutedStyle);
                continue;
            }
            foreach (var effect in level.Effects)
            {
                Rect r = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.DrawRect(r, ThemeField);
                string target = effect.EffectType == "StatBonus"
                    ? (_attributeData.Attributes.FirstOrDefault(a => a.Id == effect.AttributeID)?.FullName ?? $"Attr {effect.AttributeID}")
                    : (_skillData.ActiveSkills.FirstOrDefault(s => s.SkillID == effect.SkillID)?.SkillName ?? $"Skill {effect.SkillID}");
                GUI.Label(new Rect(r.x + 6, r.y, 96, r.height), $"{level.RequiredPieces} pieces");
                GUI.Label(new Rect(r.x + 104, r.y, 120, r.height), effect.EffectType);
                GUI.Label(new Rect(r.x + 228, r.y, r.width - 290, r.height), target);
                GUI.Label(new Rect(r.xMax - 56, r.y, 50, r.height), effect.EffectType == "StatBonus" ? "+" + effect.Value : "", EditorStyles.boldLabel);
                GUILayout.Space(2);
            }
        }
    }
    #endregion
}
