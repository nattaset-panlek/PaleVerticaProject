using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Diagnostics
{
    public static class UILayoutInspector
    {
        #region [Variables/Fields]
        private const float Tolerance = 1.5f;
        private const int MaxWordsMeasured = 40;
        private static readonly Regex RichTag = new Regex("<[^>]+>");
        private static readonly char[] WordSeparators = { ' ', '\n', '\t' };
        private static readonly Vector3[] ChildCorners = new Vector3[4];
        private static readonly Vector3[] OtherCorners = new Vector3[4];
        #endregion

        #region [Public Methods]
        public static List<UILayoutIssue> Inspect(RectTransform root, UITheme theme = null)
        {
            var issues = new List<UILayoutIssue>();
            if (root == null) return issues;

            Canvas.ForceUpdateCanvases();
            Visit(root, root, theme ?? UITheme.Default, issues);
            for (int i = 0; i < issues.Count; i++) issues[i].Number = i + 1;
            return issues;
        }

        public static string FormatReport(string title, RectTransform root, List<UILayoutIssue> issues)
        {
            var sb = new StringBuilder();
            int errors = 0;
            foreach (var issue in issues) if (issue.Severity == UIIssueSeverity.Error) errors++;

            sb.AppendLine($"=== UI layout report: {title}");
            sb.AppendLine($"Root size {Size(root)} | issues {issues.Count} ({errors} errors, {issues.Count - errors} warnings)");
            sb.AppendLine();
            foreach (var issue in issues) sb.AppendLine(issue.ToString());
            sb.AppendLine();
            sb.AppendLine("=== Layout tree (size | min | preferred | flexible | layout | text) ; <-- #n marks issues");
            sb.Append(DescribeTree(root, issues));
            return sb.ToString();
        }

        public static string DescribeTree(RectTransform root, List<UILayoutIssue> issues = null)
        {
            var marks = new Dictionary<RectTransform, List<int>>();
            if (issues != null)
            {
                foreach (var issue in issues)
                {
                    if (issue.Target == null) continue;
                    if (!marks.TryGetValue(issue.Target, out var list)) marks[issue.Target] = list = new List<int>();
                    list.Add(issue.Number);
                }
            }

            var sb = new StringBuilder();
            Describe(root, 0, marks, sb);
            return sb.ToString();
        }
        #endregion

        #region [Private Methods]
        private static void Visit(RectTransform rect, RectTransform root, UITheme theme, List<UILayoutIssue> issues)
        {
            if (!rect.gameObject.activeInHierarchy) return;

            CheckElement(rect, root, theme, issues);
            for (int i = 0; i < rect.childCount; i++)
            {
                if (rect.GetChild(i) is RectTransform child) Visit(child, root, theme, issues);
            }
            CheckSiblings(rect, root, issues);
        }

        private static void CheckElement(RectTransform rect, RectTransform root, UITheme theme, List<UILayoutIssue> issues)
        {
            Rect r = rect.rect;
            var parent = rect.parent as RectTransform;
            var parentGroup = parent != null && rect != root ? parent.GetComponent<LayoutGroup>() : null;
            bool managed = parentGroup != null && parentGroup.enabled && !IgnoresLayout(rect);

            var fitter = rect.GetComponent<ContentSizeFitter>();
            if (managed && fitter != null && fitter.enabled
                && (fitter.horizontalFit != ContentSizeFitter.FitMode.Unconstrained || fitter.verticalFit != ContentSizeFitter.FitMode.Unconstrained))
            {
                Add(issues, UILayoutIssueKind.NestedFitter, UIIssueSeverity.Error, rect, root,
                    $"ContentSizeFitter inside a {parentGroup.GetType().Name}; both try to set the size",
                    "Remove the fitter: the parent layout group already sizes this element.");
            }

            if (managed && !(parentGroup is GridLayoutGroup))
            {
                float minWidth = LayoutUtility.GetMinWidth(rect);
                float minHeight = LayoutUtility.GetMinHeight(rect);
                if (r.width + Tolerance < minWidth)
                {
                    Add(issues, UILayoutIssueKind.BelowMinSize, UIIssueSeverity.Error, rect, root,
                        $"width {r.width:0} is below its minimum {minWidth:0}",
                        "The parent is narrower than the sum of its children's minimum widths. Give the parent more room (Fill, wider window, scroll) or lower sibling MinWidths.");
                }
                if (r.height + Tolerance < minHeight)
                {
                    Add(issues, UILayoutIssueKind.BelowMinSize, UIIssueSeverity.Error, rect, root,
                        $"height {r.height:0} is below its minimum {minHeight:0}",
                        "The parent is shorter than its children need. Let the parent grow with its content.");
                }
            }

            var text = rect.GetComponent<TMP_Text>();
            if (text != null && text.enabled && !string.IsNullOrWhiteSpace(text.text))
            {
                CheckText(text, rect, root, issues);
            }
            else if (rect.GetComponent<Graphic>() != null && (r.width < 1f || r.height < 1f) && rect.childCount == 0 && !IsDrivenFill(rect))
            {
                Add(issues, UILayoutIssueKind.ZeroSize, UIIssueSeverity.Warning, rect, root,
                    $"visible element collapsed to {r.width:0}x{r.height:0}",
                    "Give it a size (Width/Height) or content.");
            }

            var selectable = rect.GetComponent<Selectable>();
            if (selectable != null && !(selectable is Scrollbar) && !(parentGroup is GridLayoutGroup) && r.height > theme.ControlHeight * 2f)
            {
                Add(issues, UILayoutIssueKind.OversizedControl, UIIssueSeverity.Warning, rect, root,
                    $"{selectable.GetType().Name} is {r.height:0}px tall (theme control height {theme.ControlHeight:0})",
                    "A prefab size is probably leaking into the layout. Builder controls use UIElementFactory.FitControl; otherwise set Height().");
            }

            if (managed) CheckInsideParent(rect, parent, root, issues);
        }

        private static void CheckText(TMP_Text text, RectTransform rect, RectTransform root, List<UILayoutIssue> issues)
        {
            Rect r = rect.rect;
            string plain = RichTag.Replace(text.text, string.Empty);
            float available = r.width - text.margin.x - text.margin.z;
            bool wraps = text.textWrappingMode == TextWrappingModes.Normal || text.textWrappingMode == TextWrappingModes.PreserveWhitespace;

            if (wraps)
            {
                float longestWord = LongestWordWidth(text, plain);
                if (longestWord > available + Tolerance)
                {
                    Add(issues, UILayoutIssueKind.LabelSquashed, UIIssueSeverity.Error, rect, root,
                        $"\"{Shorten(plain)}\": a word needs {longestWord:0}px but the text is {available:0}px wide, so it breaks inside words",
                        "Give the element more width (MinWidth/Fill) or less competition from siblings. Button labels should not wrap.");
                }

                float needed = text.GetPreferredValues(text.text, Mathf.Max(1f, available), 0f).y;
                if (needed > r.height + Tolerance)
                {
                    Add(issues, UILayoutIssueKind.TextOverflow, UIIssueSeverity.Error, rect, root,
                        $"\"{Shorten(plain)}\" needs {needed:0}px of height but has {r.height:0}px, so it spills onto what follows",
                        "Do not fix the height of text (no Height/preferredHeight); let TMP size it and let the parent grow.");
                }
            }
            else
            {
                Vector2 needed = text.GetPreferredValues(text.text, float.PositiveInfinity, float.PositiveInfinity);
                if (needed.x > available + Tolerance)
                {
                    Add(issues, UILayoutIssueKind.LabelTruncated, UIIssueSeverity.Warning, rect, root,
                        $"\"{Shorten(plain)}\" needs {needed.x:0}px but has {available:0}px, so it is cut off",
                        "Widen the control (MinWidth/Fill) or shorten the label.");
                }

                float height = r.height - text.margin.y - text.margin.w;
                if (needed.y > height + Tolerance)
                {
                    bool hides = text.overflowMode != TextOverflowModes.Overflow;
                    Add(issues, UILayoutIssueKind.TextOverflow, hides ? UIIssueSeverity.Error : UIIssueSeverity.Warning, rect, root,
                        hides
                            ? $"\"{Shorten(plain)}\" needs {needed.y:0}px of height but has {height:0}px; overflow mode {text.overflowMode} hides the whole line"
                            : $"\"{Shorten(plain)}\" needs {needed.y:0}px of height but has {height:0}px and spills outside its box",
                        "Make the box at least as tall as the font's line height (controls use UIElementFactory.FitControl) or use overflow mode Overflow.");
                }
            }
        }

        private static void CheckInsideParent(RectTransform rect, RectTransform parent, RectTransform root, List<UILayoutIssue> issues)
        {
            rect.GetWorldCorners(ChildCorners);
            Rect bounds = parent.rect;
            Vector2 min = parent.InverseTransformPoint(ChildCorners[0]);
            Vector2 max = parent.InverseTransformPoint(ChildCorners[2]);

            float overflow = Mathf.Max(bounds.xMin - min.x, max.x - bounds.xMax, bounds.yMin - min.y, max.y - bounds.yMax);
            if (overflow > Tolerance)
            {
                Add(issues, UILayoutIssueKind.OutsideParent, UIIssueSeverity.Error, rect, root,
                    $"extends {overflow:0}px outside its parent \"{parent.name}\"",
                    "The parent layout cannot fit its children. Check the BelowMinSize issues and the children's min/preferred sizes in the tree.");
            }
        }

        private static void CheckSiblings(RectTransform parent, RectTransform root, List<UILayoutIssue> issues)
        {
            var group = parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (group == null || !group.enabled) return;

            RectTransform previous = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (!(parent.GetChild(i) is RectTransform child) || !child.gameObject.activeInHierarchy || IgnoresLayout(child)) continue;

                if (previous != null)
                {
                    previous.GetWorldCorners(OtherCorners);
                    child.GetWorldCorners(ChildCorners);
                    Vector2 aMin = parent.InverseTransformPoint(OtherCorners[0]);
                    Vector2 aMax = parent.InverseTransformPoint(OtherCorners[2]);
                    Vector2 bMin = parent.InverseTransformPoint(ChildCorners[0]);
                    Vector2 bMax = parent.InverseTransformPoint(ChildCorners[2]);
                    float overlapX = Mathf.Min(aMax.x, bMax.x) - Mathf.Max(aMin.x, bMin.x);
                    float overlapY = Mathf.Min(aMax.y, bMax.y) - Mathf.Max(aMin.y, bMin.y);
                    if (overlapX > Tolerance && overlapY > Tolerance)
                    {
                        Add(issues, UILayoutIssueKind.SiblingOverlap, UIIssueSeverity.Error, child, root,
                            $"overlaps its previous sibling \"{previous.name}\" by {overlapX:0}x{overlapY:0}px",
                            "Children of a layout group should never overlap; check childControlWidth/Height on the group and fixed sizes on the children.");
                    }
                }
                previous = child;
            }
        }

        private static void Describe(RectTransform rect, int depth, Dictionary<RectTransform, List<int>> marks, StringBuilder sb)
        {
            sb.Append(' ', depth * 2);
            sb.Append(rect.gameObject.activeInHierarchy ? "" : "(inactive) ");
            sb.Append(rect.name);
            sb.Append(" [").Append(Components(rect)).Append("] ");
            sb.Append(Size(rect));
            sb.Append($" | min {LayoutUtility.GetMinWidth(rect):0}x{LayoutUtility.GetMinHeight(rect):0}");
            sb.Append($" | pref {LayoutUtility.GetPreferredWidth(rect):0}x{LayoutUtility.GetPreferredHeight(rect):0}");
            sb.Append($" | flex {LayoutUtility.GetFlexibleWidth(rect):0.#}x{LayoutUtility.GetFlexibleHeight(rect):0.#}");

            var group = rect.GetComponent<LayoutGroup>();
            if (group != null)
            {
                RectOffset p = group.padding;
                sb.Append($" | {GroupName(group)} pad {p.left},{p.right},{p.top},{p.bottom} align {group.childAlignment}");
                if (group is HorizontalOrVerticalLayoutGroup hv)
                {
                    sb.Append($" gap {hv.spacing:0} ctrl {(hv.childControlWidth ? 1 : 0)}{(hv.childControlHeight ? 1 : 0)} expand {(hv.childForceExpandWidth ? 1 : 0)}{(hv.childForceExpandHeight ? 1 : 0)}");
                }
                else if (group is GridLayoutGroup grid)
                {
                    sb.Append($" cell {grid.cellSize.x:0}x{grid.cellSize.y:0} gap {grid.spacing.x:0} {grid.constraint} {grid.constraintCount}");
                }
            }

            var text = rect.GetComponent<TMP_Text>();
            if (text != null)
            {
                sb.Append($" | \"{Shorten(RichTag.Replace(text.text ?? string.Empty, string.Empty))}\" {text.fontSize:0}pt {text.textWrappingMode}");
            }

            if (marks.TryGetValue(rect, out var numbers))
            {
                sb.Append("  <--");
                foreach (int n in numbers) sb.Append(" #").Append(n);
            }
            sb.AppendLine();

            for (int i = 0; i < rect.childCount; i++)
            {
                if (rect.GetChild(i) is RectTransform child) Describe(child, depth + 1, marks, sb);
            }
        }

        private static string Components(RectTransform rect)
        {
            var parts = new List<string>();
            if (rect.GetComponent<ScrollRect>() != null) parts.Add("Scroll");
            if (rect.GetComponent<Button>() != null) parts.Add("Button");
            if (rect.GetComponent<TMP_InputField>() != null) parts.Add("Input");
            if (rect.GetComponent<Toggle>() != null) parts.Add("Toggle");
            if (rect.GetComponent<TMP_Text>() != null) parts.Add("Text");
            else if (rect.GetComponent<Image>() != null) parts.Add("Image");
            var group = rect.GetComponent<LayoutGroup>();
            if (group != null) parts.Add(GroupName(group));
            var fitter = rect.GetComponent<ContentSizeFitter>();
            if (fitter != null) parts.Add($"Fitter {(int)fitter.horizontalFit}{(int)fitter.verticalFit}");
            var le = rect.GetComponent<LayoutElement>();
            if (le != null) parts.Add(le.ignoreLayout ? "LE ignore" : "LE");
            return string.Join(",", parts);
        }

        private static string GroupName(LayoutGroup group)
        {
            if (group is HorizontalLayoutGroup) return "Row";
            if (group is VerticalLayoutGroup) return "Column";
            if (group is GridLayoutGroup) return "Grid";
            return group.GetType().Name;
        }

        private static float LongestWordWidth(TMP_Text text, string plain)
        {
            float longest = 0f;
            string[] words = plain.Split(WordSeparators, System.StringSplitOptions.RemoveEmptyEntries);
            int count = Mathf.Min(words.Length, MaxWordsMeasured);
            for (int i = 0; i < count; i++)
            {
                float width = text.GetPreferredValues(words[i], float.PositiveInfinity, float.PositiveInfinity).x;
                if (width > longest) longest = width;
            }
            return longest;
        }

        private static bool IsDrivenFill(RectTransform rect)
        {
            var slider = rect.GetComponentInParent<Slider>();
            if (slider != null && (slider.fillRect == rect || slider.handleRect == rect)) return true;
            var scrollbar = rect.GetComponentInParent<Scrollbar>();
            return scrollbar != null && scrollbar.handleRect == rect;
        }

        private static bool IgnoresLayout(RectTransform rect)
        {
            var le = rect.GetComponent<LayoutElement>();
            return le != null && le.ignoreLayout;
        }

        private static void Add(List<UILayoutIssue> issues, UILayoutIssueKind kind, UIIssueSeverity severity, RectTransform rect, RectTransform root, string message, string hint)
        {
            issues.Add(new UILayoutIssue(kind, severity, rect, PathOf(rect, root), message, hint));
        }

        private static string PathOf(RectTransform rect, RectTransform root)
        {
            var names = new List<string>();
            Transform current = rect;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Add(root.name);
            names.Reverse();
            return string.Join("/", names);
        }

        private static string Size(RectTransform rect) => $"{rect.rect.width:0}x{rect.rect.height:0}";

        private static string Shorten(string value)
        {
            value = value.Replace('\n', ' ');
            return value.Length <= 32 ? value : value.Substring(0, 29) + "...";
        }
        #endregion
    }
}
