using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CoreSystem.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.UI.Diagnostics
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class UIDebugCapture : MonoBehaviour
    {
        #region [Variables/Fields]
        private static UIDebugCapture _instance;
        private bool _isCapturing;
        #endregion

        #region [Properties]
        public static UIDebugCapture Instance => _instance;
        public static bool IsReady => _instance != null;
        public static event Action<string, int> OnCaptured;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f9Key.wasPressedThisFrame) CaptureOpenModals();
        }
        #endregion

        #region [Public Methods]
        public static UIDebugCapture Create()
        {
            return RuntimeRoot.CreateChild("UIDebugCapture").AddComponent<UIDebugCapture>();
        }

        public static UIDebugCapture Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public static void Capture(UIGlobalModal modal)
        {
            if (modal == null) return;
            Ensure().StartCapture(new List<UIGlobalModal> { modal });
        }

        public void CaptureOpenModals()
        {
            var modals = new List<UIGlobalModal>();
            if (UIManager.Instance != null && UIManager.Instance.ModalContainer != null)
            {
                foreach (var modal in UIManager.Instance.ModalContainer.GetComponentsInChildren<UIGlobalModal>(false))
                {
                    if (modal.IsOpen) modals.Add(modal);
                }
            }
            StartCapture(modals);
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isEditor && !Debug.isDebugBuild) return;
            Ensure();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnCaptured = null;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Ensure();
        }

        private void StartCapture(List<UIGlobalModal> modals)
        {
            if (_isCapturing) return;
            StartCoroutine(CaptureRoutine(modals));
        }

        private IEnumerator CaptureRoutine(List<UIGlobalModal> modals)
        {
            _isCapturing = true;
            string name = modals.Count == 1 ? "capture_" + modals[0].WindowName : "capture_screen";
            string basePath = UIDebugSnapshot.GetBasePath(name);

            var report = new StringBuilder();
            var allIssues = new List<UILayoutIssue>();
            foreach (var modal in modals)
            {
                var issues = UILayoutInspector.Inspect(modal.WindowRect, modal.Theme);
                foreach (var issue in issues) issue.Number = allIssues.Count + issue.Number;
                allIssues.AddRange(issues);
                report.AppendLine(UILayoutInspector.FormatReport(modal.WindowName, modal.WindowRect, issues));
            }
            if (modals.Count == 0) report.AppendLine("No open UIGlobalModal; screen only.");

            yield return new WaitForEndOfFrame();
            var clean = ScreenCapture.CaptureScreenshotAsTexture();
            UIDebugSnapshot.SaveImage(basePath, string.Empty, clean);
            Destroy(clean);

            var overlays = new List<GameObject>();
            foreach (var modal in modals)
            {
                var canvas = modal.GetComponentInParent<Canvas>();
                var mine = allIssues.FindAll(issue => issue.Target != null && issue.Target.IsChildOf(modal.transform));
                RectTransform visible = modal.ScrollRect != null && modal.ScrollRect.viewport != null ? modal.ScrollRect.viewport : modal.WindowRect;
                var overlay = UIDebugOverlay.Build(canvas, mine, visible);
                if (overlay != null) overlays.Add(overlay);
            }

            yield return new WaitForEndOfFrame();
            var annotated = ScreenCapture.CaptureScreenshotAsTexture();
            UIDebugSnapshot.SaveImage(basePath, ".issues", annotated);
            Destroy(annotated);
            foreach (var overlay in overlays) Destroy(overlay);

            UIDebugSnapshot.SaveReport(basePath, report.ToString());
            Debug.Log($"[UIDebugCapture] {allIssues.Count} layout issue(s). Saved {basePath}.png / .issues.png / .txt");
            OnCaptured?.Invoke(basePath, allIssues.Count);
            _isCapturing = false;
        }
        #endregion
    }
}
