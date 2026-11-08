using System.Collections.Generic;
using CoreSystem.Dialogue;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DialogueDataValidatorTests
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void ValidDialogueHasNoProblems()
        {
            Assert.AreEqual(0, Check(Valid()).Count);
        }

        [Test]
        public void MissingStartNodeIsReported()
        {
            DialogueModel dialogue = Valid();
            dialogue.StartNode = 9;
            Assert.AreEqual(1, Check(dialogue).Count);
        }

        [Test]
        public void MissingNextNodeIsReported()
        {
            DialogueModel dialogue = Valid();
            dialogue.Nodes[0].Choices[0].Next = 9;
            Assert.AreEqual(3, Check(dialogue).Count);
        }

        [Test]
        public void UnreachableNodeIsReported()
        {
            DialogueModel dialogue = Valid();
            dialogue.Nodes.Add(new DialogueNode { Id = 5 });
            Assert.AreEqual(1, Check(dialogue).Count);
        }

        [Test]
        public void DialogueWithNoEndIsReported()
        {
            DialogueModel dialogue = Valid();
            dialogue.Nodes[1].Choices = new List<DialogueChoice> { new DialogueChoice { Next = 0 } };
            Assert.AreEqual(1, Check(dialogue).Count);
        }

        [Test]
        public void DuplicateNodeIdIsReported()
        {
            DialogueModel dialogue = Valid();
            dialogue.Nodes.Add(new DialogueNode { Id = 1 });
            Assert.AreEqual(1, Check(dialogue).Count);
        }

        [Test]
        public void EmptyDialogueIsReported()
        {
            Assert.AreEqual(1, Check(new DialogueModel()).Count);
        }
        #endregion

        #region [Private Methods]
        private static List<string> Check(DialogueModel dialogue)
        {
            return DialogueDataValidator.Validate(dialogue, new List<DialogueModel> { dialogue });
        }

        private static DialogueModel Valid()
        {
            return new DialogueModel
            {
                Id = 1,
                StartNode = 0,
                Nodes = new List<DialogueNode>
                {
                    new DialogueNode
                    {
                        Id = 0,
                        Choices = new List<DialogueChoice>
                        {
                            new DialogueChoice { Next = 1 },
                            new DialogueChoice { Next = DialogueDataValidator.LeaveNode }
                        }
                    },
                    new DialogueNode { Id = 1 }
                }
            };
        }
        #endregion
    }
}
