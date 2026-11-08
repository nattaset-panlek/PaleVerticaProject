using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSystem.Quests
{
    public sealed class QuestDataLookups
    {
        #region [Variables/Fields]
        public Func<int, bool> CharacterExists;
        public Func<int, bool> CharacterHasDialogue;
        public Func<int, bool> ItemExists;
        public Func<string, bool> SceneExists;
        public Func<int, bool> DialogueExists;
        #endregion
    }

    public static class QuestDataValidator
    {
        #region [Public Methods]
        public static List<string> Validate(QuestModel quest, IReadOnlyList<QuestModel> all, QuestDataLookups lookups)
        {
            var problems = new List<string>();
            if (quest == null) return problems;

            if (all.Count(other => other.Id == quest.Id) > 1) problems.Add($"Quest id {quest.Id} is used more than once.");
            CheckNpc(problems, "Giver", quest.GiverCharacterId, lookups);
            CheckNpc(problems, "Turn-in NPC", quest.TurnInCharacterId, lookups);
            CheckRequiredQuest(problems, quest, all);

            if (quest.Objectives == null || quest.Objectives.Count == 0) problems.Add("The quest has no objectives.");
            else foreach (QuestObjectiveModel objective in quest.Objectives) CheckObjective(problems, objective, lookups);

            CheckDialogue(problems, "Accept dialogue", quest.AcceptDialogueId, lookups);
            CheckDialogue(problems, "Turn-in dialogue", quest.TurnInDialogueId, lookups);
            CheckReward(problems, quest.Reward, lookups);
            return problems;
        }
        #endregion

        #region [Private Methods]
        private static void CheckNpc(List<string> problems, string label, int characterId, QuestDataLookups lookups)
        {
            if (!lookups.CharacterExists(characterId)) problems.Add($"{label}: character {characterId} does not exist.");
            else if (!lookups.CharacterHasDialogue(characterId)) problems.Add($"{label}: character {characterId} has no Dialogue Id, so nobody can talk to it.");
        }

        private static void CheckRequiredQuest(List<string> problems, QuestModel quest, IReadOnlyList<QuestModel> all)
        {
            if (quest.RequiresQuestIds == null) return;
            foreach (int id in quest.RequiresQuestIds)
            {
                if (id != quest.Id && all.All(other => other.Id != id)) problems.Add($"Required quest {id} does not exist.");
            }
            if (ReachesItself(quest, quest, all, new HashSet<int>())) problems.Add("Required quests form a loop, so this quest can never unlock.");
        }

        private static void CheckObjective(List<string> problems, QuestObjectiveModel objective, QuestDataLookups lookups)
        {
            if (objective.Required <= 0) problems.Add("An objective has Required below 1.");

            switch (objective.Type)
            {
                case QuestObjectiveType.Kill:
                    if (!lookups.CharacterExists(objective.TargetId)) problems.Add($"Kill target {objective.TargetId} does not exist.");
                    break;
                case QuestObjectiveType.Collect:
                    if (!lookups.ItemExists(objective.TargetId)) problems.Add($"Collect item {objective.TargetId} does not exist.");
                    break;
                case QuestObjectiveType.Talk:
                    CheckNpc(problems, "Talk target", objective.TargetId, lookups);
                    CheckDialogue(problems, "Objective dialogue", objective.DialogueId, lookups);
                    break;
                case QuestObjectiveType.ReachMap:
                    if (!lookups.SceneExists(objective.TargetScene)) problems.Add($"Scene '{objective.TargetScene}' is not in Build Settings.");
                    break;
            }
        }

        private static void CheckDialogue(List<string> problems, string label, int dialogueId, QuestDataLookups lookups)
        {
            if (dialogueId > 0 && lookups.DialogueExists != null && !lookups.DialogueExists(dialogueId))
            {
                problems.Add($"{label} {dialogueId} does not exist.");
            }
        }

        private static bool ReachesItself(QuestModel start, QuestModel current, IReadOnlyList<QuestModel> all, HashSet<int> visited)
        {
            if (current.RequiresQuestIds == null) return false;
            foreach (int id in current.RequiresQuestIds)
            {
                if (id == start.Id) return true;
                if (!visited.Add(id)) continue;
                QuestModel next = all.FirstOrDefault(other => other.Id == id);
                if (next != null && ReachesItself(start, next, all, visited)) return true;
            }
            return false;
        }

        private static void CheckReward(List<string> problems, QuestRewardModel reward, QuestDataLookups lookups)
        {
            if (reward == null || reward.Items == null) return;
            foreach (QuestRewardItem item in reward.Items)
            {
                if (!lookups.ItemExists(item.ItemId)) problems.Add($"Reward item {item.ItemId} does not exist.");
                if (item.Quantity <= 0) problems.Add($"Reward item {item.ItemId} has quantity below 1.");
            }
        }
        #endregion
    }
}
