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
        private GameObject _customIndicatorInstance;
        private GameObject _activeCastIndicatorInstance;

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
            if (_customIndicatorInstance != null)
            {
                Destroy(_customIndicatorInstance);
                _customIndicatorInstance = null;
            }
        }

        public void HideIndicator() => HideCurrent();

        public void StartCastIndicator(SkillPayload payload)
        {
            StopCastIndicator();

            if (payload == null || payload.SkillIndicatorPrefab == null || payload.TotalCastTime <= 0f) return;

            Vector3 spawnPos = payload.TargetPosition;
            _activeCastIndicatorInstance = Instantiate(payload.SkillIndicatorPrefab, spawnPos, Quaternion.identity);

            var visual = _activeCastIndicatorInstance.GetComponent<ISkillVisual>();
            if (visual != null)
            {
                visual.Initialize(payload);
            }
            else
            {
                var visuals = _activeCastIndicatorInstance.GetComponentsInChildren<ISkillVisual>();
                foreach (var v in visuals)
                {
                    v.Initialize(payload);
                }
            }

            var indicator = _activeCastIndicatorInstance.GetComponent<ISkillIndicator>();
            if (indicator != null)
            {
                indicator.Show(payload, payload.SkillTargeting);
            }
        }

        public void UpdateCastIndicatorProgress(float progress)
        {
            if (_activeCastIndicatorInstance != null)
            {
                var indicator = _activeCastIndicatorInstance.GetComponent<ISkillIndicator>();
                if (indicator != null)
                {
                    indicator.SetProgress(progress);
                }
            }
        }

        public void StopCastIndicator()
        {
            if (_activeCastIndicatorInstance != null)
            {
                var indicator = _activeCastIndicatorInstance.GetComponent<ISkillIndicator>();
                if (indicator != null)
                {
                    indicator.Hide();
                }
                Destroy(_activeCastIndicatorInstance);
                _activeCastIndicatorInstance = null;
            }
        }

        private class CustomIndicatorWrapper : ISkillIndicator
        {
            private GameObject targetGo;
            
            public CustomIndicatorWrapper(GameObject go)
            {
                targetGo = go;
            }

            public void Show(SkillPayload payload, TargetingLogic logic)
            {
                if (targetGo != null)
                {
                    targetGo.SetActive(true);
                    UpdateState(payload.OriginPosition, payload.TargetPosition, Vector2.up);
                }
            }

            public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
            {
                if (targetGo != null)
                {
                    targetGo.transform.position = target;
                }
            }

            public void SetProgress(float progress)
            {
            }

            public void StartExecution(SkillPayload payload)
            {
            }

            public void Hide()
            {
                if (targetGo != null)
                {
                    Destroy(targetGo);
                }
            }
        }
    }
}
