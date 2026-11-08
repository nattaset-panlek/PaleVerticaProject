using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        public event Action OnClosed;
        public event Action<Vector2> OnResized;

        private bool _isResizing;
        private Vector2 _startSize;
        private Vector2 _startPosition;
        private Vector2 _startPointerPos;
        private static Sprite _cachedGripSprite;
        #endregion

        #region [Properties]
        public RectTransform ContentContainer 
        {
            get 
            {
                if (_contentContainer != null)
                {
                    var scrollRect = _contentContainer.GetComponent<ScrollRect>();
                    if (scrollRect != null && scrollRect.content != null)
                    {
                        return scrollRect.content;
                    }
                }
                return _contentContainer;
            }
        }

        public bool IsResizable => _isResizable;
        public RectTransform ResizeHandle => _resizeHandle;
        public Vector2 MinSize => _minSize;
        public Vector2 MaxSize => _maxSize;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            if (_isResizable)
            {
                EnsureResizeHandle();
            }
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
                _closeButton.onClick.AddListener(() => UIManager.Instance.Close(WindowName));
            }

            if (_isResizable)
            {
                EnsureResizeHandle();
                _resizeHandle?.SetAsLastSibling();
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            OnClosed?.Invoke();
        }

        public UIGlobalModal Build(Action<UIBuilderContext> builderAction)
        {
            ClearContent();
            if (builderAction != null)
            {
                var registry = UIManager.Instance != null ? UIManager.Instance.ElementRegistry : null;
                var context = new UIBuilderContext(ContentContainer, registry);
                builderAction(context);
            }

            if (_isResizable && _resizeHandle != null)
            {
                _resizeHandle.SetAsLastSibling();
            }

            return this;
        }

        public UIGlobalModal SetTitle(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
            return this;
        }

        public UIGlobalModal ClearContent()
        {
            var container = ContentContainer;
            if (container != null)
            {
                foreach (Transform child in container)
                {
                    Destroy(child.gameObject);
                }
            }
            return this;
        }

        public UIGlobalModal SetWindowSize(Vector2 size)
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = size;
            }
            return this;
        }

        public UIGlobalModal SetWindowPosition(Vector2 position)
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = position;
            }
            return this;
        }

        public UIGlobalModal SetWindowAnchors(Vector2 min, Vector2 max, Vector2 pivot)
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = min;
                rect.anchorMax = max;
                rect.pivot = pivot;
            }
            return this;
        }
        
        public UIGlobalModal SetCloseButtonVisible(bool isVisible)
        {
            if (_closeButton != null)
            {
                _closeButton.gameObject.SetActive(isVisible);
            }
            return this;
        }

        public UIGlobalModal OverrideCloseAction(Action onClose)
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                if (onClose != null)
                {
                    _closeButton.onClick.AddListener(new UnityEngine.Events.UnityAction(onClose));
                }
                else
                {
                    _closeButton.onClick.AddListener(() => UIManager.Instance.Close(WindowName));
                }
            }
            return this;
        }

        public UIGlobalModal AutoFitWindow(bool fitWidth = true, bool fitHeight = true, float paddingX = 0f, float paddingY = 60f)
        {
            Canvas.ForceUpdateCanvases();
            var rect = GetComponent<RectTransform>();
            if (rect == null) return this;
            
            float newWidth = rect.sizeDelta.x;
            float newHeight = rect.sizeDelta.y;
            
            var container = ContentContainer;
            if (container != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(container);
                
                if (fitWidth) newWidth = container.rect.width + paddingX;
                if (fitHeight) newHeight = container.rect.height + paddingY;
            }
            
            rect.sizeDelta = new Vector2(newWidth, newHeight);
            return this;
        }

        public UIGlobalModal SetScrollMovementType(ScrollRect.MovementType movementType)
        {
            if (_contentContainer != null)
            {
                var scrollRect = _contentContainer.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.movementType = movementType;
                }
            }
            return this;
        }

        public void DestroyModal()
        {
            if (UIManager.Instance != null && !string.IsNullOrEmpty(WindowName))
            {
                UIManager.Instance.DestroyModal(WindowName);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public UIGlobalModal SetResizable(bool isResizable)
        {
            _isResizable = isResizable;
            if (_isResizable)
            {
                EnsureResizeHandle();
                if (_resizeHandle != null)
                {
                    _resizeHandle.gameObject.SetActive(true);
                    _resizeHandle.SetAsLastSibling();
                }
            }
            else
            {
                if (_resizeHandle != null)
                {
                    _resizeHandle.gameObject.SetActive(false);
                }
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
                var img = _resizeHandle.GetComponent<Image>();
                if (img != null) img.sprite = _resizeHandleSprite;
            }
            return this;
        }

        public void OnResizeBegin(PointerEventData eventData)
        {
            if (!_isResizable) return;

            RectTransform rect = WindowRect;
            if (rect == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform parentRect = rect.parent as RectTransform;
            if (parentRect == null && canvas != null)
            {
                parentRect = canvas.transform as RectTransform;
            }
            if (parentRect == null) return;

            _isResizing = true;
            _startSize = rect.sizeDelta;
            _startPosition = rect.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.pressPosition,
                eventData.pressEventCamera,
                out _startPointerPos);
        }

        public void OnResize(PointerEventData eventData)
        {
            if (!_isResizing || !_isResizable) return;

            RectTransform rect = WindowRect;
            if (rect == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform parentRect = rect.parent as RectTransform;
            if (parentRect == null && canvas != null)
            {
                parentRect = canvas.transform as RectTransform;
            }
            if (parentRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 currentPointerPos))
            {
                Vector2 delta = currentPointerPos - _startPointerPos;

                float targetWidth = _startSize.x + delta.x;
                float targetHeight = _startSize.y - delta.y;

                float clampedWidth = Mathf.Clamp(targetWidth, _minSize.x, _maxSize.x);
                float clampedHeight = Mathf.Clamp(targetHeight, _minSize.y, _maxSize.y);

                float dW = clampedWidth - _startSize.x;
                float dH = clampedHeight - _startSize.y;

                Vector2 pivot = rect.pivot;
                rect.sizeDelta = new Vector2(clampedWidth, clampedHeight);
                rect.anchoredPosition = new Vector2(
                    _startPosition.x + pivot.x * dW,
                    _startPosition.y - (1f - pivot.y) * dH
                );

                LayoutRebuilder.MarkLayoutForRebuild(rect);
                OnResized?.Invoke(rect.sizeDelta);
            }
        }

        public void OnResizeEnd(PointerEventData eventData)
        {
            _isResizing = false;
        }
        #endregion

        #region [Private Methods]
        private void EnsureResizeHandle()
        {
            if (_resizeHandle != null)
            {
                var existingComp = _resizeHandle.GetComponent<UIModalResizeHandle>();
                if (existingComp == null)
                {
                    existingComp = _resizeHandle.gameObject.AddComponent<UIModalResizeHandle>();
                }
                existingComp.Initialize(this);
                return;
            }

            Transform existing = transform.Find("ResizeHandle");
            if (existing != null)
            {
                _resizeHandle = existing as RectTransform;
            }
            else
            {
                GameObject handleObj = new GameObject("ResizeHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                handleObj.transform.SetParent(transform, false);

                _resizeHandle = handleObj.GetComponent<RectTransform>();
                _resizeHandle.anchorMin = new Vector2(1f, 0f);
                _resizeHandle.anchorMax = new Vector2(1f, 0f);
                _resizeHandle.pivot = new Vector2(1f, 0f);
                _resizeHandle.sizeDelta = new Vector2(20f, 20f);
                _resizeHandle.anchoredPosition = new Vector2(-2f, 2f);

                var img = handleObj.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.35f);
                img.raycastTarget = true;
                img.sprite = _resizeHandleSprite != null ? _resizeHandleSprite : CreateResizeGripSprite();
            }

            var comp = _resizeHandle.GetComponent<UIModalResizeHandle>();
            if (comp == null)
            {
                comp = _resizeHandle.gameObject.AddComponent<UIModalResizeHandle>();
            }
            comp.Initialize(this);
        }

        private static Sprite CreateResizeGripSprite()
        {
            if (_cachedGripSprite != null) return _cachedGripSprite;

            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "ModalResizeGrip"
            };

            Color transparent = new Color(0f, 0f, 0f, 0f);
            Color white = Color.white;
            Color[] pixels = new Color[size * size];

            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int cornerDist = (size - 1 - x) + y;
                    if (cornerDist == 3 || cornerDist == 7 || cornerDist == 11)
                    {
                        pixels[y * size + x] = white;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedGripSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(1f, 0f));
            return _cachedGripSprite;
        }
        #endregion
    }
}
