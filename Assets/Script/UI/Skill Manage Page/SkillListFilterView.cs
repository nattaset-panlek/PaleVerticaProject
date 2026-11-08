using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillListFilterView : MonoBehaviour, IPointerClickHandler
    {
        #region [Variables/Fields]
        [SerializeField] private SkillManageWindow _window;
        [SerializeField] private string _currentMark = "• ";

        private readonly List<ElementType> _elements = new List<ElementType>();
        private SkillListFilter _filter = SkillListFilter.All;
        private ElementType _element = ElementType.Neutral;

        public event Action OnFilterChanged;
        #endregion

        #region [Properties]
        public SkillListFilter Filter => _filter;
        public ElementType Element => _element;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            _window = GetComponentInParent<SkillManageWindow>(true);
        }

        private void Awake()
        {
            if (_window == null) _window = GetComponentInParent<SkillManageWindow>(true);
        }
        #endregion

        #region [Public Methods]
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null) OpenMenu(eventData.position);
        }

        public void OpenMenu(Vector2 screenPosition)
        {
            if (ContextMenuManager.Instance == null) return;
            ContextMenuManager.Instance.Show(BuildItems(), screenPosition);
        }

        public List<ContextMenuItem> BuildItems()
        {
            var items = new List<ContextMenuItem>
            {
                Item("All Skills", SkillListFilter.All, ElementType.Neutral),
                Item("Equipped", SkillListFilter.Equipped, ElementType.Neutral),
                Item("Active", SkillListFilter.Active, ElementType.Neutral),
                Item("Toggle", SkillListFilter.Toggle, ElementType.Neutral)
            };

            CollectElements();
            for (int i = 0; i < _elements.Count; i++)
            {
                items.Add(Item(_elements[i].ToString(), SkillListFilter.Element, _elements[i]));
            }
            return items;
        }

        public void SetFilter(SkillListFilter filter, ElementType element = ElementType.Neutral)
        {
            if (_filter == filter && _element == element) return;
            _filter = filter;
            _element = element;
            OnFilterChanged?.Invoke();
        }

        public bool Matches(SkillInstance instance, Character player)
        {
            if (instance == null || instance.SkillData == null) return false;

            switch (_filter)
            {
                case SkillListFilter.Equipped:
                    return player != null && player.skillCaster != null && player.skillCaster.EquippedSkills.Contains(instance);
                case SkillListFilter.Active:
                    return !instance.SkillData.IsToggle;
                case SkillListFilter.Toggle:
                    return instance.SkillData.IsToggle;
                case SkillListFilter.Element:
                    return instance.SkillData.SkillElement == _element;
                default:
                    return true;
            }
        }
        #endregion

        #region [Private Methods]
        private ContextMenuItem Item(string label, SkillListFilter filter, ElementType element)
        {
            bool current = _filter == filter && (filter != SkillListFilter.Element || _element == element);
            return new ContextMenuItem(current ? _currentMark + label : label, () => SetFilter(filter, element));
        }

        private void CollectElements()
        {
            _elements.Clear();
            var player = _window != null ? _window.TargetPlayer : null;
            if (player == null || player.skillCaster == null) return;

            var granted = player.skillCaster.GrantedSkills;
            for (int i = 0; i < granted.Count; i++)
            {
                var data = granted[i] != null ? granted[i].SkillData : null;
                if (data == null || data.SkillElement == ElementType.Neutral || _elements.Contains(data.SkillElement)) continue;
                _elements.Add(data.SkillElement);
            }
            _elements.Sort();
        }
        #endregion
    }
}
