using System;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Combat
{
    public static class CombatEvents
    {
        #region [Variables/Fields]
        public static event Action<IEntity, IEntity> OnKilled;
        #endregion

        #region [Public Methods]
        public static void RaiseKilled(IEntity killer, IEntity victim)
        {
            if (killer == null || victim == null || ReferenceEquals(killer, victim)) return;
            OnKilled?.Invoke(killer, victim);
        }

        public static bool IsKillingBlow(float hpBefore, float hpAfter)
        {
            return hpBefore > 0f && hpAfter <= 0f;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnKilled = null;
        }
        #endregion
    }
}
