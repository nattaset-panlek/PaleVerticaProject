namespace CoreSystem.Enums
{
    #region [Public Enums]
    public enum WeaponType
    {
        None,
        Sword,
        Dagger,
        Blade,
        Axe,
        Mace,
        Staff,
        Wand,
        Spear,
        Bow
    }
    #endregion

    #region [Public Extensions]
    public static class WeaponTypeExtensions
    {
        public static bool IsEdgedWeapon(this WeaponType type)
        {
            return type == WeaponType.Sword ||
                   type == WeaponType.Dagger ||
                   type == WeaponType.Blade ||
                   type == WeaponType.Axe;
        }
    }
    #endregion
}
