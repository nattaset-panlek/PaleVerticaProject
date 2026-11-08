using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Skills;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillBasicPageView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private SkillManageWindow _window;
        [SerializeField] private SkillAttributeRowView _rowPrefab;
        [SerializeField] private Transform _rowContainer;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Color _changeColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private readonly List<SkillAttributeRowView> _rows = new List<SkillAttributeRowView>();
        private int _usedRows;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            _window = GetComponentInParent<SkillManageWindow>(true);
        }

        private void Awake()
        {
            if (_window == null) _window = GetComponentInParent<SkillManageWindow>(true);
        }

        private void OnEnable()
        {
            if (_window == null) return;
            _window.OnSelectedSkillChanged += Render;
            Render(_window.SelectedSkillInstance);
        }

        private void OnDisable()
        {
            if (_window != null) _window.OnSelectedSkillChanged -= Render;
        }
        #endregion

        #region [Public Methods]
        public void Render(SkillInstance instance)
        {
            _usedRows = 0;
            ActiveSkill skill = instance != null ? instance.SkillData : null;
            if (skill != null)
            {
                SkillPayload payload = PreviewPayload(_window, instance);

                AddRow("Damage", skill.BaseDamage, payload.BaseDamage * payload.DamageMultiplier, string.Empty);
                AddRow("Cooldown", skill.BaseCooldown, payload.Cooldown, " s");
                AddRow("Mana Cost", skill.BaseManaCost, payload.ManaCost, string.Empty);
                AddRow("Cast Time", skill.BaseVCT + skill.BaseFCT, payload.TotalCastTime, " s");
                AddRow("Cast Delay", skill.BaseCastDelay, payload.CastDelay, " s");
                AddRow("AOE Radius", skill.BaseAOERadius, payload.AOERadius, " m");
                AddRow("Projectiles", skill.BaseProjectileCount, payload.ProjectileCount, string.Empty);
                AddRow("Heal", 0f, payload.Heal, string.Empty);
            }

            for (int i = _usedRows; i < _rows.Count; i++)
            {
                if (_rows[i] != null) _rows[i].gameObject.SetActive(false);
            }
            if (_scroll != null) _scroll.verticalNormalizedPosition = 1f;
        }

        public static SkillPayload PreviewPayload(SkillManageWindow window, SkillInstance instance)
        {
            SkillCaster caster = window != null && window.TargetPlayer != null ? window.TargetPlayer.skillCaster : null;
            return caster != null ? caster.BuildPayload(instance) : DefaultPayload(instance.SkillData);
        }
        #endregion

        #region [Private Methods]
        private void AddRow(string label, float baseValue, float finalValue, string unit)
        {
            if (SkillAttributeRowView.IsEmpty(baseValue, finalValue)) return;
            if (_rowPrefab == null || _rowContainer == null) return;

            SkillAttributeRowView row;
            if (_usedRows < _rows.Count)
            {
                row = _rows[_usedRows];
            }
            else
            {
                row = Instantiate(_rowPrefab, _rowContainer);
                _rows.Add(row);
            }

            row.gameObject.SetActive(true);
            row.Setup(label, SkillAttributeRowView.FormatChange(baseValue, finalValue, unit, _changeColor));
            _usedRows++;
        }

        private static SkillPayload DefaultPayload(ActiveSkill skill)
        {
            return new SkillPayload
            {
                BaseDamage = skill.BaseDamage,
                AOERadius = skill.BaseAOERadius,
                ProjectileCount = skill.BaseProjectileCount,
                CastDelay = skill.BaseCastDelay,
                Cooldown = skill.BaseCooldown,
                ManaCost = skill.BaseManaCost,
                TotalCastTime = skill.BaseVCT + skill.BaseFCT
            };
        }

        #endregion
    }
}
