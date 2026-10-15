using UnityEngine;
using CoreSystem.Controllers;
using CoreSystem.Entity;

namespace CoreSystem.Agent
{
    [RequireComponent(typeof(AgentCore))]
    [RequireComponent(typeof(NonPlayerCharacter))]
    public class NPCBrain : MonoBehaviour
    {
        #region [Variables/Fields]
        [Header("Brain Settings")]
        [SerializeField] private NPCGoal currentGoal = NPCGoal.Idle;
        [SerializeField] private Transform currentGoalTarget;
        
        private AgentCore _agent;
        private NonPlayerCharacter _npc;
        
        private float _thinkTimer;
        private float _thinkInterval = 1.0f;
        #endregion

        #region [Properties]
        public NPCGoal CurrentGoal => currentGoal;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _agent = GetComponent<AgentCore>();
            _npc = GetComponent<NonPlayerCharacter>();
            
            // _inventory = GetComponent<NPCInventory>(); // (Optional hook if available)
        }

        private void Update()
        {
            _thinkTimer += Time.deltaTime;
            if (_thinkTimer >= _thinkInterval)
            {
                _thinkTimer = 0f;
                ProcessBrain();
            }
        }
        #endregion

        #region [Public Methods]
        public void AssignGoal(NPCGoal newGoal, Transform target = null)
        {
            currentGoal = newGoal;
            currentGoalTarget = target;
            
            // Clear previous manual agent commands before starting new goal
            if (_agent != null)
            {
                _agent.ClearCommand();
            }
        }
        #endregion

        #region [Private Methods]
        private void ProcessBrain()
        {
            // Note: Most of the complex operations are commented out
            // because they require full physics interaction logic or item databases to be linked to NPC.

            /*
            // 1. Survival Check (Highest Priority)
            if (_npc != null && _npc.attributeSystem != null)
            {
                var hp = _npc.attributeSystem.GetAttribute(Attributes.AttributeType.HP);
                var maxHp = _npc.attributeSystem.GetAttribute(Attributes.AttributeType.MaxHP);
                
                if (hp != null && maxHp != null && (hp.CurrentValue / maxHp.CurrentValue) < 0.3f)
                {
                    // Check if NPC has inventory and potion
                    var inv = GetComponent<NPCInventory>();
                    if (inv != null)
                    {
                        // inv.ConsumePotion();
                        // currentGoal = NPCGoal.RegainHealth;
                    }
                }
            }
            */

            // 2. State Machine for Goals
            switch (currentGoal)
            {
                case NPCGoal.GatherResources:
                    if (currentGoalTarget != null)
                    {
                        // Walk toward the resource node
                        // _agent.SetCommandMove(currentGoalTarget.position);

                        // If close enough, play gather animation and spawn/give item to Inventory
                        // if (Vector3.Distance(transform.position, currentGoalTarget.position) < 2f)
                        // {
                        //      // Trigger Gather action
                        // }
                    }
                    break;

                case NPCGoal.Defend:
                    if (currentGoalTarget != null)
                    {
                        // Return to designated defend spot if wandering too far
                        // if (Vector3.Distance(transform.position, currentGoalTarget.position) > 5f)
                        // {
                        //     _agent.SetCommandMove(currentGoalTarget.position);
                        // }
                    }
                    break;

                case NPCGoal.Idle:
                default:
                    // Let normal AgentCore Routine handle finding targets and roaming
                    break;
            }
        }
        #endregion
    }
}
