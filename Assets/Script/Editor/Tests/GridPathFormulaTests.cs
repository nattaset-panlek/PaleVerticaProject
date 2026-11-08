using System.Collections.Generic;
using System.Diagnostics;
using CoreSystem.Pathfinding;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class GridPathFormulaTests
    {
        #region [Variables/Fields]
        private readonly List<Vector2> _path = new List<Vector2>();
        #endregion

        #region [Public Methods]
        [Test]
        public void OpenField_GoesStraightToTheDestination()
        {
            var grid = FromMap(0f,
                "..........",
                "..........",
                "..........",
                "..........",
                "..........");

            Assert.IsTrue(GridPathFormula.TryFindPath(grid, new Vector2(0.5f, 0.5f), new Vector2(8.5f, 4.2f), _path));
            Assert.AreEqual(1, _path.Count);
            Assert.AreEqual(new Vector2(8.5f, 4.2f), _path[0]);
        }

        [Test]
        public void Wall_PathGoesAroundThroughTheGap()
        {
            var grid = FromMap(0f,
                "..........",
                ".....#....",
                ".....#....",
                ".....#....",
                ".....#....",
                ".....#....",
                ".....#....");
            var from = new Vector2(1.5f, 1.5f);
            var to = new Vector2(8.5f, 1.5f);

            Assert.IsFalse(GridPathFormula.HasLineOfSight(grid, from, to));
            Assert.IsTrue(GridPathFormula.TryFindPath(grid, from, to, _path));

            Assert.AreEqual(to, _path[_path.Count - 1]);
            Assert.That(_path.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(MaxY(_path), Is.GreaterThanOrEqualTo(6f));
            AssertWalkable(grid, from, _path);
        }

        [Test]
        public void Diagonal_DoesNotSqueezeBetweenTwoBlockedCorners()
        {
            var grid = FromMap(0f,
                "#....",
                ".#...",
                "..#..",
                "...#.",
                "....#");
            var from = new Vector2(0.5f, 0.5f);
            var to = new Vector2(4.5f, 4.5f);

            Assert.AreEqual(2, grid.RegionCount);
            Assert.IsFalse(GridPathFormula.TryFindPath(grid, from, to, _path, snapCells: 0));
            Assert.AreEqual(0, _path.Count);

            Assert.IsTrue(GridPathFormula.TryFindPath(grid, from, to, _path));
            var end = grid.WorldToCell(_path[_path.Count - 1]);
            Assert.AreEqual(grid.RegionOf(0, 0), grid.RegionOf(end.x, end.y));
            AssertWalkable(grid, from, _path);
        }

        [Test]
        public void DestinationInsideAnObstacle_EndsAtTheNearestReachableCell()
        {
            var grid = FromMap(0f,
                ".........",
                ".........",
                ".........",
                "...###...",
                "...###...",
                "...###...",
                ".........",
                ".........",
                ".........");
            var to = new Vector2(4.5f, 4.5f);

            Assert.IsTrue(GridPathFormula.TryFindPath(grid, new Vector2(0.5f, 4.5f), to, _path));

            var last = _path[_path.Count - 1];
            Assert.IsTrue(grid.IsWalkable(last));
            Assert.That(Vector2.Distance(last, to), Is.LessThan(2.3f));
            Assert.AreEqual(new Vector2(2.5f, 4.5f), last);
        }

        [Test]
        public void StartInsideAnObstacle_FirstStepsOutToTheNearestCell()
        {
            var grid = FromMap(0f,
                ".........",
                ".........",
                ".........",
                "...###...",
                "...###...",
                "...###...",
                ".........",
                ".........",
                ".........");
            var to = new Vector2(0.5f, 4.5f);

            Assert.IsTrue(GridPathFormula.TryFindPath(grid, new Vector2(3.2f, 4.5f), to, _path));

            Assert.AreEqual(new Vector2(2.5f, 4.5f), _path[0]);
            Assert.AreEqual(to, _path[_path.Count - 1]);
        }

        [Test]
        public void SearchBudget_StopsLongSearches()
        {
            var grid = FromMap(0f,
                "..........",
                ".....#....",
                ".....#....",
                ".....#....",
                ".....#....",
                ".....#....",
                ".....#....");

            Assert.IsFalse(GridPathFormula.TryFindPath(grid, new Vector2(1.5f, 1.5f), new Vector2(8.5f, 1.5f), _path, maxExpansions: 3));
            Assert.AreEqual(0, _path.Count);
        }

        [Test]
        public void StartOutsideTheGrid_HasNoPath()
        {
            var grid = FromMap(0f,
                "...",
                "...");

            Assert.IsFalse(GridPathFormula.TryFindPath(grid, new Vector2(-1f, 0.5f), new Vector2(2.5f, 0.5f), _path));
        }

        [Test]
        public void Outcome_TellsWhetherThePathReachesTheTarget()
        {
            var grid = FromMap(0f,
                ".........",
                ".........",
                ".........",
                "...###...",
                "...###...",
                "...###...",
                ".........",
                ".........",
                ".........");
            var from = new Vector2(0.5f, 4.5f);
            var inside = new Vector2(4.5f, 4.5f);

            Assert.AreEqual(PathOutcome.ReachesTarget, GridPathFormula.FindPath(grid, from, new Vector2(8.5f, 4.5f), _path));
            Assert.AreEqual(PathOutcome.StopsShort, GridPathFormula.FindPath(grid, from, inside, _path));
            Assert.AreEqual(new Vector2(2.5f, 4.5f), _path[_path.Count - 1], "the path ends beside the obstacle, so the agent must stop there");
            Assert.AreEqual(PathOutcome.NoRoute, GridPathFormula.FindPath(grid, from, inside, _path, snapCells: 0));
            Assert.AreEqual(0, _path.Count);
            Assert.AreEqual(PathOutcome.NoGrid, GridPathFormula.FindPath(grid, new Vector2(-1f, 4.5f), inside, _path));
        }

        [Test]
        public void SnapPoint_KeepsWalkablePointsAndMovesTheRestOutOfObstacles()
        {
            var grid = FromMap(0f,
                "#....",
                ".#...",
                "..#..",
                "...#.",
                "....#");
            var from = new Vector2(0.5f, 0.5f);

            Assert.AreEqual(PathOutcome.ReachesTarget, GridPathFormula.SnapPoint(grid, from, new Vector2(2.2f, 0.7f), out Vector2 kept));
            Assert.AreEqual(new Vector2(2.2f, 0.7f), kept);

            Assert.AreEqual(PathOutcome.StopsShort, GridPathFormula.SnapPoint(grid, from, new Vector2(1.5f, 3.5f), out Vector2 moved));
            Assert.IsTrue(grid.IsWalkable(moved));
            var cell = grid.WorldToCell(moved);
            Assert.AreEqual(grid.RegionOf(0, 0), grid.RegionOf(cell.x, cell.y), "a roam point on the other side of the wall is moved to this side");

            Assert.AreEqual(PathOutcome.NoRoute, GridPathFormula.SnapPoint(grid, from, new Vector2(4.5f, 4.5f), out _, 0));
            Assert.AreEqual(PathOutcome.NoGrid, GridPathFormula.SnapPoint(grid, new Vector2(9f, 9f), new Vector2(1.5f, 0.5f), out _));
        }

        [Test]
        public void LineOfSight_IsBlockedWhenPassingThroughABlockedCorner()
        {
            var open = FromMap(0f,
                "..",
                "..");
            var corner = FromMap(0f,
                "#.",
                ".#");

            Assert.IsTrue(GridPathFormula.HasLineOfSight(open, new Vector2(0.5f, 0.5f), new Vector2(1.5f, 1.5f)));
            Assert.IsFalse(GridPathFormula.HasLineOfSight(corner, new Vector2(0.5f, 0.5f), new Vector2(1.5f, 1.5f)));
        }

        [Test]
        public void LineOfSight_KeepsTheAgentBodyOffObstacles()
        {
            string[] map =
            {
                "..........",
                ".....#....",
                "..........",
            };
            var from = new Vector2(0.5f, 0.8f);
            var to = new Vector2(9.5f, 0.8f);

            Assert.IsTrue(GridPathFormula.HasLineOfSight(FromMap(0f, map), from, to));
            Assert.IsFalse(GridPathFormula.HasLineOfSight(FromMap(0.4f, map), from, to));
        }

        [Test]
        public void DemoBake_PathsAroundObstacles()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(WalkableGridTests.DemoWalkablePath);
            if (json == null) Assert.Ignore($"MapLab demo bake not found at {WalkableGridTests.DemoWalkablePath}.");
            Assert.IsTrue(WalkableGridLoader.TryLoad(json, 0.25f, out var grid, out string error), error);

            int region = LargestRegion(grid);
            Assert.IsTrue(TryPickBlockedPair(grid, region, out var from, out var to), "No pair of reachable cells with an obstacle between them.");

            var timer = Stopwatch.StartNew();
            Assert.IsTrue(GridPathFormula.TryFindPath(grid, from, to, _path));
            timer.Stop();
            TestContext.WriteLine($"Demo path {from} -> {to}: {_path.Count} waypoints in {timer.Elapsed.TotalMilliseconds:F2} ms");

            Assert.That(_path.Count, Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(to, _path[_path.Count - 1]);
            AssertWalkable(grid, from, _path);
        }
        #endregion

        #region [Private Methods]
        private static WalkableGrid FromMap(float clearance, params string[] rowsTopFirst)
        {
            int rows = rowsTopFirst.Length;
            int columns = rowsTopFirst[0].Length;
            var cells = new bool[columns * rows];
            for (int row = 0; row < rows; row++)
            {
                string line = rowsTopFirst[rows - 1 - row];
                for (int column = 0; column < columns; column++) cells[row * columns + column] = line[column] != '#';
            }
            return new WalkableGrid(new Rect(0f, 0f, columns, rows), 1f, columns, rows, cells, clearance);
        }

        private static void AssertWalkable(WalkableGrid grid, Vector2 from, List<Vector2> path)
        {
            var previous = from;
            foreach (var point in path)
            {
                Assert.IsTrue(grid.IsWalkable(point), $"Waypoint {point} is not walkable.");
                Assert.IsTrue(GridPathFormula.HasLineOfSight(grid, previous, point), $"Segment {previous} -> {point} crosses a blocked cell.");
                previous = point;
            }
        }

        private static float MaxY(List<Vector2> path)
        {
            float max = float.MinValue;
            foreach (var point in path) max = Mathf.Max(max, point.y);
            return max;
        }

        private static int LargestRegion(WalkableGrid grid)
        {
            var sizes = new int[grid.RegionCount + 1];
            for (int row = 0; row < grid.Rows; row++)
            {
                for (int column = 0; column < grid.Columns; column++) sizes[grid.RegionOf(column, row)]++;
            }

            int best = 1;
            for (int region = 2; region < sizes.Length; region++)
            {
                if (sizes[region] > sizes[best]) best = region;
            }
            return best;
        }

        private static bool TryPickBlockedPair(WalkableGrid grid, int region, out Vector2 from, out Vector2 to)
        {
            for (int row = grid.Rows / 2, step = 0; step < grid.Rows; step++, row = grid.Rows / 2 + ((step + 1) / 2) * (step % 2 == 0 ? 1 : -1))
            {
                int left = -1;
                int right = -1;
                for (int column = 0; column < grid.Columns; column++)
                {
                    if (grid.RegionOf(column, row) != region) continue;
                    if (left < 0) left = column;
                    right = column;
                }

                if (left < 0 || right - left < grid.Columns / 2) continue;

                from = grid.CellCenter(left, row);
                to = grid.CellCenter(right, row);
                if (!GridPathFormula.HasLineOfSight(grid, from, to)) return true;
            }

            from = to = Vector2.zero;
            return false;
        }
        #endregion
    }
}
