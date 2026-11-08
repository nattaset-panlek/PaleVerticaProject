using CoreSystem.Entity;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Visuals.WeaponVfx
{
    public static class WeaponVfxFactory
    {
        #region [Public Methods]
        public static GameObject Create(WeaponVfxVariant variant, WeaponVfxRequest request, VfxColorRamp elementRamp)
        {
            if (variant == null || !variant.IsValid || request == null || request.Attacker == null) return null;

            Vector3 origin = request.Attacker.transform.position;
            Vector3 targetCenter = GetTargetCenter(request.Target, origin);
            Vector3 aim = targetCenter - origin;
            aim.z = 0f;
            if (aim.sqrMagnitude < 0.0001f)
            {
                aim = request.Attacker.FacingDirection != Vector3.zero ? request.Attacker.FacingDirection : Vector3.right;
            }
            aim.Normalize();

            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            Quaternion rotation = variant.AlignToAim ? Quaternion.AngleAxis(angle, Vector3.forward) : Quaternion.identity;
            bool mirror = variant.AlignToAim && variant.MirrorWhenAimingLeft && aim.x < 0f;
            bool flipY = mirror ^ variant.FlipVertical;
            Vector3 signedScale = new Vector3(variant.Scale, flipY ? -variant.Scale : variant.Scale, 1f);

            Vector3 anchor = variant.Anchor == WeaponVfxAnchor.Target ? targetCenter : origin;
            Vector3 offset = rotation * Vector3.Scale(variant.ImpactOffset, signedScale);
            Vector3 position = variant.Anchor == WeaponVfxAnchor.Target ? anchor - offset : anchor + offset;

            GameObject instance = Object.Instantiate(variant.Prefab, position, rotation, RuntimeRoot.Get());
            instance.name = "[Runtime] " + variant.Prefab.name;

            VfxColorRamp ramp = ResolveRamp(variant, elementRamp);
            var sheet = instance.GetComponent<SpriteSheetVfxController>();
            if (sheet != null)
            {
                instance.transform.localScale = Vector3.Scale(instance.transform.localScale, signedScale);
                sheet.SetRamp(ramp);
            }
            else
            {
                instance.transform.localScale = instance.transform.localScale * variant.Scale;
                if (flipY) FlipParticles(instance);
                if (variant.ColorMode != WeaponVfxColorMode.Neutral) TintParticles(instance, ramp.Mid);
            }

            return instance;
        }

        public static Vector3 GetTargetCenter(IEntity target, Vector3 fallback)
        {
            if (target == null || target.transform == null) return fallback;
            var collider = target.transform.GetComponent<Collider2D>();
            if (collider != null) return collider.bounds.center;
            return target.transform.position + Vector3.up * 0.5f;
        }
        #endregion

        #region [Private Methods]
        private static VfxColorRamp ResolveRamp(WeaponVfxVariant variant, VfxColorRamp elementRamp)
        {
            switch (variant.ColorMode)
            {
                case WeaponVfxColorMode.Fixed: return VfxColorRamp.FromColor(variant.FixedColor);
                case WeaponVfxColorMode.Neutral: return VfxColorRamp.Neutral;
                default: return elementRamp;
            }
        }

        private static void FlipParticles(GameObject instance)
        {
            var renderers = instance.GetComponentsInChildren<ParticleSystemRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Vector3 flip = renderers[i].flip;
                flip.y = flip.y > 0.5f ? 0f : 1f;
                renderers[i].flip = flip;
            }
        }

        private static void TintParticles(GameObject instance, Color color)
        {
            var system = instance.GetComponent<ParticleSystem>();
            if (system == null) return;
            var main = system.main;
            main.startColor = color;
        }
        #endregion
    }
}
