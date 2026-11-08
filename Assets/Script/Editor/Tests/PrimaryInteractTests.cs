using System.Collections.Generic;
using CoreSystem.Interaction;
using CoreSystem.Player;
using CoreSystem.Refinement;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class PrimaryInteractTests
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void CoreOptionsAreNotInteractions()
        {
            Assert.IsTrue(PlayerInteraction.IsCoreOption(PlayerInteraction.InspectId));
            Assert.IsTrue(PlayerInteraction.IsCoreOption(PlayerInteraction.AttackId));
            Assert.IsTrue(PlayerInteraction.IsCoreOption(PlayerInteraction.FollowId));
            Assert.IsTrue(PlayerInteraction.IsCoreOption(PlayerInteraction.StopFollowId));
            Assert.IsFalse(PlayerInteraction.IsCoreOption(RefinementMerchantSystem.OptionId));
            Assert.IsFalse(PlayerInteraction.IsCoreOption(null));
        }

        [Test]
        public void FilterKeepsOnlyInteractionsInOrder()
        {
            var options = new List<InteractionOption>
            {
                new InteractionOption(PlayerInteraction.InspectId, "Inspect", null, priority: 1000),
                new InteractionOption(RefinementMerchantSystem.OptionId, "Refine", null, 1.5f),
                new InteractionOption(PlayerInteraction.AttackId, "Attack", null, priority: 900),
                new InteractionOption("shop.buy", "Buy", null, 1.5f)
            };

            var interacts = PlayerInteraction.FilterInteractOptions(options);

            Assert.AreEqual(2, interacts.Count);
            Assert.AreEqual(RefinementMerchantSystem.OptionId, interacts[0].Id);
            Assert.AreEqual("shop.buy", interacts[1].Id);
        }

        [Test]
        public void FilterOfOnlyCoreOptionsIsEmpty()
        {
            var options = new List<InteractionOption>
            {
                new InteractionOption(PlayerInteraction.InspectId, "Inspect", null),
                new InteractionOption(PlayerInteraction.AttackId, "Attack", null)
            };

            Assert.AreEqual(0, PlayerInteraction.FilterInteractOptions(options).Count);
            Assert.AreEqual(0, PlayerInteraction.FilterInteractOptions(null).Count);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
