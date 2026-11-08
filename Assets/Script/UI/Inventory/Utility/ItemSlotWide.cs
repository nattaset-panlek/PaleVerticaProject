using CoreSystem.Data.Items;
using UnityEngine;

namespace CoreSystem.UI.Inventory.Utility
{
    public class ItemSlotWide : ItemSlotGlobal
    {
        #region [Variables/Fields]
        [SerializeField] private ItemCardView _card;
        #endregion

        #region [Properties]
        public ItemCardView Card => _card;
        #endregion

        #region [Public Methods]
        public override void UpdateUI(ItemInstance item)
        {
            base.UpdateUI(item);
            if (_card != null) _card.Show(ItemCardDisplay.From(item));
        }
        #endregion
    }
}
