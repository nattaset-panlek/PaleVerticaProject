using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Shop
{
    [DisallowMultipleComponent]
    public class QuantityModalView : MonoBehaviour
    {
        #region [Variables/Fields]
        public const string AddFormat = "Add x{0}";

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private Button _minus;
        [SerializeField] private Button _plus;
        [SerializeField] private Button[] _presets = new Button[3];
        [SerializeField] private TMP_Text[] _presetLabels = new TMP_Text[3];
        [SerializeField] private RectTransform _presetActive;
        [SerializeField] private Color _presetInk = new Color(0.078f, 0.075f, 0.075f, 1f);
        [SerializeField] private Color _presetActiveInk = Color.white;
        [SerializeField] private Button _cancel;
        [SerializeField] private Button _close;
        [SerializeField] private Button _confirm;
        [SerializeField] private TMP_Text _confirmLabel;

        private int _quantity = 1;
        private int _max = 1;
        private Action<int> _onConfirm;

        public event Action OnClosed;
        #endregion

        #region [Properties]
        public int Quantity => _quantity;
        public int Max => _max;
        public bool IsOpen => gameObject.activeSelf;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_minus != null) _minus.onClick.AddListener(() => SetQuantity(_quantity - 1));
            if (_plus != null) _plus.onClick.AddListener(() => SetQuantity(_quantity + 1));
            for (int i = 0; i < _presets.Length; i++)
            {
                int preset = i;
                if (_presets[i] != null) _presets[i].onClick.AddListener(() => SetQuantity(PresetValue(preset)));
            }
            if (_cancel != null) _cancel.onClick.AddListener(Close);
            if (_close != null) _close.onClick.AddListener(Close);
            if (_confirm != null) _confirm.onClick.AddListener(Confirm);
        }
        #endregion

        #region [Public Methods]
        public void Open(string title, int max, int initial, Action<int> onConfirm)
        {
            _max = Mathf.Max(1, max);
            _onConfirm = onConfirm;
            if (_title != null) _title.text = title;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            SetQuantity(initial);
        }

        public void SetQuantity(int quantity)
        {
            _quantity = Mathf.Clamp(quantity, 1, _max);
            if (_value != null) _value.text = _quantity.ToString();
            if (_confirmLabel != null) _confirmLabel.text = string.Format(AddFormat, _quantity);
            if (_minus != null) _minus.interactable = _quantity > 1;
            if (_plus != null) _plus.interactable = _quantity < _max;
            ShowPreset(ActivePreset());
        }

        public int PresetValue(int preset)
        {
            switch (preset)
            {
                case 0: return 1;
                case 1: return Mathf.Max(1, _max / 2);
                default: return _max;
            }
        }

        public void Confirm()
        {
            Action<int> callback = _onConfirm;
            int quantity = _quantity;
            Close();
            callback?.Invoke(quantity);
        }

        public void Close()
        {
            _onConfirm = null;
            if (!gameObject.activeSelf) return;
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }
        #endregion

        #region [Private Methods]
        private int ActivePreset()
        {
            for (int i = _presets.Length - 1; i >= 0; i--)
            {
                if (PresetValue(i) == _quantity) return i;
            }
            return -1;
        }

        private void ShowPreset(int active)
        {
            if (_presetActive != null)
            {
                _presetActive.gameObject.SetActive(active >= 0);
                if (active >= 0 && _presets[active] != null)
                {
                    var target = (RectTransform)_presets[active].transform;
                    _presetActive.anchoredPosition = target.anchoredPosition;
                }
            }
            for (int i = 0; i < _presetLabels.Length; i++)
            {
                if (_presetLabels[i] != null) _presetLabels[i].color = i == active ? _presetActiveInk : _presetInk;
            }
        }
        #endregion
    }
}
