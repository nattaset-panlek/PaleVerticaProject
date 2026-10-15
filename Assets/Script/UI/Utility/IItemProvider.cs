using CoreSystem.Data.Items;
using CoreSystem.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystem.UI.Utility
{
    public interface IItemProvider
    {
        ItemInstance GetItem();
        Sprite GetIcon();
    }
}
