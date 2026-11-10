using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Dialogue;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DialogueConversationTests
    {
        #region [Variables/Fields]
        private const int CharacterId = 77;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TearDown]
        public void CloseActive()
        {
            DialogueConversation.Active?.Close();
        }

        [Test]
        public void Start_BecomesActiveInTalkMode()
        {
            DialogueConversation conversation = DialogueConversation.Start("Tester", TwoNodeDialogue(), CharacterId);

            Assert.AreSame(conversation, DialogueConversation.Active);
            Assert.AreEqual(DialogueConversationMode.Talk, conversation.Mode);
            Assert.AreEqual(2, conversation.Choices.Count);
        }

        [Test]
        public void Choose_MovesToTheNextNode()
        {
            DialogueConversation conversation = DialogueConversation.Start("Tester", TwoNodeDialogue(), CharacterId);

            conversation.Choose(0);

            Assert.AreEqual(1, conversation.CurrentNode.Id);
            Assert.IsTrue(conversation.CanContinue);
        }

        [Test]
        public void ChoosingAnEndChoice_ClosesTheConversation()
        {
            DialogueConversation conversation = DialogueConversation.Start("Tester", TwoNodeDialogue(), CharacterId);

            conversation.Choose(1);

            Assert.IsTrue(conversation.IsClosed);
            Assert.IsNull(DialogueConversation.Active);
        }

        [Test]
        public void StartingAnother_ClosesThePrevious()
        {
            DialogueConversation first = DialogueConversation.Start("Tester", TwoNodeDialogue(), CharacterId);
            DialogueConversation second = DialogueConversation.Start("Tester", TwoNodeDialogue(), CharacterId);

            Assert.IsTrue(first.IsClosed);
            Assert.AreSame(second, DialogueConversation.Active);
        }
        #endregion

        #region [Private Methods]
        private static DialogueModel TwoNodeDialogue()
        {
            return new DialogueModel
            {
                Id = 9001,
                StartNode = 0,
                Nodes = new List<DialogueNode>
                {
                    new DialogueNode
                    {
                        Id = 0,
                        Text = new LocalizedString { EN = "Hello" },
                        Choices = new List<DialogueChoice>
                        {
                            new DialogueChoice { Text = new LocalizedString { EN = "Ask" }, Next = 1 },
                            new DialogueChoice { Text = new LocalizedString { EN = "Bye" }, Next = -1 }
                        }
                    },
                    new DialogueNode { Id = 1, Text = new LocalizedString { EN = "Answer" } }
                }
            };
        }
        #endregion
    }
}
