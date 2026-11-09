using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Quests;
using NUnit.Framework;

namespace CoreSystem.Editor.Tests
{
    public class PlayerQuestsTrackingTests
    {
        #region [Variables/Fields]
        private PlayerQuests _quests;
        private List<QuestModel> _all;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            _quests = new PlayerQuests(_ => null);
            _all = new List<QuestModel>();
            for (int i = 1; i <= QuestTextFormula.MaxTrackedQuests + 1; i++)
            {
                _all.Add(new QuestModel { Id = i, Objectives = new List<QuestObjectiveModel> { new QuestObjectiveModel { Required = 1 } } });
            }
        }

        [Test]
        public void Accept_TracksUntilLimit()
        {
            foreach (QuestModel quest in _all) _quests.Accept(quest);
            Assert.AreEqual(QuestTextFormula.MaxTrackedQuests, _quests.TrackedCount);
            Assert.IsFalse(_quests.IsTracked(_all[_all.Count - 1].Id));
            Assert.AreEqual(_all.Count, _quests.GetActive(_all).Count);
        }

        [Test]
        public void SetTracked_RespectsLimitAndUntrack()
        {
            foreach (QuestModel quest in _all) _quests.Accept(quest);
            int last = _all[_all.Count - 1].Id;
            Assert.IsFalse(_quests.SetTracked(last, true));
            Assert.IsTrue(_quests.SetTracked(1, false));
            Assert.IsFalse(_quests.IsTracked(1));
            Assert.IsTrue(_quests.SetTracked(last, true));
            Assert.IsTrue(_quests.IsTracked(last));
        }

        [Test]
        public void Untrack_SurvivesCaptureAndRestore()
        {
            _quests.Accept(_all[0]);
            _quests.SetTracked(1, false);
            var restored = new PlayerQuests(_ => null);
            restored.Restore(_quests.Capture());
            Assert.IsFalse(restored.IsTracked(1));
            Assert.AreEqual(1, restored.GetActive(_all).Count);
        }

        [Test]
        public void Abandon_ReturnsQuestToAvailable()
        {
            _quests.Accept(_all[0]);
            Assert.IsTrue(_quests.Abandon(_all[0]));
            Assert.AreEqual(QuestStatus.Available, _quests.GetStatus(_all[0]));
        }
        #endregion
    }
}
