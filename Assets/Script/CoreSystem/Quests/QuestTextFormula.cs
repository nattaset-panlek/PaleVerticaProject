using System;
using System.Collections.Generic;
using System.Text;
using CoreSystem.Data;

namespace CoreSystem.Quests
{
    public static class QuestTextFormula
    {
        #region [Variables/Fields]
        public const int MaxTrackedQuests = 5;
        #endregion

        #region [Public Methods]
        public static string Tracker(PlayerQuests quests, IEnumerable<QuestModel> all, Func<int, string> npcName)
        {
            var text = new StringBuilder();
            List<QuestModel> tracked = quests.GetTracked(all);
            int shown = Math.Min(tracked.Count, MaxTrackedQuests);

            for (int i = 0; i < shown; i++)
            {
                if (i > 0) text.Append('\n');
                text.Append("<b>").Append(tracked[i].Name).Append("</b>");
                foreach (string line in ProgressLines(quests, tracked[i], npcName)) text.Append('\n').Append(line);
            }
            return text.ToString();
        }

        public static List<string> ProgressLines(PlayerQuests quests, QuestModel quest, Func<int, string> npcName)
        {
            var lines = new List<string>();
            if (quests.GetStatus(quest) == QuestStatus.ReadyToTurnIn)
            {
                lines.Add($"Return to {npcName(quest.TurnInCharacterId)}");
                return lines;
            }

            int last = quest.Sequential ? Math.Min(quests.CurrentObjective(quest), quest.Objectives.Count - 1) : quest.Objectives.Count - 1;
            for (int i = 0; i <= last; i++)
            {
                QuestObjectiveModel objective = quest.Objectives[i];
                lines.Add($"{objective.Text} {quests.GetCount(quest.Id, i)}/{objective.Required}");
            }
            return lines;
        }
        #endregion
    }
}
