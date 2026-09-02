using System.Collections;
using UnityEngine;

namespace CoreSystem.Visuals
{
    public class CombatJuiceDirector : MonoBehaviour
    {
        #region [Variables/Fields]
        private static CombatJuiceDirector _instance;

        [Header("Hit Stop Defaults")]
        [SerializeField] private float _defaultHitStopDuration = 0.05f;
        [SerializeField] private float _criticalHitStopDuration = 0.09f;

        [Header("Camera Shake (Fallback without Cinemachine)")]
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private float _defaultShakeIntensity = 0.15f;
        [SerializeField] private float _defaultShakeDuration = 0.12f;

        private Coroutine _hitStopCoroutine;
        private Coroutine _cameraShakeCoroutine;
        private Vector3 _originalCamLocalPos;
        private bool _isHitStopping;
        #endregion

        #region [Properties]
        public static CombatJuiceDirector Instance => _instance;
        public bool IsHitStopping => _isHitStopping;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }

            if (_targetCamera != null)
            {
                _originalCamLocalPos = _targetCamera.transform.localPosition;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Time.timeScale = 1f;
                _instance = null;
            }
        }
        #endregion

        #region [Public Methods]
        public void TriggerHitStop(float duration = -1f)
        {
            float durationToUse = duration > 0f ? duration : _defaultHitStopDuration;

            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
            }

            if (gameObject.activeInHierarchy)
            {
                _hitStopCoroutine = StartCoroutine(HitStopRoutine(durationToUse));
            }
        }

        public void TriggerCriticalHitStop()
        {
            TriggerHitStop(_criticalHitStopDuration);
        }

        public void TriggerCameraShake(float intensity = -1f, float duration = -1f)
        {
            float intensityToUse = intensity > 0f ? intensity : _defaultShakeIntensity;
            float durationToUse = duration > 0f ? duration : _defaultShakeDuration;

            if (_cameraShakeCoroutine != null)
            {
                StopCoroutine(_cameraShakeCoroutine);
                if (_targetCamera != null)
                {
                    _targetCamera.transform.localPosition = _originalCamLocalPos;
                }
            }

            if (gameObject.activeInHierarchy && _targetCamera != null)
            {
                _cameraShakeCoroutine = StartCoroutine(CameraShakeRoutine(intensityToUse, durationToUse));
            }
        }

        public void TriggerCombatImpact(bool isCritical = false, float shakeMultiplier = 1.0f)
        {
            if (isCritical)
            {
                TriggerCriticalHitStop();
                TriggerCameraShake(_defaultShakeIntensity * 1.8f * shakeMultiplier, _defaultShakeDuration * 1.5f);
            }
            else
            {
                TriggerHitStop(_defaultHitStopDuration);
                TriggerCameraShake(_defaultShakeIntensity * shakeMultiplier, _defaultShakeDuration);
            }
        }
        #endregion

        #region [Private Methods]
        private IEnumerator HitStopRoutine(float duration)
        {
            _isHitStopping = true;
            Time.timeScale = 0f;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f;
            _isHitStopping = false;
            _hitStopCoroutine = null;
        }

        private IEnumerator CameraShakeRoutine(float intensity, float duration)
        {
            if (_targetCamera == null) yield break;

            _originalCamLocalPos = _targetCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float currentIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);
                Vector2 randomOffset = Random.insideUnitCircle * currentIntensity;
                _targetCamera.transform.localPosition = _originalCamLocalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);
                yield return null;
            }

            _targetCamera.transform.localPosition = _originalCamLocalPos;
            _cameraShakeCoroutine = null;
        }
        #endregion
    }
}
