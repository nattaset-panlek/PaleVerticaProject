using CoreSystem.Settings;
using CoreSystem.UI.ActionProgress;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class ActionProgressTests
    {
        #region [Variables/Fields]
        private const string TestWindow = "ActionProgressTests.Window";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        [TearDown]
        public void TearDown()
        {
            GameSetting.ClearWindowPosition(TestWindow);
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void TimeIsFormattedWithTwoDecimalsAndSeconds()
        {
            Assert.AreEqual("99.99s", ActionProgressWindow.FormatTime(99.99f));
            Assert.AreEqual("0.50s", ActionProgressWindow.FormatTime(0.5f));
            Assert.AreEqual("0.00s", ActionProgressWindow.FormatTime(-1f));
            Assert.AreEqual("1.25<size=66%>s</size>", ActionProgressWindow.FormatTime(1.25f, "<size=66%>s</size>"));
        }

        [Test]
        public void PaletteMatchesTheDesign()
        {
            Assert.AreEqual(new Color(0.133f, 0.827f, 0.933f, 1f), ActionProgressPalette.Get(ActionProgressColor.Cyan));
            Assert.AreEqual(new Color(1f, 0f, 0.435f, 1f), ActionProgressPalette.Get(ActionProgressColor.Magenta));
        }

        [Test]
        public void RequestWithoutIconOrDurationIsManualAndIconless()
        {
            var request = new ActionProgressRequest { ActionName = "Opening Chest", ActionType = "INTERACT" };
            Assert.IsFalse(request.IsTimed);
            Assert.IsFalse(request.HasIcon);
            request.Duration = 1.5f;
            Assert.IsTrue(request.IsTimed);
        }

        [Test]
        public void WindowPositionIsRememberedUntilCleared()
        {
            Assert.IsFalse(GameSetting.TryGetWindowPosition(TestWindow, out _));
            GameSetting.SetWindowPosition(TestWindow, new Vector2(120f, -340f));
            Assert.IsTrue(GameSetting.TryGetWindowPosition(TestWindow, out Vector2 saved));
            Assert.AreEqual(new Vector2(120f, -340f), saved);
            GameSetting.ClearWindowPosition(TestWindow);
            Assert.IsFalse(GameSetting.TryGetWindowPosition(TestWindow, out _));
        }

        [Test]
        public void CastRequestIsMagentaAndOwnedByTheCaster()
        {
            var request = CastProgressEvents.BuildRequest(null, "Fireball");
            Assert.AreEqual("Fireball", request.ActionName);
            Assert.AreEqual(CastProgressEvents.CastActionType, request.ActionType);
            Assert.AreEqual(ActionProgressColor.Magenta, request.Color);
            Assert.IsFalse(request.IsTimed);
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
