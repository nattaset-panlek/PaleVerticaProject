using CoreSystem.Data;
using CoreSystem.Quests;
using UnityEngine;

namespace CoreSystem.UI.Quests
{
    public sealed class QuestLogWindow
    {
        #region [Variables/Fields]
        private const string ModalName = "Quest Log";
        private const string ActiveHeading = "Active";
        private const string CompletedHeading = "Completed";
        private const string EmptyText = "No quests.";
        private static readonly Vector2 WindowSize = new Vector2(480f, 320f);
        private static readonly Vector2 MaxWindowSize = new Vector2(480f, 680f);
        private static QuestLogWindow _open;

        private UIGlobalModal _modal;
        #endregion

        #region [Public Methods]
        public static void Toggle()
        {
            if (_open != null && _open._modal != null)
            {
                _open.Close();
                return;
            }

            _open?.Unsubscribe();
            _open = new QuestLogWindow();
            _open.Show();
        }

        public static string NpcName(int characterId)
        {
            CharacterModel model = Database.GetCharacter(characterId);
            return model != null ? model.Name : string.Empty;
        }
        #endregion

        #region [Private Methods]
        private void Show()
        {
            _modal = UIBuilder.BuildGlobalModal(ModalName, null, WindowSize);
            if (_modal == null) return;

            _modal.DestroyOnClose = true;
            _modal.OnClosed += Unsubscribe;
            GameData.Player.Quests.OnQuestsChanged += Refresh;
            _modal.SetTitle(ModalName);
            Refresh();
        }

        private void Refresh()
        {
            if (_modal == null)
            {
                Unsubscribe();
                return;
            }
            _modal.Build(BuildUI).FitContent(true, MaxWindowSize, false);
        }

        private void BuildUI(UIBuilderContext ui)
        {
            PlayerQuests quests = GameData.Player.Quests;
            var tracked = quests.GetTracked(QuestDatabase.All);
            var completed = quests.GetCompleted(QuestDatabase.All);

            if (tracked.Count == 0 && completed.Count == 0)
            {
                ui.Label(EmptyText);
                return;
            }

            if (tracked.Count > 0) ui.Heading(ActiveHeading);
            foreach (QuestModel quest in tracked)
            {
                ui.Label(quest.Name);
                ui.Caption(quest.Description);
                foreach (string line in QuestTextFormula.ProgressLines(quests, quest, NpcName)) ui.Caption(line);
            }

            if (completed.Count > 0) ui.Heading(CompletedHeading);
            foreach (QuestModel quest in completed) ui.Caption(quest.Name);
        }

        private void Close()
        {
            if (UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            GameData.Player.Quests.OnQuestsChanged -= Refresh;
            if (_open == this) _open = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _open = null;
        }
        #endregion
    }
}
