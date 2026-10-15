using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Utility
{
    public class GlobalDragObject : MonoBehaviour
    {
        #region [Variables/Fields]
        [Header("UI References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _rarityObj;
        [SerializeField] private Image _rarityImage;
        [SerializeField] private Image _objectImage;
        #endregion

        #region [Properties]
        public RectTransform RectTransform => _rectTransform;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public Image ObjectImage => _objectImage;
        public Image RarityImage => _rarityImage;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }
        #endregion

        #region [Public Methods]
        public void InitializeReferences(RectTransform rect, CanvasGroup group, GameObject rarityObj, Image rarityImg, Image objImg)
        {
            _rectTransform = rect;
            _canvasGroup = group;
            _rarityObj = rarityObj;
            _rarityImage = rarityImg;
            _objectImage = objImg;
        }

        public void Setup(Sprite icon, Sprite raritySprite = null, Color? rarityColor = null)
        {
            if (_objectImage != null)
            {
                _objectImage.sprite = icon;
                _objectImage.enabled = (icon != null);
            }

            bool hasRarity = (raritySprite != null || rarityColor.HasValue);
            if (_rarityObj != null)
            {
                _rarityObj.SetActive(hasRarity);
            }
            if (_rarityImage != null && hasRarity)
            {
                if (raritySprite != null) _rarityImage.sprite = raritySprite;
                _rarityImage.color = rarityColor ?? Color.white;
            }
        }

        public void SetPosition(Vector2 screenPos, Canvas canvas)
        {
            if (_rectTransform == null || canvas == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos))
            {
                _rectTransform.localPosition = localPos;
            }
        }

        public void SetSize(Vector2 size)
        {
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = size;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
