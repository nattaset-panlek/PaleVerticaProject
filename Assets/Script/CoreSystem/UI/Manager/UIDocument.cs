using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CoreSystem.UI
{
    public sealed class UIDocument
    {
        #region [Variables/Fields]
        private readonly RectTransform _root;
        private readonly UIElementFactory _factory;
        private readonly Dictionary<string, GameObject> _elements = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Region> _regions = new Dictionary<string, Region>();
        private readonly Dictionary<string, UIPageSet> _pageSets = new Dictionary<string, UIPageSet>();
        private readonly Dictionary<string, object> _state = new Dictionary<string, object>();
        private readonly List<string> _staleKeys = new List<string>();
        private Action<UIBuilderContext> _rootBuilder;

        private sealed class Region
        {
            public Transform Container;
            public Action<UIBuilderContext> Builder;
        }
        #endregion

        #region [Properties]
        public RectTransform Root => _root;
        public UIElementFactory Factory => _factory;
        public UITheme Theme => _factory.Theme;
        public event Action OnRebuilt;
        #endregion

        #region [Public Methods]
        public UIDocument(RectTransform root, UIElementRegistrySO registry)
        {
            _root = root;
            _factory = new UIElementFactory(registry);
        }

        public void Build(Action<UIBuilderContext> builder)
        {
            _rootBuilder = builder;
            Refresh();
        }

        public void Refresh()
        {
            if (_root == null) return;

            var scroll = CaptureScroll();
            ClearChildren(_root);
            _elements.Clear();
            _regions.Clear();
            _pageSets.Clear();
            _rootBuilder?.Invoke(new UIBuilderContext(_root, this));
            RestoreScroll(scroll);
            OnRebuilt?.Invoke();
        }

        public bool Refresh(string regionId)
        {
            if (!TryGetRegion(regionId, out Region region)) return false;

            var scroll = CaptureScroll();
            ForgetDescendants(region.Container);
            ClearChildren(region.Container);
            region.Builder?.Invoke(new UIBuilderContext(region.Container, this));
            RestoreScroll(scroll);
            OnRebuilt?.Invoke();
            return true;
        }

        public void Clear()
        {
            _rootBuilder = null;
            if (_root != null) ClearChildren(_root);
            _elements.Clear();
            _regions.Clear();
            _pageSets.Clear();
        }

        public bool HasRegion(string regionId) => TryGetRegion(regionId, out _);

        public UIElementRef Find(string id)
        {
            TryFind(id, out UIElementRef element);
            return element;
        }

        public bool TryFind(string id, out UIElementRef element)
        {
            if (id != null && _elements.TryGetValue(id, out GameObject go) && go != null)
            {
                element = new UIElementRef(go, this);
                return true;
            }

            element = new UIElementRef(null, this);
            return false;
        }

        public T GetState<T>(string key, T fallback = default)
        {
            return key != null && _state.TryGetValue(key, out object value) && value is T typed ? typed : fallback;
        }

        public void SetState<T>(string key, T value)
        {
            if (key != null) _state[key] = value;
        }

        public int GetPage(string pagesId) => GetState(PageKey(pagesId), 0);

        public void ShowPage(string pagesId, int index)
        {
            SetState(PageKey(pagesId), index);
            Refresh(NavId(pagesId));
            Refresh(ContentId(pagesId));
        }

        public void ShowPage(string pagesId, string title)
        {
            if (pagesId == null || !_pageSets.TryGetValue(pagesId, out UIPageSet set)) return;
            int index = set.IndexOf(title);
            if (index >= 0) ShowPage(pagesId, index);
        }

        public bool RefreshPage(string pagesId) => Refresh(ContentId(pagesId));

        public bool TryGetPages(string pagesId, out UIPageSet pages)
        {
            pages = null;
            return pagesId != null && _pageSets.TryGetValue(pagesId, out pages) && pages != null;
        }

        public void RegisterElement(string id, GameObject go)
        {
            if (!string.IsNullOrEmpty(id) && go != null) _elements[id] = go;
        }

        public void RegisterRegion(string id, Transform container, Action<UIBuilderContext> builder)
        {
            if (string.IsNullOrEmpty(id) || container == null) return;
            _regions[id] = new Region { Container = container, Builder = builder };
            _elements[id] = container.gameObject;
        }

        public void RegisterPages(string id, UIPageSet pages)
        {
            if (!string.IsNullOrEmpty(id) && pages != null) _pageSets[id] = pages;
        }

        public static string PageKey(string pagesId) => pagesId + ".page";

        public static string NavId(string pagesId) => pagesId + ".nav";

        public static string ContentId(string pagesId) => pagesId + ".content";
        #endregion

        #region [Private Methods]
        private bool TryGetRegion(string regionId, out Region region)
        {
            region = null;
            return !string.IsNullOrEmpty(regionId)
                && _regions.TryGetValue(regionId, out region)
                && region != null
                && region.Container != null;
        }

        private void ForgetDescendants(Transform container)
        {
            _staleKeys.Clear();
            foreach (var pair in _elements)
            {
                if (pair.Value == null || IsInside(pair.Value.transform, container)) _staleKeys.Add(pair.Key);
            }
            foreach (string key in _staleKeys) _elements.Remove(key);

            _staleKeys.Clear();
            foreach (var pair in _regions)
            {
                if (pair.Value.Container == null || IsInside(pair.Value.Container, container)) _staleKeys.Add(pair.Key);
            }
            foreach (string key in _staleKeys) _regions.Remove(key);
        }

        private static bool IsInside(Transform target, Transform container)
        {
            return target != container && target.IsChildOf(container);
        }

        private Dictionary<string, Vector2> CaptureScroll()
        {
            Dictionary<string, Vector2> positions = null;
            foreach (var pair in _elements)
            {
                if (pair.Value == null) continue;
                var scrollRect = FindScrollRect(pair.Value);
                if (scrollRect == null) continue;
                if (positions == null) positions = new Dictionary<string, Vector2>();
                positions[pair.Key] = scrollRect.normalizedPosition;
            }
            return positions;
        }

        private void RestoreScroll(Dictionary<string, Vector2> positions)
        {
            if (positions == null) return;

            foreach (var pair in positions)
            {
                if (!_elements.TryGetValue(pair.Key, out GameObject go) || go == null) continue;
                var scrollRect = FindScrollRect(go);
                if (scrollRect == null || scrollRect.content == null) continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                scrollRect.normalizedPosition = pair.Value;
            }
        }

        private static ScrollRect FindScrollRect(GameObject go)
        {
            var scrollRect = go.GetComponent<ScrollRect>();
            return scrollRect != null ? scrollRect : go.GetComponentInChildren<ScrollRect>(true);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(child);
                    continue;
                }

                child.SetActive(false);
                child.transform.SetParent(null, false);
                Object.Destroy(child);
            }
        }
        #endregion
    }
}
