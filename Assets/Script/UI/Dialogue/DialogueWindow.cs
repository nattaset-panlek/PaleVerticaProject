using CoreSystem.Dialogue;
using UnityEngine;

namespace CoreSystem.UI.Dialogue
{
    public sealed class DialogueWindow
    {
        #region [Variables/Fields]
        private const string ModalName = "Dialogue";
        private const string CloseLabel = "Close";
        private static readonly Vector2 WindowSize = new Vector2(520f, 240f);
        private static readonly Vector2 MaxWindowSize = new Vector2(520f, 520f);

        private readonly DialogueSession _session;
        private UIGlobalModal _modal;
        #endregion

        #region [Public Methods]
        public static DialogueWindow Open(string speakerName, DialogueModel dialogue)
        {
            var window = new DialogueWindow(dialogue);
            window.Show(speakerName);
            return window;
        }
        #endregion

        #region [Private Methods]
        private DialogueWindow(DialogueModel dialogue)
        {
            _session = new DialogueSession(dialogue);
        }

        private void Show(string speakerName)
        {
            if (_session.IsFinished) return;

            _modal = UIBuilder.BuildGlobalModal(ModalName, null, WindowSize);
            if (_modal == null) return;

            _modal.DestroyOnClose = true;
            _modal.SetTitle(speakerName)
                  .Build(BuildUI)
                  .FitContent(true, MaxWindowSize, false);
        }

        private void BuildUI(UIBuilderContext ui)
        {
            ui.Label(_session.Current.Text);

            var choices = _session.Current.Choices;
            if (choices == null || choices.Count == 0)
            {
                ui.Button(CloseLabel, Close);
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                ui.Button(choices[i].Text, () => Choose(index));
            }
        }

        private void Choose(int index)
        {
            _session.Choose(index);
            if (_session.IsFinished)
            {
                Close();
                return;
            }
            _modal.Build(BuildUI).FitContent(true, MaxWindowSize, false);
        }

        private void Close()
        {
            if (_modal != null && UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            _modal = null;
        }
        #endregion
    }
}
