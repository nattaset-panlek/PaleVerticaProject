using System.Collections.Generic;
using CoreSystem.Data.Items.Effects;
using UnityEngine;

namespace CoreSystem.Data.Items.Model
{
    /// <summary>
    /// Describes a prefab visual attachment for a Costume.
    /// PrefabKey is the Addressable key to load the prefab at runtime.
    /// </summary>
    [System.Serializable]
    public class CostumeVisualPrefabData
    {
        public string PrefabKey;         // Addressable key (e.g. "Costume/Hair_A_Prefab")
        public CostumeSlot TargetNode;   // Which part of the model to attach to
        public Vector3 Offset;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
    }
}
