using System.Collections.Generic;
using CoreSystem.Entity;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class ControlSystemTests
    {
        #region [Variables/Fields]
        private readonly object _player = new object();
        private readonly object _agent = new object();
        private readonly object _skill = new object();
        private readonly object _dev = new object();
        #endregion

        #region [Public Methods]
        [Test]
        public void NoLease_AnySenderCanCommand()
        {
            var control = Create();

            Assert.IsTrue(control.CanCommand(_player));
            Assert.IsTrue(control.CanCommand(null));
            Assert.IsNull(control.CurrentHolder);
        }

        [Test]
        public void OwnerLease_OnlyOwnerCanCommand()
        {
            var control = Create();
            control.Claim(_agent, ControlLane.Owner);

            Assert.IsTrue(control.CanCommand(_agent));
            Assert.IsFalse(control.CanCommand(_player));
            Assert.IsFalse(control.IsOverridden);
        }

        [Test]
        public void HigherLane_TakesOver_AndReturnsWhenReleased()
        {
            var control = Create();
            control.Claim(_agent, ControlLane.Owner);
            var lease = control.Claim(_skill, ControlLane.Possession);

            Assert.AreSame(_skill, control.CurrentHolder);
            Assert.IsTrue(control.IsOverridden);
            Assert.IsFalse(control.CanCommand(_agent));

            control.Release(lease);

            Assert.AreSame(_agent, control.CurrentHolder);
            Assert.IsFalse(control.IsOverridden);
        }

        [Test]
        public void TimedLease_ExpiresOnTick_AndCallsOnEnded()
        {
            var control = Create();
            control.Claim(_agent, ControlLane.Owner);
            bool ended = false;
            control.Claim(_player, ControlLane.Possession, 5f, null, () => ended = true);

            control.Tick(4.9f);
            Assert.AreSame(_player, control.CurrentHolder);

            control.Tick(0.2f);
            Assert.IsTrue(ended);
            Assert.AreSame(_agent, control.CurrentHolder);
        }

        [Test]
        public void KeepWhileCondition_EndsLeaseWhenFalse()
        {
            var control = Create();
            bool keep = true;
            control.Claim(_skill, ControlLane.Scripted, -1f, () => keep);

            control.Tick(0.1f);
            Assert.AreSame(_skill, control.CurrentHolder);

            keep = false;
            control.Tick(0.1f);
            Assert.IsNull(control.CurrentHolder);
        }

        [Test]
        public void SameLane_OtherHolder_IsDenied()
        {
            var control = Create();
            Assert.IsNotNull(control.Claim(_agent, ControlLane.Owner));

            Assert.IsNull(control.Claim(_player, ControlLane.Owner));
            Assert.AreSame(_agent, control.CurrentHolder);
        }

        [Test]
        public void LowerLane_CanQueueUnderHigherLane()
        {
            var control = Create();
            var dev = control.Claim(_dev, ControlLane.Dev);
            Assert.IsNotNull(control.Claim(_agent, ControlLane.Owner));

            Assert.AreSame(_dev, control.CurrentHolder);
            control.Release(dev);
            Assert.AreSame(_agent, control.CurrentHolder);
        }

        [Test]
        public void ClaimFilter_BlocksPossessionButNotDev()
        {
            var control = Create();
            control.ClaimFilter = lane => lane != ControlLane.Possession;

            Assert.IsNull(control.Claim(_skill, ControlLane.Possession));
            Assert.IsNotNull(control.Claim(_dev, ControlLane.Dev));
        }

        [Test]
        public void TransferOwnership_ReplacesPreviousOwner()
        {
            var control = Create();
            control.Claim(_agent, ControlLane.Owner);

            Assert.IsNotNull(control.TransferOwnership(_player));
            Assert.AreSame(_player, control.CurrentHolder);
            Assert.AreEqual(1, control.Leases.Count);
        }

        [Test]
        public void ReleaseHolder_RemovesAllItsLeases()
        {
            var control = Create();
            control.Claim(_player, ControlLane.Owner);
            control.Claim(_player, ControlLane.Possession);

            control.Release(_player);

            Assert.AreEqual(0, control.Leases.Count);
        }

        [Test]
        public void HolderChangedEvent_ReportsTransitions()
        {
            var control = Create();
            var changes = new List<(object, object)>();
            control.OnHolderChanged += (from, to) => changes.Add((from, to));

            control.Claim(_agent, ControlLane.Owner);
            var lease = control.Claim(_skill, ControlLane.Possession);
            control.Release(lease);

            Assert.AreEqual(3, changes.Count);
            Assert.AreSame(_agent, changes[0].Item2);
            Assert.AreSame(_skill, changes[1].Item2);
            Assert.AreSame(_agent, changes[2].Item2);
        }

        [Test]
        public void LegacyOverrideApi_StillWorks()
        {
            var control = Create();
            control.Claim(_agent, ControlLane.Owner);

            Assert.IsTrue(control.RequestOverride(_skill));
            Assert.IsFalse(control.RequestOverride(_player));
            Assert.IsTrue(control.IsOverridden);

            control.ReleaseOverride(_skill);
            Assert.AreSame(_agent, control.CurrentHolder);

            control.RequestOverride(_skill);
            control.ForceClearOverride();
            Assert.AreSame(_agent, control.CurrentHolder);
        }
        #endregion

        #region [Private Methods]
        private static ControlSystem Create()
        {
            var control = new ControlSystem();
            control.Initialize(null);
            return control;
        }
        #endregion
    }
}
