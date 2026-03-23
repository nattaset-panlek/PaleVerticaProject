using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Visuals
{
    public class CircleIndicator : MonoBehaviour, ISkillIndicator
    {
        [SerializeField] private SpriteRenderer _mainDisc;
        [SerializeField] private SpriteRenderer _rangeDisc;
        
        private Material _mainMat;
        private Material _rangeMat;

        private void Awake()
        {
            if (_mainDisc != null) _mainMat = _mainDisc.material;
            if (_rangeDisc != null) _rangeMat = _rangeDisc.material;
        }

        public void Show(SkillPayload payload, TargetingLogic logic)
        {
            gameObject.SetActive(true);
            
            // Set scale for target AOE circle
            if (_mainDisc != null)
            {
                float diameter = payload.AOERadius * 2f;
                _mainDisc.transform.localScale = new Vector3(diameter, diameter, 1f);
            }

            // Set scale for caster range circle (Usually fixed)
            if (_rangeDisc != null && logic is TargetLocationTargeting targetLoc)
            {
                float rangeDiameter = targetLoc.CastRange * 2f;
                _rangeDisc.transform.localScale = new Vector3(rangeDiameter, rangeDiameter, 1f);
                _rangeDisc.gameObject.SetActive(true);
            }
            else if (_rangeDisc != null)
            {
                _rangeDisc.gameObject.SetActive(false);
            }
        }

        public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
        {
            // Position the range circle at origin (caster)
            if (_rangeDisc != null) _rangeDisc.transform.position = origin;

            // Position the main AOE circle at the target (clamped)
            if (_mainDisc != null) _mainDisc.transform.position = target;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
