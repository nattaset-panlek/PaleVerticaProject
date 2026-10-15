using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Agent
{
    public static class AgentRegistry
    {
        #region [Variables/Fields]
        private static readonly Dictionary<int, List<AgentHandle>> _groups = new Dictionary<int, List<AgentHandle>>();
        #endregion

        #region [Public Methods]
        public static void Register(AgentHandle agent)
        {
            if (agent == null || agent.GroupID == 0) return;

            if (!_groups.TryGetValue(agent.GroupID, out var list))
            {
                list = new List<AgentHandle>();
                _groups[agent.GroupID] = list;
            }

            if (!list.Contains(agent)) list.Add(agent);
        }

        public static void Unregister(AgentHandle agent)
        {
            if (agent == null || agent.GroupID == 0) return;
            if (_groups.TryGetValue(agent.GroupID, out var list))
            {
                list.Remove(agent);
                if (list.Count == 0) _groups.Remove(agent.GroupID);
            }
        }

        public static void AlertGroup(AgentHandle caller, Transform target, float alertRadius)
        {
            if (caller == null || caller.GroupID == 0 || target == null || caller.Character == null) return;
            if (!_groups.TryGetValue(caller.GroupID, out var members)) return;

            Vector3 origin = caller.Character.transform.position;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member == caller || member == null || member.Character == null) continue;
                if (Vector3.Distance(origin, member.Character.transform.position) <= alertRadius) member.ReceiveGroupAlert(target);
            }
        }

        public static List<AgentHandle> GetNearbyGroupMembers(int groupId, Vector3 point, float radius)
        {
            var result = new List<AgentHandle>();
            if (!_groups.TryGetValue(groupId, out var members)) return result;

            foreach (var member in members)
            {
                if (member == null || member.Character == null) continue;
                if (Vector3.Distance(point, member.Character.transform.position) <= radius) result.Add(member);
            }
            return result;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _groups.Clear();
        }
        #endregion
    }
}
