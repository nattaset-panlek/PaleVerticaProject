namespace CoreSystem.Interaction
{
    public enum InteractionStep
    {
        Approach,
        Execute,
        Cancel
    }

    public enum InteractionCancelReason
    {
        Replaced,
        Cancelled,
        TargetInvalid,
        ControlLost,
        TimedOut
    }
}
