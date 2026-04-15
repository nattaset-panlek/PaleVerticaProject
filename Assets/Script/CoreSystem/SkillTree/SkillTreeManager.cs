using System.Collections.Generic;
using UnityEngine;
using CoreSystem.SkillTree.Models;

namespace CoreSystem.SkillTree
{
    /// <summary>
    /// MonoBehaviour Singleton สำหรับระบบ SkillTree
    /// วาง Component นี้ลงบน Database prefab หรือ Scene GameObject
    /// </summary>
    public class SkillTreeManager : MonoBehaviour
    {
        #region Variables/Fields

        private static SkillTreeManager _instance;

        [Header("Skill Points")]
        [SerializeField] private int _skillPoints = 0;

        #endregion

        #region Properties

        public static SkillTreeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<SkillTreeManager>();
                return _instance;
            }
        }

        public int SkillPoints
        {
            get => _skillPoints;
            set => _skillPoints = Mathf.Max(0, value);
        }

        #endregion

        #region Events

        /// <summary>เรียกเมื่อ Node ถูก unlock สำเร็จ — ส่ง node id (int)</summary>
        public static System.Action<int> OnNodeUnlocked;

        /// <summary>เรียกเมื่อ Node ถูกรีเซ็ต</summary>
        public static System.Action<int> OnNodeReset;

        /// <summary>เรียกเมื่อรีเซ็ตทุก Node</summary>
        public static System.Action OnAllReset;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SkillTreeDatabase.Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public Methods — Node

        /// <summary>ดึงข้อมูล Node จาก int id</summary>
        public SkillTreeNode GetNode(int nodeId)
            => SkillTreeDatabase.GetNode(nodeId);

        /// <summary>ดึง Node ทั้งหมด</summary>
        public List<SkillTreeNode> GetAllNodes()
            => SkillTreeDatabase.GetAllNodes();

        /// <summary>ดึงเฉพาะจุดอาชีพ (role = "base")</summary>
        public List<SkillTreeNode> GetBaseClassNodes()
            => SkillTreeDatabase.GetBaseClassNodes();

        /// <summary>ดึงโหนดที่ผู้เล่น unlock ไว้แล้ว</summary>
        public List<SkillTreeNode> GetActiveNodes()
            => SkillTreeDatabase.GetActiveNodes();

        /// <summary>ค้นหา Node จากชื่อ (ไม่ case-sensitive)</summary>
        public List<SkillTreeNode> FindNodes(string label)
            => SkillTreeDatabase.FindNodesByLabel(label);

        #endregion

        #region Public Methods — Connection

        /// <summary>ดึง id ของโหนดที่เชื่อมออกไป</summary>
        public List<int> GetNeighbors(int nodeId)
            => SkillTreeDatabase.GetNeighborIds(nodeId);

        /// <summary>ตรวจว่าโหนดนี้ unlock ได้ไหม</summary>
        public bool CanUnlock(int nodeId)
            => SkillTreeDatabase.CanUnlock(nodeId);

        #endregion

        #region Public Methods — Unlock / Reset

        /// <summary>
        /// พยายาม unlock Node — ตรวจ prerequisite + Skill Point อัตโนมัติ
        /// คืน true ถ้าสำเร็จ
        /// </summary>
        public bool TryUnlockNode(int nodeId)
        {
            if (!CanUnlock(nodeId))
            {
                Debug.Log($"[SkillTree] ไม่สามารถ unlock node {nodeId}: prerequisite ไม่ครบ");
                return false;
            }

            if (_skillPoints <= 0)
            {
                Debug.Log("[SkillTree] Skill Point ไม่เพียงพอ");
                return false;
            }

            SkillTreeDatabase.SetNodeActive(nodeId, true);
            _skillPoints--;
            OnNodeUnlocked?.Invoke(nodeId);

            // Notify Player Character to refresh attributes
            FindObjectOfType<CoreSystem.Entity.PlayerCharacter>()?.RecalculateStats();

            return true;
        }

        /// <summary>รีเซ็ต Node เดียว — คืน 1 SP</summary>
        public bool TryResetNode(int nodeId)
        {
            var node = GetNode(nodeId);
            if (node == null || !node.isActive) return false;

            SkillTreeDatabase.SetNodeActive(nodeId, false);
            _skillPoints++;
            OnNodeReset?.Invoke(nodeId);

            // Notify Player Character to refresh attributes
            FindObjectOfType<CoreSystem.Entity.PlayerCharacter>()?.RecalculateStats();

            return true;
        }

        /// <summary>รีเซ็ตทุก Node — คืน SP ทั้งหมด</summary>
        public void ResetAll()
        {
            int refund = SkillTreeDatabase.GetActiveNodes().Count;
            SkillTreeDatabase.ResetAllNodes();
            _skillPoints += refund;
            OnAllReset?.Invoke();

            // Notify Player Character to refresh attributes
            FindObjectOfType<CoreSystem.Entity.PlayerCharacter>()?.RecalculateStats();
        }

        #endregion

        #region Public Methods — Stats

        /// <summary>
        /// คืน SkillTreeStats จากโหนดที่ active ทั้งหมด
        /// นำไป apply บน PlayerData / CharacterStats ได้เลย
        /// </summary>
        public SkillTreeStats GetCurrentStats() => SkillTreeDatabase.GetActiveStats();

        #endregion

        #region Public Methods — Skill Points

        public void AddSkillPoints(int amount)
            => _skillPoints += Mathf.Max(0, amount);

        #endregion

        #region Public Methods — Save / Load

        /// <summary>ดึง active node ids (int) สำหรับ save</summary>
        public List<int> GetSaveData()
            => SkillTreeDatabase.GetSaveData();

        /// <summary>โหลด save data กลับมา — เรียกหลัง Load Game</summary>
        public void LoadSaveData(List<int> activeNodeIds, int skillPoints)
        {
            _skillPoints = skillPoints;
            SkillTreeDatabase.ApplySaveData(activeNodeIds);
        }

        #endregion
    }
}
