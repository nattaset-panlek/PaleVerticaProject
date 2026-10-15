using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Data
{
    [System.Serializable]
    public class FactionEntry
    {
        public int Id;
        public string Name;
        public List<int> Positive;
        public List<int> Negative;
    }
}
