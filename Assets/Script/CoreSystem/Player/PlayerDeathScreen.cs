using System;
using CoreSystem.UI;
using TMPro;
using UnityEngine;

namespace CoreSystem.Player
{
    public sealed class PlayerDeathScreen
    {
        #region [Variables/Fields]
        private const string ModalName = "Death Screen";
        private static readonly Vector2 MaxWindowSize = new Vector2(520f, 420f);

        private UIGlobalModal _modal;
        private Action _onRevive;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null;
        #endregion

        #region [Public Methods]
        public void Show(Action onRevive)
        {
            if (_modal != null) return;

            _onRevive = onRevive;
            _modal = UIBuilder.BuildGlobalModal(ModalName, null);
            if (_modal == null) return;

            Setup(_modal);
            _modal.OnClosed += HandleClosed;
        }

        public void Setup(UIGlobalModal modal)
        {
            modal.SetTitle(string.Empty)
                 .SetDismissable(false)
                 .SetResizable(false)
                 .SetDraggable(false)
                 .Center()
                 .Build(BuildUI)
                 .FitContent(true, MaxWindowSize);
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
            ui.Column(body =>
            {
                body.Title("YOU ARE DEAD").TextAlign(TextAlignmentOptions.Center).TextColor(body.Theme.Danger);
                body.Caption("The pale throne whispers your name...").TextAlign(TextAlignmentOptions.Center);
                body.Spacer(12f);
                body.Row(actions =>
                {
                    actions.Spacer();
                    actions.Button("Revive", () => _onRevive?.Invoke(), UIVariant.Primary).Size(200f, 44f);
                    actions.Spacer();
                });
            }).Padding(24, 24, 16, 20).Gap(8f);
        }

        private void HandleClosed()
        {
            if (_modal == null) return;
            _modal.OnClosed -= HandleClosed;
            _modal = null;
        }
        #endregion
    }
}
