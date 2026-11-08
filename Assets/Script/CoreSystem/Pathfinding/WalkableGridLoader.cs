using System;
using UnityEngine;

namespace CoreSystem.Pathfinding
{
    public static class WalkableGridLoader
    {
        #region [Variables/Fields]
        public const int SupportedVersion = 1;

        [Serializable]
        private class WalkableFile
        {
            public int version;
            public Area bounds;
            public float cellSize;
            public int columns;
            public int rows;
            public string cells;
        }

        [Serializable]
        private struct Area
        {
            public float x;
            public float y;
            public float width;
            public float height;
        }
        #endregion

        #region [Public Methods]
        public static bool TryLoad(TextAsset walkableData, float clearance, out WalkableGrid grid, out string error)
        {
            grid = null;
            if (walkableData == null)
            {
                error = "No walkable data assigned.";
                return false;
            }

            if (TryParse(walkableData.text, clearance, out grid, out error)) return true;
            error = $"{walkableData.name}: {error}";
            return false;
        }

        public static bool TryParse(string json, float clearance, out WalkableGrid grid, out string error)
        {
            grid = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Walkable data is empty.";
                return false;
            }

            WalkableFile file;
            try
            {
                file = JsonUtility.FromJson<WalkableFile>(json);
            }
            catch (ArgumentException e)
            {
                error = $"Walkable data is not valid JSON: {e.Message}";
                return false;
            }

            if (file == null)
            {
                error = "Walkable data is not valid JSON.";
                return false;
            }

            if (file.version != SupportedVersion)
            {
                error = $"Walkable format version {file.version} is not supported (expected {SupportedVersion}).";
                return false;
            }

            if (file.columns <= 0 || file.rows <= 0 || file.cellSize <= 0f || (long)file.columns * file.rows > int.MaxValue)
            {
                error = $"Grid size {file.columns} x {file.rows} with cell size {file.cellSize} is not valid.";
                return false;
            }

            byte[] bits;
            try
            {
                bits = Convert.FromBase64String(file.cells ?? string.Empty);
            }
            catch (FormatException)
            {
                error = "Cells are not valid base64.";
                return false;
            }

            int count = file.columns * file.rows;
            if (bits.Length < (count + 7) / 8)
            {
                error = $"Cells hold {bits.Length * 8} bits but the grid needs {count}.";
                return false;
            }

            var walkable = new bool[count];
            for (int i = 0; i < count; i++) walkable[i] = (bits[i >> 3] & (1 << (i & 7))) != 0;

            var area = new Rect(file.bounds.x, file.bounds.y, file.bounds.width, file.bounds.height);
            grid = new WalkableGrid(area, file.cellSize, file.columns, file.rows, walkable, clearance);
            return true;
        }
        #endregion
    }
}
