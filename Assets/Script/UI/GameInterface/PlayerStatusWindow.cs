using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Entity;
using CoreSystem.Utility;
using CoreSystem.Data;

namespace CoreSystem.UI.GameInterface
{
    public class PlayerStatusWindow : UIWindow
    {
        #region [Variables/Fields]
        [Header("UI Reference")]
        [SerializeField] private Image _healthBar;
        [SerializeField] private Image _manaBar;

        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _manaText;

        [SerializeField] private TMP_Text _baseLevel;
        [SerializeField] private TMP_Text _jobLevel;

        [SerializeField] private TMP_Text _expText;
        [SerializeField] private Image _expBar;

        [SerializeField] private Character _targetPlayer;
        #endregion

        #region [Properties]
        public Image HealthBar => _healthBar;
        public Image ManaBar => _manaBar;
        public TMP_Text HealthText => _healthText;
        public TMP_Text ManaText => _manaText;
        public TMP_Text BaseLevel => _baseLevel;
        public TMP_Text JobLevel => _jobLevel;
        public TMP_Text ExpText => _expText;
        public Image ExpBar => _expBar;
        public Character TargetPlayer => _targetPlayer;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            if (_targetPlayer == null)
            {
                _targetPlayer = GameData.Player.PlayerCharacterModel;
            }
        }

        private void Update()
        {
            UpdateUI();
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            if (data is Character player)
            {
                _targetPlayer = player;
            }
            UpdateUI();
        }

        public void Initialize(Character player)
        {
            _targetPlayer = player;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_targetPlayer == null)
            {
                _targetPlayer = GameData.Player.PlayerCharacterModel;
            }

            if (_targetPlayer == null || _targetPlayer.attributeSystem == null) return;

            if (_healthBar != null)
            {
                float hp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                float maxHp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.MaxHP).CurrentValue;
                _healthBar.fillAmount = maxHp > 0 ? (hp / maxHp) : 0f;
            }

            if (_manaBar != null)
            {
                float mp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                float maxMp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.MaxMP).CurrentValue;
                _manaBar.fillAmount = maxMp > 0 ? (mp / maxMp) : 0f;
            }

            if (_healthText != null)
            {
                float hp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
                float maxHp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.MaxHP).CurrentValue;
                _healthText.text = $"{hp:F0}/{maxHp:F0}";
            }

            if (_manaText != null)
            {
                float mp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.MP).CurrentValue;
                float maxMp = _targetPlayer.attributeSystem.GetAttribute(AttributeType.MaxMP).CurrentValue;
                _manaText.text = $"{mp:F0}/{maxMp:F0}";
            }

            if (_baseLevel != null)
            {
                int baseLevel = (int)(_targetPlayer.attributeSystem.GetAttribute(AttributeType.BaseLevel)?.CurrentValue ?? 1);
                _baseLevel.text = $"{baseLevel.ToString("000")}";
            }

            if (_jobLevel != null)
            {
                int jobLevel = (int)(_targetPlayer.attributeSystem.GetAttribute(AttributeType.JobLevel)?.CurrentValue ?? 1);
                _jobLevel.text = $"{jobLevel.ToString("000")}";
            }

            if (_expText != null && _targetPlayer.expSystem != null)
            {
                float exp = _targetPlayer.expSystem.CurrentBaseExp;
                float nextLevelExp = _targetPlayer.expSystem.GetNextBaseExp((int)_targetPlayer.attributeSystem.GetAttribute(AttributeType.BaseLevel));
                _expText.text = $"{NumberFormatter.FormatWithCommas((long)exp)} / {NumberFormatter.FormatWithCommas((long)nextLevelExp)}";
            }

            if (_expBar != null && _targetPlayer.expSystem != null)
            {
                float exp = _targetPlayer.expSystem.CurrentBaseExp;
                float nextLevelExp = _targetPlayer.expSystem.GetNextBaseExp((int)_targetPlayer.attributeSystem.GetAttribute(AttributeType.BaseLevel));
                _expBar.fillAmount = nextLevelExp > 0 ? (exp / nextLevelExp) : 0f;
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
