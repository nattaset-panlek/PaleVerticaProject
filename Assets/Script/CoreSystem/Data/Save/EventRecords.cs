using System;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class EventRecord
    {
        public int EventId;
        public int Count;
        public bool Fired;
    }

    [Serializable]
    public class EventObjectRecord
    {
        public string ObjectId;
        public bool Active;
    }
}
