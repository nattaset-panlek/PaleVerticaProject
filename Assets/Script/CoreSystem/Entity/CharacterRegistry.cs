using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Entity
{
    public static class CharacterRegistry
    {
        #region [Variables/Fields]
        private static readonly Dictionary<int, Character> _characters = new Dictionary<int, Character>();
        private static int _nextId = 1;
        #endregion

        #region [Properties]
        public static IReadOnlyCollection<Character> All => _characters.Values;
        public static int Count => _characters.Count;
        public static event System.Action<Character> OnRegistered;
        public static event System.Action<Character> OnUnregistered;
        #endregion

        #region [Public Methods]
        public static int NewId()
        {
            return _nextId++;
        }

        public static void Register(Character character)
        {
            if (character == null || character.RuntimeId <= 0) return;
            if (_characters.TryGetValue(character.RuntimeId, out var existing) && existing == character) return;
            _characters[character.RuntimeId] = character;
            OnRegistered?.Invoke(character);
        }

        public static void Unregister(Character character)
        {
            if (character == null) return;
            if (_characters.TryGetValue(character.RuntimeId, out var existing) && existing == character)
            {
                _characters.Remove(character.RuntimeId);
                OnUnregistered?.Invoke(character);
            }
        }

        public static Character Get(int runtimeId)
        {
            return _characters.TryGetValue(runtimeId, out var character) ? character : null;
        }

        public static bool TryGet(int runtimeId, out Character character)
        {
            return _characters.TryGetValue(runtimeId, out character);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _characters.Clear();
            _nextId = 1;
            OnRegistered = null;
            OnUnregistered = null;
        }
        #endregion
    }
}
