using System;
using UnityEngine;

namespace CoreSystem.UI.SkillManagePage
{
    [Serializable]
    public struct ElementTagColor
    {
        public ElementType Element;
        public Color Color;

        public ElementTagColor(ElementType element, Color color)
        {
            Element = element;
            Color = color;
        }
    }

    public enum SkillListFilter
    {
        All,
        Equipped,
        Active,
        Toggle,
        Element
    }
}
