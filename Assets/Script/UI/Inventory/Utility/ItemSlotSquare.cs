using CoreSystem.Data.Items;
using UnityEngine;

namespace CoreSystem.UI.Inventory.Utility
{
    public class ItemSlotSquare : ItemSlotGlobal
    {
        #region [Variables/Fields]
        [SerializeField] private ItemSlotArtView _art;
        #endregion

        #region [Properties]
        public ItemSlotArtView Art => _art;
        #endregion

        #region [Public Methods]
        public override void UpdateUI(ItemInstance item)
        {
            base.UpdateUI(item);
            if (_art != null) _art.Show(ItemSlotDisplay.From(item));
        }
        #endregion
    }
}
