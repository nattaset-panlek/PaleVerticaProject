using System.Collections.Generic;
using CoreSystem.Data.Items.Model;
using CoreSystem.Entity;
using CoreSystem.Enums;

namespace CoreSystem.Visuals.WeaponVfx
{
    public class WeaponVfxRequest
    {
        #region [Variables/Fields]
        public readonly List<WeaponVfxVariant> Variants = new List<WeaponVfxVariant>();
        #endregion

        #region [Properties]
        public Character Attacker { get; }
        public IEntity Target { get; }
        public Equipment Weapon { get; }
        public WeaponType WeaponType { get; }
        public bool IsRanged { get; }
        public ElementType Element { get; set; }
        public bool Cancelled { get; set; }
        #endregion

        #region [Public Methods]
        public WeaponVfxRequest(Character attacker, IEntity target, Equipment weapon, WeaponType weaponType, ElementType element, bool isRanged)
        {
            Attacker = attacker;
            Target = target;
            Weapon = weapon;
            WeaponType = weaponType;
            Element = element;
            IsRanged = isRanged;
        }
        #endregion
    }
}
