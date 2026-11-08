using System.Collections;
using System.Collections.Generic;
using CoreSystem.Skills;
using CoreSystem.Skills.Visuals;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Visuals
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SlashVisualEffect : MonoBehaviour, ISkillVisual
    {
        #region [Variables/Fields]
        private const string ResourcePath = "Textures/VFX/VFX_Pure_Slash_Sheet";
        private const int Columns = 4;
        private const int Rows = 4;
        private const int TotalFrames = 16;
        private const int FrameSize = 256;

        private static Sprite[] _cachedFrames;
        private static Texture2D _cachedTexture;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _framesPerSecond = 30f;
        [SerializeField] private Color _tintColor = Color.white;
        [SerializeField] private float _uniformScale = 1.2f;

        private Coroutine _animationCoroutine;
        #endregion

        #region [Properties]
        public SpriteRenderer Renderer => _spriteRenderer;
        public float FrameDuration => 1f / Mathf.Max(1f, _framesPerSecond);
        public float TotalDuration => TotalFrames * FrameDuration;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureFramesLoaded();
        }

        private void Start()
        {
            if (_animationCoroutine == null)
            {
                _animationCoroutine = StartCoroutine(PlayFramesRoutine());
            }
        }

        private void OnDestroy()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }
        #endregion

        #region [Public Methods]
        public static SlashVisualEffect Create(Vector3 position, Quaternion rotation, Color? tint = null, float scale = 1.2f)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/VFX/SlashVisualEffect");
            SlashVisualEffect effect;

            if (prefab != null)
            {
                var go = Object.Instantiate(prefab, position, rotation);
                go.name = "[Runtime] SlashVisualEffect";
                effect = go.GetComponent<SlashVisualEffect>();
                if (effect == null)
                {
                    effect = go.AddComponent<SlashVisualEffect>();
                }
            }
            else
            {
                var go = new GameObject("[Runtime] SlashVisualEffect");
                go.transform.position = position;
                go.transform.rotation = rotation;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 50;

                effect = go.AddComponent<SlashVisualEffect>();
            }

            effect.SetScale(scale);
            if (tint.HasValue)
            {
                effect.SetColor(tint.Value);
            }

            return effect;
        }

        public void SetColor(Color color)
        {
            _tintColor = color;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _tintColor;
            }
        }

        public void SetScale(float scale)
        {
            _uniformScale = scale;
            transform.localScale = Vector3.one * _uniformScale;
        }

        public void Initialize(SkillPayload payload)
        {
            if (payload == null) return;

            Vector3 origin = payload.OriginPosition;
            Vector3 target = payload.IsTargetPositionSet ? payload.TargetPosition : origin;

            Vector3 dir = (target - origin).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            if (payload.AOERadius > 0f)
            {
                SetScale(payload.AOERadius * 0.8f);
            }
        }
        #endregion

        #region [Private Methods]
        private static void EnsureFramesLoaded()
        {
            if (_cachedFrames != null && _cachedFrames.Length == TotalFrames) return;

            if (_cachedTexture == null)
            {
                _cachedTexture = Resources.Load<Texture2D>(ResourcePath);
            }

            if (_cachedTexture == null) return;

            _cachedFrames = new Sprite[TotalFrames];

            for (int i = 0; i < TotalFrames; i++)
            {
                int col = i % Columns;
                int row = i / Columns;
                int invRow = Rows - 1 - row;

                Rect rect = new Rect(col * FrameSize, invRow * FrameSize, FrameSize, FrameSize);
                _cachedFrames[i] = Sprite.Create(
                    _cachedTexture,
                    rect,
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect
                );
            }
        }

        private IEnumerator PlayFramesRoutine()
        {
            if (_cachedFrames == null || _cachedFrames.Length == 0)
            {
                Destroy(gameObject);
                yield break;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _tintColor;
            }

            float delay = FrameDuration;

            for (int i = 0; i < TotalFrames; i++)
            {
                if (_spriteRenderer != null && _cachedFrames[i] != null)
                {
                    _spriteRenderer.sprite = _cachedFrames[i];
                }
                yield return new WaitForSeconds(delay);
            }

            Destroy(gameObject);
        }
        #endregion
    }
}
