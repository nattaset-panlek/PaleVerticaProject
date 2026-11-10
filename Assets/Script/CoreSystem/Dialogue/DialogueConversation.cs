using System;
using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Quests;

namespace CoreSystem.Dialogue
{
    public sealed class DialogueConversation
    {
        #region [Variables/Fields]
        private static readonly List<DialogueChoice> NoChoices = new List<DialogueChoice>();

        private readonly DialogueSession _session;
        private QuestModel _selectedQuest;
        private DialogueSession _questDialogue;
        private Action _questDialogueDone;
        private int _questDialogueId;

        public static event Action<DialogueConversation> OnStarted;
        public event Action OnChanged;
        public event Action OnClosed;
        #endregion

        #region [Properties]
        public static DialogueConversation Active { get; private set; }
        public string Speaker { get; }
        public int CharacterId { get; }
        public int DialogueId { get; }
        public bool IsClosed { get; private set; }
        public QuestModel SelectedQuest => _selectedQuest;

        public DialogueConversationMode Mode
        {
            get
            {
                if (IsClosed) return DialogueConversationMode.Closed;
                if (_questDialogue != null) return DialogueConversationMode.QuestDialogue;
                return _selectedQuest != null ? DialogueConversationMode.Quest : DialogueConversationMode.Talk;
            }
        }

        public DialogueNode CurrentNode
        {
            get
            {
                switch (Mode)
                {
                    case DialogueConversationMode.QuestDialogue: return _questDialogue.Current;
                    case DialogueConversationMode.Talk: return _session.Current;
                    default: return null;
                }
            }
        }

        public IReadOnlyList<DialogueChoice> Choices
        {
            get
            {
                DialogueNode node = CurrentNode;
                return node != null && node.Choices != null ? node.Choices : NoChoices;
            }
        }

        public bool CanContinue => CurrentNode != null && Choices.Count == 0;
        public QuestStatus SelectedQuestStatus => _selectedQuest != null ? GameData.Player.Quests.GetStatus(_selectedQuest) : QuestStatus.Locked;
        public IEnumerable<QuestTalk> QuestTalks => GameData.Player.Quests.GetDialogueTalks(CharacterId, QuestDatabase.All);
        public IEnumerable<QuestModel> QuestOffers => GameData.Player.Quests.GetOffers(CharacterId, QuestDatabase.All);
        #endregion

        #region [Public Methods]
        public static DialogueConversation Start(string speaker, DialogueModel dialogue, int characterId)
        {
            var conversation = new DialogueConversation(speaker, dialogue, characterId);
            if (conversation._session.IsFinished) return null;

            if (Active != null && !Active.IsClosed) Active.Close();
            Active = conversation;
            OnStarted?.Invoke(conversation);
            return conversation;
        }

        public void Choose(int index)
        {
            if (Mode == DialogueConversationMode.QuestDialogue)
            {
                _questDialogue.Choose(index);
                if (_questDialogue.IsFinished)
                {
                    _questDialogue = null;
                    _questDialogueDone = null;
                }
                Changed();
                return;
            }

            if (Mode != DialogueConversationMode.Talk) return;
            _session.Choose(index);
            if (_session.IsFinished)
            {
                Close();
                return;
            }
            Changed();
        }

        public void Continue()
        {
            if (Mode == DialogueConversationMode.QuestDialogue)
            {
                Action done = _questDialogueDone;
                _questDialogue = null;
                _questDialogueDone = null;
                done?.Invoke();
                DialogueManager.RaiseDialogueEnded(_questDialogueId);
                Changed();
                return;
            }

            if (Mode != DialogueConversationMode.Talk) return;
            Close();
            DialogueManager.RaiseDialogueEnded(DialogueId);
        }

        public void PlayQuestTalk(QuestTalk talk)
        {
            PlayQuestDialogue(talk.Quest.Objectives[talk.ObjectiveIndex].DialogueId, () =>
            {
                GameData.Player.Quests.CompleteDialogueObjective(talk.Quest, talk.ObjectiveIndex);
                _selectedQuest = null;
            });
        }

        public void OpenOffer(QuestModel quest)
        {
            if (quest == null) return;
            bool isTurnIn = GameData.Player.Quests.GetStatus(quest) == QuestStatus.ReadyToTurnIn;
            PlayQuestDialogue(isTurnIn ? quest.TurnInDialogueId : quest.AcceptDialogueId, () => _selectedQuest = quest);
        }

        public void Accept()
        {
            if (_selectedQuest == null) return;
            GameData.Player.Quests.Accept(_selectedQuest);
            Back();
        }

        public void TurnIn()
        {
            if (_selectedQuest == null) return;
            QuestManager.TurnIn(_selectedQuest);
            Back();
        }

        public void Back()
        {
            _selectedQuest = null;
            Changed();
        }

        public void Close()
        {
            if (IsClosed) return;
            IsClosed = true;
            if (Active == this) Active = null;
            OnClosed?.Invoke();
        }
        #endregion

        #region [Private Methods]
        private DialogueConversation(string speaker, DialogueModel dialogue, int characterId)
        {
            _session = new DialogueSession(dialogue);
            Speaker = speaker;
            CharacterId = characterId;
            DialogueId = dialogue != null ? dialogue.Id : 0;
        }

        private void PlayQuestDialogue(int dialogueId, Action onDone)
        {
            var session = new DialogueSession(DialogueDatabase.Get(dialogueId));
            if (session.IsFinished)
            {
                onDone();
                Changed();
                return;
            }

            _questDialogue = session;
            _questDialogueId = dialogueId;
            _questDialogueDone = onDone;
            Changed();
        }

        private void Changed()
        {
            OnChanged?.Invoke();
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active = null;
            OnStarted = null;
        }
        #endregion
    }
}
