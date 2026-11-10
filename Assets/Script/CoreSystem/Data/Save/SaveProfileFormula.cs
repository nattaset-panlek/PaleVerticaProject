using System.IO;
using System.Text;

namespace CoreSystem.Data.Save
{
    public static class SaveProfileFormula
    {
        #region [Variables/Fields]
        public const string ProfilesFolderName = "Profiles";
        private const int MaxLength = 64;
        #endregion

        #region [Public Methods]
        public static string Sanitize(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile)) return string.Empty;

            var builder = new StringBuilder(profile.Length);
            foreach (char character in profile.Trim())
            {
                bool allowed = char.IsLetterOrDigit(character) || character == '-' || character == '_';
                builder.Append(allowed ? character : '_');
                if (builder.Length >= MaxLength) break;
            }
            return builder.ToString().Trim('_');
        }

        public static string FolderFor(string root, string profile)
        {
            string clean = Sanitize(profile);
            return clean.Length == 0 ? root : Path.Combine(root, ProfilesFolderName, clean);
        }
        #endregion
    }
}
