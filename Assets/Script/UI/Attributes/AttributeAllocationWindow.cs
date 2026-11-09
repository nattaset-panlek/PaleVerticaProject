using CoreSystem.Data;
using CoreSystem.Entity;
using CoreSystem.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreSystem.UI.Attributes
{
    public sealed class AttributeAllocationWindow
    {
        #region [Variables/Fields]
        public const string ModalName = "Attribute Allocation";

        private static readonly Vector2 WindowSize = new Vector2(520f, 400f);
        private static readonly (AttributeType type, string effect)[] Rows =
        {
            (AttributeType.STR, "+1 ATK, +30 Weight"),
            (AttributeType.VIT, "+1% Max HP, +0.7 Status DEF"),
            (AttributeType.AGI, "+1 Flee, +1.5 ASPD"),
            (AttributeType.INT, "+1.5 MATK, +1% Max MP"),
            (AttributeType.DEX, "+1 Hit, +0.2 ATK and MATK"),
            (AttributeType.LUK, "+0.3 Crit, +0.33 ATK and MATK")
        };

        private readonly Character _player;
        private UIGlobalModal _modal;
        #endregion

        #region [Properties]
        private bool IsOpen => _modal != null && _modal.IsOpen;
        #endregion

        #region [Public Methods]
        public AttributeAllocationWindow(Character player)
        {
            _player = player;
        }

        public void Tick()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.cKey.wasPressedThisFrame || UIManager.IsTypingInField()) return;
            if (IsOpen) Close();
            else Open();
        }

        public void Close()
        {
            if (IsOpen) _modal.CloseSelf();
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AttachToPlayerCore()
        {
            PlayerCore.OnReady += core =>
            {
                AttributeAllocationWindow window = core.Controlled != null ? new AttributeAllocationWindow(core.Controlled) : null;
                core.OnPossessed += character => window = new AttributeAllocationWindow(character);
                core.OnReleased += _ =>
                {
                    window?.Close();
                    window = null;
                };
                core.OnTick += _ => window?.Tick();
            };
        }

        private void Open()
        {
            _modal = UIGlobalModal.GetOrCreate(ModalName);
            if (_modal == null) return;

            _modal.OnClosed -= HandleClosed;
            _modal.OnClosed += HandleClosed;
            GameData.Player.AttributePoints.OnChanged += Refresh;
            _player.expSystem.OnBaseLevelUp += HandleBaseLevelUp;

            UIMiniHeaderStyleRegistry.Prepare(_modal, "Attributes")
                .SetScrollable(false)
                .SetWindowSize(WindowSize)
                .Center()
                .Build(BuildUI);
        }

        private void HandleClosed()
        {
            _modal.OnClosed -= HandleClosed;
            GameData.Player.AttributePoints.OnChanged -= Refresh;
            _player.expSystem.OnBaseLevelUp -= HandleBaseLevelUp;
        }

        private void HandleBaseLevelUp(int level)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (IsOpen) _modal.Refresh();
        }

        private void BuildUI(UIBuilderContext ui)
        {
            ui.Label($"Attribute Points: {GameData.Player.AttributePoints.Available}").Bold().TextAlign(TextAlignmentOptions.Center).Fill();
            ui.Spacer(6f);
            foreach (var row in Rows) BuildRow(ui, row.type, row.effect);
        }

        private void BuildRow(UIBuilderContext ui, AttributeType type, string effect)
        {
            float value = _player.attributeSystem.GetAttribute(type).CurrentValue;
            ui.Row(row =>
            {
                row.Label(type.ToString()).Bold().Width(60f);
                row.Label($"{value:F0}").Bold().Width(60f);
                row.Caption(effect).Fill();
                row.Button("+", () => GameData.Player.AttributePoints.Spend(type), UIVariant.Primary).Width(44f);
            }).Gap(8f);
        }
        #endregion
    }
}
