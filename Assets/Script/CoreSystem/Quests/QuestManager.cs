using CoreSystem.Combat;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.UI;
using CoreSystem.UI.Quests;
using CoreSystem.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CoreSystem.Quests
{
    public sealed class QuestManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private const string TrackerWindowName = "QuestTracker";
        private static QuestManager _instance;
        private PlayerData _player;
        #endregion

        #region [Properties]
        public static QuestManager Instance => _instance;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            _player = GameData.Player;
            CombatEvents.OnKilled += HandleKilled;
            DialogueManager.OnTalkStarted += HandleTalkStarted;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            _player.Inventory.OnInventoryChanged += SyncState;
            _player.Quests.OnQuestsChanged += SyncState;
        }

        private void Start()
        {
            SyncState();
            if (UIManager.Instance != null) UIManager.Instance.Open(TrackerWindowName);
        }

        private void OnDestroy()
        {
            CombatEvents.OnKilled -= HandleKilled;
            DialogueManager.OnTalkStarted -= HandleTalkStarted;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _player.Inventory.OnInventoryChanged -= SyncState;
            _player.Quests.OnQuestsChanged -= SyncState;
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static QuestManager Create()
        {
            return RuntimeRoot.CreateChild("QuestManager").AddComponent<QuestManager>();
        }

        public static QuestManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public static bool TurnIn(QuestModel quest)
        {
            Character player = PlayerCore.GetPlayer();
            PlayerData data = GameData.Player;
            if (player == null || quest == null || !data.Quests.Complete(quest)) return false;

            data.Inventory.BeginBatch();
            try
            {
                ConsumeCollected(quest, data.Inventory);
                GrantReward(quest.Reward, player, data.Inventory);
            }
            finally
            {
                data.Inventory.EndBatch();
            }
            return true;
        }
        #endregion

        #region [Private Methods]
        private static void ConsumeCollected(QuestModel quest, PlayerInventory inventory)
        {
            foreach (QuestObjectiveModel objective in quest.Objectives)
            {
                if (objective.Type != QuestObjectiveType.Collect) continue;

                int remaining = objective.Required;
                foreach (var instance in inventory.ItemInstances)
                {
                    if (remaining <= 0) break;
                    if (instance.ItemId != objective.TargetId) continue;

                    int taken = Mathf.Min(remaining, instance.Quantity);
                    if (inventory.RemoveItemByInstance(instance.Id, taken)) remaining -= taken;
                }
            }
        }

        private static void GrantReward(QuestRewardModel reward, Character player, PlayerInventory inventory)
        {
            if (reward == null) return;
            if (reward.BaseExp > 0) player.AddBaseExp(reward.BaseExp);
            if (reward.JobExp > 0) player.AddJobExp(reward.JobExp);
            if (reward.Items == null) return;

            foreach (QuestRewardItem item in reward.Items)
            {
                if (item != null && item.Quantity > 0) inventory.AddItem(item.ItemId, item.Quantity);
            }
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!(victim is Character character)) return;

            Character player = PlayerCore.GetPlayer();
            if (player == null || KillCreditRegistry.ShareOf(victim, player) <= 0f) return;

            _player.Quests.Report(QuestObjectiveType.Kill, character.CharacterId);
        }

        private void HandleTalkStarted(int characterId)
        {
            _player.Quests.Report(QuestObjectiveType.Talk, characterId);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _player.Quests.ReportScene(scene.name);
        }

        private void SyncState()
        {
            _player.Quests.SyncCollect(CountItem);
            _player.Quests.ReportScene(SceneManager.GetActiveScene().name);
        }

        private int CountItem(int itemId)
        {
            int total = 0;
            foreach (var instance in _player.Inventory.Items)
            {
                if (instance.ItemId == itemId) total += instance.Quantity;
            }
            return total;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
