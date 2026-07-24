using CoreSystem.Entity;
using CoreSystem;
using UnityEngine;

namespace CoreSystem.Data.Items.Effects
{
    [System.Serializable]
    public class AutoAttackModifierEffect : EquipmentEffect
    {
        #region [Variables/Fields]
        [SerializeField] private bool overrideType;
        [SerializeField] private AutoAttackType newType;

        [SerializeField] private bool overrideDamageType;
        [SerializeField] private DamageType newDamageType;

        [SerializeField] private bool overrideRange;
        [SerializeField] private float newRange;

        [SerializeField] private bool overrideProjectileAddress;
        [SerializeField] private string newProjectileAddress;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public override void Apply(Character player, ItemInstance instance = null)
        {
            if (overrideType) player.OnResolveAutoAttackType += ResolveType;
            if (overrideDamageType) player.OnResolveAutoAttackDamageType += ResolveDamageType;
            if (overrideRange) player.OnResolveAutoAttackRange += ResolveRange;
            if (overrideProjectileAddress) player.OnResolveProjectileAddress += ResolveProjectileAddress;
        }

        public override void Remove(Character player, ItemInstance instance = null)
        {
            if (overrideType) player.OnResolveAutoAttackType -= ResolveType;
            if (overrideDamageType) player.OnResolveAutoAttackDamageType -= ResolveDamageType;
            if (overrideRange) player.OnResolveAutoAttackRange -= ResolveRange;
            if (overrideProjectileAddress) player.OnResolveProjectileAddress -= ResolveProjectileAddress;
        }
        #endregion

        #region [Private Methods]
        private AutoAttackType ResolveType(AutoAttackType current) => newType;
        private DamageType ResolveDamageType(DamageType current) => newDamageType;
        private float ResolveRange(float current) => newRange;
        private string ResolveProjectileAddress(string current) => newProjectileAddress;
        #endregion
    }
}
