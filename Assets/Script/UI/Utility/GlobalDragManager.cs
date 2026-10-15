using UnityEngine;

namespace CoreSystem.UI.Utility
{
    public class GlobalDragManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private static GlobalDragManager _instance;

        [Header("Drag Visual Setup")]
        [SerializeField] private GlobalDragObject _dragObjectInstance;

        private Canvas _canvas;
        #endregion

        #region [Properties]
        public static GlobalDragManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<GlobalDragManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GlobalDragManager");
                        _instance = go.AddComponent<GlobalDragManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public static object Payload { get; private set; }
        public static MonoBehaviour Source { get; private set; }
        public static bool IsDragging => Payload != null;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_dragObjectInstance != null && _dragObjectInstance.gameObject != null)
            {
                Destroy(_dragObjectInstance.gameObject);
            }
            Payload = null;
            Source = null;
        }
        #endregion

        #region [Public Methods]
        public void StartDrag(MonoBehaviour source, object payload, Sprite icon, Vector2 screenPos, Vector2? customSize = null, Sprite raritySprite = null, Color? rarityColor = null)
        {
            Source = source;
            Payload = payload;

            EnsureCanvasAndVisual(source);

            if (_dragObjectInstance == null)
            {
                Debug.LogWarning("[GlobalDragManager] DragObject instance is not assigned! Please assign it in the Inspector or place one in the scene.");
                return;
            }

            _dragObjectInstance.transform.SetAsLastSibling();
            _dragObjectInstance.Setup(icon, raritySprite, rarityColor);

            Vector2 size = customSize ?? (source != null && source.GetComponent<RectTransform>() != null ? source.GetComponent<RectTransform>().sizeDelta : new Vector2(55, 55));
            _dragObjectInstance.SetSize(size);
            _dragObjectInstance.SetAlpha(0.75f);
            if (_dragObjectInstance.CanvasGroup != null)
            {
                _dragObjectInstance.CanvasGroup.blocksRaycasts = false;
                _dragObjectInstance.CanvasGroup.interactable = false;
            }

            _dragObjectInstance.SetActive(true);
            _dragObjectInstance.SetPosition(screenPos, _canvas);
        }

        public void UpdatePosition(Vector2 screenPos)
        {
            if (!IsDragging || _dragObjectInstance == null || _canvas == null) return;
            _dragObjectInstance.SetPosition(screenPos, _canvas);
        }

        public void FinishDrag()
        {
            if (_dragObjectInstance != null)
            {
                _dragObjectInstance.SetActive(false);
            }

            Payload = null;
            Source = null;
        }
        #endregion

        #region [Private Methods]
        private void EnsureCanvasAndVisual(MonoBehaviour source)
        {
            if (_canvas == null && source != null)
            {
                _canvas = source.GetComponentInParent<Canvas>();
            }
            if (_canvas == null)
            {
                _canvas = Object.FindAnyObjectByType<Canvas>();
            }

            if (_dragObjectInstance == null || _dragObjectInstance.gameObject == null)
            {
                _dragObjectInstance = Object.FindAnyObjectByType<GlobalDragObject>(FindObjectsInactive.Include);
            }
            else if (_canvas != null && _dragObjectInstance.transform.parent != _canvas.transform)
            {
                _dragObjectInstance.transform.SetParent(_canvas.transform, false);
            }
        }
        #endregion
    }
}
