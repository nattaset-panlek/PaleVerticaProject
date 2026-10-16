using System;
using System.Collections;
using System.Collections.Generic;
using CoreSystem.Agent;
using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;
using UnityEngine;

namespace CoreSystem.UI.Inspect
{
    public sealed class CharacterInspectWindow
    {
        #region [Variables/Fields]
        private const string StatsRegion = "inspect.stats";
        private static readonly Vector2 WindowSize = new Vector2(480f, 560f);
        private static readonly Vector2 MaxWindowSize = new Vector2(560f, 760f);
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
            CharacterRegistry.OnUnregistered += HandleUnregistered;
            OpenWindows[_runtimeId] = this;
        }

        public void Setup(UIGlobalModal modal)
        {
            modal.SetTitle($"Inspect - {_target.Name}")
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

            ui.Row(header =>
            {
                header.Title(_target.Name).FontSize(24f).Fill();
                var (relation, color) = Relation(ui.Theme);
                header.Badge(relation, color);
                header.Badge(ControllerLabel());
            }).Gap(6f);

            ui.Caption($"{FactionName()}  |  Base Lv {Value(AttributeType.BaseLevel):0}  |  Job Lv {Value(AttributeType.JobLevel):0}");
            ui.Region(StatsRegion, BuildStats);
        }

        private void BuildStats(UIBuilderContext ui)
        {
            if (_target == null) return;

            ui.Panel(vitals =>
            {
                vitals.Heading("Vitals");
                Bar(vitals, "HP", Value(AttributeType.HP), Value(AttributeType.MaxHP), UIVariant.Danger);
                Bar(vitals, "MP", Value(AttributeType.MP), Value(AttributeType.MaxMP), UIVariant.Primary);
            });

            ui.Panel(panel =>
            {
                panel.Heading("Attributes");
                panel.Grid(grid =>
                {
                    foreach (var type in CoreAttributes) StatCell(grid, type.ToString(), Value(type));
                }).Cell(68f, 52f).Gap(6f);
            });

            ui.Panel(panel =>
            {
                panel.Heading("Combat");
                panel.Grid(grid =>
                {
                    foreach (var stat in CombatAttributes) StatCell(grid, stat.Label, Value(stat.Type));
                }).Cell(132f, 52f).Gap(6f);
            });

            if (_target.Tags != CharacterTag.None)
            {
                ui.Panel(panel =>
                {
                    panel.Heading("Tags");
                    panel.Row(row =>
                    {
                        foreach (CharacterTag tag in Enum.GetValues(typeof(CharacterTag)))
                        {
                            if (_target.HasTag(tag)) row.Badge(tag.ToString());
                        }
                    }).Gap(6f);
                });
            }
        }

        private static void Bar(UIBuilderContext ui, string label, float current, float max, UIVariant variant)
        {
            ui.Row(row =>
            {
                row.Label(label).Bold().Width(36f);
                row.Progress(max > 0f ? current / max : 0f, variant).Fill();
                row.Caption($"{current:0} / {max:0}").Width(96f).TextAlign(TMPro.TextAlignmentOptions.MidlineRight);
            }).Gap(8f);
        }

        private static void StatCell(UIBuilderContext grid, string label, float value)
        {
            grid.Column(cell =>
            {
                cell.Caption(label).TextAlign(TMPro.TextAlignmentOptions.Center);
                cell.Label(FormatValue(value)).Bold().TextAlign(TMPro.TextAlignmentOptions.Center);
            }).Background(grid.Theme.PanelAlt).Padding(4).Gap(0f).Align(TextAnchor.MiddleCenter);
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
