using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Diagnostics
{
    public static class UIDebugOverlay
    {
        #region [Variables/Fields]
        private const float LineWidth = 2f;
        private static readonly Color ErrorColor = new Color(1f, 0.15f, 0.2f, 0.95f);
        private static readonly Color WarningColor = new Color(1f, 0.6f, 0f, 0.95f);
        #endregion

        #region [Public Methods]
        public static GameObject Build(Canvas canvas, IReadOnlyList<UILayoutIssue> issues, RectTransform visibleArea = null)
        {
            if (canvas == null || issues == null || issues.Count == 0) return null;

            Canvas root = canvas.rootCanvas;
            var overlay = new GameObject("[UIDebugOverlay]", typeof(RectTransform));
            var overlayRect = (RectTransform)overlay.transform;
            overlayRect.SetParent(root.transform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.SetAsLastSibling();
            overlay.AddComponent<LayoutElement>().ignoreLayout = true;

            Rect visible = GetArea(overlayRect, visibleArea);
            foreach (var issue in issues)
            {
                if (issue.Target == null) continue;
                Vector2 min = overlayRect.InverseTransformPoint(issue.WorldCorners[0]);
                Vector2 max = overlayRect.InverseTransformPoint(issue.WorldCorners[2]);
                if (visibleArea != null && !visible.Overlaps(Rect.MinMaxRect(min.x, min.y, max.x, max.y))) continue;
                Color color = issue.Severity == UIIssueSeverity.Error ? ErrorColor : WarningColor;
                DrawBox(overlayRect, min, max, color);
                DrawLabel(overlayRect, min, max, issue.Number, color);
            }
            return overlay;
        }
        #endregion

        #region [Private Methods]
        private static Rect GetArea(RectTransform overlayRect, RectTransform area)
        {
            if (area == null) return overlayRect.rect;
            var corners = new Vector3[4];
            area.GetWorldCorners(corners);
            Vector2 min = overlayRect.InverseTransformPoint(corners[0]);
            Vector2 max = overlayRect.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static void DrawBox(RectTransform parent, Vector2 min, Vector2 max, Color color)
        {
            float width = Mathf.Max(LineWidth, max.x - min.x);
            float height = Mathf.Max(LineWidth, max.y - min.y);
            Line(parent, new Vector2(min.x, max.y - LineWidth), new Vector2(width, LineWidth), color);
            Line(parent, new Vector2(min.x, min.y), new Vector2(width, LineWidth), color);
            Line(parent, new Vector2(min.x, min.y), new Vector2(LineWidth, height), color);
            Line(parent, new Vector2(max.x - LineWidth, min.y), new Vector2(LineWidth, height), color);
        }

        private static void Line(RectTransform parent, Vector2 position, Vector2 size, Color color)
        {
            var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)line.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = position + parent.rect.center * -1f;
            rect.sizeDelta = size;
            var image = line.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static void DrawLabel(RectTransform parent, Vector2 min, Vector2 max, int number, Color color)
        {
            var tag = new GameObject($"#{number}", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)tag.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(min.x, max.y) + parent.rect.center * -1f;
            rect.sizeDelta = new Vector2(number >= 10 ? 30f : 22f, 18f);
            var background = tag.GetComponent<Image>();
            background.color = color;
            background.raycastTarget = false;

            var label = new GameObject("Text", typeof(RectTransform));
            var labelRect = (RectTransform)label.transform;
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var text = label.AddComponent<TextMeshProUGUI>();
            text.text = number.ToString();
            text.fontSize = 13f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }
        #endregion
    }
}
