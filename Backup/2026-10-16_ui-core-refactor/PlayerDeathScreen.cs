using CoreSystem.UI;
using UnityEngine;

namespace CoreSystem.Player
{
    public sealed class PlayerDeathScreen
    {
        #region [Variables/Fields]
        private UIGlobalModal _modal;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null;
        #endregion

        #region [Public Methods]
        public void Show(System.Action onRevive)
        {
            if (_modal != null) return;

            _modal = UIBuilder.BuildGlobalModal("Death Screen", ctx =>
            {
                ctx.Vertical("Div", content =>
                {
                    content.Text("Text", "YOU ARE DEAD")
                           .SetFontSize(48)
                           .SetTextColor(Color.red)
                           .SetFontStyle(TMPro.FontStyles.Bold)
                           .SetTextAlignment(TMPro.TextAlignmentOptions.Center)
                           .SetPreferredSize(-1, 80);

                    content.Text("Text", "The pale throne whispers your name...")
                           .SetFontSize(20)
                           .SetTextColor(Color.gray)
                           .SetTextAlignment(TMPro.TextAlignmentOptions.Center)
                           .SetPreferredSize(-1, 40);

                    content.Button("Primary Button", "Revive", () => onRevive?.Invoke()).SetPreferredSize(200, -1);
                }).SetChildAlignment(TextAnchor.MiddleCenter)
                  .SetFitParent();
            }, new Vector2(600, 200));

            if (_modal == null) return;
            _modal.OnClosed += () => { _modal = null; };
            _modal.SetCloseButtonVisible(false);
            _modal.SetScrollMovementType(UnityEngine.UI.ScrollRect.MovementType.Clamped);
            _modal.AutoFitWindow(fitWidth: false, fitHeight: true, paddingX: 0, paddingY: 80f);
        }

        public void Close()
        {
            if (_modal == null) return;
            UIManager.Instance.Close(_modal.WindowName);
            _modal = null;
        }
        #endregion
    }
}
