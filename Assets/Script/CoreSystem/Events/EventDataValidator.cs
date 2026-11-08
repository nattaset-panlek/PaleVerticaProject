using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSystem.Events
{
    public sealed class EventDataLookups
    {
        #region [Variables/Fields]
        public Func<int, bool> CharacterExists;
        public Func<int, bool> ItemExists;
        public Func<int, bool> QuestExists;
        public Func<int, bool> DialogueExists;
        #endregion
    }

    public static class EventDataValidator
    {
        #region [Public Methods]
        public static List<string> Validate(EventModel model, IReadOnlyList<EventModel> all, EventDataLookups lookups)
        {
            var problems = new List<string>();
            if (model == null) return problems;

            if (all.Count(other => other.Id == model.Id) > 1) problems.Add($"Event id {model.Id} is used more than once.");
            CheckTrigger(problems, model.Trigger, lookups);

            if (model.Actions == null || model.Actions.Count == 0) problems.Add("The event does nothing: add an action.");
            else foreach (EventAction action in model.Actions) CheckAction(problems, action, lookups);
            return problems;
        }
        #endregion

        #region [Private Methods]
        private static void CheckTrigger(List<string> problems, EventTrigger trigger, EventDataLookups lookups)
        {
            switch (trigger.Type)
            {
                case EventTriggerType.EnterZone:
                    if (string.IsNullOrEmpty(trigger.ZoneId)) problems.Add("Zone Id is empty.");
                    break;
                case EventTriggerType.QuestAccepted:
                case EventTriggerType.QuestObjectiveDone:
                case EventTriggerType.QuestCompleted:
                    if (!lookups.QuestExists(trigger.QuestId)) problems.Add($"Quest {trigger.QuestId} does not exist.");
                    break;
                case EventTriggerType.KillCount:
                    if (!lookups.CharacterExists(trigger.CharacterId)) problems.Add($"Monster {trigger.CharacterId} does not exist.");
                    if (trigger.Count < 1) problems.Add("Kill Count must be 1 or more.");
                    break;
                case EventTriggerType.DialogueEnded:
                    if (!lookups.DialogueExists(trigger.DialogueId)) problems.Add($"Dialogue {trigger.DialogueId} does not exist.");
                    break;
            }
        }

        private static void CheckAction(List<string> problems, EventAction action, EventDataLookups lookups)
        {
            switch (action.Type)
            {
                case EventActionType.PlayDialogue:
                    if (!lookups.DialogueExists(action.DialogueId)) problems.Add($"Dialogue {action.DialogueId} does not exist.");
                    break;
                case EventActionType.SpawnMonster:
                    if (!lookups.CharacterExists(action.CharacterId)) problems.Add($"Monster {action.CharacterId} does not exist.");
                    if (string.IsNullOrEmpty(action.SpawnPointId)) problems.Add("Spawn Point Id is empty.");
                    break;
                case EventActionType.GiveReward:
                    if (action.ItemId > 0 && !lookups.ItemExists(action.ItemId)) problems.Add($"Item {action.ItemId} does not exist.");
                    if (action.ItemId <= 0 && action.BaseExp <= 0 && action.JobExp <= 0) problems.Add("The reward gives nothing.");
                    break;
                case EventActionType.SetObjectActive:
                    if (string.IsNullOrEmpty(action.ObjectId)) problems.Add("Object Id is empty.");
                    break;
            }
        }
        #endregion
    }
}
