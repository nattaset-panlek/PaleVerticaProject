using System.IO;
using System.Linq;
using CoreSystem.Data;
using CoreSystem.Entity;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CharacterDropdownAttribute))]
public class CharacterDropdownDrawer : PropertyDrawer
{
    #region [Variables/Fields]
    private const string JsonPath = "Assets/Resources/Data/Character.json";
    private const string FieldLabel = "Game Data Character";
    private const float WarningHeight = 38f;

    private static CharacterDataList _data;
    private static System.DateTime _loadedAt;
    #endregion

    #region [Unity Methods]
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        return Find(property.intValue) == null && !property.hasMultipleDifferentValues ? height + WarningHeight : height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect field = EditorGUI.PrefixLabel(line, new GUIContent(FieldLabel, "Links this character to its entry in Game Data."));

        CharacterModel current = Find(property.intValue);
        string text = property.hasMultipleDifferentValues ? "-" : current != null ? $"[{current.Id}] {current.Name}" : $"Not in Game Data ({property.intValue})";
        if (EditorGUI.DropdownButton(field, new GUIContent(text), FocusType.Keyboard)) ShowMenu(field, property);

        if (current != null || property.hasMultipleDifferentValues) return;
        Rect warning = new Rect(position.x, line.yMax + 2f, position.width, WarningHeight - 4f);
        EditorGUI.HelpBox(warning, "This character has no Game Data entry. Quests, dialogue and drops will not work.", MessageType.Error);
    }
    #endregion

    #region [Private Methods]
    private static void ShowMenu(Rect field, SerializedProperty property)
    {
        var target = property.Copy();
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("None"), property.intValue == 0, () => Assign(target, 0));
        foreach (CharacterModel character in Load().Characters.OrderBy(c => c.Id))
        {
            int id = character.Id;
            menu.AddItem(new GUIContent($"[{id}] {character.Name}"), id == property.intValue, () => Assign(target, id));
        }
        menu.DropDown(field);
    }

    private static void Assign(SerializedProperty property, int id)
    {
        property.serializedObject.Update();
        property.intValue = id;
        property.serializedObject.ApplyModifiedProperties();
    }

    private static CharacterModel Find(int id)
    {
        return Load().Characters.Find(c => c.Id == id);
    }

    private static CharacterDataList Load()
    {
        System.DateTime written = File.GetLastWriteTimeUtc(JsonPath);
        if (_data != null && written == _loadedAt) return _data;

        _loadedAt = written;
        _data = File.Exists(JsonPath) ? JsonUtility.FromJson<CharacterDataList>(File.ReadAllText(JsonPath)) : new CharacterDataList();
        if (_data.Characters == null) _data.Characters = new System.Collections.Generic.List<CharacterModel>();
        return _data;
    }
    #endregion
}
