using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoreSystem.UI.Editor
{
    [CustomEditor(typeof(UIElementRegistrySO))]
    public class UIElementRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty _entriesProperty;

        private void OnEnable()
        {
            _entriesProperty = serializedObject.FindProperty("_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("UI Element Registry", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < _entriesProperty.arraySize; i++)
            {
                SerializedProperty entryProp = _entriesProperty.GetArrayElementAtIndex(i);
                DrawEntry(entryProp, i);
            }

            if (GUILayout.Button("Add New Element"))
            {
                _entriesProperty.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntry(SerializedProperty entryProp, int index)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Element {index}", EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                _entriesProperty.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            SerializedProperty styleNameProp = entryProp.FindPropertyRelative("StyleName");
            SerializedProperty typeProp = entryProp.FindPropertyRelative("Type");
            SerializedProperty prefabProp = entryProp.FindPropertyRelative("Prefab");
            SerializedProperty pathProp = entryProp.FindPropertyRelative("TargetComponentPath");

            EditorGUILayout.PropertyField(styleNameProp);
            EditorGUILayout.PropertyField(typeProp);
            EditorGUILayout.PropertyField(prefabProp);

            // Draw Dropdown for component selection if prefab is assigned
            GameObject prefab = prefabProp.objectReferenceValue as GameObject;
            if (prefab != null)
            {
                UIElementType type = (UIElementType)typeProp.intValue;
                DrawComponentDropdown(prefab, type, pathProp);
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a Prefab to select the Target Component.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawComponentDropdown(GameObject prefab, UIElementType type, SerializedProperty pathProp)
        {
            Type targetComponentType = GetTargetComponentType(type);
            
            if (targetComponentType == null)
            {
                EditorGUILayout.HelpBox("No specific component required for this type.", MessageType.Info);
                return;
            }

            var components = prefab.GetComponentsInChildren(targetComponentType, true);
            if (components.Length == 0)
            {
                EditorGUILayout.HelpBox($"No {targetComponentType.Name} found in the prefab or its children!", MessageType.Error);
                return;
            }

            List<string> options = new List<string>();
            List<string> paths = new List<string>();

            int selectedIndex = 0;

            for (int i = 0; i < components.Length; i++)
            {
                Transform t = components[i].transform;
                string path = GetGameObjectPath(t, prefab.transform);
                
                paths.Add(path);
                options.Add(string.IsNullOrEmpty(path) ? "Root" : path);

                if (path == pathProp.stringValue)
                {
                    selectedIndex = i;
                }
            }

            int newSelectedIndex = EditorGUILayout.Popup("Target Component", selectedIndex, options.ToArray());
            if (newSelectedIndex >= 0 && newSelectedIndex < paths.Count)
            {
                pathProp.stringValue = paths[newSelectedIndex];
            }
        }

        private Type GetTargetComponentType(UIElementType type)
        {
            switch (type)
            {
                case UIElementType.Text: return typeof(TMP_Text);
                case UIElementType.Button: return typeof(Button);
                case UIElementType.Image: return typeof(Image);
                case UIElementType.InputField: return typeof(TMP_InputField);
                case UIElementType.Dropdown: return typeof(TMP_Dropdown);
                case UIElementType.Toggle: return typeof(Toggle);
                case UIElementType.Slider: return typeof(Slider);
                case UIElementType.ScrollRect: return typeof(UnityEngine.UI.ScrollRect);
                case UIElementType.Scrollbar: return typeof(Scrollbar);
                case UIElementType.RawImage: return typeof(RawImage);
                case UIElementType.ProgressBar: return typeof(Slider);
                case UIElementType.Divider: return typeof(Image);
                case UIElementType.Custom: return typeof(UICustomElement);
                case UIElementType.Mask: return typeof(RectMask2D);
                case UIElementType.Container: return null;
                default: return null;
            }
        }

        private string GetGameObjectPath(Transform transform, Transform root)
        {
            if (transform == root) return "";
            
            string path = transform.name;
            while (transform.parent != null && transform.parent != root)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}
