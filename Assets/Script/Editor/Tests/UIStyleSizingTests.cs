using CoreSystem.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class UIStyleSizingTests
    {
        #region [Variables/Fields]
        private GameObject _root;
        private GameObject _buttonPrefab;
        private UIElementRegistrySO _registry;
        private UIDocument _document;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            _buttonPrefab = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var label = new GameObject("Text", typeof(RectTransform));
            label.transform.SetParent(_buttonPrefab.transform, false);
            label.AddComponent<TextMeshProUGUI>();

            _registry = ScriptableObject.CreateInstance<UIElementRegistrySO>();
            _registry.Entries.Add(new UIElementEntry { StyleName = "Tall", Type = UIElementType.Button, Prefab = _buttonPrefab, MinSize = new Vector2(0f, 50f) });
            _registry.Entries.Add(new UIElementEntry { StyleName = "Locked", Type = UIElementType.Button, Prefab = _buttonPrefab, FixedSize = new Vector2(120f, 40f) });
            _registry.Entries.Add(new UIElementEntry { StyleName = "Plain", Type = UIElementType.Button, Prefab = _buttonPrefab });
            _registry.Entries.Add(new UIElementEntry { StyleName = "Round", Type = UIElementType.Button, Prefab = _buttonPrefab, CornerRadius = 10f });

            _root = new GameObject("Root", typeof(RectTransform));
            var group = _root.AddComponent<VerticalLayoutGroup>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            _document = new UIDocument((RectTransform)_root.transform, _registry);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_buttonPrefab);
            Object.DestroyImmediate(_registry);
        }

        [Test]
        public void MinSize_RaisesControlHeight()
        {
            _document.Build(ui => ui.Button("Tall", "OK", null).Id("button"));

            var le = _document.Find("button").GameObject.GetComponent<LayoutElement>();
            Assert.AreEqual(50f, le.minHeight);
            Assert.GreaterOrEqual(le.preferredHeight, 50f);
        }

        [Test]
        public void FixedSize_LocksBothAxes()
        {
            _document.Build(ui => ui.Button("Locked", "A long button label", null).Id("button"));

            var le = _document.Find("button").GameObject.GetComponent<LayoutElement>();
            Assert.AreEqual(120f, le.minWidth);
            Assert.AreEqual(120f, le.preferredWidth);
            Assert.AreEqual(0f, le.flexibleWidth);
            Assert.AreEqual(40f, le.minHeight);
            Assert.AreEqual(40f, le.preferredHeight);
        }

        [Test]
        public void CodeModifiers_StillWinOverStyle()
        {
            _document.Build(ui => ui.Button("Locked", "OK", null).Id("button").Width(200f));

            Assert.AreEqual(200f, _document.Find("button").GameObject.GetComponent<LayoutElement>().preferredWidth);
        }

        [Test]
        public void StyleWithoutSizing_KeepsLabelSizing()
        {
            _document.Build(ui => ui.Button("Plain", "OK", null).Id("button"));

            var le = _document.Find("button").GameObject.GetComponent<LayoutElement>();
            Assert.AreEqual(_registry.Theme.ControlHeight, le.minHeight, 20f);
            Assert.Less(le.minHeight, 50f);
        }

        [Test]
        public void StyleCornerRadius_RoundsTheControl()
        {
            _document.Build(ui => ui.Button("Round", "OK", null).Id("button"));

            var image = _document.Find("button").GameObject.GetComponent<Image>();
            Assert.AreSame(UIShapes.RoundedRect, image.sprite);
            Assert.AreEqual(Image.Type.Sliced, image.type);
            Assert.AreEqual(UIShapes.RoundedRadius / 10f, image.pixelsPerUnitMultiplier, 0.001f);
        }

        [Test]
        public void CodeCornerRadius_WinsOverStyle()
        {
            _document.Build(ui => ui.Button("Round", "OK", null).Id("button").CornerRadius(16f));

            var image = _document.Find("button").GameObject.GetComponent<Image>();
            Assert.AreEqual(UIShapes.RoundedRadius / 16f, image.pixelsPerUnitMultiplier, 0.001f);
        }

        [Test]
        public void StyleWithoutCornerRadius_KeepsPrefabShape()
        {
            _document.Build(ui => ui.Button("Plain", "OK", null).Id("button"));

            var image = _document.Find("button").GameObject.GetComponent<Image>();
            Assert.IsNull(image.sprite);
            Assert.AreEqual(Image.Type.Simple, image.type);
            Assert.AreEqual(1f, image.pixelsPerUnitMultiplier, 0.001f);
        }
        #endregion
    }
}
