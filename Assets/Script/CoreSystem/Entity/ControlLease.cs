namespace CoreSystem.Entity
{
    public sealed class ControlLease
    {
        #region [Properties]
        public object Holder { get; }
        public ControlLane Lane { get; }
        public float RemainingTime { get; internal set; }
        public bool IsTimed => RemainingTime >= 0f;
        public System.Func<bool> KeepWhile { get; }
        public System.Action OnEnded { get; }
        public bool IsActive { get; internal set; } = true;
        #endregion

        #region [Public Methods]
        public ControlLease(object holder, ControlLane lane, float duration, System.Func<bool> keepWhile, System.Action onEnded)
        {
            Holder = holder;
            Lane = lane;
            RemainingTime = duration;
            KeepWhile = keepWhile;
            OnEnded = onEnded;
        }
        #endregion
    }
}
