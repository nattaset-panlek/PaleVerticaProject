using CoreSystem.Entity.UniquePassives;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class TreeUniquePassiveSystemTests
    {
        #region [Variables/Fields]
        private const string TreePassive = "DoubleAttack";
        private const string OtherPassive = "DoubleSpell";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void EveryTreeUniquePassive_HasARegisteredScript()
        {
            CoreSystem.SkillTree.SkillTreeDatabase.Reload();
            foreach (var node in CoreSystem.SkillTree.SkillTreeDatabase.GetAllNodes())
            {
                if (node.HasUniquePassive) Assert.IsTrue(UniquePassiveSystem.RegisteredTypes.ContainsKey(node.uniquePassiveId), node.uniquePassiveId);
            }
        }

        [Test]
        public void Sync_GrantsWantedPassive()
        {
            UniquePassiveSystem target = CreateTarget();
            var system = new TreeUniquePassiveSystem();

            system.Sync(new[] { TreePassive }, target);

            Assert.IsTrue(target.HasPassive(TreePassive));
            Assert.IsTrue(system.IsGranted(TreePassive));
        }

        [Test]
        public void Sync_RemovesPassiveWhenNodeIsRefunded()
        {
            UniquePassiveSystem target = CreateTarget();
            var system = new TreeUniquePassiveSystem();
            system.Sync(new[] { TreePassive }, target);

            system.Sync(new string[0], target);

            Assert.IsFalse(target.HasPassive(TreePassive));
            Assert.IsFalse(system.IsGranted(TreePassive));
        }

        [Test]
        public void Sync_KeepsPassiveGrantedBySomethingElse()
        {
            UniquePassiveSystem target = CreateTarget();
            target.AddPassive(OtherPassive);
            var system = new TreeUniquePassiveSystem();
            system.Sync(new[] { OtherPassive }, target);

            system.Sync(new string[0], target);

            Assert.IsTrue(target.HasPassive(OtherPassive));
            Assert.IsFalse(system.IsGranted(OtherPassive));
        }

        [Test]
        public void Sync_RestoresPassiveAfterTargetIsCleared()
        {
            UniquePassiveSystem target = CreateTarget();
            var system = new TreeUniquePassiveSystem();
            system.Sync(new[] { TreePassive }, target);
            target.Clear();

            system.Sync(new[] { TreePassive }, target);

            Assert.IsTrue(target.HasPassive(TreePassive));
            Assert.IsTrue(system.IsGranted(TreePassive));
        }
        #endregion

        #region [Private Methods]
        private static UniquePassiveSystem CreateTarget()
        {
            var target = new UniquePassiveSystem();
            target.Initialize(null);
            return target;
        }
        #endregion
    }
}
