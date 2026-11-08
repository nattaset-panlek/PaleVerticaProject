using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Loot
{
    public static class DropTableDatabase
    {
        #region [Variables/Fields]
        private const string ResourcePath = "Data/DropTable";
        private static readonly Dictionary<int, DropTableModel> _tables = new Dictionary<int, DropTableModel>();
        private static DropTableDataList _data;
        #endregion

        #region [Properties]
        public static float GroundLifetimeSeconds => Data.GroundLifetimeSeconds;
        public static float OwnerLockSeconds => Data.OwnerLockSeconds;

        private static DropTableDataList Data
        {
            get
            {
                if (_data == null) Load();
                return _data;
            }
        }
        #endregion

        #region [Public Methods]
        public static DropTableModel Get(int id)
        {
            if (_data == null) Load();
            _tables.TryGetValue(id, out DropTableModel table);
            return table;
        }
        #endregion

        #region [Private Methods]
        private static void Load()
        {
            _tables.Clear();
            TextAsset file = Resources.Load<TextAsset>(ResourcePath);
            if (file == null) Debug.LogError("DropTableDatabase: DropTable.json not found in Resources/Data/");
            _data = file != null ? JsonUtility.FromJson<DropTableDataList>(file.text) : new DropTableDataList();
            foreach (DropTableModel table in _data.Tables) _tables[table.Id] = table;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _tables.Clear();
            _data = null;
        }
        #endregion
    }
}
