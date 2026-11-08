using System.Collections.Generic;
using CoreSystem.Loot;
using UnityEngine;

namespace CoreSystem.UI.Loot
{
    public sealed class LootLabelWindow : UIWindow
    {
        #region [Variables/Fields]
        public const string WindowName = "LootLabels";
        private const float LabelGap = 2f;
        private const float HeightAboveItem = 24f;

        [SerializeField] private RectTransform _container;
        [SerializeField] private LootLabelView _labelTemplate;

        private readonly List<LootLabelView> _labels = new List<LootLabelView>();
        private readonly List<Vector2> _desired = new List<Vector2>();
        private readonly List<Vector2> _sizes = new List<Vector2>();
        private readonly List<Vector2> _resolved = new List<Vector2>();
        private Camera _camera;
        private Camera _canvasCamera;
        #endregion

        #region [Unity Methods]
        protected override void OnEnable()
        {
            base.OnEnable();
            _camera = Camera.main;
            Canvas canvas = GetComponentInParent<Canvas>();
            _canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

            GroundItem.OnSpawned += AddLabel;
            GroundItem.OnRemoved += RemoveLabel;
            foreach (GroundItem item in GroundItem.Active) AddLabel(item);
        }

        private void OnDisable()
        {
            GroundItem.OnSpawned -= AddLabel;
            GroundItem.OnRemoved -= RemoveLabel;
            foreach (LootLabelView label in _labels)
            {
                if (label != null) Destroy(label.gameObject);
            }
            _labels.Clear();
        }

        private void LateUpdate()
        {
            if (_camera == null || _labels.Count == 0) return;

            _desired.Clear();
            _sizes.Clear();
            foreach (LootLabelView label in _labels)
            {
                Vector2 screen = _camera.WorldToScreenPoint(label.Item.transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, screen, _canvasCamera, out Vector2 local);
                _desired.Add(local + Vector2.up * HeightAboveItem);
                _sizes.Add(label.Rect.sizeDelta);
            }

            LootLabelLayout.Resolve(_desired, _sizes, LabelGap, _resolved);
            for (int i = 0; i < _labels.Count; i++) _labels[i].Rect.anchoredPosition = _resolved[i];
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            transform.SetAsFirstSibling();
        }
        #endregion

        #region [Private Methods]
        private void AddLabel(GroundItem item)
        {
            LootLabelView label = Instantiate(_labelTemplate, _container);
            label.gameObject.SetActive(true);
            label.Bind(item);
            _labels.Add(label);
        }

        private void RemoveLabel(GroundItem item)
        {
            for (int i = _labels.Count - 1; i >= 0; i--)
            {
                if (_labels[i].Item != item) continue;
                Destroy(_labels[i].gameObject);
                _labels.RemoveAt(i);
            }
        }
        #endregion
    }
}
