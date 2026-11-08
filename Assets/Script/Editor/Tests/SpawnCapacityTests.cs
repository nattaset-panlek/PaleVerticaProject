using CoreSystem.Agent;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SpawnCapacityTests
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [TestCase(0, 5, true)]
        [TestCase(4, 5, true)]
        [TestCase(5, 5, false)]
        [TestCase(99, 0, true)]
        public void HasRoom_RespectsMaxAlive(int alive, int max, bool expected)
        {
            Assert.AreEqual(expected, EntitySpawner.HasRoom(alive, max));
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
