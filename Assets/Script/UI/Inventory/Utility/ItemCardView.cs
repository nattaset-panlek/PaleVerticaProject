using System;
using CoreSystem.UI.Utility;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.Inventory.Utility
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ItemCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IHoverInteractable
    {
        #region [Variables/Fields]
        public const float CardWidth = 496f;
        public const float CardHeight = 72f;
        public const float DisabledAlpha = 0.45f;
        public const float FadeDuration = 0.15f;

        [Serializable]
        public struct Palette
        {
            public Sprite Body;
            public Color Name;
            public Color Meta;
            public Color On;
            public Color Off;
            public Color BadgeFill;
            public Color BadgeText;
            public Color LabelFill;
            public Color LabelText;
            public bool LabelOutline;
        }

        [SerializeField] private ItemCardTheme _theme = ItemCardTheme.Dark;
        [SerializeField] private bool _pointerStates = true;
        [SerializeField] private RectTransform _card;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _body;
        [SerializeField] private Image _bodyFade;
        [SerializeField] private Image _ring;
        [SerializeField] private Image _ringFade;
        [SerializeField] private Image _stripe;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _meta;
        [SerializeField] private GameObject _starRow;
        [SerializeField] private Image[] _stars = new Image[0];
        [SerializeField] private GameObject _socketRow;
        [SerializeField] private Image[] _sockets = new Image[0];
        [SerializeField] private RectTransform _badge;
        [SerializeField] private Image _badgeFill;
        [SerializeField] private TMP_Text _badgeText;
        [SerializeField] private float _badgePadding = 9f;
        [SerializeField] private RectTransform _label;
        [SerializeField] private Image _labelFill;
        [SerializeField] private Image _labelOutline;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private float _labelPadding = 10f;
        [SerializeField] private float _rightEdge = 480f;
        [SerializeField] private Sprite _ringHover;
        [SerializeField] private Sprite _ringFocus;
        [SerializeField] private Sprite _ringPressed;
        [SerializeField] private Palette _white;
        [SerializeField] private Palette _dark;

        private ItemCardDisplay _display;
        private bool _hovered;
        private bool _pressed;
        private bool _focused;
        private bool _interactable = true;
        private bool _hasApplied;
        private ItemCardState _appliedState;
        private Palette _from;
        private Palette _to;
        private float _fromAlpha = 1f;
        private float _toAlpha = 1f;
        private float _fade = 1f;
        private Tween _tween;
        #endregion

        #region [Properties]
        public ItemCardDisplay Display => _display;
        public ItemCardTheme Theme => _theme;
        public bool Interactable => _interactable;
        public bool ShowsInteractCursor => _interactable;
        public ItemCardState State
        {
            get
            {
                if (!_interactable) return ItemCardState.Disabled;
                if (_pressed) return ItemCardState.Pressed;
                if (_focused) return ItemCardState.Focus;
                if (_hovered) return ItemCardState.Hover;
                return ItemCardState.Default;
            }
        }
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            Fit();
        }

        private void OnDisable()
        {
            _hovered = false;
            _pressed = false;
            if (_tween == null) return;
            KillFade();
            FinishFade();
        }

        private void OnRectTransformDimensionsChange()
        {
            Fit();
        }
        #endregion

        #region [Public Methods]
        public void Show(ItemCardDisplay display)
        {
            bool sameItem = _display.HasItem && display.HasItem && _display.Name == display.Name && _display.Icon == display.Icon;
            _display = display;
            ApplyContent();
            ApplyState(sameItem);
        }

        public void SetTheme(ItemCardTheme theme)
        {
            _theme = theme;
            ApplyState(false);
        }

        public void SetFocused(bool focused)
        {
            _focused = focused;
            ApplyState(true);
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (!interactable)
            {
                _hovered = false;
                _pressed = false;
            }
            ApplyState(true);
        }

        public void ForceState(ItemCardState state)
        {
            _interactable = state != ItemCardState.Disabled;
            _hovered = state == ItemCardState.Hover;
            _focused = state == ItemCardState.Focus;
            _pressed = state == ItemCardState.Pressed;
            ApplyState(false);
        }

        public void Fit()
        {
            if (_card == null) return;
            Rect area = ((RectTransform)transform).rect;
            if (area.width <= 0f || area.height <= 0f) return;
            float scale = Mathf.Min(area.width / CardWidth, area.height / CardHeight);
            _card.localScale = new Vector3(scale, scale, 1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_pointerStates || !_interactable) return;
            _hovered = true;
            ApplyState(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_pointerStates) return;
            _hovered = false;
            _pressed = false;
            ApplyState(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_pointerStates || !_interactable || eventData.button != PointerEventData.InputButton.Left) return;
            _pressed = true;
            ApplyState(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pointerStates || !_pressed) return;
            _pressed = false;
            ApplyState(true);
        }
        #endregion

        #region [Private Methods]
        private void ApplyContent()
        {
            bool has = _display.HasItem;
            if (_stripe != null)
            {
                _stripe.enabled = has;
                if (has) _stripe.color = _display.Rarity.GetColor();
            }
            if (_icon != null)
            {
                _icon.sprite = has ? _display.Icon : null;
                _icon.enabled = has && _display.Icon != null;
            }
            if (_name != null) _name.text = has ? _display.Name : string.Empty;

            bool refine = has && _display.ShowRefine;
            if (_starRow != null) _starRow.SetActive(refine);
            if (_socketRow != null) _socketRow.SetActive(refine && _display.SocketSlots > 0);
            for (int i = 0; i < _sockets.Length; i++)
            {
                if (_sockets[i] != null) _sockets[i].gameObject.SetActive(i < _display.SocketSlots);
            }
            if (_meta != null)
            {
                _meta.gameObject.SetActive(has && !refine && !string.IsNullOrEmpty(_display.Meta));
                _meta.text = _display.Meta ?? string.Empty;
            }

            SetPill(_badge, _badgeText, _display.Badge, _badgePadding);
            SetPill(_label, _labelText, _display.Label, _labelPadding);
        }

        private void ApplyState(bool animate)
        {
            ItemCardState state = State;
            Palette target = PaletteFor(state);
            float targetAlpha = state == ItemCardState.Disabled ? DisabledAlpha : 1f;
            bool changed = !_hasApplied || state != _appliedState || _to.Body != target.Body;
            if (!changed)
            {
                PaintColors();
                return;
            }

            bool fade = animate && _hasApplied && Application.isPlaying && isActiveAndEnabled;
            Palette current = Blend(_from, _to, _fade);
            float currentAlpha = Mathf.Lerp(_fromAlpha, _toAlpha, _fade);
            Sprite previousBody = _to.Body;
            Sprite previousRing = _ring != null ? _ring.sprite : null;
            KillFade();

            _hasApplied = true;
            _appliedState = state;
            _to = target;
            _toAlpha = targetAlpha;
            SetRing(RingFor(state));

            if (!fade)
            {
                _from = target;
                _fromAlpha = targetAlpha;
                _fade = 1f;
                FinishFade();
                return;
            }

            _from = current;
            _fromAlpha = currentAlpha;
            _fade = 0f;
            if (_body != null) _body.sprite = _bodyFade != null ? previousBody : target.Body;
            if (_bodyFade != null)
            {
                _bodyFade.sprite = target.Body;
                _bodyFade.enabled = target.Body != null && target.Body != previousBody;
            }
            if (_ringFade != null)
            {
                _ringFade.sprite = previousRing;
                _ringFade.enabled = previousRing != null;
            }
            PaintColors();
            _tween = DOTween.To(() => _fade, value =>
            {
                _fade = value;
                PaintColors();
            }, 1f, FadeDuration).SetEase(Ease.Linear).SetUpdate(true).SetLink(gameObject).OnComplete(FinishFade);
        }

        private Palette PaletteFor(ItemCardState state)
        {
            bool dark = _theme == ItemCardTheme.Dark || state == ItemCardState.Hover || state == ItemCardState.Focus || state == ItemCardState.Pressed;
            return dark ? _dark : _white;
        }

        private static Palette Blend(Palette from, Palette to, float t)
        {
            return new Palette
            {
                Body = to.Body,
                Name = Color.Lerp(from.Name, to.Name, t),
                Meta = Color.Lerp(from.Meta, to.Meta, t),
                On = Color.Lerp(from.On, to.On, t),
                Off = Color.Lerp(from.Off, to.Off, t),
                BadgeFill = Color.Lerp(from.BadgeFill, to.BadgeFill, t),
                BadgeText = Color.Lerp(from.BadgeText, to.BadgeText, t),
                LabelFill = Color.Lerp(from.LabelFill, to.LabelFill, t),
                LabelText = Color.Lerp(from.LabelText, to.LabelText, t),
                LabelOutline = to.LabelOutline
            };
        }

        private void SetRing(Sprite ring)
        {
            if (_ring == null) return;
            _ring.sprite = ring;
            _ring.enabled = ring != null;
        }

        private void PaintColors()
        {
            Palette palette = Blend(_from, _to, _fade);
            if (_bodyFade != null) _bodyFade.color = new Color(1f, 1f, 1f, _fade);
            if (_ring != null) _ring.color = new Color(1f, 1f, 1f, _fade);
            if (_ringFade != null) _ringFade.color = new Color(1f, 1f, 1f, 1f - _fade);
            if (_group != null) _group.alpha = Mathf.Lerp(_fromAlpha, _toAlpha, _fade);

            if (_name != null) _name.color = palette.Name;
            if (_meta != null) _meta.color = palette.Meta;
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null) _stars[i].color = i < _display.Stars ? palette.On : palette.Off;
            }
            for (int i = 0; i < _sockets.Length; i++)
            {
                if (_sockets[i] != null) _sockets[i].color = i < _display.SocketFilled ? palette.On : palette.Off;
            }
            if (_badgeFill != null) _badgeFill.color = palette.BadgeFill;
            if (_badgeText != null) _badgeText.color = palette.BadgeText;
            if (_labelFill != null) _labelFill.color = palette.LabelFill;
            if (_labelOutline != null)
            {
                float outline = Mathf.Lerp(_from.LabelOutline ? 1f : 0f, _to.LabelOutline ? 1f : 0f, _fade);
                _labelOutline.enabled = outline > 0f;
                _labelOutline.color = new Color(palette.LabelText.r, palette.LabelText.g, palette.LabelText.b, outline);
            }
            if (_labelText != null) _labelText.color = palette.LabelText;
        }

        private void FinishFade()
        {
            _tween = null;
            _fade = 1f;
            _from = _to;
            _fromAlpha = _toAlpha;
            if (_body != null && _to.Body != null) _body.sprite = _to.Body;
            if (_bodyFade != null) _bodyFade.enabled = false;
            if (_ringFade != null) _ringFade.enabled = false;
            PaintColors();
        }

        private void KillFade()
        {
            if (_tween == null) return;
            _tween.Kill();
            _tween = null;
        }

        private Sprite RingFor(ItemCardState state)
        {
            switch (state)
            {
                case ItemCardState.Hover: return _ringHover;
                case ItemCardState.Focus: return _ringFocus;
                case ItemCardState.Pressed: return _ringPressed;
                default: return null;
            }
        }

        private void SetPill(RectTransform pill, TMP_Text text, string value, float padding)
        {
            if (pill == null) return;
            bool visible = !string.IsNullOrEmpty(value);
            pill.gameObject.SetActive(visible);
            if (!visible || text == null) return;
            text.text = value;
            float width = Mathf.Ceil(text.GetPreferredValues(value).x) + padding * 2f;
            Vector2 size = pill.sizeDelta;
            pill.sizeDelta = new Vector2(width, size.y);
            pill.anchoredPosition = new Vector2(_rightEdge - width, pill.anchoredPosition.y);
        }
        #endregion
    }
}
