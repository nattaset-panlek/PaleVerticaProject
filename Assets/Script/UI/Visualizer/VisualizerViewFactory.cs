using GlobalVisualizer;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Visualizer
{
    public static class VisualizerViewFactory
    {
        #region [Public Methods]
        public static GlobalVisualizerView AttachTo(Transform content, RectTransform itemPrefab)
        {
            if (content == null || itemPrefab == null) return null;
            ScrollRect scroll = content.GetComponentInParent<ScrollRect>(true);
            if (scroll == null)
            {
                Debug.LogWarning($"[VisualizerViewFactory] '{content.name}' is not inside a ScrollRect, so it cannot be virtualized.", content);
                return null;
            }

            GlobalVisualizerView view = scroll.GetComponent<GlobalVisualizerView>();
            if (view != null) return view;

            if (scroll.content == null) scroll.content = (RectTransform)content;
            VisualLayoutSettings settings = FromLayoutGroup(scroll.content.GetComponent<LayoutGroup>(), itemPrefab);
            DisableLayoutDrivers(scroll.content);
            ClearChildren(scroll.content);

            view = scroll.gameObject.AddComponent<GlobalVisualizerView>();
            view.FocusOnClick = false;
            view.SetLayout(settings);
            view.ItemPrefab = itemPrefab;
            return view;
        }

        public static VisualLayoutSettings FromLayoutGroup(LayoutGroup group, RectTransform itemPrefab)
        {
            Vector2 prefabSize = itemPrefab != null ? itemPrefab.rect.size : Vector2.zero;
            VisualLayoutSettings settings;
            switch (group)
            {
                case GridLayoutGroup grid:
                    settings = VisualLayoutSettings.Grid(grid.cellSize, 0f, ToConstraint(grid.constraint), grid.constraintCount);
                    settings.Spacing = grid.spacing;
                    break;
                case HorizontalLayoutGroup horizontal:
                    settings = VisualLayoutSettings.Horizontal(0f, horizontal.spacing);
                    if (!(horizontal.childControlHeight && horizontal.childForceExpandHeight))
                    {
                        settings.ItemSize = new Vector2(0f, prefabSize.y);
                        settings.CrossAlignment = ToAlignment((int)horizontal.childAlignment / 3);
                    }
                    break;
                case VerticalLayoutGroup vertical:
                    settings = VisualLayoutSettings.Vertical(0f, vertical.spacing);
                    if (!(vertical.childControlWidth && vertical.childForceExpandWidth))
                    {
                        settings.ItemSize = new Vector2(prefabSize.x, 0f);
                        settings.CrossAlignment = ToAlignment((int)vertical.childAlignment % 3);
                    }
                    break;
                default:
                    settings = VisualLayoutSettings.Vertical(0f, 0f);
                    break;
            }
            if (group != null)
            {
                RectOffset padding = group.padding;
                settings.Padding = new VisualPadding(padding.left, padding.right, padding.top, padding.bottom);
            }
            return settings;
        }
        #endregion

        #region [Private Methods]
        private static VisualGridConstraint ToConstraint(GridLayoutGroup.Constraint constraint)
        {
            switch (constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount: return VisualGridConstraint.FixedColumns;
                case GridLayoutGroup.Constraint.FixedRowCount: return VisualGridConstraint.FixedRows;
                default: return VisualGridConstraint.Fit;
            }
        }

        private static VisualAlignment ToAlignment(int slot)
        {
            if (slot == 1) return VisualAlignment.Center;
            return slot == 2 ? VisualAlignment.End : VisualAlignment.Start;
        }

        private static void DisableLayoutDrivers(RectTransform content)
        {
            LayoutGroup group = content.GetComponent<LayoutGroup>();
            if (group != null) group.enabled = false;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;
        }

        private static void ClearChildren(Transform content)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject child = content.GetChild(i).gameObject;
                if (Application.isPlaying) Object.Destroy(child);
                else Object.DestroyImmediate(child);
            }
        }
        #endregion
    }
}
