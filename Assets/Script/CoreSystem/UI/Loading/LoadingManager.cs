using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreSystem.UI.Loading
{
    public sealed class LoadingManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private const string DefaultMessage = "Loading";
        private const int OverlaySortingOrder = 5000;
        private const float FadeSpeed = 5f;
        private const float BarSpeed = 2.5f;
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 BarSize = new Vector2(640f, 6f);
        private static readonly Vector2 BarPosition = new Vector2(0f, 96f);
        private static readonly Vector2 MessagePosition = new Vector2(0f, 128f);
        private static LoadingManager _instance;

        private Canvas _canvas;
        private CanvasGroup _group;
        private RectTransform _barFill;
        private TextMeshProUGUI _message;
        private Coroutine _routine;
        private float _targetProgress;
        private float _shownProgress;
        private bool _wantVisible;
        #endregion

        #region [Properties]
        public static LoadingManager Instance => _instance;
        public static bool IsVisible => _instance != null && _instance._wantVisible;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            BuildView();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static LoadingManager Create()
        {
            return new GameObject("[Runtime] LoadingManager").AddComponent<LoadingManager>();
        }

        public static LoadingManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public static bool LoadScene(string sceneName, string message = null, Func<bool> keepVisibleUntil = null)
        {
            if (IsVisible) return false;
            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[LoadingManager] Scene '{sceneName}' is not in Build Settings.");
                return false;
            }

            LoadingManager manager = Ensure();
            manager.Show(message);
            manager.StartCoroutine(manager.RunLoad(sceneName, keepVisibleUntil));
            return true;
        }
        #endregion

        #region [Private Methods]
        private IEnumerator RunLoad(string sceneName, Func<bool> keepVisibleUntil)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!operation.isDone)
            {
                _targetProgress = LoadingFormula.NormalizeSceneProgress(operation.progress);
                yield return null;
            }

            _targetProgress = 1f;
            if (keepVisibleUntil != null) yield return new WaitUntil(keepVisibleUntil);
            _wantVisible = false;
        }

        private void Show(string message)
        {
            _message.text = string.IsNullOrEmpty(message) ? DefaultMessage : message;
            _targetProgress = 0f;
            _shownProgress = 0f;
            ApplyBar();
            _group.alpha = 1f;
            _canvas.enabled = true;
            _wantVisible = true;
            if (_routine == null) _routine = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            while (_wantVisible || _shownProgress < 1f || _group.alpha > 0f)
            {
                float dt = Time.unscaledDeltaTime;
                _shownProgress = LoadingFormula.StepToward(_shownProgress, _targetProgress, BarSpeed, dt);
                ApplyBar();

                if (!_wantVisible && _shownProgress >= 1f)
                {
                    _group.alpha = Mathf.MoveTowards(_group.alpha, 0f, FadeSpeed * dt);
                }
                yield return null;
            }

            _canvas.enabled = false;
            _routine = null;
        }

        private void ApplyBar()
        {
            _barFill.anchorMax = new Vector2(_shownProgress, 1f);
        }

        private void BuildView()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = OverlaySortingOrder;
            _canvas.enabled = false;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();
            _group = gameObject.AddComponent<CanvasGroup>();

            Image background = CreateImage("Background", transform, Color.black);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            Image track = CreateImage("Bar Track", transform, new Color(1f, 1f, 1f, 0.15f));
            PlaceBottomCenter(track.rectTransform, BarPosition, BarSize);

            Image fill = CreateImage("Bar Fill", track.transform, Color.white);
            _barFill = fill.rectTransform;
            _barFill.anchorMin = Vector2.zero;
            _barFill.anchorMax = new Vector2(0f, 1f);
            _barFill.offsetMin = Vector2.zero;
            _barFill.offsetMax = Vector2.zero;

            var textObject = new GameObject("Message", typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            _message = textObject.AddComponent<TextMeshProUGUI>();
            _message.fontSize = 22f;
            _message.alignment = TextAlignmentOptions.Center;
            _message.color = Color.white;
            _message.raycastTarget = false;
            PlaceBottomCenter(_message.rectTransform, MessagePosition, new Vector2(BarSize.x, 40f));
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void PlaceBottomCenter(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
