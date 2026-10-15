namespace CoreSystem.Combat
{
    public sealed class UnityCombatRandom : ICombatRandom
    {
        #region [Variables/Fields]
        public static readonly UnityCombatRandom Instance = new UnityCombatRandom();
        #endregion

        #region [Public Methods]
        public float Range(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        #endregion
    }
}
