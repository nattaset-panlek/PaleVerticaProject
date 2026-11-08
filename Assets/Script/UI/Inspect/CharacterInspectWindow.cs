using System;
using System.Collections;
using System.Collections.Generic;
using CoreSystem.Agent;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.UI.Inspect
{
    public sealed class CharacterInspectWindow
    {
        #region [Variables/Fields]
        private const string StatsRegion = "inspect.stats";
        private const string WindowTitle = "Inspect";
        private const string VitalsStyle = "Status Vitals";
        private const string AttrCellStyle = "Attr Cell";
        private const int CombatColumns = 3;
        private const float ChipRadius = 10f;
        private static readonly Vector2 WindowSize = new Vector2(560f, 760f);
        private static readonly Vector2 MaxWindowSize = new Vector2(560f, 860f);
        private static readonly Dictionary<int, CharacterInspectWindow> OpenWindows = new Dictionary<int, CharacterInspectWindow>();

        private static readonly AttributeType[] CoreAttributes =
        {
            AttributeType.STR, AttributeType.AGI, AttributeType.VIT,
            AttributeType.INT, AttributeType.DEX, AttributeType.LUK
        };

        private static readonly (string Label, AttributeType Type)[] CombatAttributes =
        {
            ("ATK", AttributeType.ATK), ("MATK", AttributeType.MATK), ("DEF", AttributeType.Defense),
            ("MDEF", AttributeType.MDEF), ("HIT", AttributeType.Hit), ("FLEE", AttributeType.Flee),
            ("CRIT", AttributeType.CriticalRate), ("ASPD", AttributeType.ASPD), ("MOVE", AttributeType.MoveSpeed)
        };

        private readonly Character _target;
        private readonly int _runtimeId;
        private UIGlobalModal _modal;
        private bool _refreshQueued;
        #endregion

        #region [Properties]
        public Character Target => _target;
        public bool IsOpen => _modal != null;
        #endregion

        #region [Public Methods]
        public CharacterInspectWindow(Character target)
        {
            _target = target;
            _runtimeId = target != null ? target.RuntimeId : 0;
        }

        public static CharacterInspectWindow Open(Character target)
        {
            if (target == null) return null;

            if (OpenWindows.TryGetValue(target.RuntimeId, out var existing) && existing.IsOpen)
            {
                existing._modal.transform.SetAsLastSibling();
                return existing;
            }

            var window = new CharacterInspectWindow(target);
            window.Show();
            return window.IsOpen ? window : null;
        }

        public void Show()
        {
            if (_modal != null || _target == null) return;

            _modal = UIBuilder.BuildGlobalModal($"Inspect #{_runtimeId}", null, WindowSize);
            if (_modal == null) return;

            _modal.DestroyOnClose = true;
            Setup(_modal);
            _modal.OnClosed += HandleClosed;
            if (_target.attributeSystem != null) _target.attributeSystem.OnAttributesChanged += HandleAttributesChanged;
            if (_target.expSystem != null) _target.expSystem.OnBaseExpChanged += HandleExpChanged;
            CharacterRegistry.OnUnregistered += HandleUnregistered;
            OpenWindows[_runtimeId] = this;
        }

        public void Setup(UIGlobalModal modal)
        {
            modal.SetTitle(WindowTitle)
                 .SetMinSize(new Vector2(380f, 300f))
                 .Build(BuildUI)
                 .FitContent(true, MaxWindowSize, false);
        }

        public void Close()
        {
            if (_modal == null) return;
            if (UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            HandleClosed();
        }
        #endregion

        #region [Private Methods]
        private void BuildUI(UIBuilderContext ui)
        {
            if (_target == null)
            {
                ui.Caption("This character no longer exists.");
                return;
            }

            ui.Column(header =>
            {
                header.Title(_target.Name.ToUpperInvariant());
                header.Row(badges =>
                {
                    var (relation, color) = Relation(ui.Theme);
                    Chip(badges, relation, color);
                    Chip(badges, ControllerLabel(), ui.Theme.Selected);
                }).Gap(6f);
                header.Caption($"{FactionName()}  ·  Base Lv {Value(AttributeType.BaseLevel):0}  ·  Job Lv {Value(AttributeType.JobLevel):0}  ·  #{_runtimeId}");
            }).Gap(6f);

            ui.Region(StatsRegion, BuildStats);
        }

        private void BuildStats(UIBuilderContext ui)
        {
            if (_target == null) return;

            ui.SectionHeader("VITALS");
            BuildVitals(ui);

            ui.SectionHeader("ATTRIBUTES");
            ui.Row(row =>
            {
                for (int i = 0; i < CoreAttributes.Length; i++)
                {
                    if (i > 0) row.Spacer();
                    row.Custom(AttrCellStyle)
                       .SetText("Label", CoreAttributes[i].ToString())
                       .SetText("Value", FormatValue(Value(CoreAttributes[i])));
                }
            });

            ui.SectionHeader("COMBAT");
            ui.Column(table =>
            {
                for (int start = 0; start < CombatAttributes.Length; start += CombatColumns)
                {
                    int first = start;
                    table.Row(row =>
                    {
                        int end = Mathf.Min(first + CombatColumns, CombatAttributes.Length);
                        for (int i = first; i < end; i++)
                        {
                            row.KeyValue(CombatAttributes[i].Label, FormatValue(Value(CombatAttributes[i].Type))).Fill();
                        }
                    }).Gap(8f);
                }
            }).Gap(8f);

            if (_target.Tags == CharacterTag.None) return;

            ui.SectionHeader("TAGS");
            ui.Row(row =>
            {
                foreach (CharacterTag tag in Enum.GetValues(typeof(CharacterTag)))
                {
                    if (tag != CharacterTag.None && _target.HasTag(tag)) Chip(row, tag.ToString(), TagColor(tag, ui.Theme));
                }
            }).Gap(6f);
        }

        private void BuildVitals(UIBuilderContext ui)
        {
            float hp = Value(AttributeType.HP), maxHp = Value(AttributeType.MaxHP);
            float mp = Value(AttributeType.MP), maxMp = Value(AttributeType.MaxMP);
            long exp = _target.CurrentBaseExp, nextExp = _target.NextBaseExp;

            ui.Custom(VitalsStyle)
              .SetFill("HP Fill", Ratio(hp, maxHp))
              .SetText("HP Value", $"{hp:F0}/{maxHp:F0}")
              .SetFill("MP Fill", Ratio(mp, maxMp))
              .SetText("MP Value", $"{mp:F0}/{maxMp:F0}")
              .SetFill("EXP Fill", Ratio(exp, nextExp))
              .SetText("EXP Value", $"{NumberFormatter.FormatWithCommas(exp)} / {NumberFormatter.FormatWithCommas(nextExp)}")
              .SetText("Base", ((int)Value(AttributeType.BaseLevel)).ToString("000"))
              .SetText("Job", ((int)Value(AttributeType.JobLevel)).ToString("000"));
        }

        private static float Ratio(float current, float max)
        {
            return max > 0f ? current / max : 0f;
        }

        private static void Chip(UIBuilderContext ui, string label, Color? color)
        {
            ui.Badge(label.ToUpperInvariant(), color).Padding(10, 10, 3, 3).CornerRadius(ChipRadius);
        }

        private static Color? TagColor(CharacterTag tag, UITheme theme)
        {
            switch (tag)
            {
                case CharacterTag.Boss: return theme.Warning;
                case CharacterTag.Monster: return theme.Selected;
                default: return null;
            }
        }

        private static string FormatValue(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value)) ? value.ToString("0") : value.ToString("0.##");
        }

        private float Value(AttributeType type)
        {
            var attr = _target.attributeSystem != null ? _target.attributeSystem.GetAttribute(type) : null;
            return attr != null ? attr.CurrentValue : 0f;
        }

        private (string, Color?) Relation(UITheme theme)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null) return ("Unknown", null);
            if (player == _target) return ("You", theme.Primary);
            if (FactionDatabase.IsHostile(player.Faction, _target.Faction)) return ("Hostile", theme.Danger);
            if (FactionDatabase.IsAlly(player.Faction, _target.Faction)) return ("Ally", theme.Success);
            return ("Neutral", null);
        }

        private string ControllerLabel()
        {
            if (PlayerCore.IsPlayer(_target)) return "Player";
            if (AgentCore.Instance != null && AgentCore.Instance.GetHandle(_target) != null) return "AI";
            return _target.DefaultController.ToString();
        }

        private string FactionName()
        {
            var faction = FactionDatabase.GetFaction(_target.Faction);
            return faction != null && !string.IsNullOrEmpty(faction.Name) ? faction.Name : $"Faction {_target.Faction}";
        }

        private void HandleAttributesChanged()
        {
            if (_modal == null || _refreshQueued || !_modal.isActiveAndEnabled) return;
            _refreshQueued = true;
            _modal.StartCoroutine(RefreshNextFrame());
        }

        private void HandleExpChanged(long current, long next)
        {
            HandleAttributesChanged();
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;
            _refreshQueued = false;
            if (_modal != null) _modal.Refresh(StatsRegion);
        }

        private void HandleUnregistered(Character character)
        {
            if (character == _target) Close();
        }

        private void HandleClosed()
        {
            if (_modal == null) return;
            _modal.OnClosed -= HandleClosed;
            _modal = null;
            _refreshQueued = false;
            if (_target != null && _target.attributeSystem != null) _target.attributeSystem.OnAttributesChanged -= HandleAttributesChanged;
            if (_target != null && _target.expSystem != null) _target.expSystem.OnBaseExpChanged -= HandleExpChanged;
            CharacterRegistry.OnUnregistered -= HandleUnregistered;
            if (OpenWindows.TryGetValue(_runtimeId, out var current) && current == this) OpenWindows.Remove(_runtimeId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OpenWindows.Clear();
        }
        #endregion
    }
}
