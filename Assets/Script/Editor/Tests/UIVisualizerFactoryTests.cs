using System.Collections.Generic;
using System.Linq;
using CoreSystem.UI;
using GlobalVisualizer;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class UIVisualizerFactoryTests
    {
        #region [Variables/Fields]
        private GameObject _root;
        private UIDocument _document;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform));
            ((RectTransform)_root.transform).sizeDelta = new Vector2(400f, 600f);
            var group = _root.AddComponent<VerticalLayoutGroup>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            _document = new UIDocument((RectTransform)_root.transform, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void Visualizer_BuildsOnlyTheVisibleCells()
        {
            List<int> items = Enumerable.Range(0, 500).ToList();
            int built = 0;

            _document.Build(ui => ui.Visualizer(items, VisualLayoutSettings.Vertical(30f, 0f), (cell, value, index) =>
            {
                built++;
                cell.Label(value.ToString());
            }, 300f).Id("list"));
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_root.transform);

            GlobalVisualizerView view = UIVisualizerFactory.Find(_document.Find("list"));
            Assert.IsNotNull(view);
            Assert.AreEqual(500, view.Count);
            Assert.Greater(view.ActiveViewCount, 0);
            Assert.Less(view.CreatedViewCount, 40);
            Assert.Less(built, 80);
            Assert.IsFalse(view.Content.GetComponent<LayoutGroup>() != null && view.Content.GetComponent<LayoutGroup>().enabled);
        }

        [Test]
        public void Visualizer_RebuildsRecycledCells()
        {
            List<int> items = Enumerable.Range(0, 500).ToList();
            _document.Build(ui => ui.Visualizer(items, VisualLayoutSettings.Vertical(30f, 0f), (cell, value, index) => cell.Label("Row " + value), 300f).Id("list"));
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_root.transform);
            GlobalVisualizerView view = UIVisualizerFactory.Find(_document.Find("list"));

            view.ScrollPosition = new Vector2(0f, 3000f);

            RectTransform cell = view.GetView(100);
            Assert.IsNotNull(cell);
            Assert.AreEqual(1, cell.childCount);
            TMP_Text label = cell.GetComponentInChildren<TMP_Text>(true);
            if (label != null) Assert.AreEqual("Row 100", label.text);
            Assert.IsNull(view.GetView(0));
        }
        #endregion
    }
}
