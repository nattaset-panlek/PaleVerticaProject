using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.UI.DevTools;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DevToolCharacterSpawnTests
    {
        #region [Variables/Fields]
        private static readonly List<CharacterModel> Models = new List<CharacterModel>
        {
            new CharacterModel { Id = 2, Name = "Slime", PrefabPath = "a" },
            new CharacterModel { Id = 1, Name = "Golem", PrefabPath = "b" },
            new CharacterModel { Id = 3, Name = "NoPrefab" }
        };
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void Filter_SkipsEntriesWithoutPrefabAndSortsById()
        {
            var found = DevToolCharacterSpawnPage.Filter(Models, "");
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(1, found[0].Id);
        }

        [TestCase("sli", 2)]
        [TestCase("1", 1)]
        public void Filter_MatchesNameOrId(string search, int expectedId)
        {
            Assert.AreEqual(expectedId, DevToolCharacterSpawnPage.Filter(Models, search)[0].Id);
        }

        [TestCase("5", 5)]
        [TestCase("999", 20)]
        [TestCase("0", 1)]
        [TestCase("abc", 3)]
        public void ParseCount_ClampsAndFallsBack(string text, int expected)
        {
            Assert.AreEqual(expected, DevToolCharacterSpawnPage.ParseCount(text, 3));
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
