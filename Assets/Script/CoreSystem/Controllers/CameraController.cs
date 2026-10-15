using UnityEngine;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;

namespace CoreSystem.Controllers
{
    /// <summary>
    /// Versatile 2D Camera Controller with Smooth Follow, Zoom, and Screen Shake.
    /// Supports dynamic target switching and special skill effects.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Follow Settings")]
        public Transform Target;
        public Vector2 FollowOffset = Vector2.zero;
        [Range(0, 1)] public float SmoothSpeed = 0.125f;
        public bool UseFixedUpdate = false;

        [Header("Zoom Settings")]
        public float MinZoom = 3f;
        public float MaxZoom = 12f;
        public float DefaultZoom = 6f;
        public float ZoomSensitivity = 4f;
        public float ZoomSmoothSpeed = 8f;

        [Header("Boundaries (Optional)")]
        public bool UseBounds = false;
        public Vector2 MinBounds;
        public Vector2 MaxBounds;

        private Camera _cam;
        private float _targetZoom;
        private InputSystem_Actions _inputActions;
        
        // Shake States
        private float _shakeTimer;
        private float _shakeAmount;
        private float _shakeDuration;
        private Vector3 _shakeOffset;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _cam = GetComponent<Camera>();
            _targetZoom = DefaultZoom;
            if (_cam != null) _cam.orthographicSize = DefaultZoom;
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
            PlayerCore.OnPlayerChanged += HandlePlayerChanged;
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
            PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
        }

        private void Start()
        {
            if (Target == null && PlayerCore.TryGetPlayer(out var player)) Target = player.transform;
        }

        private void HandlePlayerChanged(Character player)
        {
            if (player != null) Target = player.transform;
        }

        private void Update()
        {
            HandleZoomInput();
            UpdateShake(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (!UseFixedUpdate) PerformFollow(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (UseFixedUpdate) PerformFollow(Time.fixedDeltaTime);
        }

        private void HandleZoomInput()
        {
            float scroll = 0f;
            if (_inputActions != null)
            {
                float rawScroll = _inputActions.UI.ScrollWheel.ReadValue<Vector2>().y;
                if (Mathf.Abs(rawScroll) > 0.01f)
                {
                    scroll = Mathf.Sign(rawScroll) * 0.1f;
                }
            }

            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetZoom = Mathf.Clamp(_targetZoom - scroll * ZoomSensitivity, MinZoom, MaxZoom);
            }

            if (_cam != null && !Mathf.Approximately(_cam.orthographicSize, _targetZoom))
            {
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, Time.deltaTime * ZoomSmoothSpeed);
            }
        }

        private Vector2 _currentOffset;
        private Vector2 _targetOffset;

        private void PerformFollow(float dt)
        {
            if (Target == null) return;

            // Smooth Offset Transition
            _currentOffset = Vector2.Lerp(_currentOffset, _targetOffset, dt * 5f);

            Vector3 desiredPosition = new Vector3(
                Target.position.x + _currentOffset.x,
                Target.position.y + _currentOffset.y,
                transform.position.z 
            );

            // Apply Shake Offset
            desiredPosition += _shakeOffset;

            if (UseBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, MinBounds.x, MaxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, MinBounds.y, MaxBounds.y);
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
        }

        private void UpdateShake(float dt)
        {
            if (_shakeTimer > 0)
            {
                // Simple linear decay for smoother shake feel
                float factor = _shakeDuration > 0 ? (_shakeTimer / _shakeDuration) : 1f;
                _shakeOffset = (Vector3)Random.insideUnitCircle * (_shakeAmount * factor);
                _shakeTimer -= dt;
            }
            else
            {
                _shakeOffset = Vector3.zero;
            }
        }

        #region Public API for Skills/Events
        
        public void ScreenShake(float duration, float amount)
        {
            _shakeTimer = duration;
            _shakeDuration = duration;
            _shakeAmount = amount;
        }

        public void SetTarget(Transform newTarget) => Target = newTarget;

        public void SetZoom(float zoomLevel) => _targetZoom = Mathf.Clamp(zoomLevel, MinZoom, MaxZoom);

        public void SetOffset(Vector2 offset) => _targetOffset = offset;
        
        public void ResetOffset() => _targetOffset = Vector2.zero;

        #endregion
    }
}
