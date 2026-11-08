using CoreSystem.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Inventory.Utility
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ItemSlotArtView : MonoBehaviour
    {
        #region [Variables/Fields]
        public const float ArtSize = 128f;

        [SerializeField] private RectTransform _art;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _wedge;
        [SerializeField] private Image _border;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _socketRow;
        [SerializeField] private Image[] _sockets = new Image[0];
        [SerializeField] private Sprite _socketFilled;
        [SerializeField] private Sprite _socketEmpty;
        [SerializeField] private GameObject _starColumn;
        [SerializeField] private Image[] _stars = new Image[0];
        [SerializeField] private Color _starOn = Color.white;
        [SerializeField] private Color _starOff = new Color(0.2156863f, 0.2156863f, 0.2156863f, 1f);
        [SerializeField] private GameObject _advancement;
        [SerializeField] private TMP_Text _advancementValue;
        [SerializeField] private GameObject _quantity;
        [SerializeField] private TMP_Text _quantityValue;
        [SerializeField] private bool _hideFrameWhenEmpty;

        private ItemSlotDisplay _display = ItemSlotDisplay.Empty;
        #endregion

        #region [Properties]
        public ItemSlotDisplay Display => _display;
        public Image Icon => _icon;
        public Image Wedge => _wedge;
        public Image Border => _border;
        public RectTransform Art => _art;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            Fit();
        }

        private void OnRectTransformDimensionsChange()
        {
            Fit();
        }
        #endregion

        #region [Public Methods]
        public void Show(ItemSlotDisplay display)
        {
            _display = display;
            bool hasItem = display.Kind != ItemSlotKind.Empty;

            if (_frame != null) _frame.enabled = hasItem || !_hideFrameWhenEmpty;
            if (_wedge != null)
            {
                _wedge.enabled = hasItem;
                if (hasItem) _wedge.color = display.Rarity.GetColor();
            }
            if (_icon != null)
            {
                _icon.sprite = hasItem ? display.Icon : null;
                _icon.enabled = hasItem && display.Icon != null;
            }
            if (_border != null) _border.enabled = hasItem && display.Equipped;

            ShowSockets(display);
            ShowStars(display);
            ShowNumber(_advancement, _advancementValue, display.ShowAdvancement, display.Advancement);
            ShowNumber(_quantity, _quantityValue, display.ShowQuantity, display.Quantity);
        }

        public void Clear()
        {
            Show(ItemSlotDisplay.Empty);
        }

        public void Fit()
        {
            if (_art == null) return;
            Rect area = ((RectTransform)transform).rect;
            float side = Mathf.Min(area.width, area.height);
            float scale = side > 0f ? side / ArtSize : 1f;
            _art.localScale = new Vector3(scale, scale, 1f);
        }
        #endregion

        #region [Private Methods]
        private void ShowSockets(ItemSlotDisplay display)
        {
            bool visible = display.ShowSockets;
            if (_socketRow != null) _socketRow.SetActive(visible);
            for (int i = 0; i < _sockets.Length; i++)
            {
                Image socket = _sockets[i];
                if (socket == null) continue;
                bool shown = visible && i < display.SocketSlots;
                socket.gameObject.SetActive(shown);
                if (shown) socket.sprite = i < display.SocketFilled ? _socketFilled : _socketEmpty;
            }
        }

        private void ShowStars(ItemSlotDisplay display)
        {
            bool visible = display.ShowStars;
            if (_starColumn != null) _starColumn.SetActive(visible);
            if (!visible) return;
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null) _stars[i].color = i < display.Stars ? _starOn : _starOff;
            }
        }

        private static void ShowNumber(GameObject group, TMP_Text value, bool visible, int number)
        {
            if (group != null) group.SetActive(visible);
            if (visible && value != null) value.text = NumberFormatter.Format(number);
        }
        #endregion
    }
}
