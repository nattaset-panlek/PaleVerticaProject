using UnityEngine;
using System.Collections.Generic;

namespace CoreSystem.UI
{
    [System.Serializable]
    public struct UICustomElementMapping
    {
        public string Key;
        public GameObject Target;
    }

    public class UICustomElement : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private List<UICustomElementMapping> _elements = new List<UICustomElementMapping>();
        #endregion

        #region [Public Methods]
        /// <summary>
        /// Retrieves the mapped GameObject by its assigned Key.
        /// </summary>
        public GameObject GetGameObject(string key)
        {
            foreach (var element in _elements)
            {
                if (element.Key == key) return element.Target;
            }
            Debug.LogWarning($"[UICustomElement] Cannot find element with key: {key} on {gameObject.name}");
            return null;
        }

        /// <summary>
        /// Retrieves the mapped GameObject by its assigned Key and immediately gets the requested Component.
        /// </summary>
        public T Get<T>(string key) where T : Component
        {
            GameObject obj = GetGameObject(key);
            if (obj != null)
            {
                T component = obj.GetComponent<T>();
                if (component == null)
                {
                    Debug.LogWarning($"[UICustomElement] GameObject with key '{key}' was found, but it does not have the component '{typeof(T).Name}'.");
                }
                return component;
            }
            return null;
        }
        #endregion
    }
}
