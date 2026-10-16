using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public struct UIElementRef
    {
        #region [Variables/Fields]
        public GameObject GameObject;
        private readonly UIDocument _document;
        #endregion

        #region [Properties]
        public bool IsValid => GameObject != null;
        public UIDocument Document => _document;
        public UITheme Theme => _document != null ? _document.Theme : UITheme.Default;
        public RectTransform RectTransform => GameObject != null ? GameObject.GetComponent<RectTransform>() : null;
        public TMP_Text TextComponent => GameObject != null ? GameObject.GetComponentInChildren<TMP_Text>(true) : null;
        #endregion

        #region [Public Methods]
        public UIElementRef(GameObject go)
        {
            GameObject = go;
            _document = null;
        }

        public UIElementRef(GameObject go, UIDocument document)
        {
            GameObject = go;
            _document = document;
        }

        public UIElementRef Id(string id)
        {
            if (GameObject != null && _document != null) _document.RegisterElement(id, GameObject);
            return this;
        }

        public UIElementRef Width(float width)
        {
            if (GameObject == null) return this;
            var le = GetLayout();
            le.preferredWidth = width;
            le.minWidth = width;
            le.flexibleWidth = 0f;
            UnfitAxis(true, false);
            return this;
        }

        public UIElementRef Height(float height)
        {
            if (GameObject == null) return this;
            var le = GetLayout();
            le.preferredHeight = height;
            le.minHeight = height;
            le.flexibleHeight = 0f;
            UnfitAxis(false, true);
            return this;
        }

        public UIElementRef Size(float width, float height) => Width(width).Height(height);

        public UIElementRef Fill(float weight = 1f)
        {
            if (GameObject == null) return this;
            GetLayout().flexibleWidth = weight;
            UnfitAxis(true, false);
            return this;
        }

        public UIElementRef FillHeight(float weight = 1f)
        {
            if (GameObject == null) return this;
            GetLayout().flexibleHeight = weight;
            UnfitAxis(false, true);
            return this;
        }

        public UIElementRef MinWidth(float width)
        {
            if (GameObject != null) GetLayout().minWidth = width;
            return this;
        }

        public UIElementRef MinHeight(float height)
        {
            if (GameObject != null) GetLayout().minHeight = height;
            return this;
        }

        public UIElementRef Padding(int all) => SetPadding(all, all, all, all);

        public UIElementRef Padding(int horizontal, int vertical) => SetPadding(horizontal, horizontal, vertical, vertical);

        public UIElementRef Padding(int left, int right, int top, int bottom) => SetPadding(left, right, top, bottom);

        public UIElementRef Gap(float gap)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<LayoutGroup>();
            if (group is HorizontalOrVerticalLayoutGroup hv) hv.spacing = gap;
            else if (group is GridLayoutGroup grid) grid.spacing = new Vector2(gap, gap);
            return this;
        }

        public UIElementRef Align(TextAnchor anchor)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<LayoutGroup>();
            if (group != null) group.childAlignment = anchor;
            return this;
        }

        public UIElementRef Cell(float width, float height) => SetGridCellSize(new Vector2(width, height));

        public UIElementRef Columns(int count) => SetGridConstraint(GridLayoutGroup.Constraint.FixedColumnCount, count);

        public UIElementRef Background(Color color) => SetBackgroundColor(color);

        public UIElementRef Variant(UIVariant variant)
        {
            if (GameObject == null || variant == UIVariant.Default) return this;

            UITheme theme = Theme;
            Graphic target = null;
            var selectable = GameObject.GetComponent<Selectable>();
            if (selectable != null) target = selectable.targetGraphic;
            if (target == null) target = GameObject.GetComponent<Image>();
            if (target == null) target = GameObject.AddComponent<Image>();
            target.color = theme.GetVariantColor(variant);

            var text = GameObject.GetComponentInChildren<TMP_Text>(true);
            if (text != null) text.color = theme.GetVariantTextColor(variant);
            return this;
        }

        public UIElementRef TextColor(Color color) => SetTextColor(color);

        public UIElementRef FontSize(float size) => SetFontSize(size);

        public UIElementRef Bold() => SetFontStyle(FontStyles.Bold);

        public UIElementRef TextAlign(TextAlignmentOptions alignment) => SetTextAlignment(alignment);

        public UIElementRef Text(string value)
        {
            var text = TextComponent;
            if (text != null) text.text = value;
            return this;
        }

        public UIElementRef OnClick(Action onClick) => SetOnClick(onClick);

        public UIElementRef OnSubmit(Action<string> onSubmit)
        {
            if (GameObject == null || onSubmit == null) return this;
            var input = GameObject.GetComponentInChildren<TMP_InputField>(true);
            if (input != null) input.onSubmit.AddListener(value => onSubmit(value));
            return this;
        }

        public UIElementRef Interactable(bool interactable)
        {
            if (GameObject == null) return this;
            foreach (var selectable in GameObject.GetComponentsInChildren<Selectable>(true))
            {
                selectable.interactable = interactable;
            }
            return this;
        }

        public UIElementRef Visible(bool visible)
        {
            if (GameObject != null) GameObject.SetActive(visible);
            return this;
        }

        public UIElementRef SetSize(float width, float height)
        {
            if (RectTransform != null) RectTransform.sizeDelta = new Vector2(width, height);
            return this;
        }

        public UIElementRef SetPreferredSize(float width = -1, float height = -1)
        {
            if (GameObject == null) return this;
            var le = GetLayout();
            if (width >= 0) le.preferredWidth = width;
            if (height >= 0) le.preferredHeight = height;
            UnfitAxis(width >= 0, height >= 0);
            return this;
        }

        public UIElementRef SetMinSize(float width = -1, float height = -1)
        {
            if (GameObject == null) return this;
            var le = GetLayout();
            if (width >= 0) le.minWidth = width;
            if (height >= 0) le.minHeight = height;
            return this;
        }

        public UIElementRef SetGridCellSize(Vector2 cellSize)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<GridLayoutGroup>();
            if (group != null) group.cellSize = cellSize;
            return this;
        }

        public UIElementRef SetGridConstraint(GridLayoutGroup.Constraint constraint, int count)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<GridLayoutGroup>();
            if (group != null)
            {
                group.constraint = constraint;
                group.constraintCount = Mathf.Max(1, count);
            }
            return this;
        }

        public UIElementRef SetGridSpacing(Vector2 spacing)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<GridLayoutGroup>();
            if (group != null) group.spacing = spacing;
            return this;
        }

        public UIElementRef SetFlexibleSize(float flexWidth = -1, float flexHeight = -1)
        {
            if (GameObject == null) return this;
            var le = GetLayout();
            if (flexWidth >= 0) le.flexibleWidth = flexWidth;
            if (flexHeight >= 0) le.flexibleHeight = flexHeight;
            UnfitAxis(flexWidth >= 0, flexHeight >= 0);
            return this;
        }

        public UIElementRef SetFitParent(bool stretchWidth = true, bool stretchHeight = true)
        {
            if (GameObject == null) return this;
            UnfitAxis(stretchWidth, stretchHeight);
            var le = GetLayout();
            if (stretchWidth) le.flexibleWidth = 1;
            if (stretchHeight) le.flexibleHeight = 1;
            return this;
        }

        public UIElementRef SetBackgroundColor(Color color)
        {
            if (GameObject == null) return this;
            var img = GameObject.GetComponent<Image>();
            if (img == null) img = GameObject.AddComponent<Image>();
            img.color = color;
            return this;
        }

        public UIElementRef SetSprite(Sprite sprite)
        {
            if (GameObject == null) return this;
            var img = GameObject.GetComponent<Image>();
            if (img == null) img = GameObject.AddComponent<Image>();
            img.sprite = sprite;
            if (sprite != null) img.color = Color.white;
            return this;
        }

        public UIElementRef SetOnClick(Action onClick)
        {
            if (GameObject == null) return this;
            var btn = GameObject.GetComponent<Button>();
            if (btn == null)
            {
                btn = GameObject.AddComponent<Button>();
                var graphic = GameObject.GetComponent<Graphic>();
                if (graphic == null)
                {
                    var hitArea = GameObject.AddComponent<Image>();
                    hitArea.color = Color.clear;
                    graphic = hitArea;
                }
                btn.targetGraphic = graphic;
            }
            btn.onClick.RemoveAllListeners();
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return this;
        }

        public UIElementRef SetColor(Color color)
        {
            if (GameObject == null) return this;
            var graphic = GameObject.GetComponent<Graphic>();
            if (graphic != null) graphic.color = color;
            return this;
        }

        public UIElementRef SetTextColor(Color color)
        {
            var txt = TextComponent;
            if (txt != null) txt.color = color;
            return this;
        }

        public UIElementRef SetFontSize(float size)
        {
            var txt = TextComponent;
            if (txt != null) txt.fontSize = size;
            return this;
        }

        public UIElementRef SetFontStyle(FontStyles style)
        {
            var txt = TextComponent;
            if (txt != null) txt.fontStyle = style;
            return this;
        }

        public UIElementRef SetTextAlignment(TextAlignmentOptions alignment)
        {
            var txt = TextComponent;
            if (txt != null) txt.alignment = alignment;
            return this;
        }

        public UIElementRef SetPadding(int left, int right, int top, int bottom)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<LayoutGroup>();
            if (group == null) group = GameObject.GetComponentInChildren<LayoutGroup>(true);
            if (group != null) group.padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public UIElementRef SetSpacing(float spacing)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (group == null) group = GameObject.GetComponentInChildren<HorizontalOrVerticalLayoutGroup>(true);
            if (group != null) group.spacing = spacing;
            return this;
        }

        public UIElementRef SetChildAlignment(TextAnchor anchor)
        {
            if (GameObject == null) return this;
            var group = GameObject.GetComponent<LayoutGroup>();
            if (group == null) group = GameObject.GetComponentInChildren<LayoutGroup>(true);
            if (group != null) group.childAlignment = anchor;
            return this;
        }

        public UIElementRef SetScrollMovementType(ScrollRect.MovementType movementType)
        {
            if (GameObject == null) return this;
            var sr = GameObject.GetComponentInChildren<ScrollRect>(true);
            if (sr != null) sr.movementType = movementType;
            return this;
        }

        public UIElementRef SetScrollElasticity(float elasticity)
        {
            if (GameObject == null) return this;
            var sr = GameObject.GetComponentInChildren<ScrollRect>(true);
            if (sr != null) sr.elasticity = elasticity;
            return this;
        }
        #endregion

        #region [Private Methods]
        private LayoutElement GetLayout()
        {
            var le = GameObject.GetComponent<LayoutElement>();
            return le != null ? le : GameObject.AddComponent<LayoutElement>();
        }

        private void UnfitAxis(bool horizontal, bool vertical)
        {
            var fitter = GameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null) return;
            if (horizontal) fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            if (vertical) fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        #endregion
    }
}
