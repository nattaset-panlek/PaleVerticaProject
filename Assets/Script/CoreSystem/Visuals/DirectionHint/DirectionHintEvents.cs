using System;
using CoreSystem.Entity;
using CoreSystem.Player;
using UnityEngine;

namespace CoreSystem.Visuals
{
    public static class DirectionHintEvents
    {
        #region [Variables/Fields]
        public static event Action OnChanged;

        private static Character _boundPlayer;
        #endregion

        #region [Properties]
        public static bool PlayerUsesFacing { get; private set; }
        public static Character Player => _boundPlayer;
        #endregion

        #region [Public Methods]
        public static void Refresh()
        {
            bool uses = _boundPlayer != null && _boundPlayer.UniquePassives != null && _boundPlayer.UniquePassives.UsesFacing;
            PlayerUsesFacing = uses;
            OnChanged?.Invoke();
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
            PlayerCore.OnPlayerChanged += HandlePlayerChanged;
            HandlePlayerChanged(PlayerCore.GetPlayer());
        }

        private static void HandlePlayerChanged(Character player)
        {
            if (_boundPlayer != null && _boundPlayer.UniquePassives != null) _boundPlayer.UniquePassives.OnChanged -= Refresh;
            _boundPlayer = player;
            if (_boundPlayer != null && _boundPlayer.UniquePassives != null) _boundPlayer.UniquePassives.OnChanged += Refresh;
            Refresh();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnChanged = null;
            _boundPlayer = null;
            PlayerUsesFacing = false;
        }
        #endregion
    }
}
