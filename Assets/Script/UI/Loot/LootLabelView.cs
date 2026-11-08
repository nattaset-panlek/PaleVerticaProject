using CoreSystem.Loot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Loot
{
    public sealed class LootLabelView : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float HorizontalPadding = 16f;

        [SerializeField] private RectTransform _rect;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _button;

        private GroundItem _item;
        #endregion

        #region [Properties]
        public GroundItem Item => _item;
        public RectTransform Rect => _rect;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }
        #endregion

        #region [Public Methods]
        public void Bind(GroundItem item)
        {
            _item = item;
            _text.text = item.Quantity > 1 ? $"{item.Item.Name} x{item.Quantity}" : item.Item.Name;
            _text.color = item.Item.Rarity.GetColor();
            _rect.sizeDelta = new Vector2(_text.GetPreferredValues(_text.text).x + HorizontalPadding, _rect.sizeDelta.y);
        }
        #endregion

        #region [Private Methods]
        private void HandleClick()
        {
            LootManager.RequestPickup(_item);
        }
        #endregion
    }
}
