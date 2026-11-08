using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Visuals.WeaponVfx
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSheetVfxController : MonoBehaviour, ISkillVisual
    {
        #region [Variables/Fields]
        private static readonly int ShadowColorId = Shader.PropertyToID("_ShadowColor");
        private static readonly int MidColorId = Shader.PropertyToID("_MidColor");
        private static readonly int HighlightColorId = Shader.PropertyToID("_HighlightColor");

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite[] _frames = new Sprite[0];
        [SerializeField] private float _framesPerSecond = 30f;
        [SerializeField] private float _visibleLength = 1.7f;
        [SerializeField] private bool _fitToBoxSkill = true;
        [SerializeField] private bool _destroyWhenFinished = true;
        [SerializeField] private VfxColorRamp _ramp = VfxColorRamp.Neutral;

        private MaterialPropertyBlock _block;
        private float _elapsed;
        private int _shownFrame = -1;
        #endregion

        #region [Properties]
        public float Duration => _frames.Length / Mathf.Max(1f, _framesPerSecond);
        public float VisibleLength => _visibleLength;
        public VfxColorRamp Ramp => _ramp;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            ApplyRamp();
            ShowFrame(0);
        }

        private void Update()
        {
            if (_frames.Length == 0) return;
            _elapsed += Time.deltaTime;
            int frame = Mathf.FloorToInt(_elapsed * _framesPerSecond);
            if (frame >= _frames.Length)
            {
                if (_destroyWhenFinished) Destroy(gameObject);
                else enabled = false;
                return;
            }
            ShowFrame(frame);
        }

        private void Reset()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
        #endregion

        #region [Public Methods]
        public void SetRamp(VfxColorRamp ramp)
        {
            _ramp = ramp;
            ApplyRamp();
        }

        public void Restart()
        {
            _elapsed = 0f;
            _shownFrame = -1;
            enabled = true;
            ShowFrame(0);
        }

        public void Initialize(SkillPayload payload)
        {
            if (payload == null) return;
            Vector3 aim = payload.GetAimDirection();
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

            if (_fitToBoxSkill && payload.SkillTargeting is BoxTargeting box)
            {
                Vector2 rotatedOffset = Quaternion.Euler(0f, 0f, angle - 90f) * box.Offset;
                Vector3 center = payload.OriginPosition + (Vector3)rotatedOffset;
                float scale = box.BoxSize.y / Mathf.Max(0.01f, _visibleLength);
                transform.SetPositionAndRotation(center, Quaternion.AngleAxis(angle, Vector3.forward));
                transform.localScale = new Vector3(scale, aim.x < 0f ? -scale : scale, 1f);
            }
            else if (aim.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            WeaponVfxTable table = WeaponVfxTable.Load();
            if (table != null) SetRamp(table.GetRamp(payload.SkillElement));
        }
        #endregion

        #region [Private Methods]
        private void ShowFrame(int frame)
        {
            if (_renderer == null || _frames.Length == 0 || frame == _shownFrame) return;
            _shownFrame = frame;
            _renderer.sprite = _frames[Mathf.Clamp(frame, 0, _frames.Length - 1)];
        }

        private void ApplyRamp()
        {
            if (_renderer == null) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_block);
            _block.SetColor(ShadowColorId, _ramp.Shadow);
            _block.SetColor(MidColorId, _ramp.Mid);
            _block.SetColor(HighlightColorId, _ramp.Highlight);
            _renderer.SetPropertyBlock(_block);
        }
        #endregion
    }
}
