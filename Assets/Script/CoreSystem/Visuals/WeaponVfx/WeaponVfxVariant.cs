using System;
using UnityEngine;

namespace CoreSystem.Visuals.WeaponVfx
{
    [Serializable]
    public class WeaponVfxVariant
    {
        #region [Variables/Fields]
        public string Id = "Default";
        public GameObject Prefab;
        public WeaponVfxAnchor Anchor = WeaponVfxAnchor.Target;
        public Vector2 ImpactOffset = Vector2.zero;
        public float Scale = 1f;
        public bool AlignToAim = true;
        public bool MirrorWhenAimingLeft = true;
        public bool FlipVertical = false;
        public WeaponVfxColorMode ColorMode = WeaponVfxColorMode.Element;
        public Color FixedColor = Color.white;
        #endregion

        #region [Properties]
        public bool IsValid => Prefab != null;
        #endregion
    }
}
