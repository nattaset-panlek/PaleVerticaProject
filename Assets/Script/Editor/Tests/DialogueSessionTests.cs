using System.Collections.Generic;
using CoreSystem.Dialogue;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DialogueSessionTests
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void StartsAtStartNode()
        {
            Assert.AreEqual(0, new DialogueSession(Model()).Current.Id);
        }

        [Test]
        public void ChoiceMovesToNextNode()
        {
            var session = new DialogueSession(Model());
            session.Choose(0);
            Assert.AreEqual(1, session.Current.Id);
        }

        [Test]
        public void ChoiceWithNoNextFinishes()
        {
            var session = new DialogueSession(Model());
            session.Choose(1);
            Assert.IsTrue(session.IsFinished);
        }

        [Test]
        public void InvalidChoiceChangesNothing()
        {
            var session = new DialogueSession(Model());
            session.Choose(5);
            Assert.AreEqual(0, session.Current.Id);
        }

        [Test]
        public void NullModelIsFinished()
        {
            Assert.IsTrue(new DialogueSession(null).IsFinished);
        }
        #endregion

        #region [Private Methods]
        private static DialogueModel Model()
        {
            return new DialogueModel
            {
                StartNode = 0,
                Nodes = new List<DialogueNode>
                {
                    new DialogueNode
                    {
                        Id = 0,
                        Choices = new List<DialogueChoice>
                        {
                            new DialogueChoice { Next = 1 },
                            new DialogueChoice { Next = -1 }
                        }
                    },
                    new DialogueNode { Id = 1 }
                }
            };
        }
        #endregion
    }
}
