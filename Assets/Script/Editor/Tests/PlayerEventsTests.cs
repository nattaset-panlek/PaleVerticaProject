using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Events;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class PlayerEventsTests
    {
        #region [Variables/Fields]
        private List<EventModel> _all;
        private PlayerEvents _events;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        [SetUp]
        public void SetUp()
        {
            _all = new List<EventModel>
            {
                new EventModel { Id = 1, Trigger = new EventTrigger { Type = EventTriggerType.EnterZone, ZoneId = "gate" } },
                new EventModel { Id = 2, Trigger = new EventTrigger { Type = EventTriggerType.KillCount, CharacterId = 9, Count = 3 } },
                new EventModel { Id = 3, Once = false, Trigger = new EventTrigger { Type = EventTriggerType.DialogueEnded, DialogueId = 5 } },
                new EventModel { Id = 4, Trigger = new EventTrigger { Type = EventTriggerType.QuestObjectiveDone, QuestId = 7, ObjectiveIndex = 1 } }
            };
            _events = new PlayerEvents(() => _all);
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void MatchingZoneFires()
        {
            Assert.AreEqual(1, _events.ReportZone("gate").Count);
        }

        [Test]
        public void OtherZoneDoesNotFire()
        {
            Assert.AreEqual(0, _events.ReportZone("other").Count);
        }

        [Test]
        public void OnceEventFiresOnlyOnce()
        {
            _events.ReportZone("gate");
            Assert.AreEqual(0, _events.ReportZone("gate").Count);
        }

        [Test]
        public void RepeatableEventFiresEveryTime()
        {
            _events.ReportDialogueEnded(5);
            Assert.AreEqual(1, _events.ReportDialogueEnded(5).Count);
        }

        [Test]
        public void KillCountFiresOnLastKill()
        {
            Assert.AreEqual(0, _events.ReportKill(9).Count);
            Assert.AreEqual(0, _events.ReportKill(9).Count);
            Assert.AreEqual(1, _events.ReportKill(9).Count);
        }

        [Test]
        public void ObjectiveNeedsMatchingIndex()
        {
            Assert.AreEqual(0, _events.ReportObjective(7, 0).Count);
            Assert.AreEqual(1, _events.ReportObjective(7, 1).Count);
        }

        [Test]
        public void SaveKeepsFiredCountsAndObjects()
        {
            _events.ReportZone("gate");
            _events.ReportKill(9);
            _events.SetObjectState("door", false);

            var restored = new PlayerEvents(() => _all);
            restored.Restore(_events.CaptureEvents(), _events.CaptureObjects());

            Assert.IsTrue(restored.HasFired(1));
            Assert.AreEqual(0, restored.ReportZone("gate").Count);
            Assert.AreEqual(0, restored.ReportKill(9).Count);
            Assert.AreEqual(1, restored.ReportKill(9).Count);
            Assert.IsTrue(restored.TryGetObjectState("door", out bool active));
            Assert.IsFalse(active);
        }

        [Test]
        public void OldSaveWithoutEventsLoadsEmpty()
        {
            PlayerDataModel loaded = PlayerDataModel.FromJson("{}");
            _events.Restore(loaded.Events, loaded.EventObjects);
            Assert.IsFalse(_events.HasFired(1));
        }

        [Test]
        public void ValidatorReportsEventWithNoActions()
        {
            var lookups = new EventDataLookups { CharacterExists = id => true, ItemExists = id => true, QuestExists = id => true, DialogueExists = id => true };
            Assert.AreEqual(1, EventDataValidator.Validate(_all[0], _all, lookups).Count);
        }

        [Test]
        public void ValidatorReportsMissingQuest()
        {
            var model = new EventModel
            {
                Id = 9,
                Trigger = new EventTrigger { Type = EventTriggerType.QuestCompleted, QuestId = 99 },
                Actions = new List<EventAction> { new EventAction { Type = EventActionType.SetObjectActive, ObjectId = "door" } }
            };
            var lookups = new EventDataLookups { CharacterExists = id => true, ItemExists = id => true, QuestExists = id => false, DialogueExists = id => true };
            Assert.AreEqual(1, EventDataValidator.Validate(model, new List<EventModel> { model }, lookups).Count);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
