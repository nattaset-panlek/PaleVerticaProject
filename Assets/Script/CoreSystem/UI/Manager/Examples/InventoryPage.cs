using UnityEngine;

namespace CoreSystem.UI.Examples
{
    public class InventoryPage : UIWindow
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public override void OnLoad(object data)
        {
            base.OnLoad(data);
        }

        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            SignalAction?.Invoke(data);
        }

        public override void OnClose()
        {
            base.OnClose();
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
