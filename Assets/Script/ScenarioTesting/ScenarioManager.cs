#if UNITY_EDITOR || PV_SCENARIO_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioTesting
{
    public sealed class ScenarioManager : MonoBehaviour
    {
        #region [Variables/Fields]
        public const string ObjectName = "[Runtime] ScenarioManager";
        public const string PendingKey = "PaleVertica.Scenario.Pending";
        public const string ExitKey = "PaleVertica.Scenario.ExitWhenDone";
        public const string SpeedKey = "PaleVertica.Scenario.Speed";
        public const float MinSpeed = 0.25f;
        public const float MaxSpeed = 8f;

        private static ScenarioManager _instance;
        private readonly Queue<ScenarioDefinition> _queue = new Queue<ScenarioDefinition>();
        private readonly ScenarioInput _input = new ScenarioInput();
        private Coroutine _routine;
        private bool _exitWhenDone;
        private int _runTotal;
        private int _runsFinished;

        public static event Action<ScenarioRun> OnRunStarted;
        public static event Action<ScenarioRun> OnRunChanged;
        public static event Action<ScenarioRun> OnRunFinished;
        public static event Action OnQueueFinished;
        #endregion

        #region [Properties]
        public static ScenarioManager Instance => _instance;
        public static bool IsReady => _instance != null;
        public static ScenarioRun Current { get; private set; }
        public static ScenarioRun Last { get; private set; }
        public bool IsBusy => _routine != null;
        public int Queued => _queue.Count;
        public static int RunTotal => _instance != null ? _instance._runTotal : 0;
        public static int RunsFinished => _instance != null ? _instance._runsFinished : 0;
        public static int RunNumber => Mathf.Min(RunsFinished + 1, RunTotal);

        public static float Speed
        {
            get => Time.timeScale;
            set => Time.timeScale = Mathf.Clamp(value, MinSpeed, MaxSpeed);
        }
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
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<ScenarioOverlayView>();
        }

        private void Update()
        {
            if (Current != null) _input.KeepCurrent();
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            _input.Disable();
            _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static ScenarioManager Create()
        {
            return new GameObject(ObjectName).AddComponent<ScenarioManager>();
        }

        public static ScenarioManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public void Enqueue(ScenarioDefinition definition)
        {
            if (definition == null) return;
            if (_routine == null)
            {
                _runTotal = 0;
                _runsFinished = 0;
            }
            _runTotal++;
            _queue.Enqueue(definition);
            if (_routine == null) _routine = StartCoroutine(Process());
        }

        public void StopAll()
        {
            _queue.Clear();
            _runTotal = _runsFinished + (Current != null ? 1 : 0);
            if (Current != null) Current.StopRequested = true;
        }

        public static IEnumerator RunAndWait(ScenarioDefinition definition, Action<ScenarioRun> onFinished)
        {
            ScenarioRun finished = null;
            void Capture(ScenarioRun run)
            {
                if (run.Definition == definition) finished = run;
            }

            OnRunFinished += Capture;
            Ensure().Enqueue(definition);
            while (finished == null) yield return null;
            OnRunFinished -= Capture;
            onFinished?.Invoke(finished);
        }
        #endregion

        #region [Private Methods]
        private IEnumerator Process()
        {
            _input.Enable();
            while (_queue.Count > 0) yield return Execute(_queue.Dequeue());
            _input.Disable();
            _routine = null;
            OnQueueFinished?.Invoke();
#if UNITY_EDITOR
            if (_exitWhenDone) UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private IEnumerator Execute(ScenarioDefinition definition)
        {
            var run = new ScenarioRun(definition, _input);
            Current = run;
            run.OnChanged += () => OnRunChanged?.Invoke(run);
            run.Begin();
            OnRunStarted?.Invoke(run);

            if (!definition.IsValid) run.Record("scenario file is valid", ScenarioCheckStatus.Failed, "valid JSON with steps", definition.Error);

            foreach (ScenarioStep step in definition.Steps)
            {
                if (!definition.IsValid) break;
                if (run.StopRequested)
                {
                    run.Log("stopped by the user");
                    break;
                }

                Debug.Log($"[Scenario] {definition.Name} step {step.Index + 1}/{definition.Steps.Count} (run {RunNumber}/{RunTotal}): {step.Describe()}");
                run.BeginStep(step);
                ScenarioAction action = ScenarioAction.Create(step.Action);
                if (action == null) run.FailStep($"unknown action '{step.Action}'");
                else yield return Guard(action.Run(run, step), run, step.Timeout);

                _input.ReleaseAll();
                run.EndStep();
                if (run.StepFailed && !(step.ContinueOnFail || definition.ContinueOnFail)) break;
            }

            run.Finish();
            _runsFinished++;
            Last = run;
            Current = null;
            OnRunFinished?.Invoke(run);
        }

        private static IEnumerator Guard(IEnumerator root, ScenarioRun run, float timeout)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(root);
            float deadline = Time.time + timeout;

            while (stack.Count > 0)
            {
                if (run.StepFailed || run.StopRequested) yield break;
                if (Time.time > deadline)
                {
                    run.FailStep($"step timed out after {timeout:0.#}s");
                    yield break;
                }

                IEnumerator top = stack.Peek();
                object current = null;
                bool moved;
                try
                {
                    moved = top.MoveNext();
                    if (moved) current = top.Current;
                }
                catch (Exception exception)
                {
                    run.FailStep($"{exception.GetType().Name}: {exception.Message}");
                    Debug.LogException(exception);
                    yield break;
                }

                if (!moved)
                {
                    stack.Pop();
                    continue;
                }

                if (current is IEnumerator nested && !(current is CustomYieldInstruction))
                {
                    stack.Push(nested);
                    continue;
                }
                yield return current;
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartPending()
        {
            string pending = UnityEditor.SessionState.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(pending)) return;
            UnityEditor.SessionState.EraseString(PendingKey);

            ScenarioManager manager = Ensure();
            manager._exitWhenDone = UnityEditor.SessionState.GetBool(ExitKey, false);
            Speed = UnityEditor.SessionState.GetFloat(SpeedKey, 1f);
            foreach (string path in pending.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(path)) manager.Enqueue(ScenarioDefinition.Load(path.Trim()));
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            Current = null;
            Last = null;
            OnRunStarted = null;
            OnRunChanged = null;
            OnRunFinished = null;
            OnQueueFinished = null;
        }
        #endregion
    }
}
#endif
