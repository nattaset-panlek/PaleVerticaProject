#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using CoreSystem.Entity;
using CoreSystem.Quests;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace ScenarioTesting.Actions
{
    internal static class DialogueInput
    {
        #region [Variables/Fields]
        public const string ContinueLabel = "Continue";
        public const string CloseLabel = "Close";
        public const string AcceptLabel = "Accept";
        public const string TalkLabel = "Talk";
        public const string TurnInLabel = "Complete";
        public const string BackLabel = "Back";
        private const int MaxClicks = 20;
        #endregion

        #region [Public Methods]
        public static IEnumerator ClickLabel(ScenarioRun run, string label, float max = 5f)
        {
            Button button = null;
            yield return run.WaitFor(() => (button = ScenarioInput.FindButton(label)) != null, max,
                $"a button \"{label}\" (visible: {string.Join(", ", ScenarioInput.VisibleButtonLabels())})");
            if (button == null) yield break;
            yield return ScenarioButtonPress.Press(run, button);
            yield return null;
        }

        public static IEnumerator PlayThrough(ScenarioRun run, int preferredChoice)
        {
            for (int i = 0; i < MaxClicks; i++)
            {
                DialogueConversation conversation = ScenarioProbe.Conversation;
                if (conversation == null || conversation.Mode != DialogueConversationMode.QuestDialogue) yield break;

                int count = conversation.Choices.Count;
                string label = count == 0 ? ContinueLabel : $"{conversation.Choices[Mathf.Clamp(preferredChoice, 0, count - 1)].Text}";
                yield return ClickLabel(run, label);
                if (run.StepFailed) yield break;
            }
            run.FailStep("the quest dialogue did not end");
        }

        public static DialogueConversation Require(ScenarioRun run)
        {
            DialogueConversation conversation = ScenarioProbe.Conversation;
            if (conversation == null || conversation.IsClosed) run.FailStep("no conversation is open");
            return conversation;
        }
        #endregion
    }

    [ScenarioAction("TalkTo", "NPC", "{\"do\":\"TalkTo\",\"character\":2004}  walks near the NPC and clicks it")]
    internal sealed class TalkToAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            Character npc = run.EntityFrom(step, "character", null);
            if (npc == null)
            {
                run.FailStep($"no character {step.String("character")} nearby");
                yield break;
            }

            DialogueConversation before = ScenarioProbe.Conversation;
            string ready = $"{run.Lab.Diagnose(npc)}, interact options {TalkState.Options(npc)}";
            yield return run.Lab.ClickCharacter(npc, MouseButton.Left);
            if (run.StepFailed) yield break;
            string clicked = run.Lab.LastClickReport;

            float menuWait = Time.time + 1.5f;
            while (Time.time < menuWait && !TalkState.IsTalking(npc, before)) yield return null;
            if (!TalkState.IsTalking(npc, before) && ScenarioInput.FindButton(DialogueInput.TalkLabel) != null)
            {
                run.Log("the click opened a menu, choosing Talk");
                yield return DialogueInput.ClickLabel(run, DialogueInput.TalkLabel);
            }

            yield return run.WaitFor(() => TalkState.IsTalking(npc, before), step.Float("max", 20f),
                $"a conversation with {npc.name} ({clicked}; visible buttons: {string.Join(", ", ScenarioInput.VisibleButtonLabels())})");
            if (!run.StepFailed) yield break;
            run.Log($"before the click: {ready}");
            run.Log($"at the end: {run.Lab.Diagnose(npc)}, interact options {TalkState.Options(npc)}, conversation {TalkState.Describe(ScenarioProbe.Conversation)}");
        }
        #endregion
    }

    internal static class TalkState
    {
        #region [Public Methods]
        public static bool IsTalking(Character npc, DialogueConversation before)
        {
            DialogueConversation conversation = ScenarioProbe.Conversation;
            return npc != null && conversation != null && conversation != before && !conversation.IsClosed && conversation.CharacterId == npc.CharacterId;
        }

        public static string Options(Character npc)
        {
            var core = CoreSystem.Player.PlayerCore.Instance;
            Character player = ScenarioProbe.Player;
            if (npc == null || core == null || player == null) return "unknown";
            var options = CoreSystem.Player.PlayerInteraction.FilterInteractOptions(CoreSystem.Interaction.InteractionCore.GetOptions(player, npc, core));
            if (options.Count == 0) return "none";
            var labels = new System.Collections.Generic.List<string>();
            foreach (var option in options) labels.Add(option.Label);
            return string.Join(", ", labels);
        }

        public static string Describe(DialogueConversation conversation)
        {
            if (conversation == null) return "none";
            return $"character {conversation.CharacterId}, mode {conversation.Mode}, closed {conversation.IsClosed}";
        }
        #endregion
    }

    [ScenarioAction("Choose", "NPC", "{\"do\":\"Choose\",\"index\":0}  or {\"label\":\"Goodbye.\"}  clicks a dialogue choice")]
    internal sealed class ChooseAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            string label = step.String("label");
            if (label == null)
            {
                DialogueConversation conversation = DialogueInput.Require(run);
                if (conversation == null) yield break;
                int index = step.Int("index");
                if (index < 0 || index >= conversation.Choices.Count)
                {
                    run.FailStep($"choice {index} does not exist ({conversation.Choices.Count} choices)");
                    yield break;
                }
                label = $"{conversation.Choices[index].Text}";
            }
            yield return DialogueInput.ClickLabel(run, label);
        }
        #endregion
    }

    [ScenarioAction("Continue", "NPC", "{\"do\":\"Continue\"}  clicks Continue or Close")]
    internal sealed class ContinueAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            bool hasContinue = ScenarioInput.FindButton(DialogueInput.ContinueLabel) != null;
            yield return DialogueInput.ClickLabel(run, hasContinue ? DialogueInput.ContinueLabel : DialogueInput.CloseLabel);
        }
        #endregion
    }

    [ScenarioAction("CloseDialogue", "NPC", "{\"do\":\"CloseDialogue\"}  clicks Back, Continue, Close or the last choice until the conversation ends")]
    internal sealed class CloseDialogueAction : ScenarioAction
    {
        #region [Variables/Fields]
        private const int MaxClicks = 10;
        #endregion

        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            for (int i = 0; i < MaxClicks && IsOpen(); i++)
            {
                string label = NextLabel(ScenarioProbe.Conversation);
                if (label == null || ScenarioInput.FindButton(label) == null) break;
                yield return DialogueInput.ClickLabel(run, label, 2f);
                if (run.StepFailed) yield break;
            }
            if (!IsOpen()) yield break;

            run.Log($"no button ends the conversation (visible: {string.Join(", ", ScenarioInput.VisibleButtonLabels())}), closing it");
            ScenarioProbe.Conversation.Close();
            yield return null;
        }
        #endregion

        #region [Private Methods]
        private static bool IsOpen()
        {
            DialogueConversation conversation = ScenarioProbe.Conversation;
            return conversation != null && !conversation.IsClosed;
        }

        private static string NextLabel(DialogueConversation conversation)
        {
            switch (conversation.Mode)
            {
                case DialogueConversationMode.Quest: return DialogueInput.BackLabel;
                case DialogueConversationMode.QuestDialogue:
                    return conversation.Choices.Count == 0 ? DialogueInput.ContinueLabel : $"{conversation.Choices[conversation.Choices.Count - 1].Text}";
                default:
                    return conversation.Choices.Count == 0 ? DialogueInput.CloseLabel : $"{conversation.Choices[conversation.Choices.Count - 1].Text}";
            }
        }
        #endregion
    }

    [ScenarioAction("AcceptQuest", "NPC", "{\"do\":\"AcceptQuest\",\"quest\":1,\"choice\":0}  opens the offer, plays its dialogue, clicks Accept")]
    internal sealed class AcceptQuestAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return QuestFlow.Open(run, step.Int("quest"), step.Int("choice"), DialogueInput.AcceptLabel);
        }
        #endregion
    }

    [ScenarioAction("TurnInQuest", "NPC", "{\"do\":\"TurnInQuest\",\"quest\":1}  opens the quest, plays its dialogue, clicks Complete")]
    internal sealed class TurnInQuestAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            yield return QuestFlow.Open(run, step.Int("quest"), step.Int("choice"), DialogueInput.TurnInLabel);
        }
        #endregion
    }

    [ScenarioAction("QuestTalk", "NPC", "{\"do\":\"QuestTalk\",\"quest\":3,\"choice\":0}  clicks the quest talk button and plays it")]
    internal sealed class QuestTalkAction : ScenarioAction
    {
        #region [Public Methods]
        public override IEnumerator Run(ScenarioRun run, ScenarioStep step)
        {
            DialogueConversation conversation = DialogueInput.Require(run);
            if (conversation == null) yield break;
            int questId = step.Int("quest");
            foreach (QuestTalk talk in conversation.QuestTalks)
            {
                if (talk.Quest.Id != questId) continue;
                yield return DialogueInput.ClickLabel(run, $"{talk.Quest.Name}: {talk.Quest.Objectives[talk.ObjectiveIndex].Text}");
                yield return DialogueInput.PlayThrough(run, step.Int("choice"));
                yield break;
            }
            run.FailStep($"no talk objective of quest {questId} here");
        }
        #endregion
    }

    internal static class QuestFlow
    {
        #region [Public Methods]
        public static IEnumerator Open(ScenarioRun run, int questId, int choice, string finishLabel)
        {
            QuestModel quest = QuestDatabase.Get(questId);
            DialogueConversation conversation = DialogueInput.Require(run);
            if (quest == null || conversation == null)
            {
                if (quest == null) run.FailStep($"unknown quest {questId}");
                yield break;
            }

            yield return DialogueInput.ClickLabel(run, $"{quest.Name}");
            if (run.StepFailed) yield break;
            yield return run.WaitFor(() => conversation.Mode == DialogueConversationMode.QuestDialogue || conversation.Mode == DialogueConversationMode.Quest, 2f,
                $"quest {questId} to open after clicking \"{quest.Name}\" (mode {conversation.Mode}, status {GameData.Player.Quests.GetStatus(quest)})");
            if (run.StepFailed) yield break;
            yield return DialogueInput.PlayThrough(run, choice);
            if (run.StepFailed) yield break;
            yield return DialogueInput.ClickLabel(run, finishLabel);
            run.Note($"quest {questId} is {GameData.Player.Quests.GetStatus(quest)}");
        }
        #endregion
    }
}
#endif
