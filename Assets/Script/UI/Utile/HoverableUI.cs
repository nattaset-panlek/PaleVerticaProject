using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreSystem.UI.Utile
{
    public class HoverableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Content")]
        public string Title;
        [TextArea(3, 10)]
        public string Context;

        public void Setup(string title, string context)
        {
            Title = title;
            Context = context;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HoverTextManager.Instance != null)
            {
                HoverTextManager.Instance.Show(Title, Context);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoverTextManager.Instance != null)
            {
                HoverTextManager.Instance.Hide();
            }
        }

        private void OnDisable()
        {
            if (HoverTextManager.Instance != null)
            {
                HoverTextManager.Instance.Hide();
            }
        }
    }
}
