using System;
using UnityEngine;
using CoreSystem.Utility;

namespace CoreSystem.UI.SystemMenu
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class SystemMenuController : MonoBehaviour
    {
        #region [Variables/Fields]
        public const string RuntimeName = "SystemMenuController";

        private static SystemMenuController _instance;
        private readonly SystemMenuWindow _menu = new SystemMenuWindow();

        public static event Action<SystemMenuController> OnReady;
        #endregion

        #region [Properties]
        public static SystemMenuController Instance => _instance;
        public static bool IsReady => _instance != null;
        public SystemMenuWindow Menu => _menu;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            UIManager.OnEscapeUnhandled += HandleEscape;
            OnReady?.Invoke(this);
        }

        private void OnDestroy()
        {
            UIManager.OnEscapeUnhandled -= HandleEscape;
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static SystemMenuController Create()
        {
            if (_instance != null) return _instance;
            var go = RuntimeRoot.CreateChild(RuntimeName);
            return go.AddComponent<SystemMenuController>();
        }

        public void ToggleMenu()
        {
            if (UIManager.Instance == null) return;
            _menu.Toggle();
        }
        #endregion

        #region [Private Methods]
        private void HandleEscape()
        {
            ToggleMenu();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Create();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnReady = null;
        }
        #endregion
    }
}
