using UnityEngine;

namespace CoreSystem.UI
{
    /// <summary>
    /// Makes a UI element follow a world-space target with an offset.
    /// Implements auto-hiding (SetActive false on a root container) when the target is outside the camera frustum.
    /// </summary>
    public class UIFollowTarget : MonoBehaviour
    {
        [Header("Targeting")]
        public Transform Target;
        public Vector3 Offset = new Vector3(0, 2f, 0);

        [Header("Frustum Culling")]
        [Tooltip("Disable this GameObject when the target is off-screen. To keep this script running, point it to a child container.")]
        public GameObject VisualRoot;
        
        [Tooltip("Additional padding for the viewport check. Range 0 to 1. Negative expands the check area.")]
        public float FrustumPadding = -0.05f;

        [Header("Optimization")]
        [Tooltip("If true, updates only when camera moves or target moves. (Coming soon optimization)")]
        public bool SlowUpdate = false;

        private RectTransform _rectTransform;
        private Camera _mainCam;
        private bool _isVisible = true;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _mainCam = Camera.main;

            // Auto-assign first child as VisualRoot if not set
            if (VisualRoot == null && transform.childCount > 0)
            {
                VisualRoot = transform.GetChild(0).gameObject;
            }
        }

        private void LateUpdate()
        {
            if (Target == null || _mainCam == null) return;

            // 1. Calculate world and screen positions
            Vector3 worldTargetPos = Target.position + Offset;
            Vector3 viewportPos = _mainCam.WorldToViewportPoint(worldTargetPos);

            // 2. Check if the target is within the camera frustum
            // Z > 0 ensures it's in front of the camera
            bool inCamera = viewportPos.z > 0 && 
                            viewportPos.x >= -FrustumPadding && viewportPos.x <= 1f + FrustumPadding && 
                            viewportPos.y >= -FrustumPadding && viewportPos.y <= 1f + FrustumPadding;

            // 3. Handle Visibility Toggle
            if (inCamera != _isVisible)
            {
                _isVisible = inCamera;
                if (VisualRoot != null)
                {
                    VisualRoot.SetActive(_isVisible);
                }
                else
                {
                    // If no VisualRoot, we just disable the specific components or logic here 
                    // but we do NOT disable 'gameObject' because this script would stop running.
                }
            }

            // 4. Update Position if visible
            if (_isVisible)
            {
                // Note: This works for Canvas Screen Space Overlay/Camera.
                // For World Space Canvas, simple 'transform.position = worldTargetPos' is better.
                Vector3 screenPos = _mainCam.WorldToScreenPoint(worldTargetPos);
                transform.position = screenPos;
            }
        }

        /// <summary>
        /// Updates the target transform during runtime.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            Target = newTarget;
        }

        /// <summary>
        /// Set a specific offset.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            Offset = newOffset;
        }
    }
}
