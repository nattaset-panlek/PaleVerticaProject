using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Components
{
    [AddComponentMenu("UI/Custom/UI Line Renderer")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        #region Variables/Fields

        [SerializeField] private List<Vector2> _points     = new List<Vector2>();
        [SerializeField] private float         _lineWidth  = 4f;
        [SerializeField, Range(0f, 0.5f)]
        private float _antialias = 0.05f;
        [SerializeField] private bool _closed = false;

        private static readonly string SHADER_NAME = "Custom/UI/UILine";
        private Material _lineMaterial;

        #endregion

        #region Properties

        public List<Vector2> Points
        {
            get => _points;
            set { _points = value; SetVerticesDirty(); }
        }

        public float LineWidth
        {
            get => _lineWidth;
            set { _lineWidth = Mathf.Max(0f, value); SetVerticesDirty(); }
        }

        public float Antialias
        {
            get => _antialias;
            set
            {
                _antialias = Mathf.Clamp01(value);
                if (material != null) material.SetFloat("_AntialiasWidth", _antialias);
            }
        }

        public bool Closed
        {
            get => _closed;
            set { _closed = value; SetVerticesDirty(); }
        }

        public override Material material
        {
            get
            {
                if (_lineMaterial == null) CreateMaterial();
                return _lineMaterial;
            }
            set => _lineMaterial = value;
        }

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            CreateMaterial();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (_lineMaterial != null)
                _lineMaterial.SetFloat("_AntialiasWidth", _antialias);
            SetVerticesDirty();
        }
#endif

        #endregion

        #region Public Methods

        public void SetPoints(IEnumerable<Vector2> pts)
        {
            _points.Clear();
            _points.AddRange(pts);
            SetVerticesDirty();
        }

        public void AddPoint(Vector2 pt)
        {
            _points.Add(pt);
            SetVerticesDirty();
        }

        public void ClearPoints()
        {
            _points.Clear();
            SetVerticesDirty();
        }

        #endregion

        #region Private Methods

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_points == null || _points.Count < 2) return;

            int count = _closed ? _points.Count : _points.Count - 1;
            float half = _lineWidth * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Vector2 p0 = _points[i];
                Vector2 p1 = _points[(i + 1) % _points.Count];

                Vector2 dir  = (p1 - p0).normalized;
                Vector2 perp = new Vector2(-dir.y, dir.x) * half;

                int baseIdx = vh.currentVertCount;

                // 4 vertices per segment — UV.y: 0 & 1 at edges (shader reads this for AA)
                AddVertex(vh, p0 - perp, new Vector2(0, 0));
                AddVertex(vh, p0 + perp, new Vector2(0, 1));
                AddVertex(vh, p1 - perp, new Vector2(1, 0));
                AddVertex(vh, p1 + perp, new Vector2(1, 1));

                vh.AddTriangle(baseIdx,     baseIdx + 1, baseIdx + 2);
                vh.AddTriangle(baseIdx + 1, baseIdx + 3, baseIdx + 2);
            }
        }

        private void AddVertex(VertexHelper vh, Vector2 pos, Vector2 uv)
        {
            vh.AddVert(new Vector3(pos.x, pos.y, 0), color, uv);
        }

        private void CreateMaterial()
        {
            Shader shader = Shader.Find(SHADER_NAME);
            if (shader == null)
            {
                Debug.LogWarning($"[UILineRenderer] ไม่พบ Shader '{SHADER_NAME}' — ใช้ Default UI Shader แทน");
                _lineMaterial = new Material(Shader.Find("UI/Default"));
            }
            else
            {
                _lineMaterial = new Material(shader);
            }

            _lineMaterial.SetFloat("_AntialiasWidth", _antialias);
        }

        #endregion
    }
}
