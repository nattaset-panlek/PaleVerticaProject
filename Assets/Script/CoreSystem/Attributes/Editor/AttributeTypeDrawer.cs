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

            string currentName = property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length
                ? property.enumDisplayNames[property.enumValueIndex]
                : "None";

            if (GUI.Button(position, new GUIContent(currentName), EditorStyles.popup))
            {
                var dropdown = new AttributeTypeDropdown(
                    property.enumNames, 
                    property.enumDisplayNames, 
                    property.enumValueIndex, 
                    (index) =>
                    {
                        property.serializedObject.Update();
                        property.enumValueIndex = index;
                        property.serializedObject.ApplyModifiedProperties();
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
                var item = new AdvancedDropdownItem(displayNames[i])
                {
                    id = i // Store the index corresponding to enumNames array
                };
                root.AddChild(item);
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
