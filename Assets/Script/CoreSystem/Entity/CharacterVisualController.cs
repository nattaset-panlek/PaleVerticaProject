using UnityEngine;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Items;

namespace CoreSystem.Entity
{
    /// <summary>
    /// Manages the visual representation of a character by reading costume data.
    /// Replaces sprites with costume layers but falls back to default sprites if empty.
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

        private void OnEnable()
        {
            if (_character == null) _character = GetComponentInParent<Character>();
            
            if (_character != null)
            {
                _character.OnCostumeChanged += RefreshVisuals;
            }

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
            {
                _character.OnCostumeChanged -= RefreshVisuals;
            }
        }

        [ContextMenu("Refresh Visuals")]
        public void RefreshVisuals()
        {
            if (_character == null) return;

            UpdatePart(CostumeSlot.Head, HeadRenderer, DefaultHead);
            UpdatePart(CostumeSlot.Body, BodyRenderer, DefaultBody);
            
            // Arms (L=index 0, R=index 1)
            UpdateDualPart(CostumeSlot.Arms, ArmLRenderer, DefaultArmL, 0);
            UpdateDualPart(CostumeSlot.Arms, ArmRRenderer, DefaultArmR, 1);
            
            // Legs (L=index 0, R=index 1)
            UpdateDualPart(CostumeSlot.Legs, LegLRenderer, DefaultLegL, 0);
            UpdateDualPart(CostumeSlot.Legs, LegRRenderer, DefaultLegR, 1);
        }

        private void UpdatePart(CostumeSlot slot, SpriteRenderer renderer, Sprite defaultSprite)
        {
            if (renderer == null) return;

            var costumeInst = _character.CostumeSlots[(int)slot];
            if (costumeInst != null && costumeInst.Item is Costume costume && costume.VisualSprites != null && costume.VisualSprites.Length > 0)
            {
                renderer.sprite = costume.VisualSprites[0];
                // Apply layer priority offset if needed (optional)
                // renderer.sortingOrder = costume.LayerPriority; 
            }
            else
            {
                renderer.sprite = defaultSprite;
            }
        }

        private void UpdateDualPart(CostumeSlot slot, SpriteRenderer renderer, Sprite defaultSprite, int spriteIndex)
        {
            if (renderer == null) return;

            var costumeInst = _character.CostumeSlots[(int)slot];
            if (costumeInst != null && costumeInst.Item is Costume costume && costume.VisualSprites != null && costume.VisualSprites.Length > spriteIndex)
            {
                renderer.sprite = costume.VisualSprites[spriteIndex];
            }
            else
            {
                renderer.sprite = defaultSprite;
            }
        }
    }
}
