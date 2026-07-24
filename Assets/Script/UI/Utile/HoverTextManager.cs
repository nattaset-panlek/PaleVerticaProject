using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CoreSystem.UI.Utile
{
    public class HoverTextManager : MonoBehaviour
    {
        public static HoverTextManager Instance { get; private set; }

        [Header("UI Elements")]
        public GameObject TooltipWindow;
        public TMP_Text TitleText;
        public TMP_Text ContextText;
        public RectTransform TooltipRectTransform;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Hide(); // Start hidden
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (TooltipWindow.activeSelf)
            {
                Vector2 position = UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero;
                //position.x += 15f; // Offset to not be directly under the cursor
                //position.y += 15f;
                TooltipRectTransform.position = position;

                // Clamp to screen limits to avoid clipping
                ClampToScreenEdges();
            }
        }

        public void Show(string title, string context)
        {
            if (TooltipWindow == null) return;
            
            if (TitleText != null)
                TitleText.text = title;
            
            if (ContextText != null)
                ContextText.text = context;

            TooltipWindow.SetActive(true);
        }

        public void Hide()
        {
            if (TooltipWindow != null)
                TooltipWindow.SetActive(false);
        }

        private void ClampToScreenEdges()
        {
            Vector3[] corners = new Vector3[4];
            TooltipRectTransform.GetWorldCorners(corners);

            float width = corners[2].x - corners[0].x;
            float height = corners[1].y - corners[0].y;

            Vector3 pos = TooltipRectTransform.position;

            if (pos.x + width > Screen.width) pos.x = Screen.width - width;
            if (pos.x < 0) pos.x = 0;

            if (pos.y + height > Screen.height) pos.y = Screen.height - height;
            if (pos.y < 0) pos.y = 0;

            TooltipRectTransform.position = pos;
        }
    }
}
