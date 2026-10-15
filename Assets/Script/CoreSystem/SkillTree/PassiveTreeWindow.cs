using CoreSystem.SkillTree;
using CoreSystem.SkillTree.Models;
using CoreSystem.Entity;
using CoreSystem.UI;
using System.Collections.Generic;
using System.Linq;
using CoreSystem.UI.Components;
using UnityEngine;
using CoreSystem.Data;
using TMPro;

namespace CoreSystem.SkillTree
{
    public class PassiveTreeWindow : UIWindow
    {
        public static PassiveTreeWindow Instance { get; private set; }

        #region [Variables/Fields]

        [Header("Parents")]
        public Transform SkillPageParent;

        [Header("Prefabs")]
        public GameObject ClassNodePrefab;
        public GameObject AttributeNodePrefab;
        public GameObject KeySkillNodePrefab;

        [Header("UI Containers")]
        public GameObject ActionButtonContainer;
        public TMP_Text PassivePointText;

        [Header("Position Offset")]
        public Vector2 PositionOffset = Vector2.zero;
        public float   PositionScale  = 1f;

        [Header("Lines Styling")]
        public float LineWidth        = 3f;
        [Range(0f, 0.5f)]
        public float LineAntialias     = 0.06f;
        public Color NormalLineColor   = new Color(0.3f, 0.3f, 0.3f, 1f);
        public Color UnlockedLineColor = new Color(1f, 0.8f, 0f, 1f);
        public Color SelectedLineColor = new Color(0.5f, 0.9f, 1f, 1f);

        private Dictionary<int, Vector2>          _nodePositions = new Dictionary<int, Vector2>();
        private Dictionary<int, PassiveSkillNode>  _nodeObjects   = new Dictionary<int, PassiveSkillNode>();
        private Dictionary<long, UILineRenderer>  _lineObjects   = new Dictionary<long, UILineRenderer>();
        private HashSet<int>                      _selectedNodeIds = new HashSet<int>();

        private Transform _linesRoot;
        private Transform _nodesRoot;
        private Character _currentCharacter;

        #endregion

        #region [Properties]

        public int SelectedCount => _selectedNodeIds.Count;

        private HashSet<int> UnlockedIds
        {
            get
            {
                var set = new HashSet<int>();
                if (_currentCharacter == null) return set;
                foreach (var rec in _currentCharacter.PassiveNodes)
                    set.Add(rec.NodeId);
                return set;
            }
        }

        #endregion

        #region [Unity Methods]

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        #endregion

        #region [Public Methods]

        public override void OnOpen(object data)
        {
            base.OnOpen(data);

            LoadFromCharacter(GameData.Player.PlayerCharacter);

            if (_currentCharacter != null)
            {
                LoadFromCharacter(_currentCharacter);
            }
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public void Initialize()
        {
            if (_nodeObjects.Count > 0) return;

            SkillTreeData data = SkillTreeLoader.Load();
            if (data == null) return;

            SetupRoots();
            _nodePositions.Clear();
            _nodeObjects.Clear();
            _lineObjects.Clear();
            _selectedNodeIds.Clear();

            SpawnNodes(data.nodes);
            SpawnEdges(data.edges);
        }

        [ContextMenu("Force Rebuild Tree")]
        public void ForceRebuild()
        {
            ClearParent();
            Initialize();
        }

        public void LoadFromCharacter(Character character)
        {
            _currentCharacter = character;
            Initialize();
            RefreshUI();
        }

        public void ClearParent()
        {
            if (SkillPageParent == null) return;
            for (int i = SkillPageParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(SkillPageParent.GetChild(i).gameObject);

            _nodePositions.Clear();
            _nodeObjects.Clear();
            _lineObjects.Clear();
            _selectedNodeIds.Clear();
            if (ActionButtonContainer != null) ActionButtonContainer.SetActive(false);
            _linesRoot = null;
            _nodesRoot = null;
        }

        public void OnNodeClick(int nodeId)
        {
            if (_currentCharacter == null) return;

            var unlocked = UnlockedIds;
            if (unlocked.Contains(nodeId)) return;

            if (_selectedNodeIds.Contains(nodeId))
            {
                _selectedNodeIds.Remove(nodeId);
                ValidateSelection();
            }
            else
            {
                if (IsNodeUnlockable(nodeId, _selectedNodeIds))
                {
                    int totalCost = _selectedNodeIds.Count + 1;
                    if (totalCost <= _currentCharacter.PassivePoints)
                        _selectedNodeIds.Add(nodeId);
                    else
                        Debug.Log("[PassiveSkill] Not enough points for selection!");
                }
            }

            RefreshUI();
        }

        public void Save()
        {
            if (_currentCharacter == null || _selectedNodeIds.Count == 0) return;

            _currentCharacter.PassivePoints -= _selectedNodeIds.Count;

            foreach (int nodeId in _selectedNodeIds)
            {
                var node = SkillTreeDatabase.GetNode(nodeId);
                if (node == null) continue;

                if (node.HasStats)
                {
                    foreach (var bonus in node.extras)
                    {
                        _currentCharacter.PassiveNodes.Add(
                            new PassiveNodeRecord(nodeId, bonus.id, bonus.value)
                        );
                    }
                }
                else
                {
                    _currentCharacter.PassiveNodes.Add(new PassiveNodeRecord(nodeId, 0, 0f));
                }
            }

            _selectedNodeIds.Clear();
            _currentCharacter?.RecalculateStats();

            RefreshUI();
            Debug.Log("[PassiveSkill] Changes saved!");
        }

        public void Cancel()
        {
            _selectedNodeIds.Clear();
            RefreshUI();
        }

        public void TryUnlockNode(int nodeId) => OnNodeClick(nodeId);

        #endregion

        #region Private Methods

        private void ValidateSelection()
        {
            var unlocked = UnlockedIds;
            var validSelection = new HashSet<int>();
            var queue = new Queue<int>();

            // Step 1: Find all valid roots in the current selection
            foreach (int id in _selectedNodeIds)
            {
                var nodeData = SkillTreeDatabase.GetNode(id);
                if (nodeData == null) continue;

                if (nodeData.IsBaseClass)
                {
                    validSelection.Add(id);
                    queue.Enqueue(id);
                    continue;
                }

                bool hasUnlockedNeighbor = SkillTreeDatabase.GetNeighborIds(id).Any(n => unlocked.Contains(n));
                if (hasUnlockedNeighbor)
                {
                    validSelection.Add(id);
                    queue.Enqueue(id);
                }
            }

            // Step 2: Flood fill to find all connected selected nodes
            while (queue.Count > 0)
            {
                int curr = queue.Dequeue();
                var neighbors = SkillTreeDatabase.GetNeighborIds(curr);
                foreach (int neighborId in neighbors)
                {
                    if (_selectedNodeIds.Contains(neighborId) && !validSelection.Contains(neighborId))
                    {
                        validSelection.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }

            // Step 3: Remove any disconnected islands
            _selectedNodeIds.IntersectWith(validSelection);
        }

        private void RefreshUI()
        {
            if (_currentCharacter == null) return;

            if (PassivePointText != null)
            {
                int usedPoints = _selectedNodeIds.Count;
                if (usedPoints > 0)
                {
                    int remaining = _currentCharacter.PassivePoints - usedPoints;
                    PassivePointText.text = $"{remaining} (-{usedPoints})";
                }
                else
                {
                    PassivePointText.text = _currentCharacter.PassivePoints.ToString();
                }
            }

            if (ActionButtonContainer != null)
                ActionButtonContainer.SetActive(_selectedNodeIds.Count > 0);

            var unlocked = UnlockedIds;

            foreach (var kvp in _nodeObjects)
            {
                int nodeId  = kvp.Key;
                var nodeObj = kvp.Value;

                bool isUnlocked = unlocked.Contains(nodeId);
                bool isSelected = _selectedNodeIds.Contains(nodeId);
                bool canUnlock  = IsNodeUnlockable(nodeId, _selectedNodeIds);

                PassiveSkillState state = isUnlocked ? PassiveSkillState.Upgraded :
                                         (isSelected ? PassiveSkillState.Selected :
                                         (canUnlock  ? PassiveSkillState.CanUpgrade : PassiveSkillState.CannotUpgrade));

                nodeObj.SetUp(nodeObj.Node, _nodePositions[nodeId], state);
            }

            foreach (var kvp in _lineObjects)
            {
                long key  = kvp.Key;
                var  line = kvp.Value;

                int idA = (int)(key >> 32);
                int idB = (int)(key & 0xFFFFFFFF);

                bool active   = unlocked.Contains(idA) && unlocked.Contains(idB);
                bool selected = (unlocked.Contains(idA) || _selectedNodeIds.Contains(idA)) &&
                                (unlocked.Contains(idB) || _selectedNodeIds.Contains(idB)) &&
                                !active;

                if (active)        line.color = UnlockedLineColor;
                else if (selected) line.color = SelectedLineColor;
                else               line.color = NormalLineColor;
            }
        }

        private bool IsNodeUnlockable(int nodeId, HashSet<int> selection = null)
        {
            var unlocked = UnlockedIds;
            if (unlocked.Contains(nodeId)) return false;

            var nodeData = SkillTreeDatabase.GetNode(nodeId);
            if (nodeData.IsBaseClass)
            {
                bool hasBase = unlocked
                    .Select(id => SkillTreeDatabase.GetNode(id))
                    .Any(n => n != null && n.IsBaseClass);

                if (hasBase) return false;

                if (selection != null)
                {
                    bool hasSelectedBase = selection
                        .Select(id => SkillTreeDatabase.GetNode(id))
                        .Any(n => n != null && n.IsBaseClass);
                    if (hasSelectedBase) return false;
                }

                return true;
            }

            bool hasNeighborUnlocked = SkillTreeDatabase.GetNeighborIds(nodeId)
                .Any(neighborId => unlocked.Contains(neighborId));

            if (hasNeighborUnlocked) return true;

            if (selection != null)
            {
                return SkillTreeDatabase.GetNeighborIds(nodeId)
                    .Any(neighborId => selection.Contains(neighborId));
            }

            return false;
        }

        private void SetupRoots()
        {
            if (SkillPageParent == null) return;
            _linesRoot = CreateRoot("Lines");
            _linesRoot.SetAsFirstSibling();
            _nodesRoot = CreateRoot("Nodes");
        }

        private Transform CreateRoot(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(SkillPageParent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;
            return go.transform;
        }

        private void SpawnNodes(List<SkillTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                GameObject prefab = SelectPrefab(node.shape);
                if (prefab == null) continue;

                Vector2 pos = (node.Position * PositionScale) + PositionOffset;
                _nodePositions[node.id] = pos;

                var go = Instantiate(prefab, _nodesRoot);
                var skillNode = go.GetComponent<PassiveSkillNode>();
                _nodeObjects[node.id] = skillNode;

                skillNode.SetUp(node, pos, PassiveSkillState.CannotUpgrade);
            }
        }

        private void SpawnEdges(List<SkillTreeEdge> edges)
        {
            var drawn = new HashSet<long>();
            foreach (var edge in edges)
            {
                int  lo  = Mathf.Min(edge.from, edge.to);
                int  hi  = Mathf.Max(edge.from, edge.to);
                long key = ((long)lo << 32) | (uint)hi;
                if (!drawn.Add(key)) continue;

                if (!_nodePositions.TryGetValue(edge.from, out Vector2 posFrom)) continue;
                if (!_nodePositions.TryGetValue(edge.to,   out Vector2 posTo))   continue;

                _lineObjects[key] = CreateLine(_linesRoot, posFrom, posTo, edge.id);
            }
        }

        private UILineRenderer CreateLine(Transform parent, Vector2 from, Vector2 to, int edgeId)
        {
            var go = new GameObject($"Edge_{edgeId}", typeof(RectTransform), typeof(UILineRenderer));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            Vector2 center = (from + to) * 0.5f;
            Vector2 size   = new Vector2(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
            size.x = Mathf.Max(size.x, 10f); size.y = Mathf.Max(size.y, 10f);
            rt.anchoredPosition = center; rt.sizeDelta = size;

            var line = go.GetComponent<UILineRenderer>();
            line.color     = NormalLineColor;
            line.LineWidth = LineWidth;
            line.Antialias = LineAntialias;
            line.SetPoints(new[] { from - center, to - center });
            return line;
        }

        private GameObject SelectPrefab(string shape)
        {
            return shape switch { "poly" => ClassNodePrefab, "circle" => AttributeNodePrefab, "rect" => KeySkillNodePrefab, _ => null };
        }

        #endregion
    }
}
