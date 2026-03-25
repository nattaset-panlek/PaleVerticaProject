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
using CoreSystem.Data;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private void DrawCharacterEditor()
    {
        DrawEditor(_characterData.Characters, _characterSelectedIndex, (idx) => _characterSelectedIndex = idx, (entry) => {
            entry.Id = EditorGUILayout.IntField("ID", entry.Id);
            entry.Name = EditorGUILayout.TextField("Name", entry.Name);
            entry.BaseHP = EditorGUILayout.IntField("Base HP", entry.BaseHP);
            entry.BaseMP = EditorGUILayout.IntField("Base MP", entry.BaseMP);
        }, (entry) => $"[{entry.Id}] {entry.Name}", 
        () => new CharacterModel { Id = _characterData.Characters.Count > 0 ? _characterData.Characters.Max(x => x.Id) + 1 : 1 },
        null);
    }
}
