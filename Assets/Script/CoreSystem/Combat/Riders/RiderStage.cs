namespace CoreSystem.Combat
{
    public static class RiderStage
    {
        #region [Public Methods]
        public static void Run(ResolveContext context)
        {
            var riders = context.Action.Riders;
            if (riders.Count == 0 || context.Result.IsBlocked) return;

            for (int i = 0; i < riders.Count; i++)
            {
                var rider = riders[i];
                if (rider != null && rider.IsTriggeredBy(context.Result))
                {
                    rider.TryExecute(context);
                }
            }
        }
        #endregion
    }
}
