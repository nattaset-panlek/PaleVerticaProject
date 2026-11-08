using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPsdWriter
    {
        #region [Variables/Fields]
        private static readonly short[] ChannelIds = { -1, 0, 1, 2 };
        private static readonly int[] ChannelOffsets = { 3, 0, 1, 2 };
        private const byte FlagHidden = 0x02;
        private const byte FlagModernBit = 0x08;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static byte[] Write(int width, int height, IReadOnlyList<CharacterPsdLayer> layersBottomToTop)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                WriteAscii(stream, "8BPS");
                WriteUInt16(stream, 1);
                stream.Write(new byte[6], 0, 6);
                WriteUInt16(stream, 4);
                WriteInt32(stream, height);
                WriteInt32(stream, width);
                WriteUInt16(stream, 8);
                WriteUInt16(stream, 3);
                WriteInt32(stream, 0);
                WriteInt32(stream, 0);
                byte[] layerSection = BuildLayerAndMaskSection(layersBottomToTop);
                WriteInt32(stream, layerSection.Length);
                stream.Write(layerSection, 0, layerSection.Length);
                WriteMergedImage(stream, width, height, layersBottomToTop);
                return stream.ToArray();
            }
        }

        public static byte[] Composite(int width, int height, IReadOnlyList<CharacterPsdLayer> layersBottomToTop)
        {
            byte[] canvas = new byte[width * height * 4];
            foreach (CharacterPsdLayer layer in layersBottomToTop)
            {
                if (!layer.Visible) continue;
                for (int y = 0; y < layer.Height; y++)
                {
                    int cy = layer.Top + y;
                    if (cy < 0 || cy >= height) continue;
                    for (int x = 0; x < layer.Width; x++)
                    {
                        int cx = layer.Left + x;
                        if (cx < 0 || cx >= width) continue;
                        BlendOver(canvas, (cy * width + cx) * 4, layer.Rgba, (y * layer.Width + x) * 4);
                    }
                }
            }
            return canvas;
        }
        #endregion

        #region [Private Methods]
        private static byte[] BuildLayerAndMaskSection(IReadOnlyList<CharacterPsdLayer> layers)
        {
            using (MemoryStream info = new MemoryStream())
            {
                WriteInt16(info, (short)-layers.Count);
                foreach (CharacterPsdLayer layer in layers)
                    WriteLayerRecord(info, layer);
                foreach (CharacterPsdLayer layer in layers)
                    WriteLayerChannels(info, layer);
                while (info.Length % 4 != 0) info.WriteByte(0);
                byte[] infoBytes = info.ToArray();
                using (MemoryStream section = new MemoryStream())
                {
                    WriteInt32(section, infoBytes.Length);
                    section.Write(infoBytes, 0, infoBytes.Length);
                    WriteInt32(section, 0);
                    return section.ToArray();
                }
            }
        }

        private static void WriteLayerRecord(Stream stream, CharacterPsdLayer layer)
        {
            WriteInt32(stream, layer.Top);
            WriteInt32(stream, layer.Left);
            WriteInt32(stream, layer.Top + layer.Height);
            WriteInt32(stream, layer.Left + layer.Width);
            WriteUInt16(stream, (ushort)ChannelIds.Length);
            int channelLength = 2 + layer.Width * layer.Height;
            foreach (short id in ChannelIds)
            {
                WriteInt16(stream, id);
                WriteInt32(stream, channelLength);
            }
            WriteAscii(stream, "8BIM");
            WriteAscii(stream, "norm");
            stream.WriteByte(255);
            stream.WriteByte(0);
            stream.WriteByte((byte)(FlagModernBit | (layer.Visible ? 0 : FlagHidden)));
            stream.WriteByte(0);
            byte[] extra = BuildExtraData(layer);
            WriteInt32(stream, extra.Length);
            stream.Write(extra, 0, extra.Length);
        }

        private static byte[] BuildExtraData(CharacterPsdLayer layer)
        {
            using (MemoryStream extra = new MemoryStream())
            {
                WriteInt32(extra, 0);
                WriteInt32(extra, 0);
                byte[] ascii = Encoding.ASCII.GetBytes(layer.Name.Length > 255 ? layer.Name.Substring(0, 255) : layer.Name);
                extra.WriteByte((byte)ascii.Length);
                extra.Write(ascii, 0, ascii.Length);
                int pascalLength = 1 + ascii.Length;
                while (pascalLength % 4 != 0)
                {
                    extra.WriteByte(0);
                    pascalLength++;
                }
                byte[] unicode = Encoding.BigEndianUnicode.GetBytes(layer.Name);
                int unicodeLength = 4 + unicode.Length;
                int unicodePadded = (unicodeLength + 3) / 4 * 4;
                WriteAscii(extra, "8BIM");
                WriteAscii(extra, "luni");
                WriteInt32(extra, unicodePadded);
                WriteInt32(extra, layer.Name.Length);
                extra.Write(unicode, 0, unicode.Length);
                for (int i = unicodeLength; i < unicodePadded; i++) extra.WriteByte(0);
                WriteAscii(extra, "8BIM");
                WriteAscii(extra, "lyid");
                WriteInt32(extra, 4);
                WriteInt32(extra, layer.LayerId);
                return extra.ToArray();
            }
        }

        private static void WriteLayerChannels(Stream stream, CharacterPsdLayer layer)
        {
            int pixels = layer.Width * layer.Height;
            byte[] plane = new byte[pixels];
            foreach (int offset in ChannelOffsets)
            {
                for (int i = 0; i < pixels; i++) plane[i] = layer.Rgba[i * 4 + offset];
                WriteUInt16(stream, 0);
                stream.Write(plane, 0, plane.Length);
            }
        }

        private static void WriteMergedImage(Stream stream, int width, int height, IReadOnlyList<CharacterPsdLayer> layers)
        {
            byte[] merged = Composite(width, height, layers);
            WriteUInt16(stream, 0);
            byte[] plane = new byte[width * height];
            for (int channel = 0; channel < 4; channel++)
            {
                for (int i = 0; i < plane.Length; i++) plane[i] = merged[i * 4 + channel];
                stream.Write(plane, 0, plane.Length);
            }
        }

        private static void BlendOver(byte[] dst, int d, byte[] src, int s)
        {
            float sa = src[s + 3] / 255f;
            if (sa <= 0f) return;
            float da = dst[d + 3] / 255f;
            float oa = sa + da * (1f - sa);
            for (int c = 0; c < 3; c++)
            {
                float color = (src[s + c] * sa + dst[d + c] * da * (1f - sa)) / oa;
                dst[d + c] = (byte)Math.Round(Math.Min(255f, color));
            }
            dst[d + 3] = (byte)Math.Round(oa * 255f);
        }

        private static void WriteAscii(Stream stream, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteUInt16(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteInt16(Stream stream, short value)
        {
            WriteUInt16(stream, (ushort)value);
        }

        private static void WriteInt32(Stream stream, int value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }
        #endregion
    }
}
