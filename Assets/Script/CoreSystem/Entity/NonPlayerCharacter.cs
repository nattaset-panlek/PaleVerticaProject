using UnityEngine;
using CoreSystem.Controllers;
using CoreSystem.Data.Items.Model;
using CoreSystem.Agent;

namespace CoreSystem.Entity
{
    [RequireComponent(typeof(AgentCore))]
    public class NonPlayerCharacter : Character
    {
        #region [Variables/Fields]
        [Header("NPC Settings")]
        [SerializeField] private string npcId;
        [SerializeField] private bool interactable = true;
        
        [Header("V2 Conceptual (Autonomous)")]
        [SerializeField] private NPCInventory _npcInventory;
        // [SerializeField] private CoreSystem.Agent.NPCBrain _npcBrain;

        private AgentCore _agentCore;
        #endregion

        #region [Properties]
        public string NpcId => npcId;
        public bool IsInteractable => interactable;
        public AgentCore Agent => _agentCore;
        public NPCInventory Inventory => _npcInventory;
        // public CoreSystem.Agent.NPCBrain Brain => _npcBrain;
        #endregion

        #region [Unity Methods]
        public override void Awake()
        {
            base.Awake();
            _agentCore = GetComponent<AgentCore>();

            // Attempt to grab components if they exist
            if (_npcInventory == null) _npcInventory = GetComponent<NPCInventory>();
            // if (_npcBrain == null) _npcBrain = GetComponent<CoreSystem.Agent.NPCBrain>();
        }
        #endregion

        #region [Public Methods]
        public void InitializeNPC(string newNpcId, int newFaction = 0)
        {
            npcId = newNpcId;
            faction = newFaction;
        }

        public void CommandMoveTo(Vector3 position)
        {
            if (_agentCore != null)
                _agentCore.SetCommandMove(position);
        }

        public void CommandFollow(Transform targetTransform)
        {
            if (_agentCore != null)
                _agentCore.SetCommandFollow(targetTransform);
        }

        public void ClearCommands()
        {
            if (_agentCore != null)
                _agentCore.ClearCommand();
        }

        public void SetInteractable(bool state)
        {
            interactable = state;
        }

        // --- Director Management API (V2) ---
        public void GiveItemToNPC(int itemId, int amount = 1)
        {
            if (_npcInventory != null)
            {
                // Item template = CoreSystem.Data.Database.GetItem(itemId);
                // _npcInventory.AddItem(template, amount);
            }
        }

        public void ForceEquipItem(EquipmentSlot slot, Equipment newEquip)
        {
            if (newEquip != null)
            {
                // Create instance directly bypassing inventory mapping for forced commands
                // var instance = new CoreSystem.Data.Items.EquipmentInstance(System.DateTime.Now.Ticks, newEquip.Id, 1);
                // Equip(instance, (int)slot);
            }
        }

        public void AssignHighLevelGoal(string newGoalStr, Transform targetOrArea = null)
        {
            // Note: Uncomment when NPCBrain goal system is fully integrated
            /*
            if (_npcBrain != null && System.Enum.TryParse(newGoalStr, out CoreSystem.Agent.NPCGoal parsedGoal))
            {
                _npcBrain.AssignGoal(parsedGoal, targetOrArea);
            }
            */
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
