using System;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.Entity;
using CoreSystem.SkillTree;
using NUnit.Framework;

namespace CoreSystem.Editor.Tests
{
    public class PassiveClassFormulaTests
    {
        #region [Variables/Fields]
        private const int Warrior = 5;
        private const int Mage = 0;
        private static readonly Func<int, bool> IsOrigin = id => id == Warrior || id == Mage;
        #endregion

        #region [Public Methods]
        [Test]
        public void CanChoose_NeedsJobLevelOneAndNoOrigin()
        {
            var empty = new List<PassiveNodeRecord>();
            Assert.IsFalse(PassiveClassFormula.CanChoose(empty, IsOrigin, 0));
            Assert.IsTrue(PassiveClassFormula.CanChoose(empty, IsOrigin, 1));
            Assert.IsFalse(PassiveClassFormula.CanChoose(Build(Warrior), IsOrigin, 1));
        }

        [Test]
        public void ResetKeepingOrigin_KeepsOriginAndRefundsEachNodeOnce()
        {
            var records = Build(Warrior, 100, 100, 101);
            Assert.AreEqual(2, PassiveClassFormula.ResetKeepingOrigin(records, IsOrigin));
            Assert.AreEqual(new[] { Warrior }, records.Select(r => r.NodeId).ToArray());
        }

        [Test]
        public void ChangeOrigin_SwapsOriginAndRefundsSameAsReset()
        {
            var records = Build(Warrior, 100, 101);
            Assert.AreEqual(2, PassiveClassFormula.ChangeOrigin(records, IsOrigin, Build(Mage)));
            Assert.AreEqual(new[] { Mage }, records.Select(r => r.NodeId).ToArray());
        }

        [Test]
        public void OriginOf_NoClassReturnsNoOrigin()
        {
            Assert.AreEqual(PassiveClassFormula.NoOrigin, PassiveClassFormula.OriginOf(Build(100), IsOrigin));
        }
        #endregion

        #region [Private Methods]
        private static List<PassiveNodeRecord> Build(params int[] ids)
        {
            return ids.Select(id => new PassiveNodeRecord(id, 0, 0f)).ToList();
        }
        #endregion
    }
}
