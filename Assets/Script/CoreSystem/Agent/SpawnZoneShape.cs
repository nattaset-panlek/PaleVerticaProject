using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Enums;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CoreSystem.Data;

namespace CoreSystem.Agent
{
    // ─── Spawn Zone Shape ────────────────────────────────────────────────────────
    public enum SpawnZoneShape { Circle, Box }
}
