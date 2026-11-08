using UnityEngine;
using UnityEngine.EventSystems;
using CoreSystem.Data.Items;

namespace CoreSystem.UI.Utility
{
    [DisallowMultipleComponent]
    public class ItemInsertSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        #region [Variables/Fields]
        [SerializeField] private MonoBehaviour _target;
        [SerializeField] private int _slotIndex = ItemInsertRegistry.AnySlot;

        private IItemInsertTarget _resolvedTarget;
        #endregion

        #region [Properties]
        public IItemInsertTarget Target => ResolveTarget();
        public int SlotIndex => _slotIndex;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            _target = GetComponentInParent<IItemInsertTarget>() as MonoBehaviour;
        }

        private void OnValidate()
        {
            if (_target != null && !(_target is IItemInsertTarget)) _target = null;
            _resolvedTarget = null;
        }
        #endregion

        #region [Public Methods]
        public void Setup(MonoBehaviour target, int slotIndex)
        {
            _target = target is IItemInsertTarget ? target : null;
            _slotIndex = slotIndex;
            _resolvedTarget = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            ItemInstance item = ResolveDraggedItem();
            IItemInsertTarget target = ResolveTarget();
            if (item == null || target == null) return;
            target.TryInsert(item, _slotIndex);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            ResolveTarget()?.ClearInsert(_slotIndex);
        }

        public static ItemInstance ResolveDraggedItem()
        {
            return ResolveItem(GlobalDragManager.Payload)
                ?? ResolveItem(ItemDragHandler.DraggedItem)
                ?? ResolveItem(ItemDragHandler.ActiveSource);
        }

        public static ItemInstance ResolveItem(object payload)
        {
            if (payload is ItemInstance item) return item;
            if (payload is IItemProvider provider) return provider.GetItem();
            return null;
        }
        #endregion

        #region [Private Methods]
        private IItemInsertTarget ResolveTarget()
        {
            if (_resolvedTarget != null) return _resolvedTarget;
            _resolvedTarget = _target as IItemInsertTarget ?? GetComponentInParent<IItemInsertTarget>();
            return _resolvedTarget;
        }
        #endregion
    }
}
