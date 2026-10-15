using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Agent
{
    public static class AgentGizmos
    {
        #region [Public Methods]
        public static void Draw(Character character, AgentHandle handle)
        {
#if UNITY_EDITOR
            if (character == null) return;
            var profile = character.AgentProfile;
            Vector3 position = character.transform.position;
            Vector3 home = handle != null ? handle.HomePosition : position;
            float attackRange = handle != null ? handle.AttackRange : character.AutoAttackRange;

            if (profile.UseHomeZone)
            {
                UnityEditor.Handles.color = new Color(0, 1, 0, 0.12f);
                UnityEditor.Handles.DrawSolidDisc(home, Vector3.forward, profile.HomeZoneRadius);
                Gizmos.color = new Color(0, 1, 0, 0.7f);
                Gizmos.DrawWireSphere(home, profile.HomeZoneRadius);
            }

            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawWireSphere(position, profile.DetectionRadius);

            Gizmos.color = new Color(1, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(position, attackRange);

            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
            Gizmos.DrawWireSphere(position, profile.LoseSightRadius);

            if (handle == null) return;

            if (handle.CurrentState == AIState.Roam)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(position, handle.RoamDestination);
                Gizmos.DrawWireSphere(handle.RoamDestination, 0.4f);
            }

            var path = handle.CurrentPath;
            if (path != null && path.Count > 0)
            {
                Gizmos.color = new Color(0f, 0.9f, 1f, 0.8f);
                for (int i = handle.PathIndex; i < path.Count - 1; i++) Gizmos.DrawLine(path[i], path[i + 1]);
                for (int i = handle.PathIndex; i < path.Count; i++) Gizmos.DrawWireSphere(path[i], 0.15f);
            }

            if (handle.Target != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(position, handle.Target.position);
            }
#endif
        }
        #endregion
    }
}
