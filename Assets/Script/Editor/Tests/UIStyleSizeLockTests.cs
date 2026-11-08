using System.Collections.Generic;
using CoreSystem.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class UIStyleSizeLockTests
    {
        #region [Variables/Fields]
        private readonly List<Object> _cleanup = new List<Object>();
        #endregion

        #region [Public Methods]
        [TearDown]
        public void TearDown()
        {
            foreach (var o in _cleanup) if (o != null) Object.DestroyImmediate(o);
            _cleanup.Clear();
        }

        [Test]
        public void SizeBelowMinimum_IsRaisedToRegistryMinimum()
        {
            var go = NewUI("Button");
            UIStyleSizeLock.Attach(go, new Vector2(181f, 55f), Vector2.zero);

            new UIElementRef(go).Size(200f, 44f);

            var le = go.GetComponent<LayoutElement>();
            Assert.AreEqual(200f, le.preferredWidth);
            Assert.AreEqual(55f, le.minHeight);
            Assert.AreEqual(55f, le.preferredHeight);
        }

        [Test]
        public void SizeAboveMinimum_IsKept()
        {
            var go = NewUI("Button");
            UIStyleSizeLock.Attach(go, new Vector2(181f, 55f), Vector2.zero);

            new UIElementRef(go).Height(70f).Width(300f);

            var le = go.GetComponent<LayoutElement>();
            Assert.AreEqual(70f, le.preferredHeight);
            Assert.AreEqual(300f, le.preferredWidth);
        }

        [Test]
        public void FixedSize_WinsOverHeightAndMinSize()
        {
            var go = NewUI("Button");
            UIStyleSizeLock.Attach(go, Vector2.zero, new Vector2(0f, 55f));

            new UIElementRef(go).Height(80f).SetMinSize(-1f, 20f);

            var le = go.GetComponent<LayoutElement>();
            Assert.AreEqual(55f, le.minHeight);
            Assert.AreEqual(55f, le.preferredHeight);
        }

        [Test]
        public void SetSize_RaisesRectToMinimum()
        {
            var go = NewUI("Button");
            UIStyleSizeLock.Attach(go, new Vector2(181f, 55f), Vector2.zero);

            new UIElementRef(go).SetSize(120f, 30f);

            Assert.AreEqual(new Vector2(181f, 55f), ((RectTransform)go.transform).sizeDelta);
        }

        [Test]
        public void GridCell_GrowsToFitLockedChildren()
        {
            var grid = NewUI("Grid").AddComponent<GridLayoutGroup>();
            var child = NewUI("Button");
            child.transform.SetParent(grid.transform, false);
            UIStyleSizeLock.Attach(child, new Vector2(181f, 55f), Vector2.zero);

            new UIElementRef(grid.gameObject).Cell(132f, 44f);

            Assert.AreEqual(new Vector2(181f, 55f), grid.cellSize);
        }

        [Test]
        public void GridCell_LargerThanMinimum_IsKept()
        {
            var grid = NewUI("Grid").AddComponent<GridLayoutGroup>();
            var child = NewUI("Button");
            child.transform.SetParent(grid.transform, false);
            UIStyleSizeLock.Attach(child, new Vector2(181f, 55f), Vector2.zero);

            new UIElementRef(grid.gameObject).Cell(240f, 60f);

            Assert.AreEqual(new Vector2(240f, 60f), grid.cellSize);
        }

        [Test]
        public void Minimum_WinsOverLowerLayoutElementInUnityLayout()
        {
            var go = NewUI("Button");
            UIStyleSizeLock.Attach(go, new Vector2(181f, 55f), Vector2.zero);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 20f;
            le.preferredHeight = 30f;
            le.minWidth = 50f;
            le.preferredWidth = 300f;

            var rect = (RectTransform)go.transform;
            Assert.AreEqual(55f, LayoutUtility.GetMinHeight(rect));
            Assert.AreEqual(55f, LayoutUtility.GetPreferredHeight(rect));
            Assert.AreEqual(181f, LayoutUtility.GetMinWidth(rect));
            Assert.AreEqual(300f, LayoutUtility.GetPreferredWidth(rect));
        }

        [Test]
        public void Minimum_HoldsInsideAShortRow()
        {
            var row = NewUI("Row");
            ((RectTransform)row.transform).sizeDelta = new Vector2(600f, 30f);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            var button = NewUI("Button");
            button.transform.SetParent(row.transform, false);
            UIStyleSizeLock.Attach(button, new Vector2(181f, 55f), Vector2.zero);
            button.GetComponent<LayoutElement>().preferredHeight = 20f;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)row.transform);

            Assert.AreEqual(55f, ((RectTransform)button.transform).rect.height, 0.01f);
            Assert.AreEqual(181f, ((RectTransform)button.transform).rect.width, 0.01f);
        }

        [Test]
        public void Attach_WithNoSizing_AddsNothing()
        {
            var go = NewUI("Button");
            Assert.IsNull(UIStyleSizeLock.Attach(go, Vector2.zero, Vector2.zero));
            Assert.IsNull(go.GetComponent<UIStyleSizeLock>());
        }
        #endregion

        #region [Private Methods]
        private GameObject NewUI(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            _cleanup.Add(go);
            return go;
        }
        #endregion
    }
}
