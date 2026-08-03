using UnityEngine;
using TMPro;
using CoreSystem.Skills;

namespace CoreSystem.UI.SkillManagePage
{
    public class ModelModifierFieldContainerComponent : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private TMP_Text _supportSkillNameText;
        [SerializeField] private TMP_Text _descriptionText;
        #endregion

        #region [Properties]
        public TMP_Text SupportSkillNameText => _supportSkillNameText;
        public TMP_Text DescriptionText => _descriptionText;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public void Setup(string name, string description)
        {
            if (_supportSkillNameText != null)
            {
                _supportSkillNameText.text = name;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = description;
            }
        }

        public void Setup(SupportModifier modifier)
        {
            if (modifier != null)
            {
                Setup(modifier.Name, modifier.Description);
            }
            else
            {
                Setup(string.Empty, string.Empty);
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
