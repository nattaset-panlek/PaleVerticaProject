using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class SaveMetaDataRecord
    {
        public int SlotIndex;
        public string SaveName = "New Save";
        public string SaveTimestamp = "";
        public string SaveVersion = "1.0.0";
        public float TotalPlayTimeSeconds = 0f;
    }
}
