using System.Collections.Generic;
using CoreSystem.Data;
using UnityEngine;

namespace CoreSystem.Entity
{
    public static class TeamRegistry
    {
        #region [Variables/Fields]
        private static readonly Dictionary<IEntity, IEntity> _leaders = new Dictionary<IEntity, IEntity>();
        #endregion

        #region [Public Methods]
        public static void Bind(IEntity member, IEntity leader)
        {
            if (member == null || leader == null || ReferenceEquals(member, leader)) return;
            _leaders[member] = leader;
        }

        public static void Unbind(IEntity member)
        {
            if (member != null) _leaders.Remove(member);
        }

        public static IEntity LeaderOf(IEntity member)
        {
            if (member == null) return null;
            return _leaders.TryGetValue(member, out IEntity leader) ? leader : null;
        }

        public static IEntity RootOf(IEntity entity)
        {
            IEntity current = entity;
            for (int i = 0; i < 8; i++)
            {
                IEntity leader = LeaderOf(current);
                if (leader == null) return current;
                current = leader;
            }
            return current;
        }

        public static bool AreTeammates(IEntity a, IEntity b)
        {
            if (a == null || b == null) return false;
            if (ReferenceEquals(a, b)) return true;
            return ReferenceEquals(RootOf(a), RootOf(b));
        }

        public static bool IsHostile(IEntity source, IEntity target)
        {
            if (source == null || target == null) return false;
            if (AreTeammates(source, target)) return false;
            return FactionDatabase.IsHostile(source.Faction, target.Faction);
        }

        public static bool IsAlly(IEntity source, IEntity target)
        {
            if (source == null || target == null) return false;
            if (AreTeammates(source, target)) return true;
            return FactionDatabase.IsAlly(source.Faction, target.Faction);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _leaders.Clear();
        }
        #endregion
    }
}
