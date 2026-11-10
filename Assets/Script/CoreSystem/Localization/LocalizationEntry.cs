using System;
using CoreSystem.Data;

namespace CoreSystem.Localization
{
    [Serializable]
    public class LocalizationEntry
    {
        #region [Variables/Fields]
        public string Key;
        public LocalizedString Text = new LocalizedString();
        #endregion
    }
}
