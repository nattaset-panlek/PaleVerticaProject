using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.Summon
{
    public static class SummonRegistry
    {
        #region [Variables/Fields]
        private static readonly List<SummonedUnit> _units = new List<SummonedUnit>();
        #endregion

        #region [Properties]
        public static IReadOnlyList<SummonedUnit> Units => _units;
        #endregion

        #region [Public Methods]
        public static void Register(SummonedUnit unit)
        {
            if (unit != null && !_units.Contains(unit)) _units.Add(unit);
        }

        public static void Unregister(SummonedUnit unit)
        {
            _units.Remove(unit);
        }

        public static int Count(Character owner, int skillId)
        {
            int count = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                if (Matches(_units[i], owner, skillId)) count++;
            }
            return count;
        }

        public static SummonedUnit Oldest(Character owner, int skillId)
        {
            SummonedUnit oldest = null;
            for (int i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                if (!Matches(unit, owner, skillId)) continue;
                if (oldest == null || unit.SpawnTime < oldest.SpawnTime) oldest = unit;
            }
            return oldest;
        }

        public static Character OwnerOf(IEntity entity)
        {
            if (!(entity is Character character) || character == null) return null;
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null && _units[i].Body == character) return _units[i].Owner;
            }
            return null;
        }

        public static int ExcessCount(int current, int max)
        {
            if (max <= 0) return 0;
            return Mathf.Max(0, current - max + 1);
        }
        #endregion

        #region [Private Methods]
        private static bool Matches(SummonedUnit unit, Character owner, int skillId)
        {
            return unit != null && !unit.IsExpiring && unit.Owner == owner && unit.SkillId == skillId;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _units.Clear();
        }
        #endregion
    }
}
