using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Controllers;

namespace CoreSystem.Agent
{
    /// <summary>
    /// Lightweight static registry that tracks AgentCore instances by GroupID.
    /// Allows agents to find team members and issue group alerts.
    /// GroupID = 0 means Solo (not in any group).
    /// </summary>
    public static class AgentRegistry
    {
        private static readonly Dictionary<int, List<AgentCore>> _groups = new Dictionary<int, List<AgentCore>>();

        public static void Register(AgentCore agent)
        {
            if (agent.GroupID == 0) return; // Solo agents skip group registration
            
            if (!_groups.ContainsKey(agent.GroupID))
                _groups[agent.GroupID] = new List<AgentCore>();
            
            if (!_groups[agent.GroupID].Contains(agent))
                _groups[agent.GroupID].Add(agent);
        }

        public static void Unregister(AgentCore agent)
        {
            if (agent.GroupID == 0) return;
            if (_groups.TryGetValue(agent.GroupID, out var list))
            {
                list.Remove(agent);
                if (list.Count == 0) _groups.Remove(agent.GroupID);
            }
        }

        /// <summary>
        /// Alert all agents in the same group (within alertRadius) of a target.
        /// </summary>
        public static void AlertGroup(AgentCore caller, Transform target, float alertRadius)
        {
            if (caller.GroupID == 0 || target == null) return;
            if (!_groups.TryGetValue(caller.GroupID, out var members)) return;

            foreach (var member in members)
            {
                if (member == caller || member == null) continue;
                float dist = Vector3.Distance(caller.transform.position, member.transform.position);
                if (dist <= alertRadius)
                {
                    member.ReceiveGroupAlert(target);
                }
            }
        }

        /// <summary>
        /// Get all alive group members near a point within a radius.
        /// </summary>
        public static List<AgentCore> GetNearbyGroupMembers(int groupId, Vector3 point, float radius)
        {
            var result = new List<AgentCore>();
            if (!_groups.TryGetValue(groupId, out var members)) return result;

            foreach (var member in members)
            {
                if (member == null) continue;
                if (Vector3.Distance(point, member.transform.position) <= radius)
                    result.Add(member);
            }
            return result;
        }
    }
}
