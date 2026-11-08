using System;
using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Entity;
using CoreSystem.UI;
using UnityEngine;

namespace CoreSystem.Shop
{
    public static class MerchantFactory
    {
        #region [Variables/Fields]
        public const int ShopkeeperCharacterId = 2003;
        public const int MinWares = 6;
        public const int MaxWares = 12;

        private static int _nextId = 1;
        #endregion

        #region [Public Methods]
        public static ShopSession OpenShop(ShopDefinition shop, string windowName = MerchantShopSystem.DefaultWindowName)
        {
            if (shop == null || UIManager.Instance == null) return null;
            var session = new ShopSession(shop);
            UIManager.Instance.Open(windowName, session);
            return session;
        }

        public static void SpawnRandom(Vector3 position, Action<Character> onSpawned = null)
        {
            ShopDefinition shop = CreateRandomShop(new System.Random());
            ShopDatabase.Register(shop);
            Spawn(ShopkeeperCharacterId, position, shop.Id, onSpawned);
        }

        public static void Spawn(int characterId, Vector3 position, string shopId, Action<Character> onSpawned = null)
        {
            CharacterFactory.Spawn(characterId, position, null, character =>
            {
                if (character != null) AttachShop(character, shopId);
                onSpawned?.Invoke(character);
            });
        }

        public static MerchantShopSystem AttachShop(Character character, string shopId)
        {
            character.gameObject.name = $"[Runtime] {character.Name} ({shopId})";
            character.SetInteractable(true);
            MerchantShopSystem system = character.GetComponent<MerchantShopSystem>();
            if (system == null) system = character.gameObject.AddComponent<MerchantShopSystem>();
            system.Configure(character, shopId, "Shop", 1.5f);
            return system;
        }

        public static ShopDefinition CreateRandomShop(System.Random random)
        {
            var pool = new List<Item>();
            foreach (Item item in Database.GetItems)
            {
                if (item != null && item.Type != ItemType.Currency && item.Type != ItemType.Progression) pool.Add(item);
            }

            int id = _nextId++;
            var shop = new ShopDefinition { Id = $"random_merchant_{id}", Name = $"Merchant {id}" };
            int count = Math.Min(pool.Count, random.Next(MinWares, MaxWares + 1));
            for (int i = 0; i < count; i++)
            {
                int pick = random.Next(i, pool.Count);
                (pool[i], pool[pick]) = (pool[pick], pool[i]);
                shop.Items.Add(new ShopEntry
                {
                    ItemId = pool[i].Id,
                    Price = RandomPrice(pool[i].Rarity, random),
                    Stock = RandomStock(pool[i], random)
                });
            }
            return shop;
        }
        #endregion

        #region [Private Methods]
        private static int RandomPrice(ItemRarity rarity, System.Random random)
        {
            int baseCost;
            switch (rarity)
            {
                case ItemRarity.Legendary: baseCost = 40000; break;
                case ItemRarity.Unique: baseCost = 20000; break;
                case ItemRarity.Epic: baseCost = 8000; break;
                case ItemRarity.Rare: baseCost = 2000; break;
                case ItemRarity.Uncommon: baseCost = 500; break;
                default: baseCost = 100; break;
            }
            return baseCost + random.Next(0, baseCost);
        }

        private static int RandomStock(Item item, System.Random random)
        {
            if (!item.IsStackable) return random.Next(1, 4);
            return random.Next(0, 2) == 0 ? ShopEntry.Unlimited : random.Next(5, 31);
        }
        #endregion
    }
}
