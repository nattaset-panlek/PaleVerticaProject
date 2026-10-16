using UnityEngine;

namespace CoreSystem.UI.Diagnostics
{
    public sealed class UILayoutIssue
    {
        #region [Variables/Fields]
        private readonly Vector3[] _corners = new Vector3[4];
        #endregion

        #region [Properties]
        public int Number { get; set; }
        public UILayoutIssueKind Kind { get; }
        public UIIssueSeverity Severity { get; }
        public string Path { get; }
        public string Message { get; }
        public string Hint { get; }
        public RectTransform Target { get; }
        public Vector3[] WorldCorners => _corners;
        #endregion

        #region [Public Methods]
        public UILayoutIssue(UILayoutIssueKind kind, UIIssueSeverity severity, RectTransform target, string path, string message, string hint)
        {
            Kind = kind;
            Severity = severity;
            Target = target;
            Path = path;
            Message = message;
            Hint = hint;
            if (target != null) target.GetWorldCorners(_corners);
        }

        public override string ToString()
        {
            return $"#{Number} [{Severity}] {Kind}: {Path}\n    {Message}\n    Fix: {Hint}";
        }
        #endregion
    }
}
