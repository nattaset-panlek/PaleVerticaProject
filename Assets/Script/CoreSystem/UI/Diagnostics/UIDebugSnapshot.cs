using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CoreSystem.UI.Diagnostics
{
    public static class UIDebugSnapshot
    {
        #region [Variables/Fields]
        private static readonly Regex UnsafeChars = new Regex("[^A-Za-z0-9_.-]+");
        #endregion

        #region [Properties]
        public static string OutputDirectory => Application.isEditor
            ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "UIDebug"))
            : Path.Combine(Application.persistentDataPath, "UIDebug");
        #endregion

        #region [Public Methods]
        public static string GetBasePath(string name)
        {
            Directory.CreateDirectory(OutputDirectory);
            return Path.Combine(OutputDirectory, UnsafeChars.Replace(name, "_"));
        }

        public static void SaveImage(string basePath, string suffix, Texture2D image)
        {
            if (image == null) return;
            File.WriteAllBytes(basePath + suffix + ".png", image.EncodeToPNG());
        }

        public static void SaveReport(string basePath, string report)
        {
            File.WriteAllText(basePath + ".txt", report ?? string.Empty);
        }
        #endregion
    }
}
