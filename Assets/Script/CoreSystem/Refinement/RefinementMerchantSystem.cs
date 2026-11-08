using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Interaction;
using CoreSystem.Player;
using CoreSystem.UI;
using UnityEngine;

namespace CoreSystem.Refinement
{
    [DisallowMultipleComponent]
    public class RefinementMerchantSystem : MonoBehaviour, IInteractionProvider
    {
        #region [Variables/Fields]
        public const string OptionId = "refinement.merchant";
        public const string DefaultWindowName = "Refinement";

        [SerializeField] private Character _character;
        [SerializeField] private string _label = "Refine";
        [SerializeField, Min(0f)] private float _range = 1.5f;
        [SerializeField] private int _priority = 200;
        [SerializeField] private string _windowName = DefaultWindowName;
        #endregion

        #region [Properties]
        public Character Character => _character;
        public string Label => _label;
        public float Range => _range;
        public string WindowName => string.IsNullOrEmpty(_windowName) ? DefaultWindowName : _windowName;
        #endregion

        #region [Unity Methods]
        private void Reset()
        {
            _character = GetComponentInParent<Character>();
        }

        private void Awake()
        {
            if (_character == null) _character = GetComponentInParent<Character>();
        }

        private void OnEnable()
        {
            InteractionRegistry.Register(this);
        }

        private void OnDisable()
        {
            InteractionRegistry.Unregister(this);
        }
        #endregion

        #region [Public Methods]
        public void Configure(Character character, string label, float range, string windowName)
        {
            _character = character;
            _label = label;
            _range = Mathf.Max(0f, range);
            _windowName = windowName;
        }

        public static bool ShouldOffer(InteractionContext context, Character merchant)
        {
            if (merchant == null || context.Actor == null || context.Target != merchant) return false;
            if (context.IsSelf || merchant.IsDead) return false;
            return context.Holder is PlayerCore;
        }

        public void Collect(InteractionContext context, List<InteractionOption> options)
        {
            if (!ShouldOffer(context, _character)) return;
            options.Add(new InteractionOption(OptionId, _label, _ => OpenWindow(), _range, _priority));
        }

        public void OpenWindow()
        {
            if (UIManager.Instance == null) return;
            UIManager.Instance.Open(WindowName);
        }
        #endregion
    }
}
