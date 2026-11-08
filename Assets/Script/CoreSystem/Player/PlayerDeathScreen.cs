using System;
using CoreSystem.UI;
using CoreSystem.UI.Effects;
using TMPro;
using UnityEngine;

namespace CoreSystem.Player
{
    public sealed class PlayerDeathScreen
    {
        #region [Variables/Fields]
        public const string ModalName = "Death Screen";
        public const string HeaderTitle = "FALLEN";
        public const string Heading = "YOU ARE DEAD";

        public static string FogMaterialPath = "UI/FogAura/FogAura_Ink";

        private UIGlobalModal _modal;
        private FogAuraView _fog;
        private PlayerDeathInfo _info = PlayerDeathInfo.Sample;
        private Action _onRevive;
        private Action _onReturnToTown;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null;
        public PlayerDeathInfo Info => _info;
        public FogAuraView Fog => _fog;
        #endregion

        #region [Public Methods]
        public void Show(PlayerDeathInfo info, Action onRevive, Action onReturnToTown = null)
        {
            if (_modal != null) return;

            Prime(info, onRevive, onReturnToTown);
            _modal = UIBuilder.BuildGlobalModal(ModalName, null);
            if (_modal == null) return;

            Setup(_modal);
            _modal.OnClosed += HandleClosed;
            if (_fog != null) _fog.KeepBehind(_modal.WindowRect);
        }

        public void Prime(PlayerDeathInfo info, Action onRevive, Action onReturnToTown)
        {
            _info = info;
            _onRevive = onRevive;
            _onReturnToTown = onReturnToTown;
        }

        public void Setup(UIGlobalModal modal)
        {
            if (modal == null) return;
            UIMiniHeaderStyleRegistry.Prepare(modal, HeaderTitle)
                .SetDismissable(false)
                .SetCloseButtonVisible(false)
                .SetResizable(false)
                .SetDraggable(false)
                .SetScrollable(false)
                .SetWindowSize(new Vector2(UIMiniHeaderStyleRegistry.DeathWindowWidth, 260f))
                .Center()
                .Build(BuildUI)
                .FitContent(true, null, false);
            modal.CloseOnEscape = false;

            if (_fog == null) _fog = FogAuraView.Create(modal.WindowRect, FogAuraView.LoadMaterial(FogMaterialPath), true);
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
            ui.Label("HERE LIES").FontSize(11f).TextColor(UIMiniHeaderStyleRegistry.Sub).Fill();
            ui.Label(_info.Epitaph).FontSize(22f).Bold().TextColor(Color.white).Fill();
            var heading = ui.Label(Heading).FontSize(16f).Bold().TextColor(UIMiniHeaderStyleRegistry.DeathTitle).Fill();
            if (heading.TextComponent != null) heading.TextComponent.characterSpacing = UIMiniHeaderStyleRegistry.DeathTitleSpacing;

            ui.Divider(UIMiniHeaderStyleRegistry.Separator, 1f, UIMiniHeaderStyleRegistry.SeparatorColor).Fill();
            Row(ui, "FELL AT", _info.Location);
            Row(ui, "SLAIN BY", _info.KillerName);
            ui.Divider(UIMiniHeaderStyleRegistry.Separator, 1f, UIMiniHeaderStyleRegistry.SeparatorColor).Fill();

            ui.Row(footer =>
            {
                footer.Spacer().Fill();
                footer.Button(UIMiniHeaderStyleRegistry.ButtonSecondary, "Town", ReturnToTown).Interactable(_onReturnToTown != null);
                footer.Button(UIMiniHeaderStyleRegistry.ButtonPrimary, "Revive", Revive);
            }).Gap(UIMiniHeaderStyleRegistry.FooterGap).Align(TextAnchor.MiddleRight).Fill();
        }

        private static void Row(UIBuilderContext ui, string label, string value)
        {
            ui.Row(row =>
            {
                row.Label(label).FontSize(11f).TextColor(UIMiniHeaderStyleRegistry.Sub).Fill();
                row.Label(value).FontSize(13f).TextColor(UIMiniHeaderStyleRegistry.Text).TextAlign(TextAlignmentOptions.MidlineRight).Fill();
            }).Height(22f).Align(TextAnchor.MiddleLeft).Fill();
        }

        private void Revive()
        {
            _onRevive?.Invoke();
        }

        private void ReturnToTown()
        {
            _onReturnToTown?.Invoke();
        }

        private void HandleClosed()
        {
            if (_fog != null)
            {
                _fog.Dismiss();
                _fog = null;
            }
            if (_modal == null) return;
            _modal.OnClosed -= HandleClosed;
            _modal = null;
        }
        #endregion
    }
}
