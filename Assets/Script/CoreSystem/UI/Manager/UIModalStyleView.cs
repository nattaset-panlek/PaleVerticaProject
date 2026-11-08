using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    [DisallowMultipleComponent]
    public class UIModalStyleView : MonoBehaviour
    {
        #region [Variables/Fields]
        [Header("Default Style Parts")]
        [SerializeField] private GameObject[] _defaultParts = new GameObject[0];

        [Header("Mini Header Parts")]
        [SerializeField] private GameObject _miniHeaderRoot;
        [SerializeField] private TMP_Text _miniTitle;
        [SerializeField] private Button _miniCloseButton;
        [SerializeField] private Sprite _miniWindowSprite;
        [SerializeField] private Color _miniWindowColor = Color.white;
        [Tooltip("Content insets in Mini Header style: left, right, top, bottom.")]
        [SerializeField] private Vector4 _miniContentInsets = new Vector4(14f, 14f, 45f, 14f);

        [Header("Shared Targets")]
        [SerializeField] private Image _windowImage;
        [SerializeField] private RectTransform _contentContainer;

        private bool _defaultsCaptured;
        private Sprite _defaultSprite;
        private Color _defaultColor;
        private Image.Type _defaultType;
        private float _defaultPixelsPerUnit;
        private Vector2 _defaultOffsetMin;
        private Vector2 _defaultOffsetMax;
        private UIModalStyle _current = UIModalStyle.Default;
        #endregion

        #region [Properties]
        public UIModalStyle Current => _current;
        public Button MiniCloseButton => _miniCloseButton;
        public TMP_Text MiniTitle => _miniTitle;
        public GameObject MiniHeaderRoot => _miniHeaderRoot;
        public bool HasMiniHeader => _miniHeaderRoot != null;
        #endregion

        #region [Public Methods]
        public void Apply(UIModalStyle style)
        {
            CaptureDefaults();
            bool mini = style == UIModalStyle.MiniHeader && HasMiniHeader;
            _current = mini ? UIModalStyle.MiniHeader : UIModalStyle.Default;

            foreach (GameObject part in _defaultParts)
            {
                if (part != null) part.SetActive(!mini);
            }
            if (_miniHeaderRoot != null) _miniHeaderRoot.SetActive(mini);

            if (_windowImage != null)
            {
                if (mini && _miniWindowSprite != null)
                {
                    _windowImage.sprite = _miniWindowSprite;
                    _windowImage.type = Image.Type.Sliced;
                    _windowImage.pixelsPerUnitMultiplier = 1f;
                    _windowImage.color = _miniWindowColor;
                }
                else if (!mini)
                {
                    _windowImage.sprite = _defaultSprite;
                    _windowImage.type = _defaultType;
                    _windowImage.pixelsPerUnitMultiplier = _defaultPixelsPerUnit;
                    _windowImage.color = _defaultColor;
                }
            }

            if (_contentContainer != null)
            {
                if (mini)
                {
                    _contentContainer.offsetMin = new Vector2(_miniContentInsets.x, _miniContentInsets.w);
                    _contentContainer.offsetMax = new Vector2(-_miniContentInsets.y, -_miniContentInsets.z);
                }
                else
                {
                    _contentContainer.offsetMin = _defaultOffsetMin;
                    _contentContainer.offsetMax = _defaultOffsetMax;
                }
            }
        }

        public void SetTitle(string title)
        {
            if (_miniTitle != null) _miniTitle.text = title;
        }

        public void SetCloseVisible(bool visible)
        {
            if (_miniCloseButton != null) _miniCloseButton.gameObject.SetActive(visible);
        }

        public void RecaptureDefaults()
        {
            _defaultsCaptured = false;
            CaptureDefaults();
        }
        #endregion

        #region [Private Methods]
        private void CaptureDefaults()
        {
            if (_defaultsCaptured || _current != UIModalStyle.Default) return;
            _defaultsCaptured = true;
            if (_windowImage != null)
            {
                _defaultSprite = _windowImage.sprite;
                _defaultColor = _windowImage.color;
                _defaultType = _windowImage.type;
                _defaultPixelsPerUnit = _windowImage.pixelsPerUnitMultiplier;
            }
            if (_contentContainer != null)
            {
                _defaultOffsetMin = _contentContainer.offsetMin;
                _defaultOffsetMax = _contentContainer.offsetMax;
            }
        }
        #endregion
    }
}
