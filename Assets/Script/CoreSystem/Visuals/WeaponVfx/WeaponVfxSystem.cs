using System;
using System.Collections.Generic;
using CoreSystem.Data.Items.Model;
using CoreSystem.Entity;
using CoreSystem.Enums;
using UnityEngine;

namespace CoreSystem.Visuals.WeaponVfx
{
    public class WeaponVfxSystem
    {
        #region [Variables/Fields]
        private readonly Dictionary<object, int> _comboIndex = new Dictionary<object, int>();
        private readonly Dictionary<object, float> _lastPlayTime = new Dictionary<object, float>();
        #endregion

        #region [Properties]
        public event Action<WeaponVfxRequest> OnBeforePlay;
        public event Action<WeaponVfxRequest, GameObject> OnSpawned;
        #endregion

        #region [Public Methods]
        public void Play(Character attacker, IEntity target, bool isRanged)
        {
            if (attacker == null) return;
            WeaponVfxTable table = WeaponVfxTable.Load();
            if (table == null) return;

            Equipment weapon = attacker.GetEquippedWeapon();
            if (!WeaponVfxTable.TryResolveWeaponType(weapon, out WeaponType type)) return;

            var request = new WeaponVfxRequest(attacker, target, weapon, type, attacker.Element, isRanged);
            BuildVariants(table, request);
            OnBeforePlay?.Invoke(request);
            if (request.Cancelled || request.Variants.Count == 0) return;

            VfxColorRamp ramp = table.GetRamp(request.Element);
            for (int i = 0; i < request.Variants.Count; i++)
            {
                WeaponVfxVariant variant = request.Variants[i];
                if (variant == null) continue;
                if (request.IsRanged && variant.Anchor != WeaponVfxAnchor.Attacker) continue;
                GameObject instance = WeaponVfxFactory.Create(variant, request, ramp);
                if (instance != null) OnSpawned?.Invoke(request, instance);
            }
        }

        public void ResetCombo()
        {
            _comboIndex.Clear();
            _lastPlayTime.Clear();
        }
        #endregion

        #region [Private Methods]
        private void BuildVariants(WeaponVfxTable table, WeaponVfxRequest request)
        {
            WeaponVfxItemOverride itemOverride = request.Weapon != null ? table.GetItemOverride(request.Weapon.Id) : null;
            WeaponVfxProfile profile = table.GetProfile(request.WeaponType);
            if (profile != null && !profile.Allows(request.Attacker)) profile = null;

            if (itemOverride != null && itemOverride.Mode == WeaponVfxOverrideMode.Replace && itemOverride.HasVariants)
            {
                request.Variants.Add(Pick(itemOverride, itemOverride.Variants, itemOverride.SelectMode, profile != null ? profile.ComboResetSeconds : 1.2f));
                return;
            }

            if (profile != null && profile.HasVariants)
            {
                request.Variants.Add(Pick(profile, profile.Variants, profile.SelectMode, profile.ComboResetSeconds));
            }

            if (itemOverride != null && itemOverride.Mode == WeaponVfxOverrideMode.Add && itemOverride.HasVariants)
            {
                request.Variants.AddRange(itemOverride.Variants);
            }
        }

        private WeaponVfxVariant Pick(object owner, List<WeaponVfxVariant> variants, WeaponVfxSelectMode mode, float resetSeconds)
        {
            if (variants.Count == 1) return variants[0];
            if (mode == WeaponVfxSelectMode.Random) return variants[UnityEngine.Random.Range(0, variants.Count)];

            float now = Time.time;
            _comboIndex.TryGetValue(owner, out int index);
            if (_lastPlayTime.TryGetValue(owner, out float last) && now - last > resetSeconds) index = 0;
            WeaponVfxVariant picked = variants[index % variants.Count];
            _comboIndex[owner] = (index + 1) % variants.Count;
            _lastPlayTime[owner] = now;
            return picked;
        }
        #endregion
    }
}
