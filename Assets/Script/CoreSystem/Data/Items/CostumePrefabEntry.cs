using System.Collections.Generic;
using UnityEngine;
using CoreSystem;
using CoreSystem.Data;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Attributes;
using CoreSystem.Data.Skills;

namespace CoreSystem.Data.Items
{
    [System.Serializable]
    public class CostumePrefabEntry
    {
        /// <summary>Addressable key for the prefab (e.g. "Costume/Wing_A_Prefab").</summary>
        public string PrefabKey;
        public string TargetNode; // CostumeSlot enum string
        public Vector3 Offset;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
    }
}
