using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public class UIGlobalModal : UIModal
    {
        #region [Variables/Fields]
        [Header("Global Modal Elements")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _closeButton;

        [Header("Resize Settings")]
        [SerializeField] private bool _isResizable = true;
        [SerializeField] private RectTransform _resizeHandle;
        [SerializeField] private Sprite _resizeHandleSprite;
        [SerializeField] private Vector2 _minSize = new Vector2(250f, 180f);
        [SerializeField] private Vector2 _maxSize = new Vector2(2560f, 1440f);

        public event Action OnOpened;
        public event Action OnClosed;
        public event Action<Vector2> OnResized;

        private UIDocument _document;
        private UIElementRegistrySO _registryOverride;
        private ScrollRect _scrollRect;
        private Action _closeAction;
        private bool _fitContent;
        private bool _fitWidth = true;
        private Vector2 _fitMaxSize;
        private const float CanvasMargin = 40f;
        #endregion

        #region [Properties]
        public RectTransform ContentContainer
        {
            get
            {
                var scrollRect = ScrollRect;
                if (scrollRect != null && scrollRect.content != null) return scrollRect.content;
                return _contentContainer;
            }
        }

        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null && _contentContainer != null) _scrollRect = _contentContainer.GetComponent<ScrollRect>();
                return _scrollRect;
            }
        }

        public UIDocument Document
        {
            get
            {
                if (_document == null) CreateDocument();
                return _document;
            }
        }

        public UITheme Theme => Document.Theme;
        public bool IsResizable => _isResizable;
        public RectTransform ResizeHandle => _resizeHandle;
        public Vector2 MinSize => _minSize;
        public Vector2 MaxSize => _maxSize;
        public bool IsFittingContent => _fitContent;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            if (_isResizable) EnsureResizeHandle();
        }
        #endregion

        #region [Public Methods]
        public static UIGlobalModal Create(string modalName) => GetOrCreate(modalName);

        public static UIGlobalModal GetOrCreate(string modalName)
        {
            if (UIManager.Instance == null)
            {
                Debug.LogError("[UIGlobalModal] UIManager.Instance is null.");
                return null;
            }
            return UIManager.Instance.GetOrCreateGlobalModal(modalName);
        }

        public static UIGlobalModal Get(string modalName)
        {
            if (UIManager.Instance == null || string.IsNullOrEmpty(modalName)) return null;
            return UIManager.Instance.GetView<UIGlobalModal>(modalName);
        }

        public override void OnOpen(object data)
        {
            base.OnOpen(data);

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(HandleCloseClicked);
            }

            if (_isResizable)
            {
                EnsureResizeHandle();
                if (_resizeHandle != null) _resizeHandle.SetAsLastSibling();
            }

            if (_fitContent) FitToContent();
            OnOpened?.Invoke();
        }

        public override void OnClose()
        {
            base.OnClose();
            OnClosed?.Invoke();
        }

        public UIGlobalModal Build(Action<UIBuilderContext> builderAction)
        {
            Document.Build(builderAction);
            return this;
        }

        public UIGlobalModal Refresh()
        {
            Document.Refresh();
            return this;
        }

        public bool Refresh(string regionId) => Document.Refresh(regionId);

        public bool RefreshPage(string pagesId) => Document.RefreshPage(pagesId);

        public UIGlobalModal ShowPage(string pagesId, int index)
        {
            Document.ShowPage(pagesId, index);
            return this;
        }

        public UIGlobalModal ShowPage(string pagesId, string title)
        {
            Document.ShowPage(pagesId, title);
            return this;
        }

        public UIElementRef Find(string id) => Document.Find(id);

        public UIGlobalModal SetRegistry(UIElementRegistrySO registry)
        {
            _registryOverride = registry;
            if (_document != null)
            {
                _document.OnRebuilt -= HandleRebuilt;
                _document = null;
            }
            return this;
        }

        public UIGlobalModal SetTitle(string title)
        {
            if (_titleText != null) _titleText.text = title;
            return this;
        }

        public UIGlobalModal ClearContent()
        {
            Document.Clear();
            return this;
        }

        public UIGlobalModal SetWindowSize(Vector2 size)
        {
            if (WindowRect != null) WindowRect.sizeDelta = size;
            return this;
        }

        public UIGlobalModal SetWindowPosition(Vector2 position)
        {
            if (WindowRect != null) WindowRect.anchoredPosition = position;
            return this;
        }

        public UIGlobalModal SetWindowAnchors(Vector2 min, Vector2 max, Vector2 pivot)
        {
            RectTransform rect = WindowRect;
            if (rect != null)
            {
                rect.anchorMin = min;
                rect.anchorMax = max;
                rect.pivot = pivot;
            }
            return this;
        }

        public UIGlobalModal Center()
        {
            SetWindowAnchors(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            return SetWindowPosition(Vector2.zero);
        }

        public UIGlobalModal SetCloseButtonVisible(bool isVisible)
        {
            if (_closeButton != null) _closeButton.gameObject.SetActive(isVisible);
            return this;
        }

        public UIGlobalModal SetDismissable(bool dismissable)
        {
            CloseOnEscape = dismissable;
            CloseOnOverlayClick = dismissable;
            return SetCloseButtonVisible(dismissable);
        }

        public UIGlobalModal SetDraggable(bool draggable)
        {
            IsDraggable = draggable;
            return this;
        }

        public UIGlobalModal OverrideCloseAction(Action onClose)
        {
            _closeAction = onClose;
            return this;
        }

        public UIGlobalModal FitContent(bool enabled = true, Vector2? maxSize = null, bool fitWidth = true)
        {
            _fitContent = enabled;
            _fitWidth = fitWidth;
            _fitMaxSize = maxSize ?? Vector2.zero;
            if (enabled) FitToContent();
            return this;
        }

        public UIGlobalModal FitToContent() => ApplyFit(_fitWidth, true, Vector2.zero);

        public UIGlobalModal AutoFitWindow(bool fitWidth = true, bool fitHeight = true, float paddingX = 0f, float paddingY = 0f)
        {
            return ApplyFit(fitWidth, fitHeight, new Vector2(paddingX, paddingY));
        }

        public UIGlobalModal SetScrollMovementType(ScrollRect.MovementType movementType)
        {
            if (ScrollRect != null) ScrollRect.movementType = movementType;
            return this;
        }

        public void DestroyModal()
        {
            if (UIManager.Instance != null && !string.IsNullOrEmpty(WindowName)) UIManager.Instance.DestroyModal(WindowName);
            else Destroy(gameObject);
        }

        public UIGlobalModal SetResizable(bool isResizable)
        {
            _isResizable = isResizable;
            if (_isResizable) EnsureResizeHandle();
            if (_resizeHandle != null)
            {
                _resizeHandle.gameObject.SetActive(_isResizable);
                if (_isResizable) _resizeHandle.SetAsLastSibling();
            }
            return this;
        }

        public UIGlobalModal SetResizeHandleVisible(bool isVisible) => SetResizable(isVisible);

        public UIGlobalModal SetMinSize(Vector2 minSize)
        {
            _minSize = minSize;
            return this;
        }

        public UIGlobalModal SetMaxSize(Vector2 maxSize)
        {
            _maxSize = maxSize;
            return this;
        }

        public UIGlobalModal SetResizeHandleSprite(Sprite sprite)
        {
            _resizeHandleSprite = sprite;
            if (_resizeHandle != null)
            {
                var image = _resizeHandle.GetComponent<Image>();
                if (image != null) image.sprite = _resizeHandleSprite;
            }
            return this;
        }

        public void NotifyResized(Vector2 size)
        {
            _fitContent = false;
            OnResized?.Invoke(size);
        }
        #endregion

        #region [Private Methods]
        private void CreateDocument()
        {
            var registry = _registryOverride != null ? _registryOverride : UIManager.Instance != null ? UIManager.Instance.ElementRegistry : null;
            _document = new UIDocument(ContentContainer, registry);
            PrepareRoot(ContentContainer, _document.Theme);
            _document.OnRebuilt += HandleRebuilt;
        }

        private static void PrepareRoot(RectTransform root, UITheme theme)
        {
            if (root == null) return;

            var group = root.GetComponent<VerticalLayoutGroup>();
            if (group == null) return;

            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            group.spacing = theme.Gap;

            RectOffset padding = group.padding;
            if (padding.left == 0 && padding.right == 0 && padding.top == 0 && padding.bottom == 0)
            {
                group.padding = new RectOffset(theme.Padding, theme.Padding, theme.Padding, theme.Padding);
            }
        }

        private void HandleRebuilt()
        {
            if (_fitContent) FitToContent();
        }

        private void HandleCloseClicked()
        {
            if (_closeAction != null)
            {
                _closeAction.Invoke();
                return;
            }
            CloseSelf();
        }

        private UIGlobalModal ApplyFit(bool fitWidth, bool fitHeight, Vector2 extra)
        {
            RectTransform window = WindowRect;
            RectTransform content = ContentContainer;
            if (window == null || content == null || _contentContainer == null) return this;

            Vector2 limit = GetFitLimit();
            Vector2 chrome = window.rect.size - _contentContainer.rect.size + extra;

            if (fitWidth)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
                float width = LayoutUtility.GetPreferredWidth(content) + chrome.x;
                window.sizeDelta = new Vector2(Mathf.Clamp(width, _minSize.x, Mathf.Max(_minSize.x, limit.x)), window.sizeDelta.y);
            }

            if (fitHeight)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
                float height = LayoutUtility.GetPreferredHeight(content) + chrome.y;
                window.sizeDelta = new Vector2(window.sizeDelta.x, Mathf.Clamp(height, _minSize.y, Mathf.Max(_minSize.y, limit.y)));
            }

            return this;
        }

        private Vector2 GetFitLimit()
        {
            Vector2 limit = _fitMaxSize.x > 0f && _fitMaxSize.y > 0f ? Vector2.Min(_fitMaxSize, _maxSize) : _maxSize;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.rootCanvas.transform is RectTransform canvasRect)
            {
                limit = Vector2.Min(limit, canvasRect.rect.size - new Vector2(CanvasMargin, CanvasMargin));
            }
            return limit;
        }

        private void EnsureResizeHandle()
        {
            if (_resizeHandle == null)
            {
                Transform existing = transform.Find("ResizeHandle");
                if (existing != null)
                {
                    _resizeHandle = existing as RectTransform;
                }
                else
                {
                    var handle = new GameObject("ResizeHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    handle.transform.SetParent(transform, false);

                    _resizeHandle = (RectTransform)handle.transform;
                    _resizeHandle.anchorMin = new Vector2(1f, 0f);
                    _resizeHandle.anchorMax = new Vector2(1f, 0f);
                    _resizeHandle.pivot = new Vector2(1f, 0f);
                    _resizeHandle.sizeDelta = new Vector2(20f, 20f);
                    _resizeHandle.anchoredPosition = new Vector2(-2f, 2f);

                    var image = handle.GetComponent<Image>();
                    image.color = new Color(1f, 1f, 1f, 0.35f);
                    image.raycastTarget = true;
                    image.sprite = _resizeHandleSprite != null ? _resizeHandleSprite : UIModalResizeHandle.DefaultGripSprite;
                }
            }

            var component = _resizeHandle.GetComponent<UIModalResizeHandle>();
            if (component == null) component = _resizeHandle.gameObject.AddComponent<UIModalResizeHandle>();
            component.Initialize(this);
        }
        #endregion
    }
}
