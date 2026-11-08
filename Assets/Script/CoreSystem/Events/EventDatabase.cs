using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Events
{
    public static class EventDatabase
    {
        #region [Variables/Fields]
        private const string ResourcePath = "Data/Event";
        private static List<EventModel> _events;
        #endregion

        #region [Properties]
        public static IReadOnlyList<EventModel> All
        {
            get
            {
                if (_events == null) Load();
                return _events;
            }
        }
        #endregion

        #region [Private Methods]
        private static void Load()
        {
            TextAsset file = Resources.Load<TextAsset>(ResourcePath);
            _events = file != null ? JsonUtility.FromJson<EventDataList>(file.text).Events : new List<EventModel>();
            if (file == null) Debug.LogError("EventDatabase: Event.json not found in Resources/Data/");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _events = null;
        }
        #endregion
    }
}
