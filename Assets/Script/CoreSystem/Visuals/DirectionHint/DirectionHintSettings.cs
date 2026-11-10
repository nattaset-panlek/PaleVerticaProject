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
        [SerializeField, Min(0f)] private float _sizeMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float _minimumDiameter = 0.6f;
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
