using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Visuals
{
    [DisallowMultipleComponent]
    public sealed class DirectionHintView : MonoBehaviour
    {
        #region [Variables/Fields]
        public const string ObjectName = "[Runtime] DirectionHint";

        private static Sprite _sprite;

        private Character _owner;
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private SpriteRenderer _ownerRenderer;
        #endregion

        #region [Properties]
        public Character Owner => _owner;
        public bool IsShown => _renderer != null && _renderer.enabled && gameObject.activeInHierarchy;
        #endregion

        #region [Unity Methods]
        private void LateUpdate()
        {
            if (_owner == null || _renderer == null) return;
            _renderer.enabled = !_owner.IsDead;
            if (!_renderer.enabled) return;

            transform.position = Center();
            transform.rotation = Quaternion.Euler(0f, 0f, DirectionHintFormula.AngleDegrees(_owner.FacingDirection));
            ApplySize();
            if (_ownerRenderer != null)
            {
                _renderer.sortingLayerID = _ownerRenderer.sortingLayerID;
                _renderer.sortingOrder = _ownerRenderer.sortingOrder + DirectionHintSettings.Instance.SortingOrderOffset;
            }
        }
        #endregion

        #region [Public Methods]
        public static DirectionHintView Create(Character owner)
        {
            if (owner == null) return null;
            var go = new GameObject(ObjectName);
            go.transform.SetParent(owner.transform, false);
            var view = go.AddComponent<DirectionHintView>();
            view.Bind(owner);
            return view;
        }

        public void SetShown(bool shown)
        {
            if (gameObject.activeSelf != shown) gameObject.SetActive(shown);
        }
        #endregion

        #region [Private Methods]
        private void Bind(Character owner)
        {
            _owner = owner;
            DirectionHintSettings settings = DirectionHintSettings.Instance;
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = LoadSprite();
            Material material = settings.ResolveMaterial();
            if (material != null) _renderer.sharedMaterial = material;
            _renderer.color = settings.Color;
            _collider = FindBodyCollider(owner);
            _ownerRenderer = FindBodyRenderer(owner);
            LateUpdate();
        }

        private Vector3 Center()
        {
            Vector3 center = _collider != null ? _collider.bounds.center : _owner.transform.position;
            center.z = _owner.transform.position.z;
            return center;
        }

        private void ApplySize()
        {
            Vector2 size = _collider != null ? (Vector2)_collider.bounds.size : Vector2.one;
            DirectionHintSettings settings = DirectionHintSettings.Instance;
            float diameter = DirectionHintFormula.Diameter(size, settings.SizeMultiplier, settings.MinimumDiameter);
            Vector3 parent = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(diameter / SafeScale(parent.x), diameter / SafeScale(parent.y), 1f);
        }

        private static float SafeScale(float value)
        {
            return Mathf.Abs(value) < 1e-4f ? 1f : Mathf.Abs(value);
        }

        private static Collider2D FindBodyCollider(Character owner)
        {
            Collider2D fallback = null;
            foreach (Collider2D collider in owner.GetComponentsInChildren<Collider2D>(true))
            {
                if (collider == null) continue;
                if (!collider.isTrigger) return collider;
                if (fallback == null) fallback = collider;
            }
            return fallback;
        }

        private SpriteRenderer FindBodyRenderer(Character owner)
        {
            SpriteRenderer best = null;
            float area = -1f;
            foreach (SpriteRenderer candidate in owner.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (candidate == null || candidate == _renderer || candidate.sprite == null) continue;
                float size = candidate.bounds.size.x * candidate.bounds.size.y;
                if (size > area)
                {
                    area = size;
                    best = candidate;
                }
            }
            return best;
        }

        private static Sprite LoadSprite()
        {
            if (_sprite != null) return _sprite;
            Texture2D texture = Resources.Load<Texture2D>(DirectionHintSettings.TexturePath);
            if (texture == null)
            {
                Debug.LogWarning($"[DirectionHint] Missing texture at Resources/{DirectionHintSettings.TexturePath}");
                return null;
            }
            _sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), DirectionHintFormula.Pivot, texture.width, 0, SpriteMeshType.FullRect);
            _sprite.name = "DirectionHint";
            return _sprite;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _sprite = null;
        }
        #endregion
    }
}
