using CoreSystem.Entity;
using CoreSystem.Skills.Logic;
using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    [RequireComponent(typeof(Character))]
    public class EntitySkillIndicator : MonoBehaviour
    {
        #region [Variables/Fields]
        [Header("Indicator Prefabs")]
        [SerializeField] private CircleIndicator _circleTemplate;
        [SerializeField] private LineIndicator _lineTemplate;
        [SerializeField] private ConeIndicator _coneTemplate;

        [Header("Settings")]
        [Tooltip("Tint color applied to indicator sprites to distinguish from the player's indicator.")]
        [SerializeField] private Color _indicatorTint = new Color(1f, 0.3f, 0.3f, 1f);
        [Tooltip("How long the indicator stays visible for instant skills (telegraph warning).")]
        [SerializeField] private float _telegraphDuration = 0.6f;

        private Character _character;

        private CircleIndicator _activeCircle;
        private LineIndicator _activeLine;
        private ConeIndicator _activeCone;

        private ISkillIndicator _currentActive;
        private SkillInstance _trackedInstance;
        private float _telegraphTimer;
        private bool _isTelegraphing;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _character = GetComponent<Character>();

            if (_circleTemplate != null) _activeCircle = Instantiate(_circleTemplate, transform);
            if (_lineTemplate != null) _activeLine = Instantiate(_lineTemplate, transform);
            if (_coneTemplate != null) _activeCone = Instantiate(_coneTemplate, transform);

            HideAll();
        }

        private void OnEnable()
        {
            if (_character?.skillCaster != null)
            {
                _character.skillCaster.OnCastInitiated += HandleCastInitiated;
                _character.skillCaster.OnCastProgress += HandleCastProgress;
                _character.skillCaster.OnCastExecuted += HandleCastExecuted;
                _character.skillCaster.OnCastInterrupted += HandleCastInterrupted;
            }
        }

        private void OnDisable()
        {
            if (_character?.skillCaster != null)
            {
                _character.skillCaster.OnCastInitiated -= HandleCastInitiated;
                _character.skillCaster.OnCastProgress -= HandleCastProgress;
                _character.skillCaster.OnCastExecuted -= HandleCastExecuted;
                _character.skillCaster.OnCastInterrupted -= HandleCastInterrupted;
            }

            HideCurrent();
        }

        private void Update()
        {
            if (_currentActive == null || _trackedInstance == null) return;

            if (_isTelegraphing)
            {
                _telegraphTimer -= Time.deltaTime;
                if (_telegraphTimer <= 0f)
                {
                    HideCurrent();
                    return;
                }
            }
            else
            {
                var state = _character.skillCaster.CurrentState;
                if (state != SkillCasterState.Casting && state != SkillCasterState.CastDelay)
                {
                    HideCurrent();
                    return;
                }
            }

            Vector3 origin = transform.position;
            Vector3 target = origin + transform.up;
            Vector2 direction = transform.up;

            var targeting = _trackedInstance.SkillData?.BaseTargeting;
            if (targeting is TargetLocationTargeting || targeting is SphereAOETargeting || targeting is SelfTargeting)
            {
                target = origin;
            }

            _currentActive.UpdateState(origin, target, direction);
        }
        #endregion

        #region [Private Methods]
        private void HandleCastInitiated(string skillName, float totalCastTime)
        {
            var instance = _character.skillCaster.ActiveSkillInstance;
            if (instance?.SkillData?.BaseTargeting == null) return;

            _trackedInstance = instance;
            _isTelegraphing = false;

            var payload = _character.skillCaster.BuildPayload(instance);
            var logic = instance.SkillData.BaseTargeting;

            HideAll();
            _trackedInstance = instance;
            ShowIndicatorForLogic(payload, logic);
            ApplyTint();

            if (totalCastTime <= 0f)
            {
                _isTelegraphing = true;
                _telegraphTimer = _telegraphDuration;
            }
        }

        private void HandleCastProgress(float current, float total)
        {
            if (_currentActive == null) return;
            float progress = total > 0 ? current / total : 1f;
            _currentActive.SetProgress(progress);
        }

        private void HandleCastExecuted(string skillName)
        {
            if (!_isTelegraphing)
                HideCurrent();
        }

        private void HandleCastInterrupted()
        {
            HideCurrent();
        }

        private void ShowIndicatorForLogic(SkillPayload payload, TargetingLogic logic)
        {
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

        private void ApplyTint()
        {
            if (_currentActive == null) return;

            var go = (_currentActive as MonoBehaviour)?.gameObject;
            if (go == null) return;

            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.color = _indicatorTint;
            }
        }

        private void HideCurrent()
        {
            if (_currentActive != null)
            {
                _currentActive.Hide();
                _currentActive = null;
            }
            _trackedInstance = null;
            _isTelegraphing = false;
        }

        private void HideAll()
        {
            if (_activeCircle != null) _activeCircle.Hide();
            if (_activeLine != null) _activeLine.Hide();
            if (_activeCone != null) _activeCone.Hide();
            _currentActive = null;
            _trackedInstance = null;
            _isTelegraphing = false;
        }
        #endregion
    }
}
