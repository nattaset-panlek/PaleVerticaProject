using CoreSystem.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class UIElementRefTests
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
        public void CornerRadius_RoundsAPlainBackground()
        {
            _document.Build(ui => ui.Column(null).Id("panel").Background(Color.gray).CornerRadius(12f));

            var image = _document.Find("panel").GameObject.GetComponent<Image>();
            Assert.AreSame(UIShapes.RoundedRect, image.sprite);
            Assert.AreEqual(Image.Type.Sliced, image.type);
            Assert.AreEqual(UIShapes.RoundedRadius / 12f, image.pixelsPerUnitMultiplier, 0.001f);
            Assert.AreEqual(Color.gray, image.color);
        }

        [Test]
        public void CornerRadius_BiggerRadius_MeansSmallerMultiplier()
        {
            _document.Build(ui =>
            {
                ui.Column(null).Id("small").Background(Color.white).CornerRadius(4f);
                ui.Column(null).Id("large").Background(Color.white).CornerRadius(20f);
            });

            float small = _document.Find("small").GameObject.GetComponent<Image>().pixelsPerUnitMultiplier;
            float large = _document.Find("large").GameObject.GetComponent<Image>().pixelsPerUnitMultiplier;
            Assert.Greater(small, large);
        }

        [Test]
        public void CornerRadius_Zero_RemovesGeneratedRounding()
        {
            _document.Build(ui => ui.Column(null).Id("panel").Background(Color.white).CornerRadius(10f).CornerRadius(0f));

            var image = _document.Find("panel").GameObject.GetComponent<Image>();
            Assert.IsNull(image.sprite);
            Assert.AreEqual(Image.Type.Simple, image.type);
        }

        [Test]
        public void Elastic_TogglesScrollMovement()
        {
            _document.Build(ui =>
            {
                ui.Scroll(null).Id("elastic").Elastic(true, 0.2f);
                ui.Scroll(null).Id("clamped").Elastic(false);
            });

            var elastic = _document.Find("elastic").GameObject.GetComponentInChildren<ScrollRect>();
            var clamped = _document.Find("clamped").GameObject.GetComponentInChildren<ScrollRect>();
            Assert.AreEqual(ScrollRect.MovementType.Elastic, elastic.movementType);
            Assert.AreEqual(0.2f, elastic.elasticity, 0.0001f);
            Assert.AreEqual(ScrollRect.MovementType.Clamped, clamped.movementType);
        }

        [Test]
        public void Scroll_DefaultScrollbar_KeepsThemeGapFromContent()
        {
            _document.Build(ui => ui.Scroll(null).Id("scroll"));

            UITheme theme = _document.Factory.Theme;
            var scrollRect = _document.Find("scroll").GameObject.GetComponent<ScrollRect>();
            Assert.IsNotNull(scrollRect.verticalScrollbar);
            Assert.AreEqual(ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport, scrollRect.verticalScrollbarVisibility);
            Assert.AreEqual(theme.ScrollbarGap + theme.ScrollbarInset, scrollRect.verticalScrollbarSpacing, 0.001f);
            Assert.AreEqual(-theme.ScrollbarInset, ((RectTransform)scrollRect.verticalScrollbar.transform).anchoredPosition.x, 0.001f);
        }

        [Test]
        public void ScrollbarGapAndInset_KeepEachOther()
        {
            _document.Build(ui => ui.Scroll(null).Id("scroll").ScrollbarGap(10f).ScrollbarInset(3f));

            var scrollRect = _document.Find("scroll").GameObject.GetComponent<ScrollRect>();
            var bar = (RectTransform)scrollRect.verticalScrollbar.transform;
            Assert.AreEqual(13f, scrollRect.verticalScrollbarSpacing, 0.001f);
            Assert.AreEqual(new Vector2(-3f, -3f), bar.anchoredPosition);
            Assert.AreEqual(-6f, bar.sizeDelta.y, 0.001f);

            _document.Find("scroll").ScrollbarInset(0f);

            Assert.AreEqual(10f, scrollRect.verticalScrollbarSpacing, 0.001f);
            Assert.AreEqual(0f, bar.sizeDelta.y, 0.001f);
        }

        [Test]
        public void GapAndPadding_OnScroll_ReachTheContent()
        {
            _document.Build(ui => ui.Scroll(null).Id("scroll").Gap(14f).Padding(4, 8, 2, 6));

            var scrollRect = _document.Find("scroll").GameObject.GetComponent<ScrollRect>();
            var group = scrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            Assert.AreEqual(14f, group.spacing, 0.001f);
            Assert.AreEqual(4, group.padding.left);
            Assert.AreEqual(8, group.padding.right);
            Assert.AreEqual(2, group.padding.top);
            Assert.AreEqual(6, group.padding.bottom);
        }
        #endregion
    }
}
