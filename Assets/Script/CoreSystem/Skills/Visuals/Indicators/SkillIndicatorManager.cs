using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using System.Collections.Generic;

namespace CoreSystem.Skills.Visuals
{
    public class SkillIndicatorManager : MonoBehaviour
    {
        public static SkillIndicatorManager Instance { get; private set; }

        [Header("Templates (Prefabs)")]
        [SerializeField] private CircleIndicator _circleTemplate;
        [SerializeField] private LineIndicator _lineTemplate;
        [SerializeField] private ConeIndicator _coneTemplate;
        
        private CircleIndicator _activeCircle;
        private LineIndicator _activeLine;
        private ConeIndicator _activeCone;
        
        private ISkillIndicator _currentActive;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (_circleTemplate != null) _activeCircle = Instantiate(_circleTemplate, transform);
            if (_lineTemplate != null) _activeLine = Instantiate(_lineTemplate, transform);
            if (_coneTemplate != null) _activeCone = Instantiate(_coneTemplate, transform);
            
            if (_activeCircle != null) _activeCircle.Hide();
            if (_activeLine != null) _activeLine.Hide();
            if (_activeCone != null) _activeCone.Hide();
        }

        public void ShowIndicator(SkillPayload payload, TargetingLogic logic)
        {
            HideCurrent();

            if (logic == null) return;

            if (logic is TargetLocationTargeting || logic is SphereAOETargeting || logic is SelfTargeting)
            {
                if (_activeCircle != null)
                {
                    _activeCircle.Show(payload, logic);
                    _currentActive = _activeCircle;
                }
            }
            else if (logic is BoxTargeting || logic is LinePierceTargeting)
            {
                if (_activeLine != null)
                {
                    _activeLine.Show(payload, logic);
                    _currentActive = _activeLine;
                }
            }
            else if (logic is ConeTargeting)
            {
                if (_activeCone != null)
                {
                    _activeCone.Show(payload, logic);
                    _currentActive = _activeCone;
                }
            }
        }

        public void UpdateIndicator(Vector3 origin, Vector3 target, Vector2 direction)
        {
            if (_currentActive != null)
            {
                _currentActive.UpdateState(origin, target, direction);
            }
        }

        public void SetProgress(float progress)
        {
            if (_currentActive != null)
            {
                _currentActive.SetProgress(progress);
            }
        }

        public void StartExecution(SkillPayload payload)
        {
            if (_currentActive != null)
            {
                _currentActive.StartExecution(payload);
            }
        }

        public void HideCurrent()
        {
            if (_currentActive != null)
            {
                _currentActive.Hide();
                _currentActive = null;
            }
        }

        public void HideIndicator() => HideCurrent();
    }
}
