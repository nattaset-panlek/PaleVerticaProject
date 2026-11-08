using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CoreSystem.Pathfinding;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace CoreSystem.Tests
{
    public class WalkableGridTests
    {
        #region [Variables/Fields]
        public const string DemoWalkablePath = "Assets/ThirdParty/MapLab/Demo/MapLab_Demo_MapLab/Environment/Walkable.json";
        private const string DemoMaskPath = "Assets/ThirdParty/MapLab/Demo/MapLab_Demo_MapLab/Environment/Walkable_Mask.png";

        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();
        private int _gridsChanged;
        #endregion

        #region [Unity Methods]
        [SetUp]
        public void SetUp()
        {
            _gridsChanged = 0;
            WalkableGridManager.OnGridsChanged += CountGridsChanged;
        }

        [TearDown]
        public void TearDown()
        {
            WalkableGridManager.OnGridsChanged -= CountGridsChanged;
            foreach (var obj in _created)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            _created.Clear();
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void Loader_ReadsHeaderAndBitsWithRowZeroAtTheBottom()
        {
            string json = ToJson(-1f, 2f, 0.5f,
                "..#",
                "#..");

            Assert.IsTrue(WalkableGridLoader.TryParse(json, 0f, out var grid, out string error), error);
            Assert.AreEqual(3, grid.Columns);
            Assert.AreEqual(2, grid.Rows);
            Assert.AreEqual(0.5f, grid.CellSize);
            Assert.AreEqual(new Rect(-1f, 2f, 1.5f, 1f), grid.Bounds);
            Assert.IsFalse(grid.IsWalkable(0, 0));
            Assert.IsTrue(grid.IsWalkable(1, 0));
            Assert.IsTrue(grid.IsWalkable(0, 1));
            Assert.IsFalse(grid.IsWalkable(2, 1));
            Assert.AreEqual(4, grid.WalkableCount);
            Assert.AreEqual(new Vector2(-0.75f, 2.25f), grid.CellCenter(0, 0));
            Assert.IsFalse(grid.IsWalkable(new Vector2(-0.75f, 2.25f)));
            Assert.IsTrue(grid.IsWalkable(new Vector2(-0.25f, 2.25f)));
            Assert.IsFalse(grid.IsWalkable(new Vector2(-1.1f, 2.25f)));
        }

        [Test]
        public void Loader_RejectsUnknownVersion()
        {
            string json = ToJson(0f, 0f, 1f, "..").Replace("\"version\":1", "\"version\":2");

            Assert.IsFalse(WalkableGridLoader.TryParse(json, 0f, out var grid, out string error));
            Assert.IsNull(grid);
            StringAssert.Contains("version 2", error);
        }

        [Test]
        public void Loader_RejectsBrokenFiles()
        {
            Assert.IsFalse(WalkableGridLoader.TryParse("", 0f, out _, out _));
            Assert.IsFalse(WalkableGridLoader.TryParse("not json", 0f, out _, out _));
            Assert.IsFalse(WalkableGridLoader.TryParse(Header(0f, 0f, 1f, 20, 20, "AA=="), 0f, out _, out string shortError));
            StringAssert.Contains("bits", shortError);
            Assert.IsFalse(WalkableGridLoader.TryParse(Header(0f, 0f, 1f, 2, 2, "!!!"), 0f, out _, out string base64Error));
            StringAssert.Contains("base64", base64Error);
            Assert.IsFalse(WalkableGridLoader.TryParse(Header(0f, 0f, 0f, 2, 2, "AA=="), 0f, out _, out _));
            Assert.IsFalse(WalkableGridLoader.TryLoad(null, 0f, out _, out _));
        }

        [Test]
        public void Clearance_BlocksCellsWhoseCentreIsCloserThanTheRadius()
        {
            string[] map =
            {
                ".....",
                ".....",
                "..#..",
                ".....",
                ".....",
            };

            Assert.IsTrue(WalkableGridLoader.TryParse(ToJson(0f, 0f, 1f, map), 0.5f, out var touching, out _));
            Assert.AreEqual(24, touching.WalkableCount);

            Assert.IsTrue(WalkableGridLoader.TryParse(ToJson(0f, 0f, 1f, map), 0.6f, out var sides, out _));
            Assert.IsFalse(sides.IsWalkable(1, 2));
            Assert.IsFalse(sides.IsWalkable(2, 3));
            Assert.IsTrue(sides.IsWalkable(1, 1));
            Assert.IsTrue(sides.IsBakedWalkable(1, 2));
            Assert.AreEqual(20, sides.WalkableCount);

            Assert.IsTrue(WalkableGridLoader.TryParse(ToJson(0f, 0f, 1f, map), 0.8f, out var ring, out _));
            Assert.AreEqual(16, ring.WalkableCount);
        }

        [Test]
        public void Regions_AreSplitByAFullWall()
        {
            Assert.IsTrue(WalkableGridLoader.TryParse(ToJson(0f, 0f, 1f,
                "..#..",
                "..#..",
                "..#.."), 0f, out var grid, out _));

            Assert.AreEqual(2, grid.RegionCount);
            Assert.AreEqual(grid.RegionOf(0, 0), grid.RegionOf(1, 2));
            Assert.AreNotEqual(grid.RegionOf(0, 0), grid.RegionOf(4, 0));
            Assert.AreEqual(0, grid.RegionOf(2, 1));
            Assert.AreEqual(0, grid.RegionOf(-1, 0));
        }

        [Test]
        public void Loader_ReadsTheMapLabDemoBakeLikeItsMask()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(DemoWalkablePath);
            if (json == null) Assert.Ignore($"MapLab demo bake not found at {DemoWalkablePath}.");

            Assert.IsTrue(WalkableGridLoader.TryLoad(json, 0f, out var grid, out string error), error);
            Assert.AreEqual(100, grid.Columns);
            Assert.AreEqual(68, grid.Rows);
            Assert.AreEqual(0.5f, grid.CellSize);
            Assert.AreEqual(new Rect(-25f, -17f, 50f, 34f), grid.Bounds);

            float ratio = (float)grid.WalkableCount / grid.CellCount;
            Assert.That(ratio, Is.InRange(0.75f, 0.95f));

            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(DemoMaskPath);
            if (mask == null || !mask.isReadable) return;

            Assert.AreEqual(grid.Columns, mask.width);
            Assert.AreEqual(grid.Rows, mask.height);
            int mismatches = 0;
            for (int row = 0; row < grid.Rows; row++)
            {
                for (int column = 0; column < grid.Columns; column++)
                {
                    if ((mask.GetPixel(column, row).r > 0.5f) != grid.IsWalkable(column, row)) mismatches++;
                }
            }
            Assert.AreEqual(0, mismatches);
        }

        [Test]
        public void Manager_LoadsTextAssetsAndRaisesGridsChanged()
        {
            var manager = CreateManager();
            var data = CreateTextAsset(ToJson(0f, 0f, 1f,
                "#....",
                "..#..",
                "....."));
            _gridsChanged = 0;

            manager.SetWalkableData(data);

            Assert.IsTrue(manager.HasGrids);
            Assert.AreEqual(1, _gridsChanged);
            Assert.IsNotNull(manager.GetGridAt(new Vector2(2.5f, 1.5f)));
            Assert.IsNull(manager.GetGridAt(new Vector2(-0.5f, 1.5f)));
            Assert.IsFalse(manager.IsWalkable(new Vector2(2.5f, 1.5f)));
            Assert.IsTrue(manager.IsWalkable(new Vector2(4.5f, 1.5f)));

            var path = new List<Vector2> { Vector2.one * 99f };
            Assert.IsTrue(manager.TryFindPath(new Vector2(0.5f, 1.5f), new Vector2(4.5f, 1.5f), path));
            Assert.AreEqual(new Vector2(4.5f, 1.5f), path[path.Count - 1]);
            CollectionAssert.DoesNotContain(path, Vector2.one * 99f);

            Assert.IsFalse(manager.TryFindPath(new Vector2(-3f, 1.5f), new Vector2(4.5f, 1.5f), path));
            Assert.AreEqual(0, path.Count);
            Assert.AreEqual(PathOutcome.NoGrid, manager.FindPath(new Vector2(-3f, 1.5f), new Vector2(4.5f, 1.5f), path), "off the map: the caller may walk straight");
            Assert.AreEqual(PathOutcome.StopsShort, manager.SnapPoint(new Vector2(0.5f, 1.5f), new Vector2(2.5f, 1.5f), out Vector2 snapped));
            Assert.IsTrue(manager.IsWalkable(snapped));
        }

        [Test]
        public void Manager_SkipsBrokenDataWithAWarning()
        {
            var manager = CreateManager();
            LogAssert.Expect(LogType.Warning, new Regex("WalkableGridManager.*version"));
            _gridsChanged = 0;

            manager.SetWalkableData(CreateTextAsset("{\"version\":7}"), null);

            Assert.IsFalse(manager.HasGrids);
            Assert.AreEqual(1, _gridsChanged);
        }
        #endregion

        #region [Private Methods]
        private static string ToJson(float x, float y, float cellSize, params string[] rowsTopFirst)
        {
            int rows = rowsTopFirst.Length;
            int columns = rowsTopFirst[0].Length;
            var bits = new byte[(columns * rows + 7) / 8];
            for (int row = 0; row < rows; row++)
            {
                string line = rowsTopFirst[rows - 1 - row];
                for (int column = 0; column < columns; column++)
                {
                    if (line[column] == '#') continue;
                    int index = row * columns + column;
                    bits[index >> 3] |= (byte)(1 << (index & 7));
                }
            }
            return Header(x, y, cellSize, columns, rows, Convert.ToBase64String(bits), columns * cellSize, rows * cellSize);
        }

        private static string Header(float x, float y, float cellSize, int columns, int rows, string cells, float width = 1f, float height = 1f)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{{\"version\":1,\"bounds\":{{\"x\":{0},\"y\":{1},\"width\":{2},\"height\":{3}}},\"cellSize\":{4},\"columns\":{5},\"rows\":{6},\"cells\":\"{7}\",\"outline\":[],\"holes\":[]}}",
                x, y, width, height, cellSize, columns, rows, cells);
        }

        private WalkableGridManager CreateManager()
        {
            var go = new GameObject("WalkableGridManagerTest");
            _created.Add(go);
            return go.AddComponent<WalkableGridManager>();
        }

        private TextAsset CreateTextAsset(string text)
        {
            var asset = new TextAsset(text) { name = "Walkable" };
            _created.Add(asset);
            return asset;
        }

        private void CountGridsChanged()
        {
            _gridsChanged++;
        }
        #endregion
    }
}
