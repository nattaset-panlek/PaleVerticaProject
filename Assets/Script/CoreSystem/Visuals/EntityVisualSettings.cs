using UnityEngine;

namespace CoreSystem.Visuals
{
    [CreateAssetMenu(fileName = "EntityVisualSettings", menuName = "Realm of Hero/Visuals/Entity Visual Settings")]
    public class EntityVisualSettings : ScriptableObject
    {
        #region [Variables/Fields]
        private static EntityVisualSettings _instance;
        private const string ResourcePath = "Settings/EntityVisualSettings";
        private const string FallbackResourcePath = "EntityVisualSettings";

        [Header("Master Rendering Asset")]
        [SerializeField] private Material _masterMaterial;

        [Header("Hit Flash Settings")]
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private Color _defaultFlashColor = Color.white;
        [SerializeField] private Color _criticalFlashColor = new Color(1.0f, 0.85f, 0.3f, 1.0f);

        [Header("Status Tint Settings")]
        [SerializeField] private Color _poisonColor = new Color(0.2f, 1.0f, 0.25f, 1.0f);
        [SerializeField] private Color _freezeColor = new Color(0.35f, 0.75f, 1.0f, 1.0f);
        [SerializeField] private Color _burnColor = new Color(1.0f, 0.45f, 0.1f, 1.0f);
        [SerializeField] private Color _stunColor = new Color(1.0f, 0.95f, 0.4f, 1.0f);

        [Header("Dissolve Settings")]
        [SerializeField] private float _defaultDissolveDuration = 1.0f;
        [SerializeField] private Color _dissolveEdgeColor = new Color(1.0f, 0.6f, 0.1f, 1.0f);
        [SerializeField] private float _dissolveEdgeWidth = 0.05f;
        #endregion

        #region [Properties]
        public static EntityVisualSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EntityVisualSettings>(ResourcePath);
                    if (_instance == null)
                    {
                        _instance = Resources.Load<EntityVisualSettings>(FallbackResourcePath);
                    }
                    if (_instance == null)
                    {
                        _instance = CreateInstance<EntityVisualSettings>();
                    }
                }
                return _instance;
            }
        }

        public Material MasterMaterial => _masterMaterial;
        public float FlashDuration => _flashDuration;
        public Color DefaultFlashColor => _defaultFlashColor;
        public Color CriticalFlashColor => _criticalFlashColor;
        public Color PoisonColor => _poisonColor;
        public Color FreezeColor => _freezeColor;
        public Color BurnColor => _burnColor;
        public Color StunColor => _stunColor;
        public float DefaultDissolveDuration => _defaultDissolveDuration;
        public Color DissolveEdgeColor => _dissolveEdgeColor;
        public float DissolveEdgeWidth => _dissolveEdgeWidth;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        #endregion
    }
}
