using System;
using System.Collections.Generic;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterSheetSlicer
    {
        #region [Variables/Fields]
        public const string PreviewCellName = "Preview";
        public static readonly string[] CellOrder = { PreviewCellName, "Head", "Body", "ArmL", "ArmR", "LegL", "LegR" };

        private const int AlphaCutoff = 16;
        private const int BackgroundTolerance = 48;
        private const int FringeBand = 6;
        private const float TransparentShare = 0.15f;
        private const float LineShare = 0.5f;
        private const float BorderColorShare = 0.5f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static CharacterSheetLayout ResolveLayout(CharacterSheetLayout layout, int width, int height)
        {
            if (layout != CharacterSheetLayout.Auto) return layout;
            return height > 0 && (float)width / height > 1.15f ? CharacterSheetLayout.Grid4x3 : CharacterSheetLayout.Grid3x3;
        }

        public static void GetGridSize(CharacterSheetLayout layout, out int columns, out int rows)
        {
            columns = layout == CharacterSheetLayout.Grid4x3 ? 4 : 3;
            rows = 3;
        }

        public static List<CharacterSheetCell> DetectCells(byte[] rgba, int width, int height, CharacterSheetLayout layout, out bool foundGridLines)
        {
            GetGridSize(ResolveLayout(layout, width, height), out int columns, out int rows);
            int[] hitsX = CountGridPixels(rgba, width, height, true);
            int[] hitsY = CountGridPixels(rgba, width, height, false);
            bool linesX = FindSpans(hitsX, height, columns, out int[] startX, out int[] endX);
            bool linesY = FindSpans(hitsY, width, rows, out int[] startY, out int[] endY);
            foundGridLines = linesX && linesY;
            List<CharacterSheetCell> cells = new List<CharacterSheetCell>();
            for (int i = 0; i < CellOrder.Length && i < columns * rows; i++)
            {
                int column = i % columns;
                int row = i / columns;
                cells.Add(new CharacterSheetCell(CellOrder[i], startX[column], startY[row], endX[column] - startX[column], endY[row] - startY[row]));
            }
            return cells;
        }

        public static byte[] ExtractPart(byte[] rgba, int width, CharacterSheetCell cell, int canvasSize)
        {
            byte[] pixels = new byte[cell.Width * cell.Height * 4];
            for (int y = 0; y < cell.Height; y++)
                Buffer.BlockCopy(rgba, ((cell.Y + y) * width + cell.X) * 4, pixels, y * cell.Width * 4, cell.Width * 4);
            RemoveBackground(pixels, cell.Width, cell.Height);
            return Resample(pixels, cell.Width, cell.Height, canvasSize, canvasSize);
        }

        public static CharacterPixelBounds GetOpaqueBounds(byte[] rgba, int width, int height, int threshold)
        {
            int left = width, top = height, right = -1, bottom = -1;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (rgba[(y * width + x) * 4 + 3] <= threshold) continue;
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }
            return right < 0 ? new CharacterPixelBounds(0, 0, 0, 0) : new CharacterPixelBounds(left, top, right + 1, bottom + 1);
        }

        public static byte AlphaAt(byte[] rgba, int width, int height, int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return 0;
            return rgba[(y * width + x) * 4 + 3];
        }

        public static byte MaxAlphaAround(byte[] rgba, int width, int height, int x, int y, int radius)
        {
            byte best = 0;
            for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                    best = Math.Max(best, AlphaAt(rgba, width, height, x + dx, y + dy));
            return best;
        }

        public static byte[] Crop(byte[] rgba, int width, CharacterPixelBounds bounds)
        {
            byte[] result = new byte[bounds.Width * bounds.Height * 4];
            for (int y = 0; y < bounds.Height; y++)
                Buffer.BlockCopy(rgba, ((bounds.Top + y) * width + bounds.Left) * 4, result, y * bounds.Width * 4, bounds.Width * 4);
            return result;
        }

        public static byte[] Resample(byte[] source, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            if (sourceWidth == targetWidth && sourceHeight == targetHeight) return (byte[])source.Clone();
            float[] premultiplied = Premultiply(source);
            float[] horizontal = ResampleAxis(premultiplied, sourceWidth, sourceHeight, targetWidth, true);
            float[] both = ResampleAxis(horizontal, targetWidth, sourceHeight, targetHeight, false);
            return Unpremultiply(both);
        }
        #endregion

        #region [Private Methods]
        private static bool IsGridColor(byte[] p, int o)
        {
            return p[o + 3] >= 128 && p[o] >= 180 && p[o + 2] >= 180 && p[o + 1] <= 90;
        }

        private static bool IsGridFringe(byte[] p, int o)
        {
            return p[o + 3] > 0 && p[o] >= 120 && p[o + 2] >= 120 && p[o] - p[o + 1] >= 60 && p[o + 2] - p[o + 1] >= 60;
        }

        private static int[] CountGridPixels(byte[] rgba, int width, int height, bool alongX)
        {
            int[] hits = new int[alongX ? width : height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (IsGridColor(rgba, (y * width + x) * 4)) hits[alongX ? x : y]++;
            return hits;
        }

        private static bool FindSpans(int[] hits, int crossLength, int count, out int[] starts, out int[] ends)
        {
            int length = hits.Length;
            int threshold = Math.Max(1, (int)(crossLength * LineShare));
            List<int> runStart = new List<int>();
            List<int> runEnd = new List<int>();
            int i = 0;
            while (i < length)
            {
                if (hits[i] < threshold) { i++; continue; }
                int start = i;
                while (i < length && hits[i] >= threshold) i++;
                runStart.Add(start);
                runEnd.Add(i);
            }
            starts = new int[count];
            ends = new int[count];
            float cell = (float)length / count;
            starts[0] = 0;
            ends[count - 1] = length;
            bool allFound = runStart.Count > 0;
            for (int k = 1; k < count; k++)
            {
                float expected = k * cell;
                int best = -1;
                float bestDistance = cell * 0.25f;
                for (int r = 0; r < runStart.Count; r++)
                {
                    float distance = Math.Abs((runStart[r] + runEnd[r]) * 0.5f - expected);
                    if (distance <= bestDistance)
                    {
                        bestDistance = distance;
                        best = r;
                    }
                }
                if (best >= 0)
                {
                    int center = (runStart[best] + runEnd[best]) / 2;
                    ends[k - 1] = center;
                    starts[k] = center;
                }
                else
                {
                    allFound = false;
                    ends[k - 1] = (int)Math.Round(expected);
                    starts[k] = (int)Math.Round(expected);
                }
            }
            for (int k = 0; k < count; k++)
            {
                starts[k] = Math.Max(0, Math.Min(starts[k], length - 1));
                ends[k] = Math.Max(starts[k] + 1, Math.Min(ends[k], length));
            }
            return allFound;
        }

        private static void RemoveBackground(byte[] p, int width, int height)
        {
            int band = Math.Max(FringeBand, Math.Min(width, height) / 50);
            int transparent = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int o = (y * width + x) * 4;
                    bool nearEdge = x < band || y < band || x >= width - band || y >= height - band;
                    if (nearEdge && (IsGridColor(p, o) || IsGridFringe(p, o))) p[o + 3] = 0;
                    if (p[o + 3] < AlphaCutoff) transparent++;
                }
            if (transparent < width * height * TransparentShare)
            {
                List<int[]> background = FindBorderColors(p, width, height, band + 3);
                if (background.Count > 0) FloodKey(p, width, height, background);
            }
            for (int i = 3; i < p.Length; i += 4)
                if (p[i] < AlphaCutoff / 2) p[i] = 0;
        }

        private static List<int[]> FindBorderColors(byte[] p, int width, int height, int band)
        {
            Dictionary<int, int[]> bins = new Dictionary<int, int[]>();
            int samples = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (x >= band && y >= band && x < width - band && y < height - band) continue;
                    int o = (y * width + x) * 4;
                    if (p[o + 3] < AlphaCutoff) continue;
                    int key = ((p[o] >> 3) << 10) | ((p[o + 1] >> 3) << 5) | (p[o + 2] >> 3);
                    if (!bins.TryGetValue(key, out int[] bin)) bins[key] = bin = new int[4];
                    bin[0] += p[o];
                    bin[1] += p[o + 1];
                    bin[2] += p[o + 2];
                    bin[3]++;
                    samples++;
                }
            List<int[]> sorted = new List<int[]>(bins.Values);
            sorted.Sort((a, b) => b[3].CompareTo(a[3]));
            List<int[]> colors = new List<int[]>();
            int covered = 0;
            for (int i = 0; i < sorted.Count && i < 2; i++)
            {
                int[] bin = sorted[i];
                if (i > 0 && bin[3] < samples * 0.15f) break;
                colors.Add(new[] { bin[0] / bin[3], bin[1] / bin[3], bin[2] / bin[3] });
                covered += bin[3];
            }
            return samples > 0 && covered >= samples * BorderColorShare ? colors : new List<int[]>();
        }

        private static int DistanceSquared(byte[] p, int o, List<int[]> colors)
        {
            int best = int.MaxValue;
            foreach (int[] c in colors)
            {
                int dr = p[o] - c[0], dg = p[o + 1] - c[1], db = p[o + 2] - c[2];
                best = Math.Min(best, dr * dr + dg * dg + db * db);
            }
            return best;
        }

        private static void FloodKey(byte[] p, int width, int height, List<int[]> colors)
        {
            int limit = BackgroundTolerance * BackgroundTolerance;
            bool[] keyed = new bool[width * height];
            Queue<int> queue = new Queue<int>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1) TryKey(p, x + y * width, colors, limit, keyed, queue);
            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % width, y = index / width;
                if (x > 0) TryKey(p, index - 1, colors, limit, keyed, queue);
                if (x < width - 1) TryKey(p, index + 1, colors, limit, keyed, queue);
                if (y > 0) TryKey(p, index - width, colors, limit, keyed, queue);
                if (y < height - 1) TryKey(p, index + width, colors, limit, keyed, queue);
            }
            int soft = limit * 4;
            for (int index = 0; index < keyed.Length; index++)
            {
                if (keyed[index]) continue;
                int x = index % width, y = index / width;
                bool touches = (x > 0 && keyed[index - 1]) || (x < width - 1 && keyed[index + 1]) || (y > 0 && keyed[index - width]) || (y < height - 1 && keyed[index + width]);
                if (!touches) continue;
                int o = index * 4;
                int d = DistanceSquared(p, o, colors);
                if (d >= soft) continue;
                float t = (float)(Math.Sqrt(d) - BackgroundTolerance) / BackgroundTolerance;
                p[o + 3] = (byte)(p[o + 3] * Math.Max(0f, Math.Min(1f, t)));
            }
        }

        private static void TryKey(byte[] p, int index, List<int[]> colors, int limit, bool[] keyed, Queue<int> queue)
        {
            if (keyed[index]) return;
            int o = index * 4;
            if (p[o + 3] >= AlphaCutoff && DistanceSquared(p, o, colors) > limit) return;
            keyed[index] = true;
            p[o + 3] = 0;
            queue.Enqueue(index);
        }

        private static float[] Premultiply(byte[] source)
        {
            float[] result = new float[source.Length];
            for (int i = 0; i < source.Length; i += 4)
            {
                float a = source[i + 3];
                result[i] = source[i] * a / 255f;
                result[i + 1] = source[i + 1] * a / 255f;
                result[i + 2] = source[i + 2] * a / 255f;
                result[i + 3] = a;
            }
            return result;
        }

        private static byte[] Unpremultiply(float[] source)
        {
            byte[] result = new byte[source.Length];
            for (int i = 0; i < source.Length; i += 4)
            {
                float a = Math.Max(0f, Math.Min(255f, source[i + 3]));
                result[i + 3] = (byte)Math.Round(a);
                if (a <= 0.5f) continue;
                for (int c = 0; c < 3; c++)
                    result[i + c] = (byte)Math.Round(Math.Max(0f, Math.Min(255f, source[i + c] * 255f / a)));
            }
            return result;
        }

        private static float[] ResampleAxis(float[] source, int width, int height, int newLength, bool horizontal)
        {
            int oldLength = horizontal ? width : height;
            int outWidth = horizontal ? newLength : width;
            int outHeight = horizontal ? height : newLength;
            int lines = horizontal ? height : width;
            float[] result = new float[outWidth * outHeight * 4];
            float scale = (float)oldLength / newLength;
            float radius = Math.Max(1f, scale);
            for (int d = 0; d < newLength; d++)
            {
                float center = (d + 0.5f) * scale - 0.5f;
                int first = Math.Max(0, (int)Math.Ceiling(center - radius));
                int last = Math.Min(oldLength - 1, (int)Math.Floor(center + radius));
                if (last < first) first = last = Math.Max(0, Math.Min(oldLength - 1, (int)Math.Round(center)));
                float total = 0f;
                float[] weights = new float[last - first + 1];
                for (int s = first; s <= last; s++)
                {
                    float w = Math.Max(0f, 1f - Math.Abs(s - center) / radius);
                    weights[s - first] = w;
                    total += w;
                }
                if (total <= 0f)
                {
                    for (int s = 0; s < weights.Length; s++) weights[s] = 1f;
                    total = weights.Length;
                }
                for (int line = 0; line < lines; line++)
                {
                    float r = 0f, g = 0f, b = 0f, a = 0f;
                    for (int s = first; s <= last; s++)
                    {
                        float w = weights[s - first] / total;
                        int si = (horizontal ? line * width + s : s * width + line) * 4;
                        r += source[si] * w;
                        g += source[si + 1] * w;
                        b += source[si + 2] * w;
                        a += source[si + 3] * w;
                    }
                    int di = (horizontal ? line * outWidth + d : d * outWidth + line) * 4;
                    result[di] = r;
                    result[di + 1] = g;
                    result[di + 2] = b;
                    result[di + 3] = a;
                }
            }
            return result;
        }
        #endregion
    }
}
