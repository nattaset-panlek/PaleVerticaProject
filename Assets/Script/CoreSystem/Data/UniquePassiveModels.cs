using System.Collections.Generic;

namespace CoreSystem.Data
{
    [System.Serializable]
    public class UniquePassiveEntry
    {
        #region [Variables/Fields]
        public string Id;
        public string Name;
        public string Description;
        #endregion
    }

    [System.Serializable]
    public class UniquePassiveDataList
    {
        #region [Variables/Fields]
        public List<UniquePassiveEntry> UniquePassives;
        #endregion
    }
}
