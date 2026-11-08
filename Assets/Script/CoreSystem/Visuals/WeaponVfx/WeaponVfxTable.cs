using System.Collections.Generic;
using CoreSystem.Data.Items.Model;
using CoreSystem.Enums;
using UnityEngine;

namespace CoreSystem.Visuals.WeaponVfx
{
    [CreateAssetMenu(fileName = "WeaponVfxTable", menuName = "Pale Vertica/VFX/Weapon VFX Table")]
    public class WeaponVfxTable : ScriptableObject
    {
        #region [Variables/Fields]
        public const string ResourcePath = "VFX/WeaponVfxTable";

        private static WeaponVfxTable _cached;
        private static bool _loadAttempted;

        [SerializeField] private List<WeaponVfxProfile> _profiles = new List<WeaponVfxProfile>();
        [SerializeField] private List<WeaponVfxItemOverride> _itemOverrides = new List<WeaponVfxItemOverride>();
        [SerializeField] private VfxColorRamp _defaultRamp = VfxColorRamp.Neutral;
        [SerializeField] private List<ElementVfxRamp> _elementRamps = new List<ElementVfxRamp>();
        #endregion

        #region [Properties]
        public IReadOnlyList<WeaponVfxProfile> Profiles => _profiles;
        public IReadOnlyList<WeaponVfxItemOverride> ItemOverrides => _itemOverrides;
        public VfxColorRamp DefaultRamp => _defaultRamp;
        #endregion

        #region [Public Methods]
        public static WeaponVfxTable Load()
        {
            if (_cached == null && !_loadAttempted)
            {
                _loadAttempted = true;
                _cached = Resources.Load<WeaponVfxTable>(ResourcePath);
                if (_cached == null) Debug.LogWarning("[WeaponVfxTable] Missing Resources/" + ResourcePath + ".asset; weapon attack VFX are disabled.");
            }
            return _cached;
        }

        public static bool TryResolveWeaponType(Equipment weapon, out WeaponType type)
        {
            if (weapon == null)
            {
                type = WeaponType.None;
                return true;
            }
            if (weapon.WeaponType != WeaponType.None)
            {
                type = weapon.WeaponType;
                return true;
            }
            if (weapon.IsEdgedWeapon())
            {
                type = WeaponType.Sword;
                return true;
            }
            type = WeaponType.None;
            return false;
        }

        public WeaponVfxProfile GetProfile(WeaponType type)
        {
            for (int i = 0; i < _profiles.Count; i++)
            {
                if (_profiles[i] != null && _profiles[i].Matches(type)) return _profiles[i];
            }
            return null;
        }

        public WeaponVfxItemOverride GetItemOverride(int itemId)
        {
            for (int i = 0; i < _itemOverrides.Count; i++)
            {
                if (_itemOverrides[i] != null && _itemOverrides[i].ItemId == itemId) return _itemOverrides[i];
            }
            return null;
        }

        public VfxColorRamp GetRamp(ElementType element)
        {
            for (int i = 0; i < _elementRamps.Count; i++)
            {
                if (_elementRamps[i].Element == element) return _elementRamps[i].Ramp;
            }
            return _defaultRamp;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _cached = null;
            _loadAttempted = false;
        }
        #endregion
    }
}
