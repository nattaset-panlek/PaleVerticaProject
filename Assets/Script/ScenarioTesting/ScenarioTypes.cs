#if UNITY_EDITOR || PV_SCENARIO_TESTS
namespace ScenarioTesting
{
    public enum ScenarioStatus
    {
        NotRun,
        Running,
        Passed,
        NeedsReview,
        Failed
    }

    public enum ScenarioCheckStatus
    {
        Passed,
        Review,
        Failed
    }
}
#endif
