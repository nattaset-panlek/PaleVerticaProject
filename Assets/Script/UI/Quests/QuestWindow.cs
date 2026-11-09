using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Quests;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.SystemMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.Quests
{
    public sealed class QuestWindow : UIWindow
    {
        #region [Variables/Fields]
        public new const string WindowName = "QuestLog";

        private const int NoSelection = -1;
        private const long PreviewInstanceId = -1;
        private const float RewardSlotSize = 96f;

        private static readonly Color DoneColor = new Color32(0x61, 0x61, 0x61, 0xFF);
        private static readonly Color CurrentColor = new Color32(0x14, 0x13, 0x13, 0xFF);
        private static readonly Color LockedColor = new Color32(0xA6, 0xA6, 0xA6, 0xFF);

        [Header("List")]
        [SerializeField] private QuestRowSlot _rowPrefab;
        [SerializeField] private RectTransform _activeRoot;
        [SerializeField] private RectTransform _completedRoot;
        [SerializeField] private GameObject _completedHeading;
        [SerializeField] private TMP_Text _trackedCount;

        [Header("Details")]
        [SerializeField] private GameObject _details;
        [SerializeField] private GameObject _emptyText;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _giver;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private QuestObjectiveRow _objectivePrefab;
        [SerializeField] private RectTransform _objectiveRoot;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private RectTransform _rewardRoot;
        [SerializeField] private TMP_Text _baseExp;
        [SerializeField] private TMP_Text _jobExp;
        [SerializeField] private ScrollRect _detailsScroll;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _rejectButton;
        [SerializeField] private Button _trackButton;
        [SerializeField] private TMP_Text _trackLabel;

        private readonly SystemConfirmDialog _confirm = new SystemConfirmDialog();
        private int _selectedId = NoSelection;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();
            _closeButton.onClick.AddListener(() => UIManager.Instance.Close(WindowName));
            _rejectButton.onClick.AddListener(AskReject);
            _trackButton.onClick.AddListener(ToggleTrack);
        }
        #endregion

        #region [Public Methods]
        public static string NpcName(int characterId)
        {
            CharacterModel model = Database.GetCharacter(characterId);
            return model != null ? model.Name : string.Empty;
        }

        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            GameData.Player.Quests.OnQuestsChanged -= Refresh;
            GameData.Player.Quests.OnQuestsChanged += Refresh;
            Refresh();
        }

        public override void OnClose()
        {
            GameData.Player.Quests.OnQuestsChanged -= Refresh;
            base.OnClose();
        }
        #endregion

        #region [Private Methods]
        private void Refresh()
        {
            PlayerQuests quests = GameData.Player.Quests;
            List<QuestModel> active = quests.GetActive(QuestDatabase.All);
            List<QuestModel> completed = quests.GetCompleted(QuestDatabase.All);

            QuestModel selected = FindSelected(active, completed);
            _selectedId = selected != null ? selected.Id : NoSelection;

            Clear(_activeRoot);
            Clear(_completedRoot);
            foreach (QuestModel quest in active) AddRow(_activeRoot, quest, false);
            foreach (QuestModel quest in completed) AddRow(_completedRoot, quest, true);

            _completedHeading.SetActive(completed.Count > 0);
            _trackedCount.text = $"{quests.TrackedCount} / {QuestTextFormula.MaxTrackedQuests}";
            _details.SetActive(selected != null);
            _emptyText.SetActive(selected == null);
            _rejectButton.gameObject.SetActive(selected != null);
            _trackButton.gameObject.SetActive(selected != null);
            if (selected != null) ShowDetails(selected);
        }

        private QuestModel FindSelected(List<QuestModel> active, List<QuestModel> completed)
        {
            QuestModel match = active.Find(q => q.Id == _selectedId) ?? completed.Find(q => q.Id == _selectedId);
            if (match != null) return match;
            if (active.Count > 0) return active[0];
            return completed.Count > 0 ? completed[0] : null;
        }

        private void AddRow(RectTransform root, QuestModel quest, bool completed)
        {
            QuestRowSlot row = Instantiate(_rowPrefab, root);
            int id = quest.Id;
            row.Bind($"{quest.Name}", id == _selectedId, GameData.Player.Quests.IsTracked(id), completed, () => Select(id));
        }

        private void Select(int questId)
        {
            _selectedId = questId;
            Refresh();
            _detailsScroll.verticalNormalizedPosition = 1f;
        }

        private void ShowDetails(QuestModel quest)
        {
            PlayerQuests quests = GameData.Player.Quests;
            QuestStatus status = quests.GetStatus(quest);
            _name.text = $"{quest.Name}";
            _giver.text = $"Given by {NpcName(quest.GiverCharacterId)}  /  Turn in to {NpcName(quest.TurnInCharacterId)}";
            _status.text = StatusText(status);
            _description.text = $"{quest.Description}";

            Clear(_objectiveRoot);
            for (int i = 0; i < quest.Objectives.Count; i++) AddObjective(quests, quest, i, status);

            ShowRewards(quest.Reward);

            bool canManage = status == QuestStatus.Active || status == QuestStatus.ReadyToTurnIn;
            bool tracked = quests.IsTracked(quest.Id);
            _rejectButton.interactable = canManage;
            _trackButton.interactable = canManage && (tracked || quests.TrackedCount < QuestTextFormula.MaxTrackedQuests);
            _trackLabel.text = tracked ? "Untrack Quest" : "Track Quest";
        }

        private void AddObjective(PlayerQuests quests, QuestModel quest, int index, QuestStatus status)
        {
            QuestObjectiveModel objective = quest.Objectives[index];
            int count = quests.GetCount(quest.Id, index);
            bool done = status == QuestStatus.Completed || count >= objective.Required;
            bool locked = !done && quest.Sequential && index > quests.CurrentObjective(quest);
            Color color = done ? DoneColor : locked ? LockedColor : CurrentColor;
            string value = done ? "Done" : $"{count} / {objective.Required}";
            Instantiate(_objectivePrefab, _objectiveRoot).Bind($"{objective.Text}", value, color);
        }

        private void ShowRewards(QuestRewardModel reward)
        {
            Clear(_rewardRoot);
            _baseExp.text = reward != null ? reward.BaseExp.ToString("N0") : "0";
            _jobExp.text = reward != null ? reward.JobExp.ToString("N0") : "0";
            if (reward == null || reward.Items == null) return;
            foreach (QuestRewardItem item in reward.Items) AddReward(item);
        }

        private void AddReward(QuestRewardItem reward)
        {
            var holder = new GameObject("Reward", typeof(RectTransform), typeof(LayoutElement));
            holder.transform.SetParent(_rewardRoot, false);
            var layout = holder.GetComponent<LayoutElement>();
            layout.preferredWidth = RewardSlotSize;
            layout.preferredHeight = RewardSlotSize;

            GameObject slotObject = Instantiate(_itemSlotPrefab, holder.transform);
            var rect = slotObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one * (RewardSlotSize / rect.rect.width);
            var slot = slotObject.GetComponent<ItemSlotGlobal>();
            slot.ReadOnly = true;
            slot.UpdateUI(Database.GetItem(reward.ItemId) is Equipment
                ? new EquipmentInstance(PreviewInstanceId, reward.ItemId, reward.Quantity)
                : new ItemInstance(PreviewInstanceId, reward.ItemId, reward.Quantity));
        }

        private void ToggleTrack()
        {
            if (_selectedId == NoSelection) return;
            GameData.Player.Quests.SetTracked(_selectedId, !GameData.Player.Quests.IsTracked(_selectedId));
        }

        private void AskReject()
        {
            QuestModel quest = QuestDatabase.Get(_selectedId);
            if (quest == null) return;
            _confirm.Show("Reject Quest", $"Give up {quest.Name}? Its progress will be lost.", "Reject", () => GameData.Player.Quests.Abandon(quest));
        }

        private static string StatusText(QuestStatus status)
        {
            switch (status)
            {
                case QuestStatus.ReadyToTurnIn: return "READY TO TURN IN";
                case QuestStatus.Completed: return "COMPLETED";
                default: return "IN PROGRESS";
            }
        }

        private static void Clear(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                child.SetActive(false);
                child.transform.SetParent(null, false);
                Destroy(child);
            }
        }
        #endregion
    }
}
