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

    public class AttributeTypeDropdown : AdvancedDropdown
    {
        private string[] enumNames;
        private string[] displayNames;
        private Action<int> onItemSelected;

        public AttributeTypeDropdown(string[] names, string[] displayNames, int selectedIndex, Action<int> onItemSelected) 
            : base(new AdvancedDropdownState())
        {
            this.enumNames = names;
            this.displayNames = displayNames;
            this.onItemSelected = onItemSelected;
            
            this.minimumSize = new Vector2(250, 400); // Set a good starting size with search bar
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Attribute Types");

            for (int i = 0; i < enumNames.Length; i++)
            {
                var item = new AdvancedDropdownItem(displayNames[i]);
                root.AddChild(item);
                item.id = i;
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            onItemSelected?.Invoke(item.id);
        }
    }
}
