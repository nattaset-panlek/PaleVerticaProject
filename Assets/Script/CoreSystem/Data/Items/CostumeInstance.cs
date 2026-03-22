using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class CostumeInstance : ItemInstance
    {
        public bool IsEquipped;

        public CostumeInstance(long id, int itemId, int quantity = 1) : base(id, itemId, quantity)
        {
        }
    }
}
