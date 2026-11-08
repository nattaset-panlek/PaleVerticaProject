using System;
using CoreSystem.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.ActionProgress
{
    public class ActionProgressWindow : UIWindow, IEndDragHandler
    {
        #region [Variables/Fields]
        public const string DefaultWindowName = "Action Progress";

        [Header("Background")]
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _backgroundWithIcon;
        [SerializeField] private Sprite _backgroundNoIcon;

        [Header("Icon")]
        [SerializeField] private GameObject _iconRoot;
        [SerializeField] private Image _icon;

        [Header("Body")]
        [SerializeField] private RectTransform _body;
        [SerializeField] private float _bodyLeftWithIcon = 96f;
        [SerializeField] private float _bodyLeftNoIcon = 14f;
        [SerializeField] private TMP_Text _actionName;
        [SerializeField] private TMP_Text _actionType;
        [SerializeField] private TMP_Text _time;
        [SerializeField] private string _secondsSuffix = "<size=66%>s</size>";

        [Header("Progress")]
        [SerializeField] private RectTransform _fill;
        [SerializeField] private Image _fillImage;

        private ActionProgressRequest _request;
        private float _elapsed;
        private float _progress;
        private bool _finishing;
        #endregion

        #region [Properties]
        public ActionProgressRequest Request => _request;
        public float Progress => _progress;
        public bool IsRunning => _request != null && IsOpen;

        private static ActionProgressWindow Current => UIManager.Instance != null ? UIManager.Instance.GetView<ActionProgressWindow>(DefaultWindowName) : null;
        #endregion

        #region [Unity Methods]
        private void Update()
        {
            if (_request == null || !_request.IsTimed) return;
            _elapsed += _request.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetProgress(_elapsed / _request.Duration, _request.Duration - _elapsed);
            if (_elapsed >= _request.Duration) Finish(true);
        }
        #endregion

        #region [Public Methods]
        public static ActionProgressWindow Show(ActionProgressRequest request)
        {
            if (request == null || UIManager.Instance == null) return null;
            ActionProgressWindow current = Current;
            if (current != null && current.IsRunning) current.Finish(false);
            return UIManager.Instance.Open<ActionProgressWindow>(DefaultWindowName, request);
        }

        public static bool IsRunningFor(object owner)
        {
            ActionProgressWindow current = Current;
            return current != null && current.IsRunning && (owner == null || ReferenceEquals(current._request.Owner, owner));
        }

        public static void Report(object owner, float normalized, float remainingSeconds)
        {
            if (!IsRunningFor(owner)) return;
            Current.SetProgress(normalized, remainingSeconds);
        }

        public static void Complete(object owner)
        {
            if (IsRunningFor(owner)) Current.Finish(true);
        }

        public static void Cancel(object owner)
        {
            if (IsRunningFor(owner)) Current.Finish(false);
        }

        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            ApplySavedPosition();
            if (data is ActionProgressRequest request) Begin(request);
        }

        public override void OnClose()
        {
            if (_request != null && !_finishing)
            {
                ActionProgressRequest request = _request;
                _request = null;
                request.OnCancelled?.Invoke();
            }
            base.OnClose();
        }

        public void SetProgress(float normalized, float remainingSeconds)
        {
            _progress = Mathf.Clamp01(normalized);
            if (_fill != null) _fill.anchorMax = new Vector2(_progress, _fill.anchorMax.y);
            if (_time != null) _time.text = FormatTime(remainingSeconds, _secondsSuffix);
        }

        public void Preview(ActionProgressRequest request, float normalized, float remainingSeconds)
        {
            if (request == null) return;
            Begin(request);
            _request = null;
            SetProgress(normalized, remainingSeconds);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraggable || WindowRect == null) return;
            GameSetting.SetWindowPosition(WindowName, WindowRect.localPosition);
        }

        public static string FormatTime(float seconds, string suffix = "s")
        {
            return Mathf.Max(0f, seconds).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + suffix;
        }
        #endregion

        #region [Private Methods]
        private void Begin(ActionProgressRequest request)
        {
            _request = request;
            _elapsed = 0f;
            _finishing = false;

            bool hasIcon = request.HasIcon;
            if (_iconRoot != null) _iconRoot.SetActive(hasIcon);
            if (_icon != null && hasIcon) _icon.sprite = request.Icon;
            if (_background != null)
            {
                Sprite sprite = hasIcon ? _backgroundWithIcon : _backgroundNoIcon;
                if (sprite != null) _background.sprite = sprite;
            }
            if (_body != null) _body.offsetMin = new Vector2(hasIcon ? _bodyLeftWithIcon : _bodyLeftNoIcon, _body.offsetMin.y);

            if (_actionName != null) _actionName.text = request.ActionName;
            if (_actionType != null) _actionType.text = request.ActionType;
            if (_fillImage != null) _fillImage.color = ActionProgressPalette.Get(request.Color);
            SetProgress(0f, request.IsTimed ? request.Duration : 0f);
        }

        private void Finish(bool completed)
        {
            if (_request == null) return;
            ActionProgressRequest request = _request;
            _finishing = true;
            if (completed) SetProgress(1f, 0f);
            _request = null;
            CloseSelf();
            _finishing = false;
            if (completed) request.OnCompleted?.Invoke();
            else request.OnCancelled?.Invoke();
        }

        private void ApplySavedPosition()
        {
            if (WindowRect == null) return;
            if (GameSetting.TryGetWindowPosition(WindowName, out Vector2 position)) WindowRect.localPosition = position;
        }
        #endregion
    }
}
