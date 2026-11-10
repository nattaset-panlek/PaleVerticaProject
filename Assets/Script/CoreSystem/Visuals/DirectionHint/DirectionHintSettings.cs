using UnityEngine;

namespace CoreSystem.Visuals
{
    [CreateAssetMenu(fileName = "DirectionHintSettings", menuName = "Pale Vertica/Visuals/Direction Hint Settings")]
    public class DirectionHintSettings : ScriptableObject
    {
        #region [Variables/Fields]
        public const string ResourcePath = "Settings/DirectionHintSettings";
        public const string TexturePath = "Sprite/World/DirectionHint_SDF";
        public const string MaterialPath = "Materials/DirectionHint";
        public const string ShaderPath = "Shaders/World/DirectionHint";

        private static DirectionHintSettings _instance;

        [SerializeField] private Material _material;
        [Header("Size")]
        [Tooltip("Ring diameter = the longer side of the body collider x this value.")]
        [SerializeField, Min(0f)] private float _sizeMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float _minimumDiameter = 0.6f;
        [Header("Position")]
        [Tooltip("World units added to the collider centre. Negative Y moves the ring down toward the feet.")]
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [Tooltip("Extra offset as a fraction of the collider size, so tall and short characters move the same way.")]
        [SerializeField] private Vector2 _offsetBySize = Vector2.zero;
        [Header("Look")]
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private int _sortingOrderOffset = -1;
        #endregion

        #region [Properties]
        public static DirectionHintSettings Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<DirectionHintSettings>(ResourcePath);
                if (_instance == null) _instance = CreateInstance<DirectionHintSettings>();
                return _instance;
            }
        }

        public float SizeMultiplier => _sizeMultiplier;
        public float MinimumDiameter => _minimumDiameter;
        public Vector2 Offset => _offset;
        public Vector2 OffsetBySize => _offsetBySize;
        public Color Color => _color;
        public int SortingOrderOffset => _sortingOrderOffset;
        #endregion

        #region [Public Methods]
        public Material ResolveMaterial()
        {
            if (_material != null) return _material;
            Material asset = Resources.Load<Material>(MaterialPath);
            if (asset != null) return asset;
            Shader shader = Resources.Load<Shader>(ShaderPath);
            return shader != null ? new Material(shader) : null;
        }
        #endregion
    }
}
