using UnityEngine;
using CoreSystem.Data.Items;
using System.Collections.Generic;

namespace CoreSystem.UI.Inventory
{
    public class ItemInformationManager : MonoBehaviour
    {
        public static ItemInformationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private GameObject windowPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private Vector2 stackOffset = new Vector2(30, -30);
        [SerializeField] private Vector2 initialPosition = new Vector2(0, 0);

        private int _openWindowCount = 0;
        private Dictionary<ItemInstance, ItemInformationWindow> _activeWindows = new Dictionary<ItemInstance, ItemInformationWindow>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Open(ItemInstance instance)
        {
            if (instance == null) return;

            // Check if window for this instance is already open
            if (_activeWindows.TryGetValue(instance, out var existingWindow))
            {
                if (existingWindow != null)
                {
                    // Bring to front
                    existingWindow.transform.SetAsLastSibling();
                    return;
                }
                else
                {
                    // Clean up null reference (destroyed window)
                    _activeWindows.Remove(instance);
                }
            }

            if (windowPrefab == null)
            {
                Debug.LogError("[ItemInformationManager] Window Prefab is not assigned!");
                return;
            }

            GameObject go = Instantiate(windowPrefab, container != null ? container : transform);
            var window = go.GetComponent<ItemInformationWindow>();
            
            if (window != null)
            {
                // Set position with offset
                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = initialPosition + (stackOffset * (_openWindowCount % 10));
                }

                window.Setup(instance);
                _activeWindows[instance] = window;
                _openWindowCount++;
            }
            else
            {
                Debug.LogError("[ItemInformationManager] Prefab does not have ItemInformationWindow component!");
                Destroy(go);
            }
        }

        public void OnWindowClosed(ItemInstance instance)
        {
            if (instance != null)
            {
                _activeWindows.Remove(instance);
            }
        }

        public void CloseAll()
        {
            foreach (var kvp in _activeWindows)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _activeWindows.Clear();
            _openWindowCount = 0;
        }
    }
}

