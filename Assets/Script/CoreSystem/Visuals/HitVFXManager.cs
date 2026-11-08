using CoreSystem;
using UnityEngine;

namespace CoreSystem.Visuals
{
    public static class HitVFXManager
    {
        #region [Variables/Fields]
        private const string MeleePrefabResource = "Prefab/VFX/HitMeleeVFX";
        private const string MagicPrefabResource = "Prefab/VFX/HitMagicVFX";

        private static GameObject _cachedMeleePrefab;
        private static GameObject _cachedMagicPrefab;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static void PlayHitVFX(Vector3 position, DamageType damageType, ElementType element = ElementType.Neutral, float scale = 1f)
        {
            if (damageType == DamageType.Magic || damageType == DamageType.Elemental)
            {
                SpawnMagicHit(position, element, scale);
            }
            else
            {
                SpawnMeleeHit(position, element, scale);
            }
        }

        public static void SpawnMeleeHit(Vector3 position, ElementType element = ElementType.Neutral, float scale = 1f)
        {
            if (_cachedMeleePrefab == null)
            {
                _cachedMeleePrefab = Resources.Load<GameObject>(MeleePrefabResource);
            }

            if (_cachedMeleePrefab == null) return;

            GameObject go = Object.Instantiate(_cachedMeleePrefab, position, Quaternion.identity);
            go.transform.localScale = Vector3.one * scale;

            Color tint = GetElementalColor(element);
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null && tint != Color.white)
            {
                var main = ps.main;
                main.startColor = tint;
            }
        }

        public static void SpawnMagicHit(Vector3 position, ElementType element = ElementType.Neutral, float scale = 1f)
        {
            if (_cachedMagicPrefab == null)
            {
                _cachedMagicPrefab = Resources.Load<GameObject>(MagicPrefabResource);
            }

            if (_cachedMagicPrefab == null) return;

            GameObject go = Object.Instantiate(_cachedMagicPrefab, position, Quaternion.identity);
            go.transform.localScale = Vector3.one * scale;

            Color tint = GetElementalColor(element);
            if (tint != Color.white)
            {
                var systems = go.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < systems.Length; i++)
                {
                    var main = systems[i].main;
                    main.startColor = tint;
                }
            }
        }

        public static Color GetElementalColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire: return new Color(1f, 0.45f, 0.15f, 1f);
                case ElementType.Water: return new Color(0.25f, 0.75f, 1f, 1f);
                case ElementType.Earth: return new Color(0.85f, 0.7f, 0.35f, 1f);
                case ElementType.Electric: return new Color(0.4f, 0.9f, 1f, 1f);
                case ElementType.Holy: return new Color(1f, 0.95f, 0.45f, 1f);
                case ElementType.Ghost: return new Color(0.75f, 0.5f, 0.95f, 1f);
                default: return Color.white;
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
