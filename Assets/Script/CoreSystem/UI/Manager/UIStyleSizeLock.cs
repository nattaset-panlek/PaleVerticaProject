using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class UIStyleSizeLock : MonoBehaviour, ILayoutElement
    {
        #region [Variables/Fields]
        public const int Priority = 100;

        [SerializeField] private Vector2 _minSize;
        [SerializeField] private Vector2 _fixedSize;
        #endregion

        #region [Properties]
        public Vector2 MinSize => _minSize;
        public Vector2 FixedSize => _fixedSize;

        public Vector2 RequiredSize => new Vector2(
            _fixedSize.x > 0f ? _fixedSize.x : Mathf.Max(0f, _minSize.x),
            _fixedSize.y > 0f ? _fixedSize.y : Mathf.Max(0f, _minSize.y));

        public float minWidth => RequiredSize.x > 0f ? RequiredSize.x : -1f;
        public float minHeight => RequiredSize.y > 0f ? RequiredSize.y : -1f;
        public float preferredWidth => _fixedSize.x > 0f ? _fixedSize.x : -1f;
        public float preferredHeight => _fixedSize.y > 0f ? _fixedSize.y : -1f;
        public float flexibleWidth => _fixedSize.x > 0f ? 0f : -1f;
        public float flexibleHeight => _fixedSize.y > 0f ? 0f : -1f;
        public int layoutPriority => Priority;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            MarkDirty();
        }

        private void OnDisable()
        {
            MarkDirty();
        }
        #endregion

        #region [Public Methods]
        public static UIStyleSizeLock Attach(GameObject go, Vector2 minSize, Vector2 fixedSize)
        {
            if (go == null || (minSize.x <= 0f && minSize.y <= 0f && fixedSize.x <= 0f && fixedSize.y <= 0f)) return null;
            var sizeLock = go.GetComponent<UIStyleSizeLock>();
            if (sizeLock == null) sizeLock = go.AddComponent<UIStyleSizeLock>();
            sizeLock._minSize = minSize;
            sizeLock._fixedSize = fixedSize;
            sizeLock.Enforce();
            sizeLock.MarkDirty();
            return sizeLock;
        }

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }

        public void Enforce()
        {
            var le = GetComponent<LayoutElement>();
            if (le == null) le = gameObject.AddComponent<LayoutElement>();
            EnforceAxis(le, true, _minSize.x, _fixedSize.x);
            EnforceAxis(le, false, _minSize.y, _fixedSize.y);

            var rect = transform as RectTransform;
            if (rect == null) return;
            Vector2 required = RequiredSize;
            Vector2 size = rect.sizeDelta;
            if (required.x > 0f && size.x < required.x && Mathf.Approximately(rect.anchorMin.x, rect.anchorMax.x)) size.x = required.x;
            if (required.y > 0f && size.y < required.y && Mathf.Approximately(rect.anchorMin.y, rect.anchorMax.y)) size.y = required.y;
            rect.sizeDelta = size;
        }

        public static void FitGridCell(GridLayoutGroup grid)
        {
            if (grid == null) return;
            Vector2 cell = grid.cellSize;
            Vector2 needed = cell;
            foreach (Transform child in grid.transform)
            {
                var sizeLock = child.GetComponent<UIStyleSizeLock>();
                if (sizeLock == null || !child.gameObject.activeSelf) continue;
                Vector2 required = sizeLock.RequiredSize;
                needed.x = Mathf.Max(needed.x, required.x);
                needed.y = Mathf.Max(needed.y, required.y);
            }
            if (needed != cell) grid.cellSize = needed;
        }

        public static void Enforce(GameObject go)
        {
            if (go == null) return;
            var sizeLock = go.GetComponent<UIStyleSizeLock>();
            if (sizeLock != null) sizeLock.Enforce();
        }
        #endregion

        #region [Private Methods]
        private void MarkDirty()
        {
            if (transform is RectTransform rect) LayoutRebuilder.MarkLayoutForRebuild(rect);
        }

        private static void EnforceAxis(LayoutElement le, bool horizontal, float min, float fixedValue)
        {
            if (fixedValue > 0f)
            {
                if (horizontal)
                {
                    le.minWidth = fixedValue;
                    le.preferredWidth = fixedValue;
                    le.flexibleWidth = 0f;
                }
                else
                {
                    le.minHeight = fixedValue;
                    le.preferredHeight = fixedValue;
                    le.flexibleHeight = 0f;
                }
                return;
            }

            if (min <= 0f) return;
            if (horizontal)
            {
                le.minWidth = Mathf.Max(le.minWidth, min);
                if (le.preferredWidth < le.minWidth) le.preferredWidth = le.minWidth;
            }
            else
            {
                le.minHeight = Mathf.Max(le.minHeight, min);
                if (le.preferredHeight < le.minHeight) le.preferredHeight = le.minHeight;
            }
        }
        #endregion
    }
}
