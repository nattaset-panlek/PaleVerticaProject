using System;
using System.Collections.Generic;
using CoreSystem.Data.Save;
using CoreSystem.Quests;
using UnityEngine;

namespace CoreSystem.Data
{
    public class PlayerQuests
    {
        #region [Variables/Fields]
        private readonly Dictionary<int, QuestRecord> _records = new Dictionary<int, QuestRecord>();
        private readonly Func<int, QuestModel> _lookup;

        public event Action OnQuestsChanged;
        #endregion

        #region [Properties]
        public IEnumerable<QuestRecord> Records => _records.Values;
        #endregion

        #region [Public Methods]
        public PlayerQuests() : this(QuestDatabase.Get)
        {
        }

        public PlayerQuests(Func<int, QuestModel> lookup)
        {
            _lookup = lookup;
        }

        public QuestStatus GetStatus(QuestModel quest)
        {
            if (quest == null) return QuestStatus.Locked;
            if (_records.TryGetValue(quest.Id, out QuestRecord record))
            {
                if (record.Completed) return QuestStatus.Completed;
                return AllObjectivesMet(quest, record) ? QuestStatus.ReadyToTurnIn : QuestStatus.Active;
            }
            return IsUnlocked(quest) ? QuestStatus.Available : QuestStatus.Locked;
        }

        public List<QuestModel> GetOffers(int characterId, IEnumerable<QuestModel> quests)
        {
            var result = new List<QuestModel>();
            if (quests == null) return result;

            foreach (QuestModel quest in quests)
            {
                QuestStatus status = GetStatus(quest);
                bool canAccept = status == QuestStatus.Available && quest.GiverCharacterId == characterId;
                bool canTurnIn = status == QuestStatus.ReadyToTurnIn && quest.TurnInCharacterId == characterId;
                if (canAccept || canTurnIn) result.Add(quest);
            }
            return result;
        }

        public List<QuestModel> GetTracked(IEnumerable<QuestModel> quests)
        {
            return Filter(quests, status => status == QuestStatus.Active || status == QuestStatus.ReadyToTurnIn);
        }

        public List<QuestModel> GetCompleted(IEnumerable<QuestModel> quests)
        {
            return Filter(quests, status => status == QuestStatus.Completed);
        }

        public int GetCount(int questId, int objectiveIndex)
        {
            if (!_records.TryGetValue(questId, out QuestRecord record)) return 0;
            return objectiveIndex >= 0 && objectiveIndex < record.Counts.Count ? record.Counts[objectiveIndex] : 0;
        }

        public bool Accept(QuestModel quest)
        {
            if (GetStatus(quest) != QuestStatus.Available) return false;

            var record = new QuestRecord { QuestId = quest.Id };
            foreach (QuestObjectiveModel _ in quest.Objectives) record.Counts.Add(0);
            _records[quest.Id] = record;
            OnQuestsChanged?.Invoke();
            return true;
        }

        public void Report(QuestObjectiveType type, int targetId, int amount = 1)
        {
            if (amount <= 0) return;
            Apply((objective, count) => objective.Type == type && objective.TargetId == targetId
                ? count + amount
                : count);
        }

        public void SetCount(QuestObjectiveType type, int targetId, int count)
        {
            Apply((objective, current) => objective.Type == type && objective.TargetId == targetId
                ? Mathf.Max(0, count)
                : current);
        }

        public void ReportScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            Apply((objective, count) => objective.Type == QuestObjectiveType.ReachMap && objective.TargetScene == sceneName
                ? objective.Required
                : count);
        }

        public void SyncCollect(Func<int, int> countOf)
        {
            if (countOf == null) return;
            Apply((objective, count) => objective.Type == QuestObjectiveType.Collect
                ? Mathf.Max(0, countOf(objective.TargetId))
                : count);
        }

        public bool Complete(QuestModel quest)
        {
            if (GetStatus(quest) != QuestStatus.ReadyToTurnIn) return false;
            _records[quest.Id].Completed = true;
            OnQuestsChanged?.Invoke();
            return true;
        }

        public List<QuestRecord> Capture()
        {
            var result = new List<QuestRecord>();
            foreach (QuestRecord record in _records.Values)
            {
                result.Add(new QuestRecord
                {
                    QuestId = record.QuestId,
                    Completed = record.Completed,
                    Counts = new List<int>(record.Counts)
                });
            }
            return result;
        }

        public void Restore(List<QuestRecord> records)
        {
            _records.Clear();
            if (records != null)
            {
                foreach (QuestRecord record in records)
                {
                    if (record == null) continue;
                    _records[record.QuestId] = new QuestRecord
                    {
                        QuestId = record.QuestId,
                        Completed = record.Completed,
                        Counts = record.Counts != null ? new List<int>(record.Counts) : new List<int>()
                    };
                }
            }
            OnQuestsChanged?.Invoke();
        }
        #endregion

        #region [Private Methods]
        private List<QuestModel> Filter(IEnumerable<QuestModel> quests, Func<QuestStatus, bool> keep)
        {
            var result = new List<QuestModel>();
            if (quests == null) return result;
            foreach (QuestModel quest in quests)
            {
                if (keep(GetStatus(quest))) result.Add(quest);
            }
            return result;
        }

        private bool IsUnlocked(QuestModel quest)
        {
            if (quest.RequiresQuestId <= 0) return true;
            return _records.TryGetValue(quest.RequiresQuestId, out QuestRecord required) && required.Completed;
        }

        private static bool AllObjectivesMet(QuestModel quest, QuestRecord record)
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                int count = i < record.Counts.Count ? record.Counts[i] : 0;
                if (count < quest.Objectives[i].Required) return false;
            }
            return true;
        }

        private void Apply(Func<QuestObjectiveModel, int, int> next)
        {
            bool changed = false;
            foreach (QuestRecord record in _records.Values)
            {
                if (record.Completed) continue;
                QuestModel quest = _lookup(record.QuestId);
                if (quest == null) continue;

                while (record.Counts.Count < quest.Objectives.Count) record.Counts.Add(0);
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    int updated = Mathf.Min(next(quest.Objectives[i], record.Counts[i]), quest.Objectives[i].Required);
                    if (updated == record.Counts[i]) continue;
                    record.Counts[i] = updated;
                    changed = true;
                }
            }
            if (changed) OnQuestsChanged?.Invoke();
        }
        #endregion
    }
}
