using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Visuals
{
    public class EntityVisualController : MonoBehaviour
    {
        #region [Variables/Fields]
        [Header("Entity & Hierarchy References")]
        [SerializeField] private Character _character;
        [SerializeField] private Transform _modelRoot;
        [SerializeField] private SpriteRenderer _shadowRenderer;

        [Header("Configuration Override (Optional)")]
        [SerializeField] private EntityVisualSettings _settingsOverride;

        [Header("Renderer Exclusion")]
        [SerializeField] private List<SpriteRenderer> _ignoredRenderers = new List<SpriteRenderer>();

        private readonly List<SpriteRenderer> _bodyRenderers = new List<SpriteRenderer>();
        private MaterialPropertyBlock _propBlock;
        private Coroutine _flashCoroutine;
        private Coroutine _dissolveCoroutine;

        private static Material _cachedDefaultMasterMaterial;
        private const string DefaultMaterialResourcePath = "Materials/Character2DLitMaster";
        private const string DefaultShaderName = "Custom/Character2DLitMaster";

        private float _currentFlashAmount;
        private Color _currentFlashColor = Color.white;
        private float _currentStatusIntensity;
        private Color _currentStatusColor = Color.clear;
        private float _currentDissolveAmount;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int StatusIntensityId = Shader.PropertyToID("_StatusIntensity");
        private static readonly int StatusColorId = Shader.PropertyToID("_StatusColor");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        #endregion

        #region [Properties]
        public Transform ModelRoot
        {
            get => _modelRoot;
            set => _modelRoot = value;
        }

        public SpriteRenderer ShadowRenderer
        {
            get => _shadowRenderer;
            set => _shadowRenderer = value;
        }

        public EntityVisualSettings Settings => _settingsOverride != null ? _settingsOverride : EntityVisualSettings.Instance;

        public IReadOnlyList<SpriteRenderer> BodyRenderers => _bodyRenderers;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_character == null)
            {
                _character = GetComponent<Character>();
            }

            if (_modelRoot == null)
            {
                Transform foundModel = transform.Find("Model");
                _modelRoot = foundModel != null ? foundModel : transform;
            }

            if (_shadowRenderer == null)
            {
                Transform shadowObj = transform.Find("Shadow");
                if (shadowObj != null)
                {
                    _shadowRenderer = shadowObj.GetComponent<SpriteRenderer>();
                }
            }

            _propBlock = new MaterialPropertyBlock();
            RefreshRenderers();
        }

        private void OnEnable()
        {
            if (_character != null)
            {
                _character.OnTakeDamage += HandleTakeDamage;
                _character.OnCostumeChanged += HandleCostumeChanged;
            }
        }

        private void OnDisable()
        {
            if (_character != null)
            {
                _character.OnTakeDamage -= HandleTakeDamage;
                _character.OnCostumeChanged -= HandleCostumeChanged;
            }

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            if (_dissolveCoroutine != null)
            {
                StopCoroutine(_dissolveCoroutine);
                _dissolveCoroutine = null;
            }
        }
        #endregion

        #region [Public Methods]
        public void Initialize(Character character)
        {
            _character = character;
            RefreshRenderers();
        }

        public void RefreshRenderers()
        {
            _bodyRenderers.Clear();

            if (_modelRoot == null) return;

            Material matToUse = GetOrLoadMasterMaterial();
            SpriteRenderer[] renderers = _modelRoot.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr == null) continue;
                if (sr == _shadowRenderer) continue;
                if (_ignoredRenderers.Contains(sr)) continue;
                if (sr.gameObject.name.ToLower().Contains("shadow")) continue;

                _bodyRenderers.Add(sr);

                if (matToUse != null && sr.sharedMaterial != matToUse)
                {
                    sr.sharedMaterial = matToUse;
                }
            }

            ApplyProperties();
        }

        public void RegisterDynamicRenderer(SpriteRenderer renderer)
        {
            if (renderer == null || _bodyRenderers.Contains(renderer)) return;
            if (renderer == _shadowRenderer || _ignoredRenderers.Contains(renderer)) return;

            _bodyRenderers.Add(renderer);

            Material matToUse = GetOrLoadMasterMaterial();
            if (matToUse != null && renderer.sharedMaterial != matToUse)
            {
                renderer.sharedMaterial = matToUse;
            }

            if (_propBlock != null)
            {
                renderer.SetPropertyBlock(_propBlock);
            }
        }

        public void UnregisterDynamicRenderer(SpriteRenderer renderer)
        {
            if (renderer != null)
            {
                _bodyRenderers.Remove(renderer);
            }
        }

        public void PlayHitFlash(Color? flashColor = null, float duration = -1f)
        {
            Color colorToUse = flashColor ?? Settings.DefaultFlashColor;
            float durationToUse = duration > 0f ? duration : Settings.FlashDuration;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            if (gameObject.activeInHierarchy)
            {
                _flashCoroutine = StartCoroutine(HitFlashRoutine(colorToUse, durationToUse));
            }
        }

        public void SetStatusTint(Color color, float intensity)
        {
            _currentStatusColor = color;
            _currentStatusIntensity = Mathf.Clamp01(intensity);
            ApplyProperties();
        }

        public void SetDissolve(float amount)
        {
            _currentDissolveAmount = Mathf.Clamp01(amount);
            ApplyProperties();
        }

        public void PlayDissolve(float duration = -1f, System.Action onComplete = null)
        {
            float durationToUse = duration > 0f ? duration : Settings.DefaultDissolveDuration;

            if (_dissolveCoroutine != null)
            {
                StopCoroutine(_dissolveCoroutine);
            }

            if (gameObject.activeInHierarchy)
            {
                _dissolveCoroutine = StartCoroutine(DissolveRoutine(durationToUse, onComplete));
            }
        }

        public void SetFacing(float horizontalDirection)
        {
            if (_modelRoot == null) return;
            if (Mathf.Abs(horizontalDirection) < 0.001f) return;

            float targetSign = horizontalDirection > 0 ? 1f : -1f;
            Vector3 currentScale = _modelRoot.localScale;
            _modelRoot.localScale = new Vector3(Mathf.Abs(currentScale.x) * targetSign, currentScale.y, currentScale.z);

            transform.rotation = Quaternion.identity;
        }

        [ContextMenu("Test/Play Hit Flash")]
        public void TestHitFlash()
        {
            PlayHitFlash();
        }

        [ContextMenu("Test/Toggle Status Tint (Poison)")]
        public void TestToggleStatusTintPoison()
        {
            SetStatusTint(Settings.PoisonColor, _currentStatusIntensity > 0.01f ? 0f : 0.85f);
        }

        public void ApplyProperties()
        {
            if (_propBlock == null)
            {
                _propBlock = new MaterialPropertyBlock();
            }

            _propBlock.SetFloat(FlashAmountId, _currentFlashAmount);
            _propBlock.SetColor(FlashColorId, _currentFlashColor);
            _propBlock.SetFloat(StatusIntensityId, _currentStatusIntensity);
            _propBlock.SetColor(StatusColorId, _currentStatusColor);
            _propBlock.SetFloat(DissolveAmountId, _currentDissolveAmount);

            for (int i = 0; i < _bodyRenderers.Count; i++)
            {
                SpriteRenderer sr = _bodyRenderers[i];
                if (sr != null)
                {
                    sr.SetPropertyBlock(_propBlock);
                }
            }
        }
        #endregion

        #region [Private Methods]
        private void HandleTakeDamage(IEntity attacker, float damage)
        {
            PlayHitFlash();
        }

        private void HandleCostumeChanged()
        {
            StartCoroutine(DelayedCostumeRefresh());
        }

        private IEnumerator DelayedCostumeRefresh()
        {
            yield return null;
            RefreshRenderers();
        }

        private IEnumerator HitFlashRoutine(Color color, float duration)
        {
            _currentFlashColor = color;
            _currentFlashAmount = 1f;
            ApplyProperties();

            yield return new WaitForSeconds(duration);

            _currentFlashAmount = 0f;
            ApplyProperties();
            _flashCoroutine = null;
        }

        private IEnumerator DissolveRoutine(float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _currentDissolveAmount = Mathf.Clamp01(elapsed / duration);
                ApplyProperties();
                yield return null;
            }

            _currentDissolveAmount = 1f;
            ApplyProperties();
            _dissolveCoroutine = null;
            onComplete?.Invoke();
        }

        private Material GetOrLoadMasterMaterial()
        {
            if (Settings.MasterMaterial != null)
            {
                return Settings.MasterMaterial;
            }

            if (_cachedDefaultMasterMaterial != null)
            {
                return _cachedDefaultMasterMaterial;
            }

            _cachedDefaultMasterMaterial = Resources.Load<Material>(DefaultMaterialResourcePath);
            if (_cachedDefaultMasterMaterial == null)
            {
                Shader masterShader = Shader.Find(DefaultShaderName);
                if (masterShader != null)
                {
                    _cachedDefaultMasterMaterial = new Material(masterShader)
                    {
                        name = "Character2DLitMaster_RuntimeInstance"
                    };
                }
            }

            return _cachedDefaultMasterMaterial;
        }
        #endregion
    }
}
