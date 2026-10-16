using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace CoreSystem.UI
{
    public class CursorManager : MonoBehaviour
    {
        #region [Variables/Fields]
        public enum CursorType
        {
            None = -1,
            Default,
            Attack,
            Interact,
            TextField,
            SizeVertical,
            SizeHorizontal,
            Information,
            CantInteract,
            Dialogue,
            ResizeHandle
        }

        [System.Serializable]
        private struct CursorState
        {
            public CursorType type;
            public Sprite cursorSprite;
            public Vector2 hotSpot;
        }

        [SerializeField] private List<CursorState> cursorStates;
        [SerializeField] private LayerMask attackableLayer;
        [SerializeField] private LayerMask interactableLayer;

        private Camera mainCamera;
        private CursorType currentCursorType = CursorType.None;
        private CursorType _overrideCursorType = CursorType.None;

        private Dictionary<CursorType, Texture2D> cursorTexturesCache = new Dictionary<CursorType, Texture2D>();
        private Dictionary<CursorType, Vector2> cursorHotSpotsCache = new Dictionary<CursorType, Vector2>();
        #endregion

        #region [Properties]
        public static CursorManager Instance { get; private set; }
        public CursorType CurrentCursorType => currentCursorType;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            InitializeCursorCache();
            ChangeCursor(CursorType.Default);
        }

        private void Update()
        {
            HandleCursorState();
        }
        #endregion

        #region [Public Methods]
        public void SetCursor(CursorType type)
        {
            _overrideCursorType = type;
            ChangeCursor(type);
        }

        public void ResetCursor()
        {
            _overrideCursorType = CursorType.None;
            ChangeCursor(CursorType.Default);
        }

        public void SetCursorOverride(CursorType type) => SetCursor(type);
        public void ClearCursorOverride() => ResetCursor();
        #endregion

        #region [Private Methods]
        private void InitializeCursorCache()
        {
            foreach (var state in cursorStates)
            {
                if (state.cursorSprite != null)
                {
                    cursorTexturesCache[state.type] = ExtractTextureFromSprite(state.cursorSprite);
                    cursorHotSpotsCache[state.type] = state.hotSpot;
                }
            }
        }

        private Texture2D ExtractTextureFromSprite(Sprite sprite)
        {
            if (!sprite.texture.isReadable)
            {
                Debug.LogError($"[CursorManager] The texture '{sprite.texture.name}' is not readable! Please select it in the Project window and check 'Read/Write Enabled' in the Inspector.");
                return sprite.texture;
            }

            Texture2D newText = new Texture2D((int)sprite.textureRect.width, (int)sprite.textureRect.height, TextureFormat.RGBA32, false);
            Color[] newColors = sprite.texture.GetPixels(
                (int)sprite.textureRect.x, 
                (int)sprite.textureRect.y, 
                (int)sprite.textureRect.width, 
                (int)sprite.textureRect.height);

            newText.SetPixels(newColors);
            newText.Apply();

            return newText;
        }

        private bool CheckUIHoverState()
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                if (results[0].gameObject.GetComponentInParent<CoreSystem.UI.UIModalResizeHandle>() != null)
                {
                    ChangeCursor(CursorType.ResizeHandle);
                }
                else if (results[0].gameObject.GetComponentInParent<Selectable>() != null)
                {
                    ChangeCursor(CursorType.Interact);
                }
                else
                {
                    ChangeCursor(CursorType.Default);
                }

                return true;
            }

            return false;
        }

        private void HandleCursorState()
        {
            if (_overrideCursorType != CursorType.None)
            {
                ChangeCursor(_overrideCursorType);
                return;
            }

            if (CheckUIHoverState())
            {
                return;
            }

            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Vector2 mouseWorldPosition = mainCamera != null ? (Vector2)mainCamera.ScreenToWorldPoint(mousePos) : Vector2.zero;
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition, attackableLayer | interactableLayer);

            if (hitCollider != null)
            {
                if (((1 << hitCollider.gameObject.layer) & attackableLayer) != 0)
                {
                    ChangeCursor(CursorType.Attack);
                    return;
                }

                if (((1 << hitCollider.gameObject.layer) & interactableLayer) != 0)
                {
                    ChangeCursor(CursorType.Interact);
                    return;
                }
            }

            ChangeCursor(CursorType.Default);
        }

        private void ChangeCursor(CursorType newType)
        {
            if (currentCursorType == newType) return;

            currentCursorType = newType;

            if (cursorTexturesCache.TryGetValue(newType, out Texture2D tex))
            {
                Cursor.SetCursor(tex, cursorHotSpotsCache[newType], CursorMode.Auto);
            }
            else
            {
                if (newType == CursorType.ResizeHandle)
                {
                    if (cursorTexturesCache.TryGetValue(CursorType.SizeHorizontal, out Texture2D fallbackTex))
                    {
                        Cursor.SetCursor(fallbackTex, cursorHotSpotsCache[CursorType.SizeHorizontal], CursorMode.Auto);
                        return;
                    }
                }

                if (cursorTexturesCache.TryGetValue(CursorType.Default, out Texture2D defaultTex))
                {
                    Cursor.SetCursor(defaultTex, cursorHotSpotsCache[CursorType.Default], CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }
        #endregion
    }
}
