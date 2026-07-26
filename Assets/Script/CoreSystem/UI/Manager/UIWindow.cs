using UnityEngine;
using UnityEngine.Events;

namespace CoreSystem.UI
{
    public class UIWindow : UIBaseView
    {
        #region [Variables/Fields]
        [SerializeField] private UnityEvent _onLoad;
        [SerializeField] private UnityEvent _onOpen;
        [SerializeField] private UnityEvent _onClose;
        #endregion

        #region [Properties]
        public UnityEvent OnLoadEvent => _onLoad;
        public UnityEvent OnOpenEvent => _onOpen;
        public UnityEvent OnCloseEvent => _onClose;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public override void OnLoad(object data)
        {
            base.OnLoad(data);
            _onLoad?.Invoke();
        }

        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            _onOpen?.Invoke();
        }

        public override void OnClose()
        {
            base.OnClose();
            _onClose?.Invoke();
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
