using System;
using System.Collections.Generic;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class QuestRecord
    {
        public int QuestId;
        public bool Completed;
        public bool Untracked;
        public List<int> Counts = new List<int>();
    }
}
