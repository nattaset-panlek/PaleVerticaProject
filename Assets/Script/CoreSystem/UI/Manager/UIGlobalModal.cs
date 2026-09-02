using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public class UIGlobalModal : UIModal
    {
        [Header("Global Modal Elements")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _closeButton;

        public RectTransform ContentContainer 
        {
            get 
            {
                if (_contentContainer != null)
                {
                    var scrollRect = _contentContainer.GetComponent<ScrollRect>();
                    if (scrollRect != null && scrollRect.content != null)
                    {
                        return scrollRect.content;
                    }
                }
                return _contentContainer;
            }
        }

        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => UIManager.Instance.Close(WindowName));
            }
        }

        public void SetTitle(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
        }

        public void ClearContent()
        {
            var container = ContentContainer;
            if (container == null) return;

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        public void SetWindowSize(Vector2 size)
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = size;
            }
        }

        public void SetScrollMovementType(ScrollRect.MovementType movementType)
        {
            if (_contentContainer != null)
            {
                var scrollRect = _contentContainer.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.movementType = movementType;
                }
            }
        }
    }
}
