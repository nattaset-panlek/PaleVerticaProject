using UnityEngine;
using CoreSystem.Entity;
using System.Linq;
using CoreSystem.Data;

namespace CoreSystem.UI
{
    public class NPCDebugUI : MonoBehaviour
    {
        #region [Variables/Fields]
        private string _targetNpcId = "NPC_01";
        private bool _showWindow = true;
        private Rect _windowRect = new Rect(20, 20, 300, 180);
        #endregion

        #region [Unity Methods]
        private void OnGUI()
        {
            if (!_showWindow) return;

            _windowRect = GUILayout.Window(999, _windowRect, DrawWindow, "NPC Debug Menu");
        }
        #endregion

        #region [Private Methods]
        private void DrawWindow(int windowID)
        {
            GUILayout.Label("Target NPC ID:");
            _targetNpcId = GUILayout.TextField(_targetNpcId);

            GUILayout.Space(10);

            if (GUILayout.Button("Find Player & Command Follow"))
            {
                CommandFollowPlayer();
            }

            if (GUILayout.Button("Find Player & Command Move To"))
            {
                CommandMoveToPlayer();
            }

            if (GUILayout.Button("Clear Commands"))
            {
                ClearNPCCommands();
            }

            // Draggable Window handle
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void CommandFollowPlayer()
        {
            var npc = FindTargetNPC();
            var player = GameData.Player.PlayerCharacterModel;

            if (npc != null && player != null)
            {
                npc.CommandFollow(player.transform);
                Debug.Log($"[NPCDebugUI] Commanded {npc.NpcId} to follow Player.");
            }
            else
            {
                if (npc == null) Debug.LogWarning($"[NPCDebugUI] Could not find active NPC with ID: {_targetNpcId}");
                if (player == null) Debug.LogWarning("[NPCDebugUI] Could not find active PlayerCharacter in the scene.");
            }
        }

        private void CommandMoveToPlayer()
        {
            var npc = FindTargetNPC();
            var player = GameData.Player.PlayerCharacterModel;

            if (npc != null && player != null)
            {
                npc.CommandMoveTo(player.transform.position);
                Debug.Log($"[NPCDebugUI] Commanded {npc.NpcId} to move to Player pos {player.transform.position}.");
            }
            else
            {
                if (npc == null) Debug.LogWarning($"[NPCDebugUI] Could not find active NPC with ID: {_targetNpcId}");
                if (player == null) Debug.LogWarning("[NPCDebugUI] Could not find active PlayerCharacter in the scene.");
            }
        }

        private void ClearNPCCommands()
        {
            var npc = FindTargetNPC();
            if (npc != null)
            {
                npc.ClearCommands();
                Debug.Log($"[NPCDebugUI] Cleared commands for {npc.NpcId}.");
            }
        }

        private NonPlayerCharacter FindTargetNPC()
        {
            var allNPCs = FindObjectsOfType<NonPlayerCharacter>();
            var target = allNPCs.FirstOrDefault(n => n.NpcId == _targetNpcId);
            return target;
        }
        #endregion
    }
}
