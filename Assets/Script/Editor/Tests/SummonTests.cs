using System.Collections.Generic;
using CoreSystem.Skills.Summon;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class SummonTests
    {
        #region [Variables/Fields]
        private readonly List<GameObject> _objects = new List<GameObject>();
        #endregion

        #region [Public Methods]
        [TearDown]
        public void TearDown()
        {
            foreach (var go in _objects) if (go != null) Object.DestroyImmediate(go);
            _objects.Clear();
        }

        [Test]
        public void ExcessCount_MakesRoomForOneMore()
        {
            Assert.AreEqual(0, SummonRegistry.ExcessCount(0, 2));
            Assert.AreEqual(0, SummonRegistry.ExcessCount(1, 2));
            Assert.AreEqual(1, SummonRegistry.ExcessCount(2, 2));
            Assert.AreEqual(2, SummonRegistry.ExcessCount(3, 2));
            Assert.AreEqual(0, SummonRegistry.ExcessCount(10, 0));
        }

        [Test]
        public void SpawnPoint_IsAtDistanceFromCaster()
        {
            Vector3 first = SummonFactory.SpawnPoint(Vector3.zero, Vector3.right, 1.5f, 0);
            Vector3 second = SummonFactory.SpawnPoint(Vector3.zero, Vector3.right, 1.5f, 1);

            Assert.AreEqual(1.5f, first.magnitude, 0.001f);
            Assert.AreEqual(1.5f, second.magnitude, 0.001f);
            Assert.AreNotEqual(first, second);
            Assert.AreEqual(1.5f, SummonFactory.SpawnPoint(Vector3.zero, Vector3.zero, 1.5f, 0).magnitude, 0.001f);
        }

        [Test]
        public void Registry_CountsPerSkillAndUnregistersOnRemoveOrDestroy()
        {
            var a = Spawn(1018);
            var b = Spawn(1018);
            Spawn(2000);

            Assert.AreEqual(2, SummonRegistry.Count(null, 1018));
            Assert.AreSame(a, SummonRegistry.Oldest(null, 1018));

            SummonRegistry.Unregister(a);
            Assert.AreEqual(1, SummonRegistry.Count(null, 1018));

            Object.DestroyImmediate(b.gameObject);
            Assert.AreEqual(0, SummonRegistry.Count(null, 1018));
            Assert.AreEqual(1, SummonRegistry.Count(null, 2000));
        }

        [Test]
        public void TeamRegistry_SummonIsNotHostileToOwnerEvenInUnaffiliatedFaction()
        {
            var owner = new FakeCombatant(1);
            var summon = new FakeCombatant(1);
            var stranger = new FakeCombatant(1);
            try
            {
                Assert.IsTrue(CoreSystem.Entity.TeamRegistry.IsHostile(owner, summon));
                CoreSystem.Entity.TeamRegistry.Bind(summon, owner);
                Assert.IsFalse(CoreSystem.Entity.TeamRegistry.IsHostile(owner, summon));
                Assert.IsFalse(CoreSystem.Entity.TeamRegistry.IsHostile(summon, owner));
                Assert.IsTrue(CoreSystem.Entity.TeamRegistry.IsAlly(summon, owner));
                Assert.IsTrue(CoreSystem.Entity.TeamRegistry.IsHostile(summon, stranger));
                CoreSystem.Entity.TeamRegistry.Unbind(summon);
                Assert.IsTrue(CoreSystem.Entity.TeamRegistry.IsHostile(owner, summon));
            }
            finally
            {
                CoreSystem.Entity.TeamRegistry.Unbind(summon);
                owner.Dispose();
                summon.Dispose();
                stranger.Dispose();
            }
        }
        #endregion

        #region [Private Methods]
        private SummonedUnit Spawn(int skillId)
        {
            var go = new GameObject("SummonTest");
            _objects.Add(go);
            var unit = go.AddComponent<SummonedUnit>();
            unit.Bind(null, null, skillId, 0f);
            return unit;
        }
        #endregion
    }
}
