using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Skills.Visuals
{
    public class CircleIndicator : MonoBehaviour, ISkillIndicator
    {
        #region [Variables/Fields]
        [SerializeField] private SpriteRenderer _mainDisc;
        [SerializeField] private SpriteRenderer _rangeDisc;
        [SerializeField] private SpriteRenderer _fillDisc;
        
        private float _maxAOERadius;
        #endregion

        #region [Public Methods]
        public void Show(SkillPayload payload, TargetingLogic logic)
        {
            gameObject.SetActive(true);
            _maxAOERadius = payload.AOERadius;
            
            if (_mainDisc != null)
            {
                if (logic is SingleUnitTargeting)
                {
                    _mainDisc.gameObject.SetActive(false);
                }
                else
                {
                    float diameter = _maxAOERadius * 2f;
                    _mainDisc.transform.localScale = new Vector3(diameter, diameter, 1f);
                    _mainDisc.gameObject.SetActive(true);
                }
            }

            if (_fillDisc != null)
            {
                _fillDisc.gameObject.SetActive(true);
                _fillDisc.transform.localScale = Vector3.zero;
            }

            if (_rangeDisc != null && (logic is TargetLocationTargeting || logic is SingleUnitTargeting))
            {
                float castRange = (logic is SingleUnitTargeting sut) ? sut.CastRange : payload.CastRange;
                float rangeDiameter = castRange * 2f;
                _rangeDisc.transform.localScale = new Vector3(rangeDiameter, rangeDiameter, 1f);
                _rangeDisc.gameObject.SetActive(true);
            }
            else if (_rangeDisc != null)
            {
                _rangeDisc.gameObject.SetActive(false);
            }
        }

        public void SetProgress(float progress)
        {
            //if (_fillDisc == null) return;
            //float diameter = _maxAOERadius * 2f * Mathf.Clamp01(progress);
            //_fillDisc.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        public void StartExecution(SkillPayload payload)
        {
            // Handled externally or via routine
        }

        public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
        {
            if (_rangeDisc != null) _rangeDisc.transform.position = origin;
            if (_mainDisc != null) _mainDisc.transform.position = target;
            //if (_fillDisc != null) _fillDisc.transform.position = target;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion
    }

}
