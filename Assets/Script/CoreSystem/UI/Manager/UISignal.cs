using System;

namespace CoreSystem.UI
{
    public class UISignal
    {
        #region [Variables/Fields]
        private Action<object> _onSignal;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public void Invoke(object data = null)
        {
            _onSignal?.Invoke(data);
        }

        public void AddListener(Action<object> listener)
        {
            _onSignal += listener;
        }

        public void RemoveListener(Action<object> listener)
        {
            _onSignal -= listener;
        }

        public static UISignal operator +(UISignal signal, Action<object> listener)
        {
            if (signal == null) signal = new UISignal();
            signal._onSignal += listener;
            return signal;
        }

        public static UISignal operator -(UISignal signal, Action<object> listener)
        {
            if (signal != null) signal._onSignal -= listener;
            return signal;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
