using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CoreSystem.Skills;
using CoreSystem.Entity;
using CoreSystem.Attributes;
using CoreSystem.UI.Utile;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillItemListUI : MonoBehaviour, IPointerClickHandler
    {
        #region [Variables/Fields]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _skillNameText;
        [SerializeField] private TMP_Text _skillLevelText;
        [SerializeField] private TMP_Text _dpsText;
        [SerializeField] private GameObject _selectedObject;

        private SkillInstance _skillInstance;
        private PlayerCharacter _player;
        private SkillManageWindow _ownerWindow;
        private System.Action _onRefreshCallback;
        #endregion

        #region [Properties]
        public SkillInstance SkillInstance => _skillInstance;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public void Setup(SkillInstance instance, PlayerCharacter player, SkillManageWindow window, System.Action onRefreshCallback)
        {
            _skillInstance = instance;
            _player = player;
            _ownerWindow = window;
            _onRefreshCallback = onRefreshCallback;

            UpdateUI();
        }

        public void SetSelected(bool isSelected)
        {
            if (_selectedObject != null)
            {
                _selectedObject.SetActive(isSelected);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_skillInstance == null || _player == null || _player.skillCaster == null) return;

            if (_ownerWindow != null)
            {
                _ownerWindow.SelectSkill(_skillInstance);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (ContextMenuManager.Instance == null) return;

                List<ContextMenuItem> items = new List<ContextMenuItem>();
                var equippedList = _player.skillCaster.EquippedSkills;

                for (int i = 0; i < equippedList.Count; i++)
                {
                    int slotIndex = i;
                    var currentEquipped = equippedList[i];
                    string slotLabel = (currentEquipped != null && currentEquipped.SkillData != null)
                        ? $"Slot #{i + 1} [{currentEquipped.SkillData.Name}]"
                        : $"Slot #{i + 1} None";

                    items.Add(new ContextMenuItem(slotLabel, () =>
                    {
                        _player.skillCaster.EquipSkill(_skillInstance, slotIndex);
                        _onRefreshCallback?.Invoke();
                    }));
                }

                ContextMenuManager.Instance.Show(items, eventData.position);
            }
        }
        #endregion

        #region [Private Methods]
        private void UpdateUI()
        {
            if (_skillInstance == null || _skillInstance.SkillData == null) return;

            if (_iconImage != null)
            {
                _iconImage.sprite = _skillInstance.SkillData.Icon;
                _iconImage.enabled = (_skillInstance.SkillData.Icon != null);
            }

            if (_skillNameText != null)
            {
                _skillNameText.text = _skillInstance.SkillData.Name;
            }

            if (_skillLevelText != null)
            {
                _skillLevelText.text = $"{_skillInstance.CurrentLevel:D2}";
            }

            if (_dpsText != null)
            {
                float dps = CalculateDPS60Sec();
                _dpsText.text = $"dps {dps:#,##0}";
            }
        }

        private float CalculateDPS60Sec()
        {
            if (_skillInstance == null || _skillInstance.SkillData == null || _player == null) return 0f;

            SkillPayload payload = _player.skillCaster.BuildPayload(_skillInstance);
            if (payload == null) return 0f;

            float basePower = payload.BaseDamage;
            if (_player.attributeSystem != null)
            {
                var stats = _player.attributeSystem;
                if (payload.DamageType == DamageType.Physical)
                {
                    basePower += stats.GetAttribute(AttributeType.ATK).CurrentValue;
                    basePower += stats.GetAttribute(AttributeType.WeaponATK).CurrentValue;
                }
                else
                {
                    basePower += stats.GetAttribute(AttributeType.MATK).CurrentValue;
                    basePower += stats.GetAttribute(AttributeType.WeaponMATK).CurrentValue;
                }

                float critRate = Mathf.Clamp(stats.GetAttribute(AttributeType.CriticalRate), 0f, 100f);
                float critBonus = stats.GetAttribute(AttributeType.CriticalDamage);
                float critMultiplier = 1f + (critRate / 100f) * (0.5f + critBonus);
                basePower *= critMultiplier;
            }

            int hits = Mathf.Max(1, payload.ProjectileCount);
            float damagePerCast = basePower * hits;

            if (_skillInstance.SkillData.IsContinuousArea)
            {
                float tickRate = Mathf.Max(0.1f, _skillInstance.SkillData.AreaTickRate);
                float ticksPerSec = 1f / tickRate;
                return damagePerCast * ticksPerSec;
            }
            else if (_skillInstance.SkillData.IsToggle)
            {
                return damagePerCast;
            }
            else
            {
                float castTime = payload.TotalCastTime;
                float cooldown = payload.Cooldown;
                float delay = payload.CastDelay;
                float cycleTime = Mathf.Max(0.1f, castTime + Mathf.Max(cooldown, delay));

                float castsIn60Sec = 60f / cycleTime;
                float totalDamage60Sec = damagePerCast * castsIn60Sec;
                return totalDamage60Sec / 60f;
            }
        }
        #endregion
    }
}
