using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System;

namespace CoreSystem.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(AttributeType))]
    public class AttributeTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var values = (AttributeType[])System.Enum.GetValues(typeof(AttributeType));
            int currentIndex = System.Array.IndexOf(values, (AttributeType)property.intValue);
            string currentName = currentIndex >= 0 && currentIndex < property.enumDisplayNames.Length
                ? property.enumDisplayNames[currentIndex]
                : "None";

            if (GUI.Button(position, new GUIContent(currentName), EditorStyles.popup))
            {
                var serializedObject = property.serializedObject;
                string propertyPath = property.propertyPath;
                var dropdown = new AttributeTypeDropdown(
                    property.enumNames, 
                    property.enumDisplayNames, 
                    currentIndex, 
                    (index) =>
                    {
                        serializedObject.Update();
                        var targetProperty = serializedObject.FindProperty(propertyPath);
                        if (targetProperty != null && index >= 0 && index < values.Length)
                        {
                            targetProperty.intValue = (int)values[index];
                        }
                        serializedObject.ApplyModifiedProperties();
                    });
                dropdown.Show(position);
            }
        }
    }
}
