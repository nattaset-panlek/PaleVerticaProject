using System;
using System.Collections.Generic;

namespace CoreSystem.Visuals.WeaponVfx
{
    [Serializable]
    public class WeaponVfxItemOverride
    {
        #region [Variables/Fields]
        public int ItemId;
        public WeaponVfxOverrideMode Mode = WeaponVfxOverrideMode.Add;
        public WeaponVfxSelectMode SelectMode = WeaponVfxSelectMode.Cycle;
        public List<WeaponVfxVariant> Variants = new List<WeaponVfxVariant>();
        #endregion

        #region [Properties]
        public bool HasVariants => Variants != null && Variants.Count > 0;
        #endregion
    }
}
