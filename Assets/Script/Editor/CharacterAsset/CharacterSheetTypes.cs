namespace CoreSystem.Editor.CharacterAsset
{
    public enum CharacterSheetLayout
    {
        Auto,
        Grid3x3,
        Grid4x3
    }

    public readonly struct CharacterSheetCell
    {
        #region [Variables/Fields]
        public readonly string Name;
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public CharacterSheetCell(string name, int x, int y, int width, int height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }

    public readonly struct CharacterPixelBounds
    {
        #region [Variables/Fields]
        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;
        #endregion

        #region [Properties]
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public bool IsEmpty => Width <= 0 || Height <= 0;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public CharacterPixelBounds(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }

    public class CharacterPsdLayer
    {
        #region [Variables/Fields]
        public string Name;
        public byte[] Rgba;
        public int Left;
        public int Top;
        public int Width;
        public int Height;
        public bool Visible = true;
        public int LayerId;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        #endregion

        #region [Private Methods]
        #endregion
    }
}
