using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CoreSystem.UI.Components
{
    /// <summary>
    /// A premium switch toggle component that provides smooth animations for visual states.
    /// This is designed to work with Unity's built-in Toggle component.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("UI/Components/Switch Toggle")]
    public class SwitchToggle : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform handleTransform;
        [SerializeField] private Image background;
        
        [Header("Colors")]
        [SerializeField] private Color onColor = new Color(0.298f, 0.850f, 0.392f); // Green
        [SerializeField] private Color offColor = new Color(0.913f, 0.913f, 0.921f); // Gray
        
        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        [Header("Events")]
        public UnityEvent<bool> onToggled;
        public UnityEvent onOn;
        public UnityEvent onOff;
        
        private Toggle _toggle;
        private Vector2 _handlePositionOn;
        private Vector2 _handlePositionOff;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            
            // Calculate positions based on current layout if not set
            SetupPositions();
            
            _toggle.onValueChanged.AddListener(OnToggleChanged);
            UpdateVisuals(_toggle.isOn, false);
        }

        private void SetupPositions()
        {
            float width = GetComponent<RectTransform>().rect.width;
            float handleWidth = handleTransform.rect.width;
            float margin = (GetComponent<RectTransform>().rect.height - handleWidth) / 2f;
            
            float posX = (width / 2) - (handleWidth / 2) - margin;
            _handlePositionOn = new Vector2(posX, 0);
            _handlePositionOff = new Vector2(-posX, 0);
        }

        private void OnToggleChanged(bool value)
        {
            UpdateVisuals(value, true);
            onToggled.Invoke(value);
            
            if (value) onOn.Invoke();
            else onOff.Invoke();
        }

        private void UpdateVisuals(bool value, bool animate)
        {
            if (handleTransform == null || background == null) return;

            Vector2 targetPos = value ? _handlePositionOn : _handlePositionOff;
            Color targetColor = value ? onColor : offColor;

            if (animate)
            {
                handleTransform.DOAnchorPos(targetPos, animationDuration).SetEase(moveEase);
                background.DOColor(targetColor, animationDuration);
            }
            else
            {
                handleTransform.anchoredPosition = targetPos;
                background.color = targetColor;
            }
        }

        private void OnDestroy()
        {
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}
