using System;
using TMPro;
using UnityEngine;
using CoreSystem.UI;

namespace CoreSystem.UI.SystemMenu
{
    public sealed class SystemConfirmDialog
    {
        #region [Variables/Fields]
        public const string ModalName = "System Confirm";

        private UIGlobalModal _modal;
        private string _title;
        private string _message;
        private string _confirmLabel;
        private Action _onConfirm;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null && _modal.IsOpen;
        #endregion

        #region [Public Methods]
        public void Show(string title, string message, string confirmLabel, Action onConfirm)
        {
            Prime(title, message, confirmLabel);
            _onConfirm = onConfirm;
            _modal = UIGlobalModal.GetOrCreate(ModalName);
            Setup(_modal);
        }

        public void Prime(string title, string message, string confirmLabel)
        {
            _title = title;
            _message = message;
            _confirmLabel = confirmLabel;
        }

        public void Setup(UIGlobalModal modal)
        {
            if (modal == null) return;
            UIMiniHeaderStyleRegistry.Prepare(modal, _title)
                .SetScrollable(false)
                .SetWindowSize(UIMiniHeaderStyleRegistry.ConfirmWindowSize)
                .Center()
                .Build(BuildUI)
                .FitContent(true, null, false);
        }

        public void Close()
        {
            if (_modal != null && _modal.IsOpen) _modal.CloseSelf();
        }
        #endregion

        #region [Private Methods]
        private void BuildUI(UIBuilderContext ui)
        {
            ui.Label(_message ?? string.Empty).FontSize(16f).TextColor(Color.white).TextAlign(TextAlignmentOptions.Center).Fill();
            ui.Spacer(6f);
            ui.Row(row =>
            {
                row.Button(UIMiniHeaderStyleRegistry.ButtonSecondary, "Cancel", Close);
                row.Button(UIMiniHeaderStyleRegistry.ButtonPrimary, _confirmLabel, Confirm);
            }).Gap(UIMiniHeaderStyleRegistry.ConfirmButtonGap).Align(TextAnchor.MiddleCenter).Fill();
        }

        private void Confirm()
        {
            Action action = _onConfirm;
            Close();
            action?.Invoke();
        }
        #endregion
    }
}
