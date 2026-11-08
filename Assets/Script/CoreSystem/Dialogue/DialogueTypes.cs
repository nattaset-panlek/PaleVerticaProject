using System;
using System.Collections.Generic;
using CoreSystem.Data;

namespace CoreSystem.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public LocalizedString Text;
        public int Next = -1;
    }

    [Serializable]
    public class DialogueNode
    {
        public int Id;
        public LocalizedString Text;
        public List<DialogueChoice> Choices = new List<DialogueChoice>();
    }

    [Serializable]
    public class DialogueModel
    {
        public int Id;
        public int StartNode;
        public List<DialogueNode> Nodes = new List<DialogueNode>();
    }

    [Serializable]
    public class DialogueDataList
    {
        public List<DialogueModel> Dialogues = new List<DialogueModel>();
    }
}
