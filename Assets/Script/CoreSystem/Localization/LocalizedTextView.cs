using TMPro;
using UnityEngine;

namespace CoreSystem.Localization
{
    [DisallowMultipleComponent]
    public sealed class LocalizedTextView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private string _key;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private UnityEngine.UI.Text _legacyText;

        private object[] _args;
        #endregion

        #region [Properties]
        public string Key => _key;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_legacyText == null && _text == null) _legacyText = GetComponent<UnityEngine.UI.Text>();
        }

        private void OnEnable()
        {
            LocalizationEvents.OnLanguageChanged += HandleLanguageChanged;
            Refresh();
        }

        private void OnDisable()
        {
            LocalizationEvents.OnLanguageChanged -= HandleLanguageChanged;
        }
        #endregion

        #region [Public Methods]
        public void SetKey(string key, params object[] args)
        {
            _key = key;
            _args = args;
            Refresh();
        }

        public void Refresh()
        {
            if (string.IsNullOrEmpty(_key)) return;
            string value = LocalizationDatabase.Get(_key, _args);
            if (_text != null) _text.text = value;
            else if (_legacyText != null) _legacyText.text = value;
        }
        #endregion

        #region [Private Methods]
        private void HandleLanguageChanged(string language)
        {
            Refresh();
        }
        #endregion
    }
}
