using UnityEngine;

namespace CoreSystem.UI.Effects
{
    public sealed class FogAuraPalette
    {
        #region [Variables/Fields]
        public static readonly FogAuraPalette Ink = new FogAuraPalette("Ink",
            new[] { 0f, 0.15f, 0.4f, 0.65f, 1f },
            new[] { Hex(0x000000), Hex(0x070707), Hex(0x141414), Hex(0x8E1418), Hex(0xFF5A4E) });

        public static readonly FogAuraPalette Crimson = new FogAuraPalette("Crimson",
            new[] { 0f, 0.15f, 0.4f, 0.65f, 1f },
            new[] { Hex(0x050101), Hex(0x2A0405), Hex(0x7A0E12), Hex(0xD2343A), Hex(0xFFD7C9) });

        public static readonly FogAuraPalette Pale = new FogAuraPalette("Pale",
            new[] { 0f, 0.15f, 0.4f, 0.65f, 1f },
            new[] { Hex(0x060708), Hex(0x16191C), Hex(0x4C5560), Hex(0xB9C3CC), Hex(0xF4FBFF) });

        public readonly string Name;
        public readonly float[] Positions;
        public readonly Color[] Colors;

        private static readonly int[] ColorIds =
        {
            Shader.PropertyToID("_RampColor0"), Shader.PropertyToID("_RampColor1"), Shader.PropertyToID("_RampColor2"),
            Shader.PropertyToID("_RampColor3"), Shader.PropertyToID("_RampColor4")
        };
        private static readonly int StopsId = Shader.PropertyToID("_RampStops");
        #endregion

        #region [Public Methods]
        public FogAuraPalette(string name, float[] positions, Color[] colors)
        {
            Name = name;
            Positions = positions;
            Colors = colors;
        }

        public Color Evaluate(float t)
        {
            if (Positions == null || Positions.Length == 0) return Color.black;
            if (t <= Positions[0]) return Colors[0];
            for (int i = 1; i < Positions.Length; i++)
            {
                if (t > Positions[i]) continue;
                float span = Positions[i] - Positions[i - 1];
                float k = span > 0f ? (t - Positions[i - 1]) / span : 0f;
                return Color.Lerp(Colors[i - 1], Colors[i], k);
            }
            return Colors[Colors.Length - 1];
        }

        public void ApplyTo(Material material)
        {
            if (material == null || Colors == null || Colors.Length != ColorIds.Length) return;
            for (int i = 0; i < ColorIds.Length; i++) material.SetColor(ColorIds[i], Colors[i]);
            material.SetVector(StopsId, new Vector4(Positions[1], Positions[2], Positions[3], 0f));
        }
        #endregion

        #region [Private Methods]
        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }
        #endregion
    }
}
