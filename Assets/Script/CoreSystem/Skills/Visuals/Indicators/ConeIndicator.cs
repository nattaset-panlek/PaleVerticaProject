using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Visuals
{
    public class ConeIndicator : MonoBehaviour, ISkillIndicator
    {
        [SerializeField] private SpriteRenderer _coneMesh;
        private Material _coneMat;

        private void Awake()
        {
            if (_coneMesh != null) _coneMat = _coneMesh.material;
        }

        public void Show(SkillPayload payload, TargetingLogic logic)
        {
            gameObject.SetActive(true);
            
            if (logic is ConeTargeting cone)
            {
                // Set Scale as radius
                float radius = payload.AOERadius;
                _coneMesh.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
                
                // Pass angle to shader
                if (_coneMat != null) _coneMat.SetFloat("_ConeAngle", cone.ConeAngle);
            }
        }

        public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
        {
            transform.position = origin;
            
            // Point towards direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
