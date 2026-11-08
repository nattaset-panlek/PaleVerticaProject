using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Pathfinding
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [AddComponentMenu("Pale Vertica/World/Walkable Grid Manager")]
    public sealed class WalkableGridManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private const int MaxGizmoCells = 40000;

        private static WalkableGridManager _instance;

        [Tooltip("Walkable.json files baked by MapLab (<scene>_MapLab/<map>/Walkable.json), one per map in this scene.")]
        [SerializeField] private TextAsset[] _walkableData = Array.Empty<TextAsset>();
        [Tooltip("Agent body radius in world units. Cells whose centre is closer than this to an obstacle are not walkable.")]
        [SerializeField, Min(0f)] private float _agentRadius = 0.25f;
        [Tooltip("Search budget per path query, in cells expanded.")]
        [SerializeField, Min(1)] private int _maxSearchCells = GridPathFormula.DefaultMaxExpansions;
        [Tooltip("How many cells a blocked or unreachable start or end point may move to reach the nearest usable cell.")]
        [SerializeField, Min(0)] private int _snapCells = GridPathFormula.DefaultSnapCells;

        private readonly List<WalkableGrid> _grids = new List<WalkableGrid>();
        #endregion

        #region [Properties]
        public static WalkableGridManager Instance => _instance;
        public static bool IsReady => _instance != null;
        public static event Action OnReady;
        public static event Action OnGridsChanged;
        public IReadOnlyList<WalkableGrid> Grids => _grids;
        public bool HasGrids => _grids.Count > 0;
        public float AgentRadius => _agentRadius;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[WalkableGridManager] Another manager is already active; '{name}' was removed. Keep one per scene.", this);
                Destroy(this);
                return;
            }

            _instance = this;
            Load();
            OnReady?.Invoke();
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            _instance = null;
            _grids.Clear();
            OnGridsChanged?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _walkableData = FindBakedWalkableData();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) _grids.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying && _grids.Count == 0) LoadGrids();

            foreach (var grid in _grids)
            {
                Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.9f);
                Gizmos.DrawWireCube(grid.Bounds.center, grid.Bounds.size);

                if (grid.CellCount > MaxGizmoCells) continue;
                var size = new Vector3(grid.CellSize * 0.9f, grid.CellSize * 0.9f, 0.01f);
                for (int row = 0; row < grid.Rows; row++)
                {
                    for (int column = 0; column < grid.Columns; column++)
                    {
                        if (grid.IsWalkable(column, row)) continue;
                        Gizmos.color = grid.IsBakedWalkable(column, row) ? new Color(1f, 0.6f, 0f, 0.25f) : new Color(1f, 0f, 0f, 0.3f);
                        Gizmos.DrawCube(grid.CellCenter(column, row), size);
                    }
                }
            }
        }
#endif
        #endregion

        #region [Public Methods]
        public void SetWalkableData(params TextAsset[] walkableData)
        {
            _walkableData = walkableData ?? Array.Empty<TextAsset>();
            Load();
        }

        public void Load()
        {
            LoadGrids();
            OnGridsChanged?.Invoke();
        }

        public WalkableGrid GetGridAt(Vector2 position)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                if (_grids[i].Contains(position)) return _grids[i];
            }
            return null;
        }

        public bool IsWalkable(Vector2 position)
        {
            var grid = GetGridAt(position);
            return grid != null && grid.IsWalkable(position);
        }

        public bool TryFindPath(Vector2 from, Vector2 to, List<Vector2> result)
        {
            return GridPathFormula.Found(FindPath(from, to, result));
        }

        public PathOutcome FindPath(Vector2 from, Vector2 to, List<Vector2> result)
        {
            result.Clear();
            return GridPathFormula.FindPath(GetGridAt(from), from, to, result, _maxSearchCells, _snapCells);
        }

        public PathOutcome SnapPoint(Vector2 from, Vector2 point, out Vector2 snapped)
        {
            return GridPathFormula.SnapPoint(GetGridAt(from), from, point, out snapped, _snapCells);
        }
        #endregion

        #region [Private Methods]
        private void LoadGrids()
        {
            _grids.Clear();
            if (_walkableData == null) return;

            foreach (var data in _walkableData)
            {
                if (data == null) continue;
                if (WalkableGridLoader.TryLoad(data, _agentRadius, out var grid, out string error)) _grids.Add(grid);
                else Debug.LogWarning($"[WalkableGridManager] {error}", this);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnReady = null;
            OnGridsChanged = null;
        }

#if UNITY_EDITOR
        [ContextMenu("Find Baked Walkable Data")]
        private void FillBakedWalkableData()
        {
            UnityEditor.Undo.RecordObject(this, "Find Baked Walkable Data");
            _walkableData = FindBakedWalkableData();
            _grids.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private TextAsset[] FindBakedWalkableData()
        {
            string scenePath = gameObject.scene.path;
            if (string.IsNullOrEmpty(scenePath)) return Array.Empty<TextAsset>();

            string folder = scenePath.Substring(0, scenePath.Length - System.IO.Path.GetExtension(scenePath).Length) + "_MapLab";
            if (!UnityEditor.AssetDatabase.IsValidFolder(folder)) return Array.Empty<TextAsset>();

            var found = new List<TextAsset>();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("Walkable t:TextAsset", new[] { folder }))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileName(path) != "Walkable.json") continue;
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset != null) found.Add(asset);
            }
            return found.ToArray();
        }
#endif
        #endregion
    }
}

