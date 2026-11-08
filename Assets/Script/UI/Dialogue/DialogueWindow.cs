using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using CoreSystem.Quests;
using UnityEngine;

namespace CoreSystem.UI.Dialogue
{
    public sealed class DialogueWindow
    {
        #region [Variables/Fields]
        private const string ModalName = "Dialogue";
        private const string CloseLabel = "Close";
        private const string BackLabel = "Back";
        private const string AcceptLabel = "Accept";
        private const string TurnInLabel = "Complete";
        private const string RewardsHeading = "Rewards";
        private static readonly Vector2 WindowSize = new Vector2(520f, 240f);
        private static readonly Vector2 MaxWindowSize = new Vector2(520f, 620f);

        private readonly DialogueSession _session;
        private readonly int _characterId;
        private UIGlobalModal _modal;
        private QuestModel _selectedQuest;
        #endregion

        #region [Public Methods]
        public static DialogueWindow Open(string speakerName, DialogueModel dialogue, int characterId)
        {
            var window = new DialogueWindow(dialogue, characterId);
            window.Show(speakerName);
            return window;
        }
        #endregion

        #region [Private Methods]
        private DialogueWindow(DialogueModel dialogue, int characterId)
        {
            _session = new DialogueSession(dialogue);
            _characterId = characterId;
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

        private void Rebuild()
        {
            _modal.Build(BuildUI).FitContent(true, MaxWindowSize, false);
        }

        private void BuildUI(UIBuilderContext ui)
        {
            if (_selectedQuest != null)
            {
                BuildQuest(ui, _selectedQuest);
                return;
            }

            ui.Label(_session.Current.Text);
            BuildQuestButtons(ui);

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

        private void BuildQuestButtons(UIBuilderContext ui)
        {
            foreach (QuestModel quest in GameData.Player.Quests.GetOffers(_characterId, QuestDatabase.All))
            {
                QuestModel captured = quest;
                ui.Button(quest.Name, () => Select(captured));
            }
        }

        private void BuildQuest(UIBuilderContext ui, QuestModel quest)
        {
            PlayerQuests quests = GameData.Player.Quests;
            QuestStatus status = quests.GetStatus(quest);

            ui.Heading(quest.Name);
            ui.Label(quest.Description);

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjectiveModel objective = quest.Objectives[i];
                ui.Caption($"{objective.Text.ToString()} {quests.GetCount(quest.Id, i)}/{objective.Required}");
            }

            ui.Heading(RewardsHeading);
            foreach (string line in RewardLines(quest.Reward)) ui.Caption(line);

            if (status == QuestStatus.Available) ui.Button(AcceptLabel, () => Accept(quest));
            if (status == QuestStatus.ReadyToTurnIn) ui.Button(TurnInLabel, () => TurnIn(quest));
            ui.Button(BackLabel, () => Select(null));
        }

        private static IEnumerable<string> RewardLines(QuestRewardModel reward)
        {
            if (reward == null) yield break;
            if (reward.BaseExp > 0) yield return $"Base EXP {reward.BaseExp}";
            if (reward.JobExp > 0) yield return $"Job EXP {reward.JobExp}";
            if (reward.Items == null) yield break;

            foreach (QuestRewardItem item in reward.Items)
            {
                var template = Database.GetItem(item.ItemId);
                if (template != null) yield return $"{template.Name} x{item.Quantity}";
            }
        }

        private void Select(QuestModel quest)
        {
            _selectedQuest = quest;
            Rebuild();
        }

        private void Accept(QuestModel quest)
        {
            GameData.Player.Quests.Accept(quest);
            Select(null);
        }

        private void TurnIn(QuestModel quest)
        {
            QuestManager.TurnIn(quest);
            Select(null);
        }

        private void Choose(int index)
        {
            _session.Choose(index);
            if (_session.IsFinished)
            {
                Close();
                return;
            }
            Rebuild();
        }

        private void Close()
        {
            if (_modal != null && UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            _modal = null;
        }
        #endregion
    }
}
