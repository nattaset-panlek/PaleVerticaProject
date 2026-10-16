using UnityEngine;

namespace CoreSystem.UI
{
    public class UIModal : UIBaseView
    {
        #region [Variables/Fields]
        [SerializeField] private bool _closeOnOverlayClick = true;
        [SerializeField] private bool _closeOnEscape = true;
        #endregion

        #region [Properties]
        public bool CloseOnOverlayClick
        {
            get => _closeOnOverlayClick;
            set => _closeOnOverlayClick = value;
        }

        public bool CloseOnEscape
        {
            get => _closeOnEscape;
            set => _closeOnEscape = value;
        }
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        #endregion
    }
}
