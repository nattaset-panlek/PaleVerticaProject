using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    public class UIManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private static UIManager _instance;

        [Header("Registry")]
        [SerializeField] private UIWindowRegistrySO _registry;

        [Header("Containers")]
        [SerializeField] private RectTransform _windowContainer;
        [SerializeField] private RectTransform _modalContainer;
        [SerializeField] private GameObject _modalOverlay;

        private readonly Dictionary<string, UIBaseView> _instantiatedViews = new Dictionary<string, UIBaseView>();
        private readonly Dictionary<string, UISignal> _signals = new Dictionary<string, UISignal>();
        private readonly Stack<UIModal> _modalStack = new Stack<UIModal>();
        #endregion

        #region [Properties]
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<UIManager>();
                }
                return _instance;
            }
        }

        public RectTransform WindowContainer => _windowContainer;
        public RectTransform ModalContainer => _modalContainer;
        public UIWindowRegistrySO Registry => _registry;
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
            SetupModalOverlayButton();
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_modalStack.Count > 0)
                {
                    CloseTopModal();
                    return;
                }
            }

            if (_registry != null && _registry.Entries != null)
            {
                for (int i = 0; i < _registry.Entries.Count; i++)
                {
                    var entry = _registry.Entries[i];
                    if (entry != null && entry.IsKeyPressed())
                    {
                        Toggle(entry.WindowName);
                    }
                }
            }
        }
        #endregion

        #region [Public Methods]
        public UIBaseView Open(string windowName, object data = null)
        {
            if (string.IsNullOrEmpty(windowName)) return null;

            if (!_instantiatedViews.TryGetValue(windowName, out UIBaseView view) || view == null)
            {
                view = InstantiateView(windowName, data);
            }

            if (view == null)
            {
                Debug.LogWarning($"[UIManager] Failed to open view '{windowName}'. Not found in registry or prefab missing.");
                return null;
            }

            if (!view.IsInitialized)
            {
                view.OnLoad(data);
            }

            view.transform.SetAsLastSibling();
            view.gameObject.SetActive(true);
            view.OnOpen(data);

            if (view is UIModal modal)
            {
                if (!_modalStack.Contains(modal))
                {
                    _modalStack.Push(modal);
                }
                UpdateModalOverlay();
            }

            return view;
        }

        public T Open<T>(string windowName, object data = null) where T : UIBaseView
        {
            return Open(windowName, data) as T;
        }

        public void Toggle(string windowName, object data = null)
        {
            if (IsOpen(windowName))
            {
                Close(windowName);
            }
            else
            {
                Open(windowName, data);
            }
        }

        public T Toggle<T>(string windowName, object data = null) where T : UIBaseView
        {
            if (IsOpen(windowName))
            {
                Close(windowName);
                return null;
            }
            else
            {
                return Open<T>(windowName, data);
            }
        }

        public void Close(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;

            if (_instantiatedViews.TryGetValue(windowName, out UIBaseView view) && view != null && view.IsOpen)
            {
                view.OnClose();
                view.gameObject.SetActive(false);

                if (view is UIModal modal)
                {
                    RemoveModalFromStack(modal);
                    UpdateModalOverlay();
                }

                if (view.DestroyOnClose)
                {
                    _instantiatedViews.Remove(windowName);
                    Destroy(view.gameObject);
                }
            }
        }

        public void CloseTopModal()
        {
            while (_modalStack.Count > 0)
            {
                UIModal topModal = _modalStack.Pop();
                if (topModal != null && topModal.IsOpen)
                {
                    topModal.OnClose();
                    topModal.gameObject.SetActive(false);

                    if (topModal.DestroyOnClose)
                    {
                        _instantiatedViews.Remove(topModal.WindowName);
                        Destroy(topModal.gameObject);
                    }

                    UpdateModalOverlay();
                    break;
                }
            }
        }

        public void CloseAll()
        {
            List<string> keys = new List<string>(_instantiatedViews.Keys);
            foreach (var key in keys)
            {
                Close(key);
            }
            _modalStack.Clear();
            UpdateModalOverlay();
        }

        public UISignal SignalAction(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return null;

            if (!_signals.TryGetValue(windowName, out UISignal signal))
            {
                signal = new UISignal();
                _signals[windowName] = signal;
            }
            return signal;
        }

        public T GetView<T>(string windowName) where T : UIBaseView
        {
            if (_instantiatedViews.TryGetValue(windowName, out UIBaseView view))
            {
                return view as T;
            }
            return null;
        }

        public bool IsOpen(string windowName)
        {
            if (_instantiatedViews.TryGetValue(windowName, out UIBaseView view) && view != null)
            {
                return view.IsOpen;
            }
            return false;
        }
        #endregion

        #region [Private Methods]
        private UIBaseView InstantiateView(string windowName, object data)
        {
            if (_registry == null)
            {
                Debug.LogError("[UIManager] UIWindowRegistrySO is not assigned!");
                return null;
            }

            UIWindowEntry entry = _registry.GetEntry(windowName);
            if (entry == null || entry.Prefab == null)
            {
                return null;
            }

            Transform targetParent = entry.Type == UIType.Modal ? _modalContainer : _windowContainer;
            if (targetParent == null) targetParent = transform;

            GameObject obj = Instantiate(entry.Prefab, targetParent);

            UIBaseView view = obj.GetComponent<UIBaseView>();
            if (view == null)
            {
                Debug.LogError($"[UIManager] Prefab '{entry.Prefab.name}' for '{windowName}' missing UIBaseView component.");
                Destroy(obj);
                return null;
            }

            view.WindowName = entry.WindowName;
            _instantiatedViews[windowName] = view;

            return view;
        }

        private void RemoveModalFromStack(UIModal modal)
        {
            if (_modalStack.Count == 0) return;

            Stack<UIModal> tempStack = new Stack<UIModal>();
            while (_modalStack.Count > 0)
            {
                UIModal current = _modalStack.Pop();
                if (current != modal)
                {
                    tempStack.Push(current);
                }
            }

            while (tempStack.Count > 0)
            {
                _modalStack.Push(tempStack.Pop());
            }
        }

        private void UpdateModalOverlay()
        {
            if (_modalOverlay == null) return;

            bool hasActiveModal = false;
            foreach (var modal in _modalStack)
            {
                if (modal != null && modal.IsOpen)
                {
                    hasActiveModal = true;
                    break;
                }
            }

            _modalOverlay.SetActive(hasActiveModal);
            if (hasActiveModal)
            {
                _modalOverlay.transform.SetAsLastSibling();
                if (_modalStack.Count > 0 && _modalStack.Peek() != null)
                {
                    _modalStack.Peek().transform.SetAsLastSibling();
                }
            }
        }

        private void SetupModalOverlayButton()
        {
            if (_modalOverlay != null)
            {
                Button overlayBtn = _modalOverlay.GetComponent<Button>();
                if (overlayBtn != null)
                {
                    overlayBtn.onClick.AddListener(() =>
                    {
                        if (_modalStack.Count > 0)
                        {
                            UIModal top = _modalStack.Peek();
                            if (top != null && top.CloseOnOverlayClick)
                            {
                                CloseTopModal();
                            }
                        }
                    });
                }
            }
        }
        #endregion
    }

    public static class Manager
    {
        public static UIManager Instance => UIManager.Instance;
    }
}
