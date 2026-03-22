using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    /// <summary>
    /// A premium button component that provides smooth scaling animations on hover and click.
    /// This is designed to work with Unity's built-in Button component.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("UI/Components/Animated Button")]
    public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float clickScale = 0.95f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        private Vector3 _originalScale;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            transform.DOScale(_originalScale * hoverScale, animationDuration).SetEase(scaleEase);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(_originalScale, animationDuration).SetEase(scaleEase);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            transform.DOScale(_originalScale * clickScale, animationDuration).SetEase(Ease.OutSine);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            // Return to hover scale or original? Let's return to hover if mouse is still on it
            float target = RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera) ? hoverScale : 1.0f;
            transform.DOScale(_originalScale * target, animationDuration).SetEase(scaleEase);
        }
        
        private void OnDisable()
        {
            // Reset scale if disabled to avoid being stuck in scaled state
            transform.localScale = _originalScale;
        }
    }
}
