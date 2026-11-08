using System.Collections.Generic;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Combat
{
    public static class KillCreditRegistry
    {
        #region [Variables/Fields]
        private static readonly Dictionary<IEntity, Dictionary<IEntity, float>> _damageByVictim = new Dictionary<IEntity, Dictionary<IEntity, float>>();
        #endregion

        #region [Public Methods]
        public static void Record(IEntity attacker, IEntity victim, float hpRemoved)
        {
            if (attacker == null || victim == null || hpRemoved <= 0f) return;
            IEntity credited = TeamRegistry.RootOf(attacker);
            if (credited == null || TeamRegistry.AreTeammates(credited, victim)) return;

            if (!_damageByVictim.TryGetValue(victim, out var byAttacker))
            {
                byAttacker = new Dictionary<IEntity, float>();
                _damageByVictim[victim] = byAttacker;
            }
            byAttacker.TryGetValue(credited, out float current);
            byAttacker[credited] = current + hpRemoved;
        }

        public static float Total(IEntity victim)
        {
            if (victim == null || !_damageByVictim.TryGetValue(victim, out var byAttacker)) return 0f;
            float total = 0f;
            foreach (var pair in byAttacker) total += pair.Value;
            return total;
        }

        public static float ShareOf(IEntity victim, IEntity attacker)
        {
            if (victim == null || attacker == null || !_damageByVictim.TryGetValue(victim, out var byAttacker)) return 0f;
            byAttacker.TryGetValue(attacker, out float part);
            return KillRewardFormula.Share(part, Total(victim));
        }

        public static IEntity TopAttacker(IEntity victim)
        {
            if (victim == null || !_damageByVictim.TryGetValue(victim, out var byAttacker)) return null;
            IEntity top = null;
            float best = 0f;
            foreach (var pair in byAttacker)
            {
                if (pair.Value <= best) continue;
                best = pair.Value;
                top = pair.Key;
            }
            return top;
        }

        public static void Forget(IEntity entity)
        {
            if (entity != null) _damageByVictim.Remove(entity);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _damageByVictim.Clear();
        }
        #endregion
    }
}
