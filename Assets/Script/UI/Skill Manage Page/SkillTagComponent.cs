using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoreSystem.UI.SkillManagePage
{
    [AddComponentMenu("UI/Skill Manage/Skill Tag")]
    public class SkillTagComponent : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _type;
        #endregion

        #region [Properties]
        public Image Background
        {
            get => _background;
            set => _background = value;
        }

        public TMP_Text Type
        {
            get => _type;
            set => _type = value;
        }
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public void Setup(string tagText, Color? bgColor = null)
        {
            if (_type != null)
            {
                _type.text = tagText;
            }

            if (_background != null && bgColor.HasValue)
            {
                _background.color = bgColor.Value;
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
