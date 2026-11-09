using System;
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

        private readonly DialogueSession _session;
        private readonly int _characterId;
        private UIGlobalModal _modal;
        private QuestModel _selectedQuest;
        private DialogueSession _questDialogue;
        private Action _questDialogueDone;
        private int _questDialogueId;
        private readonly int _dialogueId;
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
            _dialogueId = dialogue != null ? dialogue.Id : 0;
            _characterId = characterId;
        }

        private void Show(string speakerName)
        {
            if (_session.IsFinished) return;

            _modal = UIBuilder.BuildGlobalModal($"{ModalName} #{++_openCount}", null, WindowSize);
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
            if (_questDialogue != null)
            {
                BuildQuestDialogue(ui);
                return;
            }

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
                ui.Button(CloseLabel, FinishDialogue);
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
            PlayerQuests quests = GameData.Player.Quests;
            foreach (QuestTalk talk in quests.GetDialogueTalks(_characterId, QuestDatabase.All))
            {
                QuestTalk captured = talk;
                ui.Button($"{talk.Quest.Name}: {talk.Quest.Objectives[talk.ObjectiveIndex].Text}", () => PlayObjectiveDialogue(captured));
            }

            foreach (QuestModel quest in quests.GetOffers(_characterId, QuestDatabase.All))
            {
                QuestModel captured = quest;
                ui.Button(quest.Name, () => OpenOffer(captured));
            }
        }

        private void BuildQuestDialogue(UIBuilderContext ui)
        {
            ui.Label(_questDialogue.Current.Text);

            var choices = _questDialogue.Current.Choices;
            if (choices == null || choices.Count == 0)
            {
                ui.Button(ContinueLabel, FinishQuestDialogue);
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                ui.Button(choices[i].Text, () => ChooseQuestDialogue(index));
            }
        }

        private void PlayObjectiveDialogue(QuestTalk talk)
        {
            PlayQuestDialogue(talk.Quest.Objectives[talk.ObjectiveIndex].DialogueId, () =>
            {
                GameData.Player.Quests.CompleteDialogueObjective(talk.Quest, talk.ObjectiveIndex);
                Select(null);
            });
        }

        private void OpenOffer(QuestModel quest)
        {
            bool isTurnIn = GameData.Player.Quests.GetStatus(quest) == QuestStatus.ReadyToTurnIn;
            PlayQuestDialogue(isTurnIn ? quest.TurnInDialogueId : quest.AcceptDialogueId, () => Select(quest));
        }

        private void PlayQuestDialogue(int dialogueId, Action onDone)
        {
            var session = new DialogueSession(DialogueDatabase.Get(dialogueId));
            if (session.IsFinished)
            {
                onDone();
                return;
            }

            _questDialogue = session;
            _questDialogueId = dialogueId;
            _questDialogueDone = onDone;
            Rebuild();
        }

        private void ChooseQuestDialogue(int index)
        {
            _questDialogue.Choose(index);
            if (_questDialogue.IsFinished)
            {
                _questDialogue = null;
                _questDialogueDone = null;
            }
            Rebuild();
        }

        private void FinishQuestDialogue()
        {
            Action done = _questDialogueDone;
            _questDialogue = null;
            _questDialogueDone = null;
            done?.Invoke();
            DialogueManager.RaiseDialogueEnded(_questDialogueId);
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

        private void FinishDialogue()
        {
            Close();
            DialogueManager.RaiseDialogueEnded(_dialogueId);
        }

        private void Close()
        {
            if (_modal != null && UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            _modal = null;
        }
        #endregion
    }
}
