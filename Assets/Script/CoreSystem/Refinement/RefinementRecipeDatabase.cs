using System.Collections.Generic;
using CoreSystem.Data.Items.Model;
using CoreSystem.Enums;

namespace CoreSystem.Refinement
{
    public static class RefinementRecipeDatabase
    {
        #region [Variables/Fields]
        private static readonly Dictionary<int, RefinementRecipe> _uniqueItemRecipes = new Dictionary<int, RefinementRecipe>();
        private static readonly Dictionary<WeaponType, RefinementRecipe> _weaponRecipes = new Dictionary<WeaponType, RefinementRecipe>();
        private static readonly Dictionary<EquipmentSlot, RefinementRecipe> _armorRecipes = new Dictionary<EquipmentSlot, RefinementRecipe>();
        private static RefinementRecipe _defaultRecipe;
        private static bool _initialized = false;
        #endregion

        #region [Properties]
        public static bool IsInitialized => _initialized;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static void Initialize()
        {
            _uniqueItemRecipes.Clear();
            _weaponRecipes.Clear();
            _armorRecipes.Clear();

            var meleeRecipe = CreateRecipe("Archetype_Melee",
                new RefinementMaterialRequirement { ItemId = 5003, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5017, BaseQuantity = 1, QuantityScaleInterval = 7 });
            _weaponRecipes[WeaponType.Sword] = meleeRecipe;
            _weaponRecipes[WeaponType.Dagger] = meleeRecipe;
            _weaponRecipes[WeaponType.Blade] = meleeRecipe;
            _weaponRecipes[WeaponType.Axe] = meleeRecipe;
            _weaponRecipes[WeaponType.Mace] = meleeRecipe;

            var magicRecipe = CreateRecipe("Archetype_Magic",
                new RefinementMaterialRequirement { ItemId = 5002, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5016, BaseQuantity = 1, QuantityScaleInterval = 6 });
            _weaponRecipes[WeaponType.Staff] = magicRecipe;
            _weaponRecipes[WeaponType.Wand] = magicRecipe;

            var rangedRecipe = CreateRecipe("Archetype_Ranged",
                new RefinementMaterialRequirement { ItemId = 5001, BaseQuantity = 3, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5018, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5007, BaseQuantity = 1, QuantityScaleInterval = 8 });
            _weaponRecipes[WeaponType.Bow] = rangedRecipe;
            _weaponRecipes[WeaponType.Spear] = rangedRecipe;

            var armorRecipe = CreateRecipe("Archetype_Armor",
                new RefinementMaterialRequirement { ItemId = 5003, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5008, BaseQuantity = 1, QuantityScaleInterval = 6 });
            _armorRecipes[EquipmentSlot.Helm] = armorRecipe;
            _armorRecipes[EquipmentSlot.Armor] = armorRecipe;
            _armorRecipes[EquipmentSlot.Glove] = armorRecipe;

            var accRecipe = CreateRecipe("Archetype_Accessory",
                new RefinementMaterialRequirement { ItemId = 5006, BaseQuantity = 2, QuantityScaleInterval = 5 },
                new RefinementMaterialRequirement { ItemId = 5016, BaseQuantity = 1, QuantityScaleInterval = 5 });
            _armorRecipes[EquipmentSlot.Accessory] = accRecipe;
            _armorRecipes[EquipmentSlot.AccessoryLeft] = accRecipe;
            _armorRecipes[EquipmentSlot.AccessoryRight] = accRecipe;
            _armorRecipes[EquipmentSlot.Talisman] = accRecipe;

            RegisterUniqueRecipe(3015, CreateRecipe("Unique_DragonPendant",
                new RefinementMaterialRequirement { ItemId = 5016, BaseQuantity = 2, QuantityScaleInterval = 4 },
                new RefinementMaterialRequirement { ItemId = 5020, BaseQuantity = 1, QuantityScaleInterval = 8 },
                new RefinementMaterialRequirement { ItemId = 5014, BaseQuantity = 2, QuantityScaleInterval = 6 }));

            _defaultRecipe = CreateRecipe("Fallback_Default",
                new RefinementMaterialRequirement { ItemId = 5003, BaseQuantity = 2, QuantityScaleInterval = 5 });

            _initialized = true;
        }

        public static void RegisterUniqueRecipe(int itemId, RefinementRecipe recipe)
        {
            if (recipe == null) return;
            recipe.TargetItemId = itemId;
            _uniqueItemRecipes[itemId] = recipe;
        }

        public static RefinementRecipe GetRecipe(Equipment equipment)
        {
            if (!_initialized) Initialize();
            if (equipment == null) return _defaultRecipe;

            if (_uniqueItemRecipes.TryGetValue(equipment.Id, out var uniqueRecipe))
            {
                return uniqueRecipe;
            }

            if (equipment.WeaponType != WeaponType.None && _weaponRecipes.TryGetValue(equipment.WeaponType, out var wRecipe))
            {
                return wRecipe;
            }

            WeaponType inferred = InferWeaponType(equipment);
            if (inferred != WeaponType.None && _weaponRecipes.TryGetValue(inferred, out var infRecipe))
            {
                return infRecipe;
            }

            if (_armorRecipes.TryGetValue(equipment.Slot, out var aRecipe))
            {
                return aRecipe;
            }

            return _defaultRecipe;
        }
        #endregion

        #region [Private Methods]
        private static RefinementRecipe CreateRecipe(string id, params RefinementMaterialRequirement[] materials)
        {
            var recipe = new RefinementRecipe { RecipeId = id };
            if (materials != null)
            {
                recipe.Materials.AddRange(materials);
            }
            return recipe;
        }

        private static WeaponType InferWeaponType(Equipment equipment)
        {
            if (equipment == null) return WeaponType.None;
            string name = equipment.Name ?? string.Empty;
            if (name.IndexOf("Sword", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Sword;
            if (name.IndexOf("Dagger", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Dagger;
            if (name.IndexOf("Blade", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Blade;
            if (name.IndexOf("Axe", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Axe;
            if (name.IndexOf("Staff", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Staff;
            if (name.IndexOf("Wand", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Wand;
            if (name.IndexOf("Bow", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Bow;
            if (name.IndexOf("Spear", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Spear;
            if (name.IndexOf("Mace", System.StringComparison.OrdinalIgnoreCase) >= 0) return WeaponType.Mace;
            return WeaponType.None;
        }
        #endregion
    }
}
