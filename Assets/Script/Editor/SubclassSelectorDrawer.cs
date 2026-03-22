using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CoreSystem.Attributes;

namespace EditorScripts
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            // Get current type name
            string fullTypeName = property.managedReferenceFullTypename;
            string typeName = string.IsNullOrEmpty(fullTypeName) ? "None (Null)" : fullTypeName.Split(' ').Last().Split('.').Last();

            // Dropdown button
            Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, typeName, EditorStyles.layerMaskField))
            {
                ShowTypeMenu(property);
            }

            // Draw children
            EditorGUI.PropertyField(position, property, label, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () => {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            Type targetType = GetTargetType(property);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => targetType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

            foreach (var type in types)
            {
                string name = type.Name;
                menu.AddItem(new GUIContent(name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private Type GetTargetType(SerializedProperty property)
        {
            string[] parts = property.managedReferenceFieldTypename.Split(' ');
            if (parts.Length < 2) return typeof(object);
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }
    }
}
