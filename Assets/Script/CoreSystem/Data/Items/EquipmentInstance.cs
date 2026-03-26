using System.Collections.Generic;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class EquipmentInstance : ItemInstance
    {
        public int Refine;
        public int Reforge;
        public bool IsEquipped;
        public List<Fragment> SocketedFragments = new List<Fragment>();

        public EquipmentInstance(long id, int itemId, int quantity = 1) : base(id, itemId, quantity)
        {
        }
    }
}
