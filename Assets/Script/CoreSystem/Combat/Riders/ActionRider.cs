namespace CoreSystem.Combat
{
    [System.Serializable]
    public abstract class ActionRider
    {
        #region [Variables/Fields]
        public RiderTrigger Trigger = RiderTrigger.OnHit;
        public float ChancePercent = 100f;
        #endregion

        #region [Public Methods]
        public bool IsTriggeredBy(ActionResult result)
        {
            switch (Trigger)
            {
                case RiderTrigger.OnHit: return result.HasHit();
                case RiderTrigger.OnCrit: return result.HasCritical();
                case RiderTrigger.OnKill: return result.PredictsKill();
                default: return false;
            }
        }

        public bool TryExecute(ResolveContext context)
        {
            if (ChancePercent < 100f && context.Random.Range(0f, 100f) >= ChancePercent) return false;
            Execute(context);
            return true;
        }
        #endregion

        #region [Private Methods]
        protected abstract void Execute(ResolveContext context);
        #endregion
    }
}
