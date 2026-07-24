using UnityEngine;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;

namespace CoreSystem.Skills.Visuals
{
    public class LineIndicator : MonoBehaviour, ISkillIndicator
    {
        #region [Variables/Fields]
        [SerializeField] private Texture CuttingHead;
        [SerializeField] private Texture ArrowHead;

        [SerializeField] private SpriteRenderer _lineBox;
        [SerializeField] private SpriteRenderer _fillBox;
        
        private MaterialPropertyBlock _fillProps;
        private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");
        private static readonly int HeadTextrueID = Shader.PropertyToID("_HeadTex");
        private static readonly int WorldSizeId = Shader.PropertyToID("_WorldSize");
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _fillProps = new MaterialPropertyBlock();
        }
        #endregion

        #region [Public Methods]
        public void Show(SkillPayload payload, TargetingLogic logic)
        {
            gameObject.SetActive(true);

            float width = 1f;
            float length = 1f;
            Vector3 offset = Vector3.zero;
            Texture headTex = CuttingHead;

            if (logic is BoxTargeting box)
            {
                width = box.BoxSize.x;
                length = box.BoxSize.y;
                offset = box.Offset;
                headTex = CuttingHead;
            }
            else if (logic is LinePierceTargeting linePierce)
            {
                length = payload.AOERadius;
                width = linePierce.LineWidth * 2f;
                offset = new Vector3(0f, length / 2f, 0f);
                headTex = ArrowHead;
            }

            if (payload.SkillEffects.Exists(e => e is MovementEffectLogic m && (m.Type == MovementType.DashForward || m.Type == MovementType.DashToTarget)))
            {
                headTex = ArrowHead;
            }

            if (_lineBox != null)
            {
                _lineBox.transform.localScale = new Vector3(width, length, 1f);
                _lineBox.transform.localPosition = offset;

                _lineBox.GetPropertyBlock(_fillProps);
                _fillProps.SetTexture(HeadTextrueID, headTex);
                _fillProps.SetVector(WorldSizeId, new Vector4(width, length, 0f, 0f));
                _lineBox.SetPropertyBlock(_fillProps);

                if (_fillBox != null)
                {
                    _fillBox.gameObject.SetActive(true);
                    _fillBox.transform.localPosition = Vector3.zero;
                    _fillBox.transform.localScale = Vector3.one;

                    _fillBox.GetPropertyBlock(_fillProps);
                    _fillProps.SetFloat(FillAmountId, 0f);
                    _fillProps.SetVector(WorldSizeId, new Vector4(width, length, 0f, 0f));
                    _fillBox.SetPropertyBlock(_fillProps);
                }
            }
        }

        public void SetProgress(float progress)
        {
            if (_fillBox == null) return;
            
            _fillBox.GetPropertyBlock(_fillProps);
            _fillProps.SetFloat(FillAmountId, Mathf.Clamp01(progress));
            _fillBox.SetPropertyBlock(_fillProps);
        }

        public void StartExecution(SkillPayload payload)
        {
            // Shader-based fill handled externally via SetProgress
        }

        public void UpdateState(Vector3 origin, Vector3 target, Vector2 direction)
        {
            transform.position = origin;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion
    }

}
