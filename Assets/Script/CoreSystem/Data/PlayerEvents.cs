using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Data.Save;
using CoreSystem.Events;

namespace CoreSystem.Data
{
    public class PlayerEvents
    {
        #region [Variables/Fields]
        private readonly Dictionary<int, EventRecord> _records = new Dictionary<int, EventRecord>();
        private readonly Dictionary<string, bool> _objects = new Dictionary<string, bool>();
        private readonly Func<IReadOnlyList<EventModel>> _all;

        public event Action<string, bool> OnObjectStateChanged;
        #endregion

        #region [Public Methods]
        public PlayerEvents() : this(() => EventDatabase.All)
        {
        }

        public PlayerEvents(Func<IReadOnlyList<EventModel>> all)
        {
            _all = all;
        }

        public List<EventModel> ReportZone(string zoneId)
        {
            return Collect(EventTriggerType.EnterZone, trigger => trigger.ZoneId == zoneId);
        }

        public List<EventModel> ReportQuestAccepted(int questId)
        {
            return Collect(EventTriggerType.QuestAccepted, trigger => trigger.QuestId == questId);
        }

        public List<EventModel> ReportQuestCompleted(int questId)
        {
            return Collect(EventTriggerType.QuestCompleted, trigger => trigger.QuestId == questId);
        }

        public List<EventModel> ReportObjective(int questId, int objectiveIndex)
        {
            return Collect(EventTriggerType.QuestObjectiveDone, trigger => trigger.QuestId == questId && trigger.ObjectiveIndex == objectiveIndex);
        }

        public List<EventModel> ReportDialogueEnded(int dialogueId)
        {
            return Collect(EventTriggerType.DialogueEnded, trigger => trigger.DialogueId == dialogueId);
        }

        public List<EventModel> ReportKill(int characterId)
        {
            return Collect(EventTriggerType.KillCount, trigger => trigger.CharacterId == characterId);
        }

        public bool HasFired(int eventId)
        {
            return _records.TryGetValue(eventId, out EventRecord record) && record.Fired;
        }

        public bool TryGetObjectState(string objectId, out bool active)
        {
            return _objects.TryGetValue(objectId ?? string.Empty, out active);
        }

        public void SetObjectState(string objectId, bool active)
        {
            if (string.IsNullOrEmpty(objectId)) return;
            _objects[objectId] = active;
            OnObjectStateChanged?.Invoke(objectId, active);
        }

        public List<EventRecord> CaptureEvents()
        {
            return _records.Values.Select(r => new EventRecord { EventId = r.EventId, Count = r.Count, Fired = r.Fired }).ToList();
        }

        public List<EventObjectRecord> CaptureObjects()
        {
            return _objects.Select(pair => new EventObjectRecord { ObjectId = pair.Key, Active = pair.Value }).ToList();
        }

        public void Restore(List<EventRecord> events, List<EventObjectRecord> objects)
        {
            _records.Clear();
            _objects.Clear();
            if (events != null)
            {
                foreach (EventRecord record in events.Where(r => r != null))
                {
                    _records[record.EventId] = new EventRecord { EventId = record.EventId, Count = record.Count, Fired = record.Fired };
                }
            }
            if (objects == null) return;
            foreach (EventObjectRecord record in objects.Where(r => r != null && !string.IsNullOrEmpty(r.ObjectId)))
            {
                SetObjectState(record.ObjectId, record.Active);
            }
        }
        #endregion

        #region [Private Methods]
        private List<EventModel> Collect(EventTriggerType type, Func<EventTrigger, bool> matches)
        {
            var fired = new List<EventModel>();
            foreach (EventModel model in _all())
            {
                if (model.Trigger == null || model.Trigger.Type != type || !matches(model.Trigger)) continue;
                if (Advance(model)) fired.Add(model);
            }
            return fired;
        }

        private bool Advance(EventModel model)
        {
            if (!_records.TryGetValue(model.Id, out EventRecord record))
            {
                record = new EventRecord { EventId = model.Id };
                _records[model.Id] = record;
            }

            if (model.Once && record.Fired) return false;

            record.Count++;
            int needed = model.Trigger.Type == EventTriggerType.KillCount ? Math.Max(1, model.Trigger.Count) : 1;
            if (record.Count < needed) return false;

            record.Count = 0;
            record.Fired = true;
            return true;
        }
        #endregion
    }
}
