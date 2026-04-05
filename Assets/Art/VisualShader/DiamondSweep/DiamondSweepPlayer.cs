using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class DiamondSweepPlayer : MonoBehaviour
{
    #region [Variables/Fields]

    public enum SweepDirection { Center = 0, Vertical = 1, Horizontal = 2, VerticalBoth = 3, HorizontalBoth = 4 }

    [Serializable]
    public class SweepStep
    {
        public float TargetProgress = 1f;
        public float Duration       = 0.5f;
        public bool  OverrideColors = false;
        [HideInInspector] public Color Color1;
        [HideInInspector] public Color Color2;
        [HideInInspector] public Color Color3;
        public bool OverrideDirection = false;
        [HideInInspector] public SweepDirection Direction;

        [Space(4)]
        public UnityEvent OnStepBegin;
        public UnityEvent OnStepEnd;
    }

    [Header("Settings")]
    public bool PlayOnEnable = true;

    [Header("Steps")]
    public List<SweepStep> Steps = new List<SweepStep>();

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnSequenceEnd;

    private Graphic   _graphic;
    private Material  _materialInstance;
    private Coroutine _routine;

    private static readonly int PropProgress  = Shader.PropertyToID("_Progress");
    private static readonly int PropDirection = Shader.PropertyToID("_Direction");
    private static readonly int PropColor1    = Shader.PropertyToID("_SweepColor1");
    private static readonly int PropColor2    = Shader.PropertyToID("_SweepColor2");
    private static readonly int PropColor3    = Shader.PropertyToID("_SweepColor3");

    #endregion

    #region [Unity Methods]

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _materialInstance = Instantiate(_graphic.material);
        _graphic.material = _materialInstance;
    }

    private void OnEnable()
    {
        if (PlayOnEnable) Play();
    }

    private void OnDestroy()
    {
        if (_materialInstance != null) Destroy(_materialInstance);
    }

    #endregion

    #region [Public Methods]

    public void Play()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(RunSequence());
    }

    public void Stop()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
    }

    public void SetProgress(float value)
    {
        _materialInstance.SetFloat(PropProgress, Mathf.Clamp01(value));
    }

    public void SetColors(Color c1, Color c2, Color c3)
    {
        _materialInstance.SetColor(PropColor1, c1);
        _materialInstance.SetColor(PropColor2, c2);
        _materialInstance.SetColor(PropColor3, c3);
    }

    public void SetDirection(SweepDirection dir)
    {
        _materialInstance.SetInt(PropDirection, (int)dir);
    }

    #endregion

    #region [Private Methods]

    private IEnumerator RunSequence()
    {
        if (Steps == null || Steps.Count == 0) yield break;

        for (int i = 0; i < Steps.Count; i++)
        {
            yield return StartCoroutine(RunStep(Steps[i]));
        }

        OnSequenceEnd?.Invoke();
        _routine = null;
    }

    private IEnumerator RunStep(SweepStep step)
    {
        step.OnStepBegin?.Invoke();

        if (step.OverrideColors)
        {
            SetColors(step.Color1, step.Color2, step.Color3);
        }

        if (step.OverrideDirection)
        {
            SetDirection(step.Direction);
        }

        float startProgress = _materialInstance.GetFloat(PropProgress);
        float elapsed = 0f;

        while (elapsed < step.Duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / step.Duration);
            SetProgress(Mathf.Lerp(startProgress, step.TargetProgress, t));
            yield return null;
        }

        SetProgress(step.TargetProgress);

        step.OnStepEnd?.Invoke();
    }

    #endregion
}
