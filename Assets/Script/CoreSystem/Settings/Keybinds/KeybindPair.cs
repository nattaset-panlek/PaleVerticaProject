using System;
using UnityEngine.InputSystem;

namespace CoreSystem.Settings.Keybinds
{
    [Serializable]
    public struct KeybindPair : IEquatable<KeybindPair>
    {
        #region [Variables/Fields]
        public const int SlotCount = 2;

        public Key Primary;
        public Key Secondary;
        #endregion

        #region [Public Methods]
        public KeybindPair(Key primary, Key secondary)
        {
            Primary = primary;
            Secondary = secondary;
        }

        public Key Get(int slot) => slot == 0 ? Primary : Secondary;

        public KeybindPair With(int slot, Key key)
        {
            return slot == 0 ? new KeybindPair(key, Secondary) : new KeybindPair(Primary, key);
        }

        public bool Contains(Key key) => key != Key.None && (Primary == key || Secondary == key);

        public bool Equals(KeybindPair other) => Primary == other.Primary && Secondary == other.Secondary;

        public override bool Equals(object obj) => obj is KeybindPair other && Equals(other);

        public override int GetHashCode() => ((int)Primary * 397) ^ (int)Secondary;
        #endregion
    }
}
