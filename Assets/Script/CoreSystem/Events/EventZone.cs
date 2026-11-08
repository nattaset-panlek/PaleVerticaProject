using System;
using CoreSystem.Entity;
using CoreSystem.Player;
using UnityEngine;

namespace CoreSystem.Events
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class EventZone : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private string _zoneId;

        public static event Action<string> OnPlayerEntered;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Character player = PlayerCore.GetPlayer();
            if (player == null || other.GetComponentInParent<Character>() != player) return;
            OnPlayerEntered?.Invoke(_zoneId);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnPlayerEntered = null;
        }
        #endregion
    }
}
