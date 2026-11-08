using System.Collections.Generic;
using System.Linq;

namespace CoreSystem.Dialogue
{
    public static class DialogueDataValidator
    {
        #region [Variables/Fields]
        public const int LeaveNode = -1;
        #endregion

        #region [Public Methods]
        public static List<string> Validate(DialogueModel dialogue, IReadOnlyList<DialogueModel> all)
        {
            var problems = new List<string>();
            if (dialogue == null) return problems;

            if (all.Count(other => other.Id == dialogue.Id) > 1) problems.Add($"Dialogue id {dialogue.Id} is used more than once.");
            if (dialogue.Nodes == null || dialogue.Nodes.Count == 0)
            {
                problems.Add("The dialogue has no nodes.");
                return problems;
            }

            foreach (var group in dialogue.Nodes.GroupBy(node => node.Id).Where(group => group.Count() > 1))
            {
                problems.Add($"Node id {group.Key} is used more than once.");
            }

            if (dialogue.Nodes.All(node => node.Id != dialogue.StartNode))
            {
                problems.Add($"Start node {dialogue.StartNode} does not exist.");
                return problems;
            }

            CheckChoices(problems, dialogue);
            CheckReach(problems, dialogue);
            return problems;
        }
        #endregion

        #region [Private Methods]
        private static void CheckChoices(List<string> problems, DialogueModel dialogue)
        {
            foreach (DialogueNode node in dialogue.Nodes)
            {
                if (node.Choices == null) continue;
                foreach (DialogueChoice choice in node.Choices)
                {
                    if (choice.Next != LeaveNode && dialogue.Nodes.All(other => other.Id != choice.Next))
                    {
                        problems.Add($"Node {node.Id}: a choice goes to node {choice.Next}, which does not exist.");
                    }
                }
            }
        }

        private static void CheckReach(List<string> problems, DialogueModel dialogue)
        {
            var reached = new HashSet<int>();
            var pending = new Stack<int>();
            pending.Push(dialogue.StartNode);

            while (pending.Count > 0)
            {
                int id = pending.Pop();
                if (!reached.Add(id)) continue;
                DialogueNode node = dialogue.Nodes.Find(other => other.Id == id);
                if (node?.Choices == null) continue;
                foreach (DialogueChoice choice in node.Choices)
                {
                    if (choice.Next != LeaveNode) pending.Push(choice.Next);
                }
            }

            foreach (DialogueNode node in dialogue.Nodes.Where(node => !reached.Contains(node.Id)))
            {
                problems.Add($"Node {node.Id} can never be reached.");
            }

            bool hasEnd = dialogue.Nodes.Any(node => reached.Contains(node.Id) && (node.Choices == null || node.Choices.Count == 0));
            if (!hasEnd) problems.Add("No node ends the conversation, so a quest using it can never finish.");
        }
        #endregion
    }
}
