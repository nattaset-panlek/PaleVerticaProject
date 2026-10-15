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
            Dialogue
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

        private Dictionary<CursorType, Texture2D> cursorTexturesCache = new Dictionary<CursorType, Texture2D>();
        private Dictionary<CursorType, Vector2> cursorHotSpotsCache = new Dictionary<CursorType, Vector2>();
        #endregion

        #region [Unity Methods]
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
                return sprite.texture; // Return original to avoid hard crash, though hardware cursor might ignore it
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
                // RaycastAll returns hits sorted by depth. results[0] is the top-most UI element.
                if (results[0].gameObject.GetComponentInParent<Selectable>() != null)
                {
                    ChangeCursor(CursorType.Interact);
                }
                else
                {
                    // Hovering over a UI background that isn't interactable
                    ChangeCursor(CursorType.Default);
                }

                return true; // We are over UI, block 2D world raycasts
            }

            return false;
        }

        private void HandleCursorState()
        {
            if (CheckUIHoverState())
            {
                return;
            }

            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePos);
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
        }
        #endregion
    }
}
