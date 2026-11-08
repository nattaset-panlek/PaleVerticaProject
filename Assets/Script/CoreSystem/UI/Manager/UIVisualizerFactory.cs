using System;
using System.Collections.Generic;
using GlobalVisualizer;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public static class UIVisualizerFactory
    {
        #region [Variables/Fields]
        public const string ItemRootName = "[Visual Item]";
        #endregion

        #region [Public Methods]
        public static UIElementRef Create<T>(UIBuilderContext ui, IReadOnlyList<T> items, VisualLayoutSettings layout, Action<UIBuilderContext, T, int> buildItem, float height, VisualSizeMode sizeMode = VisualSizeMode.Fixed)
        {
            bool horizontal = layout.Kind == VisualLayoutKind.Horizontal || (layout.Kind == VisualLayoutKind.Grid && layout.GridConstraint == VisualGridConstraint.FixedRows);
            UIElementRef scroll = ui.ScrollView(UIBuilderContext.DefaultScrollStyle, null, horizontal, !horizontal, ScrollRect.MovementType.Clamped);
            if (height > 0f) scroll.Height(height);
            else scroll.Height(60f).FillHeight();

            ScrollRect scrollRect = scroll.GameObject.GetComponent<ScrollRect>();
            if (scrollRect == null) scrollRect = scroll.GameObject.GetComponentInChildren<ScrollRect>(true);
            if (scrollRect == null) return scroll;

            DisableLayoutDrivers(scrollRect.content);
            GlobalVisualizerView view = scrollRect.gameObject.AddComponent<GlobalVisualizerView>();
            view.FocusOnClick = false;
            view.SizeMode = sizeMode;
            view.SetLayout(layout);
            view.ItemPrefab = CreateItemRoot(scrollRect.transform);
            UIDocument document = ui.Document;
            view.SetItems(items, (root, item, index) => BuildItem(root, document, buildItem, item, index));
            return scroll;
        }

        public static GlobalVisualizerView Find(UIElementRef element)
        {
            if (element.GameObject == null) return null;
            return element.GameObject.GetComponentInChildren<GlobalVisualizerView>(true);
        }
        #endregion

        #region [Private Methods]
        private static RectTransform CreateItemRoot(Transform parent)
        {
            var go = new GameObject(ItemRootName, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            VerticalLayoutGroup group = go.GetComponent<VerticalLayoutGroup>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = true;
            go.SetActive(false);
            return rect;
        }

        private static void BuildItem<T>(GameObject root, UIDocument document, Action<UIBuilderContext, T, int> buildItem, T item, int index)
        {
            Transform transform = root.transform;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying) UnityEngine.Object.Destroy(child);
                else UnityEngine.Object.DestroyImmediate(child);
            }
            buildItem?.Invoke(new UIBuilderContext(transform, document), item, index);
        }

        private static void DisableLayoutDrivers(RectTransform content)
        {
            if (content == null) return;
            LayoutGroup group = content.GetComponent<LayoutGroup>();
            if (group != null) group.enabled = false;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;
        }
        #endregion
    }
}
