using System;
using System.Collections.Generic;

namespace CoreSystem.Localization
{
    [Serializable]
    public class LocalizationTable
    {
        #region [Variables/Fields]
        public List<LocalizationEntry> Entries = new List<LocalizationEntry>();
        #endregion
    }
}
