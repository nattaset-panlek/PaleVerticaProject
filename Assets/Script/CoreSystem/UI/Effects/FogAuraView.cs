using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Effects
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [RequireComponent(typeof(RawImage))]
    public sealed class FogAuraView : MonoBehaviour
    {
        #region [Variables/Fields]
        public const string ShaderName = "Custom/UI/FogAura";
        public const string ObjectName = "[Runtime] Fog Aura";

        private static readonly int StartTimeId = Shader.PropertyToID("_StartTime");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly List<FogAuraView> _active = new List<FogAuraView>();

        public static float FadeInDuration = 1.2f;
        public static float FadeOutDuration = 0.4f;

        private RawImage _image;
        private Material _material;
        private Tween _fade;
        #endregion

        #region [Properties]
        public static IReadOnlyList<FogAuraView> Active => _active;
        public Material Material => _material;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _image.raycastTarget = false;
            _active.Add(this);
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateAspect();
        }

        private void OnDestroy()
        {
            _active.Remove(this);
            _fade?.Kill();
            if (_material == null) return;
            if (Application.isPlaying) Destroy(_material);
            else DestroyImmediate(_material);
        }
        #endregion

        #region [Public Methods]
        public static Material LoadMaterial(string resourcesPath)
        {
            var material = string.IsNullOrEmpty(resourcesPath) ? null : Resources.Load<Material>(resourcesPath);
            if (material != null) return material;
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"[FogAuraView] Shader \"{ShaderName}\" was not found.");
                return null;
            }
            Debug.LogWarning($"[FogAuraView] Material \"{resourcesPath}\" was not found; using the Ink defaults.");
            material = new Material(shader) { name = "Fog Aura (Ink)", hideFlags = HideFlags.DontSave };
            FogAuraPalette.Ink.ApplyTo(material);
            return material;
        }

        public static FogAuraView Create(RectTransform behind, Material source, bool fadeIn)
        {
            if (behind == null || behind.parent == null || source == null) return null;

            var go = new GameObject(ObjectName, typeof(RectTransform), typeof(RawImage), typeof(FogAuraView));
            var rect = (RectTransform)go.transform;
            rect.SetParent(behind.parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetSiblingIndex(behind.GetSiblingIndex());

            var view = go.GetComponent<FogAuraView>();
            view.Setup(source, fadeIn);
            return view;
        }

        public void KeepBehind(RectTransform target)
        {
            if (target == null || target.parent != transform.parent) return;
            int index = target.GetSiblingIndex();
            transform.SetSiblingIndex(transform.GetSiblingIndex() < index ? index - 1 : index);
        }

        public void Dismiss()
        {
            _fade?.Kill();
            if (!Application.isPlaying || _material == null || FadeOutDuration <= 0f)
            {
                DestroySelf();
                return;
            }
            _fade = _material.DOFloat(0f, FadeId, FadeOutDuration).SetUpdate(true).OnComplete(DestroySelf);
        }
        #endregion

        #region [Private Methods]
        private void Setup(Material source, bool fadeIn)
        {
            if (_image == null) _image = GetComponent<RawImage>();
            _image.raycastTarget = false;
            _material = new Material(source) { name = source.name + " (Instance)", hideFlags = HideFlags.DontSave };
            _material.SetFloat(StartTimeId, Application.isPlaying ? Time.timeSinceLevelLoad : 1e9f);
            _image.material = _material;
            _image.color = Color.white;
            UpdateAspect();

            bool animate = fadeIn && Application.isPlaying && FadeInDuration > 0f;
            _material.SetFloat(FadeId, animate ? 0f : 1f);
            if (animate) _fade = _material.DOFloat(1f, FadeId, FadeInDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private void UpdateAspect()
        {
            if (_material == null) return;
            Rect r = ((RectTransform)transform).rect;
            if (r.height > 0f) _material.SetFloat(AspectId, r.width / r.height);
        }

        private void DestroySelf()
        {
            if (this == null) return;
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }
        #endregion
    }
}
