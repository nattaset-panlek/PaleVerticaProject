using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CoreSystem.Utile
{
    public sealed class AttributedTypeRegistry<TKey, TBase, TAttribute>
        where TBase : class
        where TAttribute : Attribute
    {
        #region [Variables/Fields]
        private readonly Func<TAttribute, TKey> _keySelector;
        private Dictionary<TKey, Type> _types;
        #endregion

        #region [Properties]
        public IReadOnlyDictionary<TKey, Type> Types
        {
            get
            {
                EnsureBuilt();
                return _types;
            }
        }
        #endregion

        #region [Public Methods]
        public AttributedTypeRegistry(Func<TAttribute, TKey> keySelector)
        {
            _keySelector = keySelector;
        }

        public TBase Create(TKey key)
        {
            EnsureBuilt();
            return key != null && _types.TryGetValue(key, out var type) ? (TBase)Activator.CreateInstance(type) : null;
        }

        public bool Contains(TKey key)
        {
            EnsureBuilt();
            return key != null && _types.ContainsKey(key);
        }
        #endregion

        #region [Private Methods]
        private void EnsureBuilt()
        {
            if (_types != null) return;

            var types = new Dictionary<TKey, Type>();
            foreach (var type in GetLoadableTypes(typeof(TBase).Assembly))
            {
                if (type == null || type.IsAbstract || !typeof(TBase).IsAssignableFrom(type)) continue;

                var attribute = type.GetCustomAttribute<TAttribute>(false);
                if (attribute == null) continue;

                var key = _keySelector(attribute);
                if (types.TryGetValue(key, out var existing))
                {
                    Debug.LogError($"[{typeof(TBase).Name} Registry] Duplicate key '{key}' on {type.FullName} and {existing.FullName}. Keeping {existing.FullName}.");
                    continue;
                }

                types[key] = type;
            }

            _types = types;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }
        #endregion
    }
}
