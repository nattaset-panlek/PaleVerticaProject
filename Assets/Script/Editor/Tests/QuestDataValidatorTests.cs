using System.Collections.Generic;
using CoreSystem.Quests;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class QuestDataValidatorTests
    {
        #region [Variables/Fields]
        private QuestDataLookups _lookups;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        [SetUp]
        public void SetUp()
        {
            _lookups = new QuestDataLookups
            {
                CharacterExists = id => id == 2004 || id == 1001,
                CharacterHasDialogue = id => id == 2004,
                ItemExists = id => id == 1003,
                SceneExists = name => name == "Field"
            };
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void ValidQuestHasNoProblems()
        {
            Assert.AreEqual(0, Check(Valid()).Count);
        }

        [Test]
        public void MissingGiverIsReported()
        {
            QuestModel quest = Valid();
            quest.GiverCharacterId = 9;
            Assert.AreEqual(1, Check(quest).Count);
        }

        [Test]
        public void NpcWithoutDialogueIsReported()
        {
            QuestModel quest = Valid();
            quest.TurnInCharacterId = 1001;
            Assert.AreEqual(1, Check(quest).Count);
        }

        [Test]
        public void NoObjectivesIsReported()
        {
            QuestModel quest = Valid();
            quest.Objectives.Clear();
            Assert.AreEqual(1, Check(quest).Count);
        }

        [Test]
        public void QuestRequiringItselfIsReported()
        {
            QuestModel quest = Valid();
            quest.RequiresQuestIds.Add(quest.Id);
            Assert.AreEqual(1, Check(quest).Count);
        }

        [Test]
        public void RequirementLoopIsReported()
        {
            QuestModel first = Valid();
            QuestModel second = Valid();
            second.Id = 2;
            first.RequiresQuestIds.Add(2);
            second.RequiresQuestIds.Add(1);
            Assert.AreEqual(1, QuestDataValidator.Validate(first, new List<QuestModel> { first, second }, _lookups).Count);
        }

        [Test]
        public void MissingDialogueIsReported()
        {
            _lookups.DialogueExists = id => false;
            QuestModel quest = Valid();
            quest.AcceptDialogueId = 99;
            Assert.AreEqual(1, Check(quest).Count);
        }

        [Test]
        public void DuplicateIdIsReported()
        {
            QuestModel quest = Valid();
            Assert.AreEqual(1, QuestDataValidator.Validate(quest, new List<QuestModel> { quest, Valid() }, _lookups).Count);
        }

        [Test]
        public void UnknownSceneIsReported()
        {
            QuestModel quest = Valid();
            quest.Objectives[0] = new QuestObjectiveModel { Type = QuestObjectiveType.ReachMap, TargetScene = "Nope", Required = 1 };
            Assert.AreEqual(1, Check(quest).Count);
        }

        [Test]
        public void BadRewardItemIsReported()
        {
            QuestModel quest = Valid();
            quest.Reward.Items.Add(new QuestRewardItem { ItemId = 5, Quantity = 1 });
            Assert.AreEqual(1, Check(quest).Count);
        }
        #endregion

        #region [Private Methods]
        private List<string> Check(QuestModel quest)
        {
            return QuestDataValidator.Validate(quest, new List<QuestModel> { quest }, _lookups);
        }

        private static QuestModel Valid()
        {
            return new QuestModel
            {
                Id = 1,
                GiverCharacterId = 2004,
                TurnInCharacterId = 2004,
                Objectives = new List<QuestObjectiveModel>
                {
                    new QuestObjectiveModel { Type = QuestObjectiveType.Kill, TargetId = 1001, Required = 3 }
                },
                Reward = new QuestRewardModel { Items = new List<QuestRewardItem> { new QuestRewardItem { ItemId = 1003, Quantity = 5 } } }
            };
        }
        #endregion
    }
}
