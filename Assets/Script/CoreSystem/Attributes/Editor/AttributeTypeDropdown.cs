using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System;

namespace CoreSystem.Attributes.Editor
{
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
