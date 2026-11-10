#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScenarioTesting
{
    public static class ScenarioButtonPress
    {
        #region [Variables/Fields]
        public const string FallbackMetric = "ui fallback clicks";
        private const int Attempts = 3;
        private const int StableFrames = 3;
        private const float ReadyTimeout = 2f;
        private const float ResultFrames = 3;
        private static readonly Vector2[] Offsets = { Vector2.zero, new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f), new Vector2(0f, 0.25f), new Vector2(0f, -0.25f) };
        #endregion

        #region [Public Methods]
        public static IEnumerator Press(ScenarioRun run, Button button)
        {
            if (button == null) yield break;
            string label = ScenarioInput.LabelOf(button);
            bool fired = false;
            void HandleClick() => fired = true;
            button.onClick.AddListener(HandleClick);

            string why = string.Empty;
            for (int attempt = 0; attempt < Attempts && !fired && button != null; attempt++)
            {
                yield return WaitUntilReady(button);
                if (button == null) break;

                Vector2 screen = PickPoint(button, out string topHit);
                yield return run.Input.Click(screen);
                for (int i = 0; i < ResultFrames && !fired; i++) yield return null;
                if (!fired) why = $"attempt {attempt + 1}: clicked {screen.x:0},{screen.y:0}, top UI there \"{topHit}\", interactable {button != null && button.IsInteractable()}";
                if (!fired) run.Log($"button \"{label}\" did not react ({why})");
            }

            if (!fired && button != null && button.isActiveAndEnabled)
            {
                run.Log($"button \"{label}\" pressed through the EventSystem after {Attempts} clicks did nothing ({why})");
                run.Report.Metrics.TryGetValue(FallbackMetric, out float count);
                run.Measure(FallbackMetric, count + 1f);
                ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }

            if (button != null) button.onClick.RemoveListener(HandleClick);
            yield return null;
        }
        #endregion

        #region [Private Methods]
        private static IEnumerator WaitUntilReady(Button button)
        {
            float deadline = Time.unscaledTime + ReadyTimeout;
            Vector2 last = ScenarioInput.ScreenOf(button.transform as RectTransform);
            int stable = 0;
            while (button != null && Time.unscaledTime < deadline && stable < StableFrames)
            {
                yield return null;
                if (button == null) yield break;
                Vector2 now = ScenarioInput.ScreenOf(button.transform as RectTransform);
                bool ready = button.IsInteractable() && (now - last).sqrMagnitude < 0.25f && IsRaycastable(button);
                stable = ready ? stable + 1 : 0;
                last = now;
            }
        }

        private static bool IsRaycastable(Button button)
        {
            foreach (CanvasGroup group in button.GetComponentsInParent<CanvasGroup>())
            {
                if (!group.blocksRaycasts || group.alpha <= 0.01f) return false;
                if (group.ignoreParentGroups) break;
            }
            return true;
        }

        private static Vector2 PickPoint(Button button, out string topHit)
        {
            var rect = button.transform as RectTransform;
            Vector2 center = ScenarioInput.ScreenOf(rect);
            Vector2 size = ScreenSize(rect);
            topHit = "none";
            foreach (Vector2 offset in Offsets)
            {
                Vector2 point = center + Vector2.Scale(offset, size);
                string top = TopHit(point, out GameObject hit);
                if (offset == Vector2.zero) topHit = top;
                if (hit != null && hit.transform.IsChildOf(button.transform))
                {
                    topHit = top;
                    return point;
                }
            }
            return center;
        }

        private static string TopHit(Vector2 screen, out GameObject hit)
        {
            hit = null;
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return "no EventSystem";
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = screen }, hits);
            if (hits.Count == 0) return "none";
            hit = hits[0].gameObject;
            return hit != null ? hit.name : "none";
        }

        private static Vector2 ScreenSize(RectTransform rect)
        {
            if (rect == null) return Vector2.zero;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            return max - min;
        }
        #endregion
    }
}
#endif
