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

                    content.Button("Button", "Revive", () => onRevive?.Invoke()).SetPreferredSize(200, 60).SetColor(new Color(0.6f, 0.1f, 0.1f));
                }).SetChildAlignment(TextAnchor.MiddleCenter)
                  .SetFitParent();
            }, new Vector2(600, 200));

            if (_modal == null) return;
            _modal.SetScrollMovementType(UnityEngine.UI.ScrollRect.MovementType.Clamped);
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
