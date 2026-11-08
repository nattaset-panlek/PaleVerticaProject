using System;
using System.Collections.Generic;

namespace CoreSystem.Events
{
    public enum EventTriggerType
    {
        EnterZone,
        QuestAccepted,
        QuestObjectiveDone,
        QuestCompleted,
        KillCount,
        DialogueEnded
    }

    public enum EventActionType
    {
        PlayDialogue,
        SpawnMonster,
        GiveReward,
        SetObjectActive
    }

    [Serializable]
    public class EventTrigger
    {
        public EventTriggerType Type;
        public string ZoneId;
        public int QuestId;
        public int ObjectiveIndex;
        public int CharacterId;
        public int Count = 1;
        public int DialogueId;
    }

    [Serializable]
    public class EventAction
    {
        public EventActionType Type;
        public int DialogueId;
        public int SpeakerCharacterId;
        public int CharacterId;
        public int Count = 1;
        public string SpawnPointId;
        public int ItemId;
        public int Quantity = 1;
        public long BaseExp;
        public long JobExp;
        public string ObjectId;
        public bool Active = true;
    }

    [Serializable]
    public class EventModel
    {
        public int Id;
        public string Name;
        public bool Once = true;
        public EventTrigger Trigger = new EventTrigger();
        public List<EventAction> Actions = new List<EventAction>();
    }

    [Serializable]
    public class EventDataList
    {
        public List<EventModel> Events = new List<EventModel>();
    }
}
