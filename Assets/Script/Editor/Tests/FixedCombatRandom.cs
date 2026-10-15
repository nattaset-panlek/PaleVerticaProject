using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Tests
{
    public sealed class FixedCombatRandom : ICombatRandom
    {
        #region [Variables/Fields]
        private readonly float _value;
        #endregion

        #region [Public Methods]
        public FixedCombatRandom(float value)
        {
            _value = value;
        }

        public float Range(float min, float max)
        {
            return _value;
        }
        #endregion
    }
}
