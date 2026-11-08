using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Quests;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class PlayerQuestsTests
    {
        #region [Variables/Fields]
        private QuestModel _kill;
        private QuestModel _chained;
        private QuestModel _map;
        private PlayerQuests _quests;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        [SetUp]
        public void SetUp()
        {
            _kill = Quest(1, 0, new QuestObjectiveModel { Type = QuestObjectiveType.Kill, TargetId = 1001, Required = 3 });
            _chained = Quest(2, 1, new QuestObjectiveModel { Type = QuestObjectiveType.Talk, TargetId = 2002, Required = 1 });
            _map = Quest(3, 0, new QuestObjectiveModel { Type = QuestObjectiveType.ReachMap, TargetScene = "Field", Required = 1 });
            var all = new Dictionary<int, QuestModel> { { 1, _kill }, { 2, _chained }, { 3, _map } };
            _quests = new PlayerQuests(id => all.TryGetValue(id, out QuestModel quest) ? quest : null);
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void NewQuestIsAvailable()
        {
            Assert.AreEqual(QuestStatus.Available, _quests.GetStatus(_kill));
        }

        [Test]
        public void ChainedQuestIsLockedUntilPreviousIsCompleted()
        {
            Assert.AreEqual(QuestStatus.Locked, _quests.GetStatus(_chained));
            Assert.IsFalse(_quests.Accept(_chained));

            FinishKillQuest();
            Assert.AreEqual(QuestStatus.Available, _quests.GetStatus(_chained));
        }

        [Test]
        public void ProgressReachesReadyToTurnIn()
        {
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 1001, 2);
            Assert.AreEqual(QuestStatus.Active, _quests.GetStatus(_kill));

            _quests.Report(QuestObjectiveType.Kill, 1001);
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, _quests.GetStatus(_kill));
        }

        [Test]
        public void WrongTargetDoesNotCount()
        {
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 9999);
            Assert.AreEqual(0, _quests.GetCount(1, 0));
        }

        [Test]
        public void ProgressStopsAtRequired()
        {
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 1001, 50);
            Assert.AreEqual(3, _quests.GetCount(1, 0));
        }

        [Test]
        public void ProgressIsIgnoredBeforeAccepting()
        {
            _quests.Report(QuestObjectiveType.Kill, 1001);
            Assert.AreEqual(0, _quests.GetCount(1, 0));
        }

        [Test]
        public void CollectCountCanGoBackDown()
        {
            _quests.Accept(_kill);
            _quests.SetCount(QuestObjectiveType.Kill, 1001, 3);
            _quests.SetCount(QuestObjectiveType.Kill, 1001, 1);
            Assert.AreEqual(1, _quests.GetCount(1, 0));
        }

        [Test]
        public void ReachingSceneFinishesObjective()
        {
            _quests.Accept(_map);
            _quests.ReportScene("Town");
            Assert.AreEqual(QuestStatus.Active, _quests.GetStatus(_map));
            _quests.ReportScene("Field");
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, _quests.GetStatus(_map));
        }

        [Test]
        public void SyncCollectFollowsInventoryUpAndDown()
        {
            var collect = Quest(4, 0, new QuestObjectiveModel { Type = QuestObjectiveType.Collect, TargetId = 2001, Required = 5 });
            var quests = new PlayerQuests(id => id == 4 ? collect : null);
            quests.Accept(collect);

            quests.SyncCollect(id => id == 2001 ? 5 : 0);
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, quests.GetStatus(collect));

            quests.SyncCollect(id => id == 2001 ? 2 : 0);
            Assert.AreEqual(2, quests.GetCount(4, 0));
            Assert.AreEqual(QuestStatus.Active, quests.GetStatus(collect));
        }

        [Test]
        public void SyncCollectLeavesOtherObjectivesAlone()
        {
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 1001, 2);
            _quests.SyncCollect(id => 99);
            Assert.AreEqual(2, _quests.GetCount(1, 0));
        }

        [Test]
        public void GiverOffersAvailableQuestOnlyToThatNpc()
        {
            _kill.GiverCharacterId = 2004;
            var all = new List<QuestModel> { _kill };
            Assert.AreEqual(1, _quests.GetOffers(2004, all).Count);
            Assert.AreEqual(0, _quests.GetOffers(2002, all).Count);
        }

        [Test]
        public void ActiveQuestIsNotOfferedUntilReady()
        {
            _kill.GiverCharacterId = 2004;
            _kill.TurnInCharacterId = 2002;
            var all = new List<QuestModel> { _kill };

            _quests.Accept(_kill);
            Assert.AreEqual(0, _quests.GetOffers(2004, all).Count);
            Assert.AreEqual(0, _quests.GetOffers(2002, all).Count);

            _quests.Report(QuestObjectiveType.Kill, 1001, 3);
            Assert.AreEqual(1, _quests.GetOffers(2002, all).Count);
        }

        [Test]
        public void CompletedQuestIsNeverOffered()
        {
            _kill.GiverCharacterId = 2004;
            _kill.TurnInCharacterId = 2004;
            FinishKillQuest();
            Assert.AreEqual(0, _quests.GetOffers(2004, new List<QuestModel> { _kill }).Count);
        }

        [Test]
        public void TrackedListHasOnlyUnfinishedAcceptedQuests()
        {
            var all = new List<QuestModel> { _kill, _map };
            Assert.AreEqual(0, _quests.GetTracked(all).Count);

            _quests.Accept(_kill);
            Assert.AreEqual(1, _quests.GetTracked(all).Count);

            _quests.Report(QuestObjectiveType.Kill, 1001, 3);
            Assert.AreEqual(1, _quests.GetTracked(all).Count);

            _quests.Complete(_kill);
            Assert.AreEqual(0, _quests.GetTracked(all).Count);
            Assert.AreEqual(1, _quests.GetCompleted(all).Count);
        }

        [Test]
        public void ProgressLineShowsCountThenReturnTarget()
        {
            _kill.TurnInCharacterId = 2004;
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 1001);
            StringAssert.EndsWith("1/3", QuestTextFormula.ProgressLines(_quests, _kill, id => "Old Man")[0]);

            _quests.Report(QuestObjectiveType.Kill, 1001, 2);
            Assert.AreEqual("Return to Old Man", QuestTextFormula.ProgressLines(_quests, _kill, id => "Old Man")[0]);
        }

        [Test]
        public void TrackerIsEmptyWithNoQuests()
        {
            Assert.AreEqual(string.Empty, QuestTextFormula.Tracker(_quests, new List<QuestModel> { _kill }, id => "x"));
        }

        [Test]
        public void ScenarioNeedsEveryRequiredQuest()
        {
            var finale = Quest(5, 0, new QuestObjectiveModel { Type = QuestObjectiveType.Kill, TargetId = 1, Required = 1 });
            finale.RequiresQuestIds = new List<int> { 1, 3 };

            FinishKillQuest();
            Assert.AreEqual(QuestStatus.Locked, _quests.GetStatus(finale));

            _quests.Accept(_map);
            _quests.ReportScene("Field");
            _quests.Complete(_map);
            Assert.AreEqual(QuestStatus.Available, _quests.GetStatus(finale));
        }

        [Test]
        public void SequentialQuestIgnoresLaterSteps()
        {
            PlayerQuests quests = SequentialSetup(out QuestModel story);
            quests.Report(QuestObjectiveType.Kill, 9);
            Assert.AreEqual(0, quests.GetCount(10, 1));

            quests.Report(QuestObjectiveType.Talk, 7);
            quests.Report(QuestObjectiveType.Kill, 9);
            Assert.AreEqual(1, quests.GetCount(10, 1));
            Assert.AreEqual(2, quests.CurrentObjective(story));
        }

        [Test]
        public void OneEventAdvancesOneSequentialStep()
        {
            PlayerQuests quests = SequentialSetup(out QuestModel story);
            quests.Report(QuestObjectiveType.Talk, 7);
            Assert.AreEqual(1, quests.CurrentObjective(story));
            Assert.AreEqual(0, quests.GetCount(10, 2));
        }

        [Test]
        public void SequentialProgressLinesHideFutureSteps()
        {
            PlayerQuests quests = SequentialSetup(out QuestModel story);
            Assert.AreEqual(1, QuestTextFormula.ProgressLines(quests, story, id => "x").Count);
            quests.Report(QuestObjectiveType.Talk, 7);
            Assert.AreEqual(2, QuestTextFormula.ProgressLines(quests, story, id => "x").Count);
        }

        [Test]
        public void DialogueTalkDoesNotCountFromPlainTalk()
        {
            var talk = Quest(6, 0, new QuestObjectiveModel { Type = QuestObjectiveType.Talk, TargetId = 7, Required = 1, DialogueId = 50 });
            var quests = new PlayerQuests(id => id == 6 ? talk : null);
            quests.Accept(talk);
            quests.Report(QuestObjectiveType.Talk, 7);
            Assert.AreEqual(0, quests.GetCount(6, 0));

            Assert.AreEqual(1, quests.GetDialogueTalks(7, new List<QuestModel> { talk }).Count);
            Assert.IsTrue(quests.CompleteDialogueObjective(talk, 0));
            Assert.AreEqual(QuestStatus.ReadyToTurnIn, quests.GetStatus(talk));
            Assert.AreEqual(0, quests.GetDialogueTalks(7, new List<QuestModel> { talk }).Count);
        }

        [Test]
        public void DialogueTalkIsOfferedOnlyOnItsTurn()
        {
            var story = Quest(11, 0, new QuestObjectiveModel { Type = QuestObjectiveType.Kill, TargetId = 9, Required = 1 });
            story.Sequential = true;
            story.Objectives.Add(new QuestObjectiveModel { Type = QuestObjectiveType.Talk, TargetId = 7, Required = 1, DialogueId = 50 });
            var quests = new PlayerQuests(id => id == 11 ? story : null);
            var all = new List<QuestModel> { story };

            quests.Accept(story);
            Assert.AreEqual(0, quests.GetDialogueTalks(7, all).Count);
            Assert.IsFalse(quests.CompleteDialogueObjective(story, 1));

            quests.Report(QuestObjectiveType.Kill, 9);
            Assert.AreEqual(1, quests.GetDialogueTalks(7, all).Count);
        }

        [Test]
        public void CannotCompleteUnfinishedQuest()
        {
            _quests.Accept(_kill);
            Assert.IsFalse(_quests.Complete(_kill));
        }

        [Test]
        public void CompletedQuestCannotBeTakenOrCompletedAgain()
        {
            FinishKillQuest();
            Assert.AreEqual(QuestStatus.Completed, _quests.GetStatus(_kill));
            Assert.IsFalse(_quests.Accept(_kill));
            Assert.IsFalse(_quests.Complete(_kill));
        }

        [Test]
        public void CaptureAndRestoreKeepsProgress()
        {
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 1001, 2);
            List<QuestRecord> saved = _quests.Capture();

            var model = new PlayerDataModel { Quests = saved };
            PlayerDataModel loaded = PlayerDataModel.FromJson(model.ToJson());

            var restored = new PlayerQuests(id => id == 1 ? _kill : null);
            restored.Restore(loaded.Quests);
            Assert.AreEqual(2, restored.GetCount(1, 0));
            Assert.AreEqual(QuestStatus.Active, restored.GetStatus(_kill));
        }

        [Test]
        public void OldSaveWithoutQuestsLoadsEmpty()
        {
            PlayerDataModel loaded = JsonUtility.FromJson<PlayerDataModel>("{}");
            _quests.Restore(loaded.Quests);
            Assert.AreEqual(QuestStatus.Available, _quests.GetStatus(_kill));
        }

        [Test]
        public void ChangeEventFiresOnProgress()
        {
            _quests.Accept(_kill);
            int fired = 0;
            _quests.OnQuestsChanged += () => fired++;
            _quests.Report(QuestObjectiveType.Kill, 1001);
            _quests.Report(QuestObjectiveType.Kill, 9999);
            Assert.AreEqual(1, fired);
        }
        #endregion

        #region [Private Methods]
        private static PlayerQuests SequentialSetup(out QuestModel story)
        {
            QuestModel quest = Quest(10, 0, new QuestObjectiveModel { Type = QuestObjectiveType.Talk, TargetId = 7, Required = 1 });
            quest.Sequential = true;
            quest.Objectives.Add(new QuestObjectiveModel { Type = QuestObjectiveType.Kill, TargetId = 9, Required = 1 });
            quest.Objectives.Add(new QuestObjectiveModel { Type = QuestObjectiveType.Talk, TargetId = 7, Required = 1 });
            story = quest;
            var quests = new PlayerQuests(id => id == 10 ? quest : null);
            quests.Accept(quest);
            return quests;
        }

        private void FinishKillQuest()
        {
            _quests.Accept(_kill);
            _quests.Report(QuestObjectiveType.Kill, 1001, 3);
            _quests.Complete(_kill);
        }

        private static QuestModel Quest(int id, int requires, QuestObjectiveModel objective)
        {
            return new QuestModel
            {
                Id = id,
                RequiresQuestIds = requires > 0 ? new List<int> { requires } : new List<int>(),
                Objectives = new List<QuestObjectiveModel> { objective }
            };
        }
        #endregion
    }
}
