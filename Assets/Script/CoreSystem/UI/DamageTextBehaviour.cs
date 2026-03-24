using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CoreSystem.UI
{
    /// <summary>
    /// Animates world-to-screen UI text for damage numbers.
    /// Works with standard Overlay/Camera Canvas.
    /// </summary>
    public class DamageTextBehaviour : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        private float _lifetime = 0.8f;
        private Vector3 _worldPos;
        private Canvas _canvas;
        private TMP_Text _text;
       

        public void Initialize(Vector3 spawnWorldPos, float amount, Color color, Canvas targetCanvas, Sprite iconSprite = null, bool isCritical = false)
        {
            _text = GetComponent<TMP_Text>();
            if (_text == null) _text = GetComponentInChildren<TMP_Text>();
            
            if (Icon == null) Icon = GetComponentInChildren<Image>(); // Auto-find if not assigned in Inspector
            
            _canvas = targetCanvas;
            _worldPos = spawnWorldPos;
            
            if (_text != null)
            {
                _text.text = amount.ToString("F0");
                _text.color = color;
            }

            if (Icon != null)
            {
                if (iconSprite != null)
                {
                    Icon.sprite = iconSprite;
                    Icon.gameObject.SetActive(true);
                    Icon.color = color;
                    Debug.Log($"[DamageTextBehaviour] Icon set: {iconSprite.name}");
                }
                else
                {
                    Icon.gameObject.SetActive(false);
                    Debug.Log("[DamageTextBehaviour] Icon sprite null, disabling icon object.");
                }
            }
            else
            {
                Debug.LogWarning("[DamageTextBehaviour] Icon (Image component) not found!");
            }

            //if (isCritical)
            //{
            //    transform.localScale = Vector3.one * 1.25f;
            //}
            //else
            //{
            //    transform.localScale = Vector3.one;
            //}

            UpdateScreenPos();
            Destroy(gameObject, _lifetime);
        }


        private void UpdateScreenPos(float verticalOffset = 0f)
        {
            if (Camera.main == null || _canvas == null) return;

            // Convert World to Scale-invariant UI space
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(_worldPos);
            
            // Adjust for canvas scaling (if standard UI)
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // We add vertical offset to the result screen point or directly to transform position
                transform.position = new Vector3(screenPoint.x, screenPoint.y + verticalOffset, 0f);
            }
            else // Camera relative or World relative
            {
                // Better to use RectTransformUtility if we are a child of a RectTransform
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform, 
                    screenPoint, 
                    _canvas.worldCamera, 
                    out Vector2 localPos);

                (transform as RectTransform).anchoredPosition = new Vector2(localPos.x, localPos.y + verticalOffset);
            }
        }
    }
}
