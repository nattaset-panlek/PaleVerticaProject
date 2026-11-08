using System;
using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Enums;

namespace CoreSystem.Visuals.WeaponVfx
{
    [Serializable]
    public class WeaponVfxProfile
    {
        #region [Variables/Fields]
        public string Name = "Profile";
        public List<WeaponType> WeaponTypes = new List<WeaponType>();
        public CharacterTag ExcludedTags = CharacterTag.None;
        public WeaponVfxSelectMode SelectMode = WeaponVfxSelectMode.Cycle;
        public float ComboResetSeconds = 1.2f;
        public List<WeaponVfxVariant> Variants = new List<WeaponVfxVariant>();
        #endregion

        #region [Properties]
        public bool HasVariants => Variants != null && Variants.Count > 0;
        #endregion

        #region [Public Methods]
        public bool Matches(WeaponType type)
        {
            return WeaponTypes != null && WeaponTypes.Contains(type);
        }

        public bool Allows(Character attacker)
        {
            return attacker == null || (attacker.Tags & ExcludedTags) == 0;
        }
        #endregion
    }
}
