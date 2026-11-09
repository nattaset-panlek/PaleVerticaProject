using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Quests
{
    public sealed class QuestRowSlot : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float CompletedAlpha = 0.45f;

        private static readonly Color DarkText = new Color32(0x14, 0x13, 0x13, 0xFF);
        private static readonly Color CompletedText = new Color32(0x61, 0x61, 0x61, 0xFF);

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _normalBackground;
        [SerializeField] private GameObject _selectedBackground;
        [SerializeField] private GameObject _trackedBadge;
        [SerializeField] private GameObject _darkStar;
        [SerializeField] private GameObject _whiteStar;
        [SerializeField] private CanvasGroup _group;
        #endregion

        #region [Public Methods]
        public void Bind(string title, bool selected, bool tracked, bool completed, Action onClick)
        {
            _label.text = title;
            _label.color = selected ? Color.white : completed ? CompletedText : DarkText;
            _normalBackground.SetActive(!selected);
            _selectedBackground.SetActive(selected);
            _darkStar.SetActive(!selected);
            _whiteStar.SetActive(selected);
            _trackedBadge.SetActive(tracked);
            _group.alpha = completed && !selected ? CompletedAlpha : 1f;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }
        #endregion
    }
}
