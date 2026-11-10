#if UNITY_EDITOR || PV_SCENARIO_TESTS
using UnityEngine;

namespace ScenarioTesting
{
    public sealed class ScenarioOverlayView : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float Width = 460f;
        private const float Margin = 12f;
        private static readonly Color PassColor = new Color(0.45f, 0.85f, 0.45f);
        private static readonly Color ReviewColor = new Color(1f, 0.78f, 0.3f);
        private static readonly Color FailColor = new Color(1f, 0.42f, 0.42f);

        private string _title = string.Empty;
        private string _step = string.Empty;
        private string _note = string.Empty;
        private string _counts = string.Empty;
        private string _lastIssue = string.Empty;
        private string _progress = string.Empty;
        private float _fraction;
        private Color _statusColor = Color.white;
        private bool _visible;
        private GUIStyle _style;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            ScenarioManager.OnRunStarted += Refresh;
            ScenarioManager.OnRunChanged += Refresh;
            ScenarioManager.OnRunFinished += Refresh;
        }

        private void OnDisable()
        {
            ScenarioManager.OnRunStarted -= Refresh;
            ScenarioManager.OnRunChanged -= Refresh;
            ScenarioManager.OnRunFinished -= Refresh;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (_style == null) _style = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 13, richText = true };

            float height = 140f + (string.IsNullOrEmpty(_lastIssue) ? 0f : 34f);
            var area = new Rect(Margin, Screen.height - height - Margin, Width, height);
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(area.x + 10f, area.y + 6f, area.width - 20f, area.height - 12f));
            GUI.color = _statusColor;
            GUILayout.Label(_title, _style);
            GUI.color = Color.white;
            GUILayout.Label(_progress, _style);
            DrawBar(GUILayoutUtility.GetRect(area.width - 20f, 6f));
            GUILayout.Label(_step, _style);
            if (!string.IsNullOrEmpty(_note)) GUILayout.Label($"<i>{_note}</i>", _style);
            GUILayout.Label(_counts, _style);
            if (!string.IsNullOrEmpty(_lastIssue))
            {
                GUI.color = FailColor;
                GUILayout.Label(_lastIssue, _style);
                GUI.color = Color.white;
            }
            GUILayout.EndArea();
        }
        #endregion

        #region [Private Methods]
        private void DrawBar(Rect rect)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = _statusColor;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * _fraction, rect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void Refresh(ScenarioRun run)
        {
            _visible = true;
            ScenarioReport report = run.Report;
            ScenarioStep step = run.CurrentStep;
            ScenarioStatus status = report.Status == ScenarioStatus.Running ? report.Evaluate() : report.Status;
            string state = report.Status == ScenarioStatus.Running ? "running" : report.Status.ToString();

            _statusColor = status == ScenarioStatus.Failed ? FailColor : status == ScenarioStatus.NeedsReview ? ReviewColor : PassColor;
            _title = $"<b>Scenario {report.Scenario}</b>  ({state}, x{ScenarioManager.Speed:0.##})";
            _step = step != null ? $"Step {step.Index + 1}/{run.Definition.Steps.Count}: {step.Describe()}" : "Starting";
            _note = step != null ? step.Note : run.Definition.Description;
            _counts = $"Passed {report.ChecksPassed}   Review {report.ChecksReview}   Failed {report.ChecksFailed}";
            _fraction = ScenarioProgressFormula.RunFraction(run);
            _progress = ScenarioProgressFormula.Describe(run);

            _lastIssue = string.Empty;
            for (int i = report.Checks.Count - 1; i >= 0; i--)
            {
                if (report.Checks[i].Passed) continue;
                _lastIssue = report.Checks[i].ToString();
                break;
            }
            for (int i = report.StepResults.Count - 1; i >= 0 && string.IsNullOrEmpty(_lastIssue); i--)
            {
                if (report.StepResults[i].Status == ScenarioStatus.Failed) _lastIssue = $"Step {i + 1} failed: {report.StepResults[i].Message}";
            }
        }
        #endregion
    }
}
#endif
