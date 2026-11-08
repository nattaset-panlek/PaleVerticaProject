using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Combat;
using CoreSystem.Skills;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillAdvancePageView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private SkillManageWindow _window;
        [SerializeField] private SkillAttributeRowView _rowPrefab;
        [SerializeField] private SkillAttributeRowView _sectionPrefab;
        [SerializeField] private Transform _rowContainer;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Color _changeColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private readonly List<SkillAttributeRowView> _rows = new List<SkillAttributeRowView>();
        private readonly List<SkillAttributeRowView> _sections = new List<SkillAttributeRowView>();
        private int _usedRows;
        private int _usedSections;
        private int _order;
        private string _pendingSection;
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
            _usedSections = 0;
            _order = 0;

            ActiveSkill skill = instance != null ? instance.SkillData : null;
            if (skill != null)
            {
                SkillPayload payload = SkillBasicPageView.PreviewPayload(_window, instance);
                BuildCasting(skill, payload);
                BuildDamage(skill, payload, instance);
                BuildArea(skill, payload);
                BuildCost(skill, payload, instance);
            }

            Hide(_rows, _usedRows);
            Hide(_sections, _usedSections);
            if (_scroll != null) _scroll.verticalNormalizedPosition = 1f;
        }
        #endregion

        #region [Private Methods]
        private void BuildCasting(ActiveSkill skill, SkillPayload payload)
        {
            Section("Casting");
            Change("Variable Cast", skill.BaseVCT, payload.VCT, " s");
            Change("Fixed Cast", skill.BaseFCT, payload.FCT, " s");
            Change("Total Cast Time", skill.BaseVCT + skill.BaseFCT, payload.TotalCastTime, " s");
            Change("Cast Delay", skill.BaseCastDelay, payload.CastDelay, " s");
            Change("Cooldown", skill.BaseCooldown, payload.Cooldown, " s");
        }

        private void BuildDamage(ActiveSkill skill, SkillPayload payload, SkillInstance instance)
        {
            Section("Damage");
            Text("Damage Type", skill.SkillDamageType.ToString().ToLowerInvariant());
            Text("Element", skill.SkillElement.ToString().ToLowerInvariant());
            Change("Base Damage", skill.BaseDamage, skill.BaseDamage, string.Empty);

            var stats = _window != null && _window.TargetPlayer != null ? _window.TargetPlayer.attributeSystem : null;
            if (skill.Scalings != null)
            {
                for (int i = 0; i < skill.Scalings.Count; i++)
                {
                    OutputScaling scaling = skill.Scalings[i];
                    if (scaling == null) continue;
                    float stat = stats != null ? stats.GetAttribute(scaling.ScalingAttribute) : 0f;
                    float bonus = SkillValueFormula.Scaling(stat, scaling.AttributeMultiplier, scaling.FlatBonus, scaling.LevelMultiplier, instance.CurrentLevel, scaling.GlobalMultiplier);
                    Text($"{scaling.ScalingAttribute} → {scaling.TargetField}", $"+{SkillAttributeRowView.Number(bonus)}  <color=#{ColorUtility.ToHtmlStringRGB(_changeColor)}>x{SkillAttributeRowView.Number(scaling.AttributeMultiplier)}</color>");
                }
            }

            if (!Mathf.Approximately(payload.DamageMultiplier, 1f)) Text("Damage Multiplier", $"{SkillAttributeRowView.Number(payload.DamageMultiplier * 100f)}%");
            Change("Final Damage", skill.BaseDamage, payload.BaseDamage * payload.DamageMultiplier, string.Empty);
            Change("Heal", 0f, payload.Heal, string.Empty);
        }

        private void BuildArea(ActiveSkill skill, SkillPayload payload)
        {
            Section("Area");
            Change("AOE Radius", skill.BaseAOERadius, payload.AOERadius, " m");
            Change("Cast Range", payload.CastRange, payload.CastRange, " m");
            Change("Projectiles", skill.BaseProjectileCount, payload.ProjectileCount, string.Empty);
            if (skill.IsContinuousArea)
            {
                Change("Area Duration", skill.AreaDuration, skill.AreaDuration, " s");
                Change("Tick Rate", skill.AreaTickRate, skill.AreaTickRate, " s");
            }
        }

        private void BuildCost(ActiveSkill skill, SkillPayload payload, SkillInstance instance)
        {
            Section("Cost & Support");
            Change("Mana Cost", skill.BaseManaCost, payload.ManaCost, string.Empty);
            Text("Support Slots", $"{CountLinked(instance)} / {skill.MaxSupportSlots}");
            Text("Skill Level", instance.CurrentLevel.ToString());
        }

        private static int CountLinked(SkillInstance instance)
        {
            int count = 0;
            if (instance.LinkedModifiers == null) return count;
            for (int i = 0; i < instance.LinkedModifiers.Count; i++)
            {
                if (instance.LinkedModifiers[i] != null) count++;
            }
            return count;
        }

        private void Section(string title)
        {
            _pendingSection = title;
        }

        private void Change(string label, float baseValue, float finalValue, string unit)
        {
            if (SkillAttributeRowView.IsEmpty(baseValue, finalValue)) return;
            Text(label, SkillAttributeRowView.FormatChange(baseValue, finalValue, unit, _changeColor));
        }

        private void Text(string label, string value)
        {
            if (_pendingSection != null)
            {
                Take(_sectionPrefab, _sections, ref _usedSections)?.Setup(_pendingSection, string.Empty);
                _pendingSection = null;
            }
            Take(_rowPrefab, _rows, ref _usedRows)?.Setup(label, value);
        }

        private SkillAttributeRowView Take(SkillAttributeRowView prefab, List<SkillAttributeRowView> pool, ref int used)
        {
            if (prefab == null || _rowContainer == null) return null;

            SkillAttributeRowView view;
            if (used < pool.Count)
            {
                view = pool[used];
            }
            else
            {
                view = Instantiate(prefab, _rowContainer);
                pool.Add(view);
            }

            used++;
            view.gameObject.SetActive(true);
            view.transform.SetSiblingIndex(_order++);
            return view;
        }

        private static void Hide(List<SkillAttributeRowView> pool, int used)
        {
            for (int i = used; i < pool.Count; i++)
            {
                if (pool[i] != null) pool[i].gameObject.SetActive(false);
            }
        }
        #endregion
    }
}
