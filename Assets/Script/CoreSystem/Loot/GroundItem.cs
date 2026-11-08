using System;
using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Entity;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Loot
{
    public sealed class GroundItem : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float FallbackWorldSize = 0.6f;
        private static readonly List<GroundItem> _active = new List<GroundItem>();
        private static LootVisualSettings _settings;

        public static event Action<GroundItem> OnSpawned;
        public static event Action<GroundItem> OnRemoved;

        private Item _item;
        private int _quantity;
        private IEntity _owner;
        private float _spawnTime;
        #endregion

        #region [Properties]
        public static IReadOnlyList<GroundItem> Active => _active;
        public Item Item => _item;
        public int Quantity => _quantity;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _active.Add(this);
        }

        private void OnDestroy()
        {
            _active.Remove(this);
            OnRemoved?.Invoke(this);
        }
        #endregion

        #region [Public Methods]
        public static GroundItem Create(Item item, int quantity, Vector3 position, IEntity owner)
        {
            GameObject go = RuntimeRoot.CreateChild("GroundItem " + item.Name);
            go.transform.position = position;

            var ground = go.AddComponent<GroundItem>();
            ground._item = item;
            ground._quantity = quantity;
            ground._owner = owner;
            ground._spawnTime = Time.time;
            ground.BuildVisual();

            Destroy(go, DropTableDatabase.GroundLifetimeSeconds);
            OnSpawned?.Invoke(ground);
            return ground;
        }

        public bool CanBePickedUpBy(IEntity picker)
        {
            return LootFormula.CanPickUp(_owner, picker, Time.time - _spawnTime, DropTableDatabase.OwnerLockSeconds);
        }

        public bool TryPickUp(IEntity picker, PlayerInventory inventory)
        {
            if (inventory == null || !CanBePickedUpBy(picker)) return false;
            inventory.AddItem(_item, _quantity);
            Destroy(gameObject);
            return true;
        }
        #endregion

        #region [Private Methods]
        private void BuildVisual()
        {
            if (_settings == null) _settings = Resources.Load<LootVisualSettings>(LootVisualSettings.ResourcePath);

            GameObject prefab = _settings != null ? _settings.GetPrefab(_item.Id) : null;
            if (prefab != null)
            {
                Instantiate(prefab, transform, false);
                return;
            }

            Sprite icon = _item.Icon;
            if (icon == null) return;
            gameObject.AddComponent<SpriteRenderer>().sprite = icon;
            float largest = Mathf.Max(icon.bounds.size.x, icon.bounds.size.y);
            if (largest > 0f) transform.localScale = Vector3.one * (FallbackWorldSize / largest);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _active.Clear();
            _settings = null;
            OnSpawned = null;
            OnRemoved = null;
        }
        #endregion
    }
}
