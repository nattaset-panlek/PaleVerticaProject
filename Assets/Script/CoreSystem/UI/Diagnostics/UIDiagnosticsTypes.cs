namespace CoreSystem.UI.Diagnostics
{
    public enum UILayoutIssueKind
    {
        TextOverflow,
        LabelSquashed,
        LabelTruncated,
        BelowMinSize,
        OutsideParent,
        SiblingOverlap,
        ZeroSize,
        NestedFitter,
        OversizedControl
    }

    public enum UIIssueSeverity
    {
        Warning,
        Error
    }
}
