using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;

namespace CoreSystem.Skills.Visuals
{
    public class ConeIndicator : MonoBehaviour, ISkillIndicator
    {
        #region [Variables/Fields]

        [SerializeField] private SpriteRenderer _coneMesh;
        [SerializeField] private SpriteRenderer _fillMesh;
        
        private MaterialPropertyBlock _props;
        private Coroutine _scaleRoutine;
        private static readonly int ConeAngleId = Shader.PropertyToID("_ConeAngle");
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _props = new MaterialPropertyBlock();
        }
        #endregion


        #region [Public Methods]
        public void Show(SkillPayload payload, TargetingLogic logic)
        {
            gameObject.SetActive(true);
            
            // Safety: Stop any running routine from a previous usage
            if (_scaleRoutine != null)
            {
                StopCoroutine(_scaleRoutine);
                _scaleRoutine = null;
            }

            if (logic is ConeTargeting cone)
            {
                float radius = payload.AOERadius;
                Vector3 scale = new Vector3(radius * 2f, radius * 2f, 1f);
                
                if (_coneMesh != null)
                {
                    _coneMesh.transform.localScale = scale;
                    
                    _coneMesh.GetPropertyBlock(_props);
                    _props.SetFloat(ConeAngleId, cone.ConeAngle);
                    _coneMesh.SetPropertyBlock(_props);

                    if (_fillMesh != null)
                    {
                        _fillMesh.gameObject.SetActive(true);
                        _fillMesh.transform.localPosition = Vector3.zero;
                        _fillMesh.transform.localScale = Vector3.zero;
                        
                        _fillMesh.GetPropertyBlock(_props);
                        _props.SetFloat(ConeAngleId, cone.ConeAngle);
                        _fillMesh.SetPropertyBlock(_props);
                    }
                }
            }
        }

        public void SetProgress(float progress)
        {
            if (_fillMesh == null) return;
            float s = Mathf.Clamp01(progress);
            _fillMesh.transform.localScale = new Vector3(s, s, 1f);
        }

        public void StartExecution(SkillPayload payload)
        {
            if (payload.TotalCastTime > 0)
            {
                if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
                _scaleRoutine = StartCoroutine(ScaleRoutine(payload.TotalCastTime));
            }
        }

        private System.Collections.IEnumerator ScaleRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetProgress(elapsed / duration);
                yield return null;
            }
            SetProgress(1f);
            _scaleRoutine = null;
        }

        public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
        {
            transform.position = origin;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        public void Hide()
        {
            if (_scaleRoutine != null)
            {
                StopCoroutine(_scaleRoutine);
                _scaleRoutine = null;
            }
            gameObject.SetActive(false);
        }
        #endregion
    }

}
