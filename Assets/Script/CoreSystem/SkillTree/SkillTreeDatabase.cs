using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoreSystem.SkillTree.Models;

namespace CoreSystem.SkillTree
{
    /// <summary>
    /// Static database สำหรับระบบ SkillTree
    /// เรียก Initialize() ผ่าน Database.Initialize() โดยอัตโนมัติ
    /// </summary>
    public static class SkillTreeDatabase
    {
        #region Variables/Fields

        private static SkillTreeData                          _data;
        private static Dictionary<int, SkillTreeNode>         _nodeMap;
        private static Dictionary<int, List<SkillTreeEdge>>   _adjacencyFrom;
        private static bool _initialized;

        #endregion

        #region Public Methods — Init

        /// <summary>โหลดข้อมูลจาก JSON — ถูกเรียกจาก Database.Initialize()</summary>
        public static void Initialize()
        {
            if (_initialized) return;

            _data = SkillTreeLoader.Load();
            if (_data == null)
            {
                Debug.LogError("[SkillTreeDatabase] โหลดข้อมูลล้มเหลว");
                return;
            }

            BuildIndex();
            _initialized = true;
            Debug.Log($"[SkillTreeDatabase] โหลดสำเร็จ: {_data.nodes.Count} nodes, {_data.edges.Count} edges");
        }

        /// <summary>รีเซ็ตและโหลดใหม่ (ใช้ใน Editor Hot-Reload)</summary>
        public static void Reload()
        {
            _initialized   = false;
            _nodeMap       = null;
            _adjacencyFrom = null;
            Initialize();
        }

        #endregion

        #region Public Methods — Node

        /// <summary>ดึง Node จาก int id — คืน null ถ้าไม่พบ</summary>
        public static SkillTreeNode GetNode(int id)
        {
            EnsureInitialized();
            return _nodeMap.TryGetValue(id, out var node) ? node : null;
        }

        /// <summary>ดึง Node ทั้งหมด</summary>
        public static List<SkillTreeNode> GetAllNodes()
        {
            EnsureInitialized();
            return _data.nodes;
        }

        /// <summary>ดึง Node ที่ role = "base" (จุดอาชีพ)</summary>
        public static List<SkillTreeNode> GetBaseClassNodes()
        {
            EnsureInitialized();
            return _data.nodes.Where(n => n.role == "base").ToList();
        }

        /// <summary>ดึง Node ที่ isActive = true</summary>
        public static List<SkillTreeNode> GetActiveNodes()
        {
            EnsureInitialized();
            return _data.nodes.Where(n => n.isActive).ToList();
        }

        /// <summary>ค้นหา Node จากชื่อ label (ไม่ case-sensitive)</summary>
        public static List<SkillTreeNode> FindNodesByLabel(string label)
        {
            EnsureInitialized();
            string lower = label.ToLowerInvariant();
            return _data.nodes.Where(n => n.label.ToLowerInvariant().Contains(lower)).ToList();
        }

        /// <summary>ดึง Node ที่มี stat bonus ใน extras</summary>
        public static List<SkillTreeNode> GetStatNodes()
        {
            EnsureInitialized();
            return _data.nodes.Where(n => n.HasStats).ToList();
        }

        #endregion

        #region Public Methods — Edge

        /// <summary>ดึง Edge ทั้งหมด</summary>
        public static List<SkillTreeEdge> GetAllEdges()
        {
            EnsureInitialized();
            return _data.edges;
        }

        /// <summary>ดึง Edge ที่ออกจาก nodeId</summary>
        public static List<SkillTreeEdge> GetEdgesFrom(int nodeId)
        {
            EnsureInitialized();
            return _adjacencyFrom.TryGetValue(nodeId, out var list) ? list : new List<SkillTreeEdge>();
        }

        /// <summary>ดึง id ของ Node ที่เชื่อมต่อออกไปจาก nodeId</summary>
        public static List<int> GetNeighborIds(int nodeId)
            => GetEdgesFrom(nodeId).Select(e => e.to).ToList();

        /// <summary>ตรวจว่า Node นี้ unlock ได้ไหม (prerequisite ต้องมีอย่างน้อย 1 active)</summary>
        public static bool CanUnlock(int nodeId)
        {
            EnsureInitialized();
            var node = GetNode(nodeId);
            if (node == null || node.isActive) return false;

            var prerequisites = _data.edges
                .Where(e => e.to == nodeId)
                .Select(e => e.from)
                .ToList();

            if (prerequisites.Count == 0) return true;

            return prerequisites.Any(pid =>
            {
                var pre = GetNode(pid);
                return pre != null && pre.isActive;
            });
        }

        #endregion

        #region Public Methods — Stats

        /// <summary>คำนวณ stats จากโหนดที่ isActive ทั้งหมด</summary>
        public static SkillTreeStats GetActiveStats()
        {
            EnsureInitialized();
            var stats = new SkillTreeStats();

            int activeCount = 0;
            foreach (var node in _data.nodes)
            {
                if (!node.isActive || !node.HasStats) continue;
                AccumulateStats(stats, node.extras);
                activeCount++;
            }

            Debug.Log($"[SkillTreeDatabase] Accumulated stats from {activeCount} active nodes. STR in stats: {stats.STR}");
            return stats;
        }

        /// <summary>คำนวณ stats จากชุด node ids ที่กำหนดเอง (ใช้ Preview / Build Planner)</summary>
        public static SkillTreeStats GetStatsForNodes(IEnumerable<int> nodeIds)
        {
            EnsureInitialized();
            var stats = new SkillTreeStats();

            foreach (var id in nodeIds)
            {
                var node = GetNode(id);
                if (node != null && node.HasStats)
                    AccumulateStats(stats, node.extras);
            }

            return stats;
        }

        #endregion

        #region Public Methods — Unlock / Save / Load

        /// <summary>ตั้งสถานะ isActive ของ Node (Runtime state)</summary>
        public static bool SetNodeActive(int nodeId, bool active)
        {
            EnsureInitialized();
            var node = GetNode(nodeId);
            if (node == null) return false;

            node.isActive = active;
            return true;
        }

        /// <summary>รีเซ็ต isActive ทุก Node กลับเป็น false</summary>
        public static void ResetAllNodes()
        {
            EnsureInitialized();
            foreach (var node in _data.nodes)
                node.isActive = false;
        }

        /// <summary>Apply save data กลับเข้าสู่ state — เรียกหลัง Load Game</summary>
        public static void ApplySaveData(List<int> activeNodeIds)
        {
            EnsureInitialized();
            ResetAllNodes();
            foreach (var id in activeNodeIds)
                SetNodeActive(id, true);
        }

        /// <summary>ดึงรายการ active node ids (int) สำหรับ save</summary>
        public static List<int> GetSaveData()
        {
            EnsureInitialized();
            return _data.nodes.Where(n => n.isActive).Select(n => n.id).ToList();
        }

        #endregion

        #region Private Methods

        private static void EnsureInitialized()
        {
            if (!_initialized) Initialize();
        }

        private static void BuildIndex()
        {
            _nodeMap = new Dictionary<int, SkillTreeNode>();
            foreach (var node in _data.nodes)
                _nodeMap[node.id] = node;

            _adjacencyFrom = new Dictionary<int, List<SkillTreeEdge>>();
            foreach (var edge in _data.edges)
            {
                if (!_adjacencyFrom.ContainsKey(edge.from))
                    _adjacencyFrom[edge.from] = new List<SkillTreeEdge>();
                _adjacencyFrom[edge.from].Add(edge);
            }
        }

        private static void AccumulateStats(SkillTreeStats stats, List<AttributeBonus> extras)
        {
            foreach (var bonus in extras)
            {
                stats.RawStats.TryGetValue(bonus.id, out float current);
                stats.RawStats[bonus.id] = current + bonus.value;

                AttributeType type = (AttributeType)bonus.id;
                switch (type)
                {
                    case AttributeType.STR: stats.STR += bonus.value; break;
                    case AttributeType.AGI: stats.AGI += bonus.value; break;
                    case AttributeType.INT: stats.INT += bonus.value; break;
                    case AttributeType.VIT: stats.VIT += bonus.value; break;
                    case AttributeType.DEX: stats.DEX += bonus.value; break;
                    case AttributeType.LUK: stats.LUK += bonus.value; break;
                }
            }
        }

        #endregion
    }
}
