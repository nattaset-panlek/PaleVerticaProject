using System.Collections;
using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.Quests;
using CoreSystem.UI.Dialogue;
using CoreSystem.Utility;
using CoreSystem.World;
using UnityEngine;

namespace CoreSystem.Events
{
    public sealed class EventManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float SpawnScatter = 1f;
        private static EventManager _instance;
        private PlayerData _player;
        #endregion

        #region [Properties]
        public static EventManager Instance => _instance;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            _player = GameData.Player;
            EventZone.OnPlayerEntered += HandleZone;
            CombatEvents.OnKilled += HandleKilled;
            DialogueManager.OnDialogueEnded += HandleDialogueEnded;
            _player.Quests.OnQuestAccepted += HandleQuestAccepted;
            _player.Quests.OnQuestCompleted += HandleQuestCompleted;
            _player.Quests.OnObjectiveCompleted += HandleObjectiveCompleted;
        }

        private void OnDestroy()
        {
            EventZone.OnPlayerEntered -= HandleZone;
            CombatEvents.OnKilled -= HandleKilled;
            DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
            _player.Quests.OnQuestAccepted -= HandleQuestAccepted;
            _player.Quests.OnQuestCompleted -= HandleQuestCompleted;
            _player.Quests.OnObjectiveCompleted -= HandleObjectiveCompleted;
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static EventManager Create()
        {
            return RuntimeRoot.CreateChild("EventManager").AddComponent<EventManager>();
        }

        public static EventManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }
        #endregion

        #region [Private Methods]
        private void HandleZone(string zoneId) => Run(_player.Events.ReportZone(zoneId));
        private void HandleDialogueEnded(int dialogueId) => Run(_player.Events.ReportDialogueEnded(dialogueId));
        private void HandleQuestAccepted(QuestModel quest) => Run(_player.Events.ReportQuestAccepted(quest.Id));
        private void HandleQuestCompleted(QuestModel quest) => Run(_player.Events.ReportQuestCompleted(quest.Id));
        private void HandleObjectiveCompleted(QuestModel quest, int index) => Run(_player.Events.ReportObjective(quest.Id, index));

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!(victim is Character character)) return;
            Character player = PlayerCore.GetPlayer();
            if (player == null || KillCreditRegistry.ShareOf(victim, player) <= 0f) return;
            Run(_player.Events.ReportKill(character.CharacterId));
        }

        private void Run(List<EventModel> fired)
        {
            if (fired.Count > 0) StartCoroutine(RunNextFrame(fired));
        }

        private IEnumerator RunNextFrame(List<EventModel> fired)
        {
            yield return null;
            foreach (EventModel model in fired)
            {
                if (model.Actions == null) continue;
                foreach (EventAction action in model.Actions) Execute(action);
            }
        }

        private void Execute(EventAction action)
        {
            switch (action.Type)
            {
                case EventActionType.PlayDialogue:
                    PlayDialogue(action);
                    break;
                case EventActionType.SpawnMonster:
                    SpawnMonsters(action);
                    break;
                case EventActionType.GiveReward:
                    GiveReward(action);
                    break;
                case EventActionType.SetObjectActive:
                    _player.Events.SetObjectState(action.ObjectId, action.Active);
                    break;
            }
        }

        private static void PlayDialogue(EventAction action)
        {
            DialogueModel dialogue = DialogueDatabase.Get(action.DialogueId);
            if (dialogue == null) return;
            CharacterModel speaker = Database.GetCharacter(action.SpeakerCharacterId);
            DialogueWindow.Open(speaker != null ? speaker.Name : string.Empty, dialogue, action.SpeakerCharacterId);
        }

        private static void SpawnMonsters(EventAction action)
        {
            CharacterModel model = Database.GetCharacter(action.CharacterId);
            MapSpawnPoint point = MapSpawnPoint.Find(action.SpawnPointId);
            if (model == null || string.IsNullOrEmpty(model.PrefabPath) || point == null)
            {
                Debug.LogWarning($"[EventManager] Cannot spawn character {action.CharacterId} at '{action.SpawnPointId}'.");
                return;
            }

            for (int i = 0; i < Mathf.Max(1, action.Count); i++)
            {
                Vector3 position = point.transform.position + (Vector3)(Random.insideUnitCircle * SpawnScatter);
                CharacterFactory.SpawnFromData(model, position, null);
            }
        }

        private void GiveReward(EventAction action)
        {
            Character player = PlayerCore.GetPlayer();
            if (player != null)
            {
                if (action.BaseExp > 0) player.AddBaseExp(action.BaseExp);
                if (action.JobExp > 0) player.AddJobExp(action.JobExp);
            }
            if (action.ItemId > 0 && action.Quantity > 0) _player.Inventory.AddItem(action.ItemId, action.Quantity);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
