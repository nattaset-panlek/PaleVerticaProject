using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Data;

[CustomPropertyDrawer(typeof(FactionDropdownAttribute))]
public class FactionDropdownDrawer : PropertyDrawer
{
    private FactionList _factionList;
    private GUIContent[] _options;
    private int[] _optionValues;

    private void LoadFactions()
    {
        if (_factionList != null) return;

        TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Data/Faction.json");
        if (json != null)
        {
            _factionList = JsonUtility.FromJson<FactionList>(json.text);
        }

        if (_factionList != null && _factionList.Factions != null)
        {
            List<GUIContent> opts = new List<GUIContent>();
            List<int> vals = new List<int>();

            opts.Add(new GUIContent("None"));
            vals.Add(0);

            foreach (var f in _factionList.Factions)
            {
                opts.Add(new GUIContent($"[{f.Id}] {f.Name}"));
                vals.Add(f.Id);
            }

            _options = opts.ToArray();
            _optionValues = vals.ToArray();
        }
        else
        {
            _options = new GUIContent[] { new GUIContent("None") };
            _optionValues = new int[] { 0 };
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        LoadFactions();

        EditorGUI.BeginProperty(position, label, property);
        
        int currentValue = property.intValue;
        int selectedIndex = 0;

        for (int i = 0; i < _optionValues.Length; i++)
        {
            if (_optionValues[i] == currentValue)
            {
                selectedIndex = i;
                break;
            }
        }

        selectedIndex = EditorGUI.Popup(position, label, selectedIndex, _options);
        property.intValue = _optionValues[selectedIndex];

        EditorGUI.EndProperty();
    }
}
