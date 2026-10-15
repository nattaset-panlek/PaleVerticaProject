namespace CoreSystem.Combat
{
    public static class OutcomeApplier
    {
        #region [Public Methods]
        public static void Apply(ActionResult result)
        {
            if (result == null || result.IsBlocked) return;

            var outcomes = result.Outcomes;
            for (int i = 0; i < outcomes.Count; i++)
            {
                var outcome = outcomes[i];
                if (outcome.IsBlocked) continue;
                EffectHandlers.For(outcome.Kind)?.Apply(outcome, result);
            }

            var subResults = result.SubResults;
            for (int i = 0; i < subResults.Count; i++)
            {
                Apply(subResults[i]);
            }
        }
        #endregion
    }
}
