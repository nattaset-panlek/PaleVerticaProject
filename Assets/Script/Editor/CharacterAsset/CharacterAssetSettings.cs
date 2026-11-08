using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    [CreateAssetMenu(fileName = "CharacterAssetSettings", menuName = "Pale Vertica/Character/Character Asset Settings")]
    public class CharacterAssetSettings : ScriptableObject
    {
        #region [Variables/Fields]
        public const string BodyPartName = "Body";
        public const string TemplateFolder = "Docs/02-Technical/Character/Template";
        private static CharacterAssetSettings _instance;

        [Header("Canvas (px, origin bottom-left)")]
        [SerializeField] private Vector2Int _canvasSize = new Vector2Int(400, 400);
        [SerializeField, Tooltip("Tallest allowed canvas; extra height is room above the character for tall headwear")] private int _maxCanvasHeight = 480;
        [SerializeField] private float _pixelsPerUnit = 100f;
        [SerializeField] private float _groundY = 22f;
        [SerializeField] private float _groundTolerance = 14f;
        [SerializeField] private string _ignoredLayerPrefix = "_";

        [Header("Checks")]
        [SerializeField] private float _jointAlphaThreshold = 0.5f;
        [SerializeField] private float _anchorTolerancePx = 0.5f;

        [Header("PSD Importer")]
        [SerializeField] private int _mosaicPadding = 8;
        [SerializeField] private FilterMode _filterMode = FilterMode.Point;
        [SerializeField] private bool _mipmapEnabled = true;
        [SerializeField] private TextureImporterCompression _compression = TextureImporterCompression.Uncompressed;
        [SerializeField] private SpriteMeshType _meshType = SpriteMeshType.FullRect;
        [SerializeField] private SpriteAlignment _documentAlignment = SpriteAlignment.BottomCenter;
        [SerializeField] private bool _automaticReslice = true;

        [Header("Output")]
        [SerializeField] private string _basePrefabPath = "Assets/Addressable/Prefab/Entity/Enemy/Skeletom.prefab";
        [SerializeField] private string _partExportFolder = "Assets/Image/Character/Parts";
        [SerializeField] private string _psbFolder = "Assets/Image/Character";

        [Header("Sorting Groups")]
        [SerializeField] private string _modelPath = "Model";
        [SerializeField] private int _modelSortingOrder = 6;
        [SerializeField] private string _shadowPath = "Model/Shadow";
        [SerializeField] private int _shadowSortingOrder = 1;

        [Header("Parts, top to bottom of the layer panel")]
        [SerializeField] private List<CharacterPartSpec> _parts = CreateDefaultParts();
        #endregion

        #region [Properties]
        public static CharacterAssetSettings Instance
        {
            get
            {
                if (_instance != null) return _instance;
                string[] guids = AssetDatabase.FindAssets("t:" + nameof(CharacterAssetSettings));
                if (guids.Length > 0)
                    _instance = AssetDatabase.LoadAssetAtPath<CharacterAssetSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (_instance == null)
                {
                    _instance = CreateInstance<CharacterAssetSettings>();
                    _instance.hideFlags = HideFlags.DontSave;
                }
                return _instance;
            }
        }

        public Vector2Int CanvasSize => _canvasSize;
        public int MaxCanvasHeight => Mathf.Max(_maxCanvasHeight, _canvasSize.y);
        public float PixelsPerUnit => _pixelsPerUnit;
        public float GroundY => _groundY;
        public float GroundTolerance => _groundTolerance;
        public string IgnoredLayerPrefix => _ignoredLayerPrefix;
        public float JointAlphaThreshold => _jointAlphaThreshold;
        public float AnchorTolerancePx => _anchorTolerancePx;
        public int MosaicPadding => _mosaicPadding;
        public FilterMode TextureFilterMode => _filterMode;
        public bool MipmapEnabled => _mipmapEnabled;
        public TextureImporterCompression Compression => _compression;
        public SpriteMeshType MeshType => _meshType;
        public SpriteAlignment DocumentAlignment => _documentAlignment;
        public bool AutomaticReslice => _automaticReslice;
        public string BasePrefabPath => _basePrefabPath;
        public string PartExportFolder => _partExportFolder;
        public string PsbFolder => _psbFolder;
        public string ModelPath => _modelPath;
        public int ModelSortingOrder => _modelSortingOrder;
        public string ShadowPath => _shadowPath;
        public int ShadowSortingOrder => _shadowSortingOrder;
        public IReadOnlyList<CharacterPartSpec> Parts => _parts;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            _parts = CreateDefaultParts();
        }
        #endregion

        #region [Public Methods]
        public CharacterPartSpec FindPart(string name)
        {
            return _parts.Find(p => p.Name == name);
        }

        public bool IsIgnoredName(string layerName)
        {
            return !string.IsNullOrEmpty(_ignoredLayerPrefix) && layerName.StartsWith(_ignoredLayerPrefix);
        }
        #endregion

        #region [Private Methods]
        private static List<CharacterPartSpec> CreateDefaultParts()
        {
            return new List<CharacterPartSpec>
            {
                new CharacterPartSpec("ArmL", new Vector2(274.49f, 128.02f), new Vector2(245.38f, 149.20f), new RectInt(232, 92, 85, 72), 11, "Model/Body/ArmL/ArmL", true),
                new CharacterPartSpec("Head", new Vector2(206.93f, 268.54f), new Vector2(206.97f, 168.40f), new RectInt(62, 144, 290, 249), 10, "Model/Body/Head/Head", true),
                new CharacterPartSpec("Body", new Vector2(206.50f, 127.00f), new Vector2(206.97f, 125.72f), new RectInt(149, 59, 115, 136), 9, "Model/Body/Body", false),
                new CharacterPartSpec("ArmR", new Vector2(139.49f, 128.02f), new Vector2(171.63f, 149.20f), new RectInt(97, 92, 85, 72), 8, "Model/Body/ArmR/ArmR", true),
                new CharacterPartSpec("LegL", new Vector2(235.48f, 57.00f), new Vector2(228.31f, 83.04f), new RectInt(213, 22, 45, 70), 7, "Model/LegL/LegL", true),
                new CharacterPartSpec("LegR", new Vector2(175.38f, 57.00f), new Vector2(174.96f, 83.04f), new RectInt(153, 22, 45, 70), 6, "Model/LegR/LegR", true),
            };
        }
        #endregion
    }
}
