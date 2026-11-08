using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillEffectPageView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private SkillManageWindow _window;
        [SerializeField] private TMP_Text _effectText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private ScrollRect _descriptionScroll;

        [Header("Keyword Colours")]
        [SerializeField] private Color _damageColor = new Color(0.878f, 0f, 0f, 1f);
        [SerializeField] private Color _timingColor = new Color(1f, 0.416f, 0f, 1f);
        [SerializeField] private Color _healColor = new Color(0.071f, 0.627f, 0.416f, 1f);

        [Header("Empty Text")]
        [SerializeField] private string _noEffectText = "This skill has no extra effect.";
        [SerializeField] private string _noDescriptionText = "No description yet.";

        private readonly List<string> _lines = new List<string>();
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
            ActiveSkill skill = instance != null ? instance.SkillData : null;

            if (_effectText != null) _effectText.text = skill != null ? BuildEffectText(skill) : string.Empty;
            if (_descriptionText != null)
            {
                _descriptionText.text = skill != null && !string.IsNullOrEmpty(skill.Description) ? skill.Description : (skill != null ? _noDescriptionText : string.Empty);
            }
            if (_descriptionScroll != null) _descriptionScroll.verticalNormalizedPosition = 1f;
        }
        #endregion

        #region [Private Methods]
        private string BuildEffectText(ActiveSkill skill)
        {
            _lines.Clear();
            if (skill.Effects != null)
            {
                for (int i = 0; i < skill.Effects.Count; i++)
                {
                    string line = Describe(skill.Effects[i], skill);
                    if (!string.IsNullOrEmpty(line)) _lines.Add(line);
                }
            }
            return _lines.Count > 0 ? string.Join("\n", _lines) : _noEffectText;
        }

        private string Describe(EffectLogic effect, ActiveSkill skill)
        {
            if (effect == null) return null;
            string target = effect.ApplyToCaster ? "yourself" : "the target";

            switch (effect)
            {
                case DamageEffect _:
                    return $"Deals {Paint(Number(skill.BaseDamage), _damageColor)} {Paint(DamageLabel(skill), _damageColor)} damage to {target}.";
                case HealEffect heal:
                    return $"Restores {Paint(Number(heal.BaseHealAmount), _healColor)} {heal.TargetAttribute} to {target}.";
                case ApplyTemporaryBuffEffect buff:
                    return DescribeTemporaryBuff(buff, target);
                case ApplyBuffEffect buff:
                    return $"Gives {target} {Paint(Modifier(buff.ModifierValue, buff.ModType), _damageColor)} {buff.TargetAttribute}.";
                case MovementEffectLogic movement:
                    return DescribeMovement(movement);
                case SummonObjectEffect summon:
                    string summoned = summon.Prefab != null ? summon.Prefab.name : "an object";
                    return summon.Lifetime > 0f
                        ? $"Summons {Paint(summoned, _damageColor)} for {Paint(Seconds(summon.Lifetime), _timingColor)}."
                        : $"Summons {Paint(summoned, _damageColor)}.";
                default:
                    return null;
            }
        }

        private string DescribeTemporaryBuff(ApplyTemporaryBuffEffect buff, string target)
        {
            string name = string.IsNullOrEmpty(buff.BuffName) ? buff.TargetAttribute.ToString() : buff.BuffName;
            var text = new StringBuilder();
            text.Append($"Inflicts {Paint(name, _damageColor)} on {target}: {Paint(Modifier(buff.ModifierValue, buff.ModType), _damageColor)} {buff.TargetAttribute} for {Paint(Seconds(buff.Duration), _timingColor)}");
            if (buff.MaxStacks > 1) text.Append($", up to {Paint(buff.MaxStacks.ToString(), _timingColor)} stacks");
            text.Append('.');
            return text.ToString();
        }

        private string DescribeMovement(MovementEffectLogic movement)
        {
            switch (movement.Type)
            {
                case MovementType.StunTarget:
                    return $"{Paint("Stuns", _damageColor)} the target for {Paint(Seconds(movement.StunDuration), _timingColor)}.";
                case MovementType.DashForward:
                    return $"Dashes forward {Paint(Meters(movement.Distance), _timingColor)}.";
                case MovementType.DashToTarget:
                    return "Dashes to the target.";
                case MovementType.TeleportToTarget:
                    return "Teleports to the target.";
                case MovementType.PushTargetAway:
                    return $"{Paint("Pushes", _damageColor)} the target {Paint(Meters(movement.Distance), _timingColor)} away.";
                case MovementType.PullTargetIn:
                    return $"{Paint("Pulls", _damageColor)} the target {Paint(Meters(movement.Distance), _timingColor)} closer.";
                case MovementType.PullToTargetPosition:
                    return $"{Paint("Pulls", _damageColor)} enemies to the target point.";
                default:
                    return null;
            }
        }

        private static string DamageLabel(ActiveSkill skill)
        {
            string type = skill.SkillDamageType.ToString().ToLowerInvariant();
            return skill.SkillElement == ElementType.Neutral ? type : $"{skill.SkillElement} {type}";
        }

        private static string Modifier(float value, ModifierType type)
        {
            string sign = value >= 0f ? "+" : "-";
            float amount = Mathf.Abs(value);
            switch (type)
            {
                case ModifierType.PercentAdd:
                case ModifierType.PercentMult:
                    return $"{sign}{Number(amount * (amount <= 1f ? 100f : 1f))}%";
                default:
                    return $"{sign}{Number(amount)}";
            }
        }

        private static string Seconds(float value) => Mathf.Approximately(value, 1f) ? "1 second" : $"{Number(value)} seconds";

        private static string Meters(float value) => $"{Number(value)} m";

        private static string Number(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value)) ? value.ToString("0") : value.ToString("0.##");
        }

        private static string Paint(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
        #endregion
    }
}
