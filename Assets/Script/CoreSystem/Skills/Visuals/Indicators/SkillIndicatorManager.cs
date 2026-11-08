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
        private readonly List<GameObject> _activeCastIndicators = new List<GameObject>();

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

            if (logic is TargetLocationTargeting || logic is SphereAOETargeting || logic is SelfTargeting || logic is SingleUnitTargeting || logic is SingleTargeting)
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

        public bool StartCastIndicator(SkillPayload payload)
        {
            return StartCastIndicators(new[] { payload });
        }

        public bool StartCastIndicators(IReadOnlyList<SkillPayload> payloads)
        {
            StopCastIndicator();
            if (payloads == null) return false;

            for (int i = 0; i < payloads.Count; i++)
            {
                GameObject instance = SpawnCastIndicator(payloads[i]);
                if (instance != null) _activeCastIndicators.Add(instance);
            }
            return _activeCastIndicators.Count > 0;
        }

        public void UpdateCastIndicatorProgress(float progress)
        {
            for (int i = 0; i < _activeCastIndicators.Count; i++)
            {
                if (_activeCastIndicators[i] == null) continue;
                var indicator = _activeCastIndicators[i].GetComponent<ISkillIndicator>();
                if (indicator != null)
                {
                    indicator.SetProgress(progress);
                }
            }
        }

        public void StopCastIndicator()
        {
            for (int i = 0; i < _activeCastIndicators.Count; i++)
            {
                GameObject instance = _activeCastIndicators[i];
                if (instance == null) continue;
                var indicator = instance.GetComponent<ISkillIndicator>();
                if (indicator != null)
                {
                    indicator.Hide();
                }
                Destroy(instance);
            }
            _activeCastIndicators.Clear();
        }

        private static GameObject SpawnCastIndicator(SkillPayload payload)
        {
            if (payload == null || payload.SkillIndicatorPrefab == null || payload.TotalCastTime <= 0f) return null;
            if (payload.SkillTargeting is SingleUnitTargeting) return null;

            GameObject instance = Instantiate(payload.SkillIndicatorPrefab, payload.TargetPosition, Quaternion.identity);

            var visual = instance.GetComponent<ISkillVisual>();
            if (visual != null)
            {
                visual.Initialize(payload);
            }
            else
            {
                foreach (var v in instance.GetComponentsInChildren<ISkillVisual>())
                {
                    v.Initialize(payload);
                }
            }

            var indicator = instance.GetComponent<ISkillIndicator>();
            if (indicator != null)
            {
                indicator.Show(payload, payload.SkillTargeting);
            }
            return instance;
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
