using CoreSystem.UI.DevTools;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DevToolAddAmountTests
    {
        #region [Public Methods]
        [TestCase("25", 1, 25)]
        [TestCase("0", 5, 1)]
        [TestCase("-3", 5, 1)]
        [TestCase("100000", 5, GameDevToolUI.MaxAddAmount)]
        [TestCase("", 7, 7)]
        [TestCase("abc", 12, 12)]
        [TestCase("abc", 0, 1)]
        public void ParseAddAmount_ClampsAndFallsBack(string text, int fallback, int expected)
        {
            Assert.AreEqual(expected, GameDevToolUI.ParseAddAmount(text, fallback));
        }
        #endregion
    }
}
