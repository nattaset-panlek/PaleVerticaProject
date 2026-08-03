using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Components
{
    [System.Serializable]
    public class Tab
    {
        public Button ActionButtonTabs;
        public GameObject Default;
        public GameObject Active;
        public GameObject Page;
        public UnityEvent OnActive;
    }

    [AddComponentMenu("UI/Components/Tabs Manager")]
    public class TabsManager : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private List<Tab> tabs = new List<Tab>();
        [SerializeField] private int defaultTabIndex = 0;
        [SerializeField] private bool selectDefaultOnStart = true;
        [SerializeField] private UnityEvent<int> onTabChanged;

        private int _currentTabIndex = -1;
        #endregion

        #region [Properties]
        public int CurrentTabIndex => _currentTabIndex;
        public Tab CurrentTab => (_currentTabIndex >= 0 && _currentTabIndex < tabs.Count) ? tabs[_currentTabIndex] : null;
        public IReadOnlyList<Tab> Tabs => tabs;
        public UnityEvent<int> OnTabChanged => onTabChanged;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            InitializeTabs();
        }

        private void Start()
        {
            if (selectDefaultOnStart && tabs.Count > 0)
            {
                SelectTab(defaultTabIndex);
            }
            else
            {
                DeselectAll();
            }
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }
        #endregion

        #region [Public Methods]
        public void SelectTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= tabs.Count) return;
            if (_currentTabIndex == tabIndex) return;

            _currentTabIndex = tabIndex;
            UpdateVisuals();

            tabs[_currentTabIndex].OnActive?.Invoke();
            onTabChanged?.Invoke(_currentTabIndex);
        }

        public void SelectNextTab(bool loop = true)
        {
            if (tabs.Count == 0) return;
            int nextIndex = _currentTabIndex + 1;
            if (nextIndex >= tabs.Count)
            {
                nextIndex = loop ? 0 : tabs.Count - 1;
            }
            SelectTab(nextIndex);
        }

        public void SelectPreviousTab(bool loop = true)
        {
            if (tabs.Count == 0) return;
            int prevIndex = _currentTabIndex - 1;
            if (prevIndex < 0)
            {
                prevIndex = loop ? tabs.Count - 1 : 0;
            }
            SelectTab(prevIndex);
        }

        public void DeselectAll()
        {
            _currentTabIndex = -1;
            UpdateVisuals();
        }

        public Tab GetTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return null;
            return tabs[index];
        }
        #endregion

        #region [Private Methods]
        private void InitializeTabs()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                Tab tab = tabs[i];
                if (tab.ActionButtonTabs != null)
                {
                    tab.ActionButtonTabs.onClick.AddListener(() => SelectTab(index));
                }
            }
        }

        private void RemoveListeners()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                if (tab != null && tab.ActionButtonTabs != null)
                {
                    tab.ActionButtonTabs.onClick.RemoveAllListeners();
                }
            }
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                bool isActive = (i == _currentTabIndex);

                if (tab.Default != null)
                {
                    tab.Default.SetActive(!isActive);
                }
                if (tab.Active != null)
                {
                    tab.Active.SetActive(isActive);
                }
                if (tab.Page != null)
                {
                    tab.Page.SetActive(isActive);
                }
            }
        }
        #endregion
    }
}
