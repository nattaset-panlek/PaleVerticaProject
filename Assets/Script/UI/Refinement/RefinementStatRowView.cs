using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Refinement
{
    public class RefinementStatRowView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private Image _backgroundImage;
        #endregion

        #region [Properties]
        public TMP_Text Label => _label;
        public TMP_Text Value => _value;
        public Image BackgroundImage => _backgroundImage;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public void Bind(RefinementStatRowData data)
        {
            if (_label != null)
            {
                _label.text = data.Label;
            }

            if (_value != null)
            {
                _value.text = data.ValueFormatted;
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
