using System;
using System.Collections.Generic;
using CoreSystem.Data;

namespace CoreSystem.Quests
{
    public enum QuestObjectiveType
    {
        Kill,
        Collect,
        Talk,
        ReachMap
    }

    public enum QuestStatus
    {
        Locked,
        Available,
        Active,
        ReadyToTurnIn,
        Completed
    }

    [Serializable]
    public class QuestObjectiveModel
    {
        public QuestObjectiveType Type;
        public int TargetId;
        public string TargetScene;
        public int Required = 1;
        public LocalizedString Text;
    }

    [Serializable]
    public class QuestRewardItem
    {
        public int ItemId;
        public int Quantity = 1;
    }

    [Serializable]
    public class QuestRewardModel
    {
        public long BaseExp;
        public long JobExp;
        public List<QuestRewardItem> Items = new List<QuestRewardItem>();
    }

    [Serializable]
    public class QuestModel
    {
        public int Id;
        public LocalizedString Name;
        public LocalizedString Description;
        public int GiverCharacterId;
        public int TurnInCharacterId;
        public int RequiresQuestId;
        public List<QuestObjectiveModel> Objectives = new List<QuestObjectiveModel>();
        public QuestRewardModel Reward = new QuestRewardModel();
    }

    [Serializable]
    public class QuestDataList
    {
        public List<QuestModel> Quests = new List<QuestModel>();
    }
}
