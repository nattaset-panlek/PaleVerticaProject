using System.Collections.Generic;
using UnityEngine;
using CoreSystem;

namespace CoreSystem.Attributes
{
    // ─── Serialisation helpers (used by JsonUtility) ──────────────────────────

    [System.Serializable]
    public class AttributeDataList
    {
        public List<AttributeEntry> Attributes;
    }
}
