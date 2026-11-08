using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Data.Items;

namespace CoreSystem.UI.Utility
{
    public static class ItemInsertRegistry
    {
        #region [Variables/Fields]
        public const int AnySlot = -1;

        private static readonly List<IItemInsertTarget> _targets = new List<IItemInsertTarget>();

        public static event Action OnTargetsChanged;
        #endregion

        #region [Properties]
        public static IReadOnlyList<IItemInsertTarget> Targets => _targets;
        public static int Count => _targets.Count;
        #endregion

        #region [Public Methods]
        public static void Register(IItemInsertTarget target)
        {
            if (target == null || _targets.Contains(target)) return;
            _targets.Add(target);
            OnTargetsChanged?.Invoke();
        }

        public static void Unregister(IItemInsertTarget target)
        {
            if (target == null || !_targets.Remove(target)) return;
            OnTargetsChanged?.Invoke();
        }

        public static List<IItemInsertTarget> GetTargetsFor(ItemInstance item)
        {
            var result = new List<IItemInsertTarget>();
            if (item == null) return result;

            RemoveDestroyed();
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                IItemInsertTarget target = _targets[i];
                if (target.CanInsert(item, AnySlot)) result.Add(target);
            }
            return result;
        }

        public static List<ContextMenuItem> BuildContextMenuItems(ItemInstance item)
        {
            var items = new List<ContextMenuItem>();
            foreach (IItemInsertTarget target in GetTargetsFor(item))
            {
                IItemInsertTarget captured = target;
                items.Add(new ContextMenuItem(captured.GetInsertLabel(item), () => captured.TryInsert(item, AnySlot)));
            }
            return items;
        }

        public static void Clear()
        {
            if (_targets.Count == 0) return;
            _targets.Clear();
            OnTargetsChanged?.Invoke();
        }
        #endregion

        #region [Private Methods]
        private static void RemoveDestroyed()
        {
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                IItemInsertTarget target = _targets[i];
                if (target == null || (target is UnityEngine.Object unityObject && unityObject == null))
                {
                    _targets.RemoveAt(i);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            _targets.Clear();
            OnTargetsChanged = null;
        }
        #endregion
    }
}
