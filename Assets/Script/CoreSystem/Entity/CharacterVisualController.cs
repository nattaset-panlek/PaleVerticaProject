using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Items;

namespace CoreSystem.Entity
{
    /// <summary>
    /// Manages the visual representation of a character.
    /// Sprite layers are loaded async from Addressables.
    /// Visual Prefabs are instantiated as children of model nodes after async load.
    /// </summary>
    public class CharacterVisualController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Character _character;

        [Header("Model Renderers")]
        public SpriteRenderer HeadRenderer;
        public SpriteRenderer BodyRenderer;
        public SpriteRenderer ArmLRenderer;
        public SpriteRenderer ArmRRenderer;
        public SpriteRenderer LegLRenderer;
        public SpriteRenderer LegRRenderer;

        [Header("Default Model Sprites (Captured at Start if empty)")]
        public Sprite DefaultHead;
        public Sprite DefaultBody;
        public Sprite DefaultArmL;
        public Sprite DefaultArmR;
        public Sprite DefaultLegL;
        public Sprite DefaultLegR;

        // Active prefab instances managed by this controller
        private List<GameObject> _activeCostumePrefabs = new List<GameObject>();

        // Addressable sessions per costume slot — key = slot index
        private Dictionary<int, CostumeLoadSession> _loadSessions = new Dictionary<int, CostumeLoadSession>();

        private void OnEnable()
        {
            if (_character == null) _character = GetComponentInParent<Character>();
            
            if (_character != null)
                _character.OnCostumeChanged += RefreshVisuals;

            // Capture defaults if not set
            if (DefaultHead == null && HeadRenderer != null) DefaultHead = HeadRenderer.sprite;
            if (DefaultBody == null && BodyRenderer != null) DefaultBody = BodyRenderer.sprite;
            if (DefaultArmL == null && ArmLRenderer != null) DefaultArmL = ArmLRenderer.sprite;
            if (DefaultArmR == null && ArmRRenderer != null) DefaultArmR = ArmRRenderer.sprite;
            if (DefaultLegL == null && LegLRenderer != null) DefaultLegL = LegLRenderer.sprite;
            if (DefaultLegR == null && LegRRenderer != null) DefaultLegR = LegRRenderer.sprite;
            
            RefreshVisuals();
        }

        private void OnDisable()
        {
            if (_character != null)
                _character.OnCostumeChanged -= RefreshVisuals;

            ReleaseAllSessions();
            ClearCostumePrefabs();
        }

        [ContextMenu("Refresh Visuals")]
        public void RefreshVisuals()
        {
            if (_character == null) return;
            StartCoroutine(RefreshVisualsAsync());
        }

        private IEnumerator RefreshVisualsAsync()
        {
            // --- 1. Reset all renderers to defaults first ---
            SetSprite(HeadRenderer, DefaultHead);
            SetSprite(BodyRenderer, DefaultBody);
            SetSprite(ArmLRenderer, DefaultArmL);
            SetSprite(ArmRRenderer, DefaultArmR);
            SetSprite(LegLRenderer, DefaultLegL);
            SetSprite(LegRRenderer, DefaultLegR);

            // --- 2. Clear all old addressable sessions and prefabs ---
            ReleaseAllSessions();
            ClearCostumePrefabs();

            // --- 3. Load each equipped costume slot ---
            for (int slotIdx = 0; slotIdx < _character.CostumeSlots.Length; slotIdx++)
            {
                var instance = _character.CostumeSlots[slotIdx];
                if (instance == null || !(instance.Item is Costume costume)) continue;

                var session = new CostumeLoadSession();
                _loadSessions[slotIdx] = session;

                // Load Sprites
                yield return StartCoroutine(CostumeAddressableLoader.LoadSpritesAsync(
                    costume,
                    session,
                    (spriteIndex, sprite) => ApplyCostumeSprite(costume.Slot, spriteIndex, sprite)
                ));

                // Load Prefabs
                yield return StartCoroutine(CostumeAddressableLoader.LoadPrefabsAsync(
                    costume,
                    session,
                    (pData, prefab) => InstantiateCostumePrefab(pData, prefab)
                ));
            }
        }

        // ── Sprite Application ──────────────────────────────────────────────

        private void ApplyCostumeSprite(CostumeSlot slot, int spriteIndex, Sprite sprite)
        {
            switch (slot)
            {
                case CostumeSlot.Head:
                    if (spriteIndex == 0) SetSprite(HeadRenderer, sprite);
                    break;
                case CostumeSlot.Body:
                    if (spriteIndex == 0) SetSprite(BodyRenderer, sprite);
                    break;
                case CostumeSlot.Arms:
                    if (spriteIndex == 0) SetSprite(ArmLRenderer, sprite);
                    else if (spriteIndex == 1) SetSprite(ArmRRenderer, sprite);
                    break;
                case CostumeSlot.Legs:
                    if (spriteIndex == 0) SetSprite(LegLRenderer, sprite);
                    else if (spriteIndex == 1) SetSprite(LegRRenderer, sprite);
                    break;
            }
        }

        private void SetSprite(SpriteRenderer renderer, Sprite sprite)
        {
            if (renderer != null) renderer.sprite = sprite;
        }

        // ── Prefab Management ───────────────────────────────────────────────

        private void InstantiateCostumePrefab(CostumeVisualPrefabData pData, GameObject prefab)
        {
            Transform target = GetTargetNode(pData.TargetNode);
            if (target == null || prefab == null) return;

            GameObject go = Instantiate(prefab, target);
            go.transform.localPosition = pData.Offset;
            go.transform.localRotation = Quaternion.Euler(pData.Rotation);
            go.transform.localScale    = pData.Scale;
            _activeCostumePrefabs.Add(go);
        }

        private void ClearCostumePrefabs()
        {
            foreach (var go in _activeCostumePrefabs)
                if (go != null) Destroy(go);
            _activeCostumePrefabs.Clear();
        }

        private void ReleaseAllSessions()
        {
            foreach (var session in _loadSessions.Values)
                session.Release();
            _loadSessions.Clear();
        }

        private Transform GetTargetNode(CostumeSlot slot)
        {
            switch (slot)
            {
                case CostumeSlot.Head: return HeadRenderer != null ? HeadRenderer.transform : transform;
                case CostumeSlot.Body: return BodyRenderer != null ? BodyRenderer.transform : transform;
                case CostumeSlot.Arms: return ArmLRenderer != null ? ArmLRenderer.transform : transform;
                case CostumeSlot.Legs: return LegLRenderer != null ? LegLRenderer.transform : transform;
                default: return transform;
            }
        }
    }
}
