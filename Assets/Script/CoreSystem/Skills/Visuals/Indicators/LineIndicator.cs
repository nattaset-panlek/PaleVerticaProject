using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Visuals
{
    public class LineIndicator : MonoBehaviour, ISkillIndicator
    {
        [SerializeField] private SpriteRenderer _lineBox;
        
        public void Show(SkillPayload payload, TargetingLogic logic)
        {
            gameObject.SetActive(true);
            
            if (logic is BoxTargeting box)
            {
                // Box size from Targeting data
                _lineBox.transform.localScale = new Vector3(box.BoxSize.x, box.BoxSize.y, 1f);
                // Apply the Box offset relative to the rotation
                _lineBox.transform.localPosition = box.Offset;
            }
        }

        public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
        {
            transform.position = origin;
            
            // Point the indicator towards the direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
