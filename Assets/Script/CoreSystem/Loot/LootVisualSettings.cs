using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Loot
{
    [CreateAssetMenu(fileName = "LootVisualSettings", menuName = "Pale Vertica/Loot/Loot Visual Settings")]
    public class LootVisualSettings : ScriptableObject
    {
        #region [Variables/Fields]
        public const string ResourcePath = "Loot/LootVisualSettings";

        [Serializable]
        public class ItemVisual
        {
            public int ItemId;
            public GameObject Prefab;
        }

        [SerializeField] private GameObject _defaultPrefab;
        [SerializeField] private List<ItemVisual> _uniqueVisuals = new List<ItemVisual>();
        #endregion

        #region [Properties]
        public GameObject DefaultPrefab
        {
            get => _defaultPrefab;
            set => _defaultPrefab = value;
        }

        public List<ItemVisual> UniqueVisuals => _uniqueVisuals;
        #endregion

        #region [Public Methods]
        public GameObject GetPrefab(int itemId)
        {
            foreach (ItemVisual visual in _uniqueVisuals)
            {
                if (visual != null && visual.ItemId == itemId && visual.Prefab != null) return visual.Prefab;
            }
            return _defaultPrefab;
        }
        #endregion
    }
}
