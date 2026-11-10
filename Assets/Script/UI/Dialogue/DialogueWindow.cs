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
        private static int _openCount;
        private const string CloseLabel = "Close";
        private const string BackLabel = "Back";
        private const string AcceptLabel = "Accept";
        private const string TurnInLabel = "Complete";
        private const string RewardsHeading = "Rewards";
        private const string ContinueLabel = "Continue";
        private static readonly Vector2 WindowSize = new Vector2(520f, 240f);
        private static readonly Vector2 MaxWindowSize = new Vector2(520f, 620f);

        private readonly DialogueConversation _conversation;
        private UIGlobalModal _modal;
        #endregion

        #region [Public Methods]
        public static DialogueWindow Open(string speakerName, DialogueModel dialogue, int characterId)
        {
            DialogueConversation conversation = DialogueConversation.Start(speakerName, dialogue, characterId);
            if (conversation == null) return null;

            var window = new DialogueWindow(conversation);
            window.Show();
            return window;
        }
        #endregion

        #region [Private Methods]
        private DialogueWindow(DialogueConversation conversation)
        {
            _conversation = conversation;
            _conversation.OnChanged += Rebuild;
            _conversation.OnClosed += HandleClosed;
        }

        private void Show()
        {
            _modal = UIBuilder.BuildGlobalModal($"{ModalName} #{++_openCount}", null, WindowSize);
            if (_modal == null) return;

            _modal.DestroyOnClose = true;
            _modal.SetTitle(_conversation.Speaker)
                  .Build(BuildUI)
                  .FitContent(true, MaxWindowSize, false);
        }

        private void Rebuild()
        {
            if (_modal == null) return;
            _modal.Build(BuildUI).FitContent(true, MaxWindowSize, false);
        }

        private void BuildUI(UIBuilderContext ui)
        {
            switch (_conversation.Mode)
            {
                case DialogueConversationMode.QuestDialogue:
                    BuildNode(ui, ContinueLabel);
                    return;
                case DialogueConversationMode.Quest:
                    BuildQuest(ui, _conversation.SelectedQuest);
                    return;
                case DialogueConversationMode.Talk:
                    ui.Label(_conversation.CurrentNode.Text);
                    BuildQuestButtons(ui);
                    BuildChoices(ui, CloseLabel);
                    return;
            }
        }

        private void BuildNode(UIBuilderContext ui, string continueLabel)
        {
            ui.Label(_conversation.CurrentNode.Text);
            BuildChoices(ui, continueLabel);
        }

        private void BuildChoices(UIBuilderContext ui, string continueLabel)
        {
            IReadOnlyList<DialogueChoice> choices = _conversation.Choices;
            if (choices.Count == 0)
            {
                ui.Button(continueLabel, _conversation.Continue);
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                ui.Button(choices[i].Text, () => _conversation.Choose(index));
            }
        }

        private void BuildQuestButtons(UIBuilderContext ui)
        {
            foreach (QuestTalk talk in _conversation.QuestTalks)
            {
                QuestTalk captured = talk;
                ui.Button($"{talk.Quest.Name}: {talk.Quest.Objectives[talk.ObjectiveIndex].Text}", () => _conversation.PlayQuestTalk(captured));
            }

            foreach (QuestModel quest in _conversation.QuestOffers)
            {
                QuestModel captured = quest;
                ui.Button(quest.Name, () => _conversation.OpenOffer(captured));
            }
        }

        private void BuildQuest(UIBuilderContext ui, QuestModel quest)
        {
            PlayerQuests quests = GameData.Player.Quests;
            QuestStatus status = quests.GetStatus(quest);

            ui.Heading(quest.Name);
            ui.Label(quest.Description);

            foreach (string line in QuestTextFormula.ProgressLines(quests, quest, CoreSystem.UI.Quests.QuestWindow.NpcName)) ui.Caption(line);

            ui.Heading(RewardsHeading);
            foreach (string line in RewardLines(quest.Reward)) ui.Caption(line);

            if (status == QuestStatus.Available) ui.Button(AcceptLabel, _conversation.Accept);
            if (status == QuestStatus.ReadyToTurnIn) ui.Button(TurnInLabel, _conversation.TurnIn);
            ui.Button(BackLabel, _conversation.Back);
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

        private void HandleClosed()
        {
            _conversation.OnChanged -= Rebuild;
            _conversation.OnClosed -= HandleClosed;
            if (_modal != null && UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            _modal = null;
        }
        #endregion
    }
}
