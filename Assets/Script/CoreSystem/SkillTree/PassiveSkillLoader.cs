using CoreSystem.SkillTree;
using CoreSystem.SkillTree.Models;
using CoreSystem.Entity;
using System.Collections.Generic;
using System.Linq;
using UI.Components;
using UnityEngine;

public class PassiveSkillLoader : MonoBehaviour
{
    public static PassiveSkillLoader Instance { get; private set; }

    #region Variables/Fields

    [Header("Parents")]
    public Transform SkillPageParent;

    [Header("Prefabs")]
    public GameObject ClassNodePrefab;
    public GameObject AttributeNodePrefab;
    public GameObject KeySkillNodePrefab;

    [Header("UI Containers")]
    public GameObject ActionButtonContainer;

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
    private Dictionary<int, PassiveSkilNode>  _nodeObjects   = new Dictionary<int, PassiveSkilNode>();
    private Dictionary<long, UILineRenderer> _lineObjects   = new Dictionary<long, UILineRenderer>();
    private HashSet<int>                      _selectedNodeIds = new HashSet<int>();

    private Transform _linesRoot;
    private Transform _nodesRoot;
    private PlayerCharacter _currentPlayer;

    #endregion

    #region Properties

    public int SelectedCount => _selectedNodeIds.Count;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region Public Methods

    /// <summary>สร้าง Node Tree ทั้งหมด (สร้างเพียงครั้งเดียวเพื่อประหยัด Memory)</summary>
    public void Initialize()
    {
        if (_nodeObjects.Count > 0) return; // ถ้าสร้างแล้วข้ามเลย

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

    /// <summary>โหลดข้อมูลสถานะการอัปสกิลจาก PlayerCharacter</summary>
    public void LoadFromPlayer(PlayerCharacter player)
    {
        _currentPlayer = player;
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

    /// <summary>เมื่อคลิกที่ Node</summary>
    public void OnNodeClick(int nodeId)
    {
        if (_currentPlayer == null) return;
        if (_currentPlayer.UnlockedPassiveNodeIds.Contains(nodeId)) return;

        if (_selectedNodeIds.Contains(nodeId))
        {
            _selectedNodeIds.Remove(nodeId);
            ValidateSelection();
        }
        else
        {
            // ตรวจสอบว่าอัปได้หรือไม่ (รวม selection ปัจจุบัน)
            if (IsNodeUnlockable(nodeId, _selectedNodeIds))
            {
                int totalCost = _selectedNodeIds.Count + 1;
                if (totalCost <= _currentPlayer.PassivePoints)
                {
                    _selectedNodeIds.Add(nodeId);
                }
                else
                {
                    Debug.Log("[PassiveSkill] Not enough points for selection!");
                }
            }
        }

        RefreshUI();
    }

    public void Save()
    {
        if (_currentPlayer == null || _selectedNodeIds.Count == 0) return;

        // ดับเบิ้ลเช็คอีกรอบ
        _currentPlayer.PassivePoints -= _selectedNodeIds.Count;
        foreach (int id in _selectedNodeIds)
        {
            _currentPlayer.UnlockedPassiveNodeIds.Add(id);
        }

        _selectedNodeIds.Clear();
        RefreshUI();
        Debug.Log("[PassiveSkill] Changes saved!");
    }

    public void Cancel()
    {
        _selectedNodeIds.Clear();
        RefreshUI();
    }

    private void ValidateSelection()
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            var currentSelection = new HashSet<int>(_selectedNodeIds);
            var toRemove = new List<int>();

            foreach (int id in currentSelection)
            {
                // ถ้า Node นี้ไม่ใช่ Base และไม่มีทางเชื่อมที่สมบูรณ์แล้ว ให้ลบออก
                var otherSelected = new HashSet<int>(currentSelection);
                otherSelected.Remove(id);
                if (!IsNodeUnlockable(id, otherSelected)) 
                {
                    toRemove.Add(id);
                }
            }

            foreach (int id in toRemove)
            {
                if (_selectedNodeIds.Remove(id)) changed = true;
            }
        }
    }

    public void TryUnlockNode(int nodeId)
    {
        // ย้าย logic selection ไปที่ OnNodeClick แล้ว
        OnNodeClick(nodeId);
    }

    #endregion

    #region Private Methods

    private void RefreshUI()
    {
        if (_currentPlayer == null) return;

        // อัปเดตการแสดงผล Container ปุ่มกด (แสดงเมื่อมีการเลือก)
        if (ActionButtonContainer != null)
        {
            ActionButtonContainer.SetActive(_selectedNodeIds.Count > 0);
        }

        // อัปเดตสถานะ Node
        foreach (var kvp in _nodeObjects)
        {
            int nodeId = kvp.Key;
            var nodeObj = kvp.Value;
            
            bool isUnlocked = _currentPlayer.UnlockedPassiveNodeIds.Contains(nodeId);
            bool isSelected = _selectedNodeIds.Contains(nodeId);
            bool canUnlock  = IsNodeUnlockable(nodeId, _selectedNodeIds);
            
            PassiveSkillState state = isUnlocked ? PassiveSkillState.Upgraded : 
                                     (isSelected ? PassiveSkillState.Selected :
                                     (canUnlock  ? PassiveSkillState.CanUpgrade : PassiveSkillState.CannotUpgrade));
            
            nodeObj.SetUp(nodeObj.Node, _nodePositions[nodeId], state);
        }

        // อัปเดตสีเส้น
        foreach (var kvp in _lineObjects)
        {
            long key = kvp.Key;
            var line = kvp.Value;
            
            int idA = (int)(key >> 32);
            int idB = (int)(key & 0xFFFFFFFF);

            // เส้นจะเปลี่ยนสีถ้า Node ทั้งสองฝั่งถูกอัปแล้ว
            bool active = _currentPlayer.UnlockedPassiveNodeIds.Contains(idA) && 
                          _currentPlayer.UnlockedPassiveNodeIds.Contains(idB);
            
            bool selected = (_currentPlayer.UnlockedPassiveNodeIds.Contains(idA) || _selectedNodeIds.Contains(idA)) &&
                           (_currentPlayer.UnlockedPassiveNodeIds.Contains(idB) || _selectedNodeIds.Contains(idB)) &&
                           !active;

            if (active) line.color = UnlockedLineColor;
            else if (selected) line.color = SelectedLineColor;
            else line.color = NormalLineColor;
        }
    }

    private bool IsNodeUnlockable(int nodeId, HashSet<int> selection = null)
    {
        if (_currentPlayer.UnlockedPassiveNodeIds.Contains(nodeId)) return false;
        
        var nodeData = SkillTreeDatabase.GetNode(nodeId);
        if (nodeData.IsBaseClass) {
             bool hasBase = _currentPlayer.UnlockedPassiveNodeIds
                .Select(id => SkillTreeDatabase.GetNode(id))
                .Any(n => n != null && n.IsBaseClass);
             
             if (hasBase) return false;

             if (selection != null) {
                 bool hasSelectedBase = selection
                    .Select(id => SkillTreeDatabase.GetNode(id))
                    .Any(n => n != null && n.IsBaseClass);
                 if (hasSelectedBase) return false;
             }

             return true;
        }

        bool hasNeighborUnlocked = SkillTreeDatabase.GetNeighborIds(nodeId)
            .Any(neighborId => _currentPlayer.UnlockedPassiveNodeIds.Contains(neighborId));

        if (hasNeighborUnlocked) return true;

        // ถ้าไม่มีเพื่อนที่ปลดแล้ว ให้เช็คเพื่อนที่กำลังเลือกอยู่ (Chain)
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
            var skillNode = go.GetComponent<PassiveSkilNode>();
            _nodeObjects[node.id] = skillNode;
            
            // เซ็ตเบื้องต้น (แบบไม่มี Player)
            skillNode.SetUp(node, pos, PassiveSkillState.CannotUpgrade);
        }
    }

    private void SpawnEdges(List<SkillTreeEdge> edges)
    {
        var drawn = new HashSet<long>();
        foreach (var edge in edges)
        {
            int lo = Mathf.Min(edge.from, edge.to);
            int hi = Mathf.Max(edge.from, edge.to);
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
        Vector2 size = new Vector2(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
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
