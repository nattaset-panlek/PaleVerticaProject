using CoreSystem.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class UIDocumentTests
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
        public void NestedContainers_UnderLayoutGroups_HaveNoContentSizeFitter()
        {
            _document.Build(ui => ui.Column(col => col.Row(row => row.Grid(grid => grid.Column(null)))));

            Assert.AreEqual(0, _root.GetComponentsInChildren<ContentSizeFitter>(true).Length);
        }

        [Test]
        public void Containers_GetTheRequestedLayoutGroup()
        {
            _document.Build(ui =>
            {
                ui.Row(null).Id("row");
                ui.Column(null).Id("column");
                ui.Grid(null).Id("grid");
                ui.Horizontal("Container", null).Id("styledRow");
            });

            Assert.IsNotNull(_document.Find("row").GameObject.GetComponent<HorizontalLayoutGroup>());
            Assert.IsNotNull(_document.Find("column").GameObject.GetComponent<VerticalLayoutGroup>());
            Assert.IsNotNull(_document.Find("grid").GameObject.GetComponent<GridLayoutGroup>());
            Assert.IsNotNull(_document.Find("styledRow").GameObject.GetComponent<HorizontalLayoutGroup>());
        }

        [Test]
        public void BlocksStretchInColumns_ButNotInRows()
        {
            _document.Build(ui =>
            {
                ui.Column(null).Id("block");
                ui.Row(row => row.Column(null).Id("inline"));
            });

            Assert.AreEqual(1f, _document.Find("block").GameObject.GetComponent<LayoutElement>().flexibleWidth);
            var inline = _document.Find("inline").GameObject.GetComponent<LayoutElement>();
            Assert.IsTrue(inline == null || inline.flexibleWidth <= 0f);
        }

        [Test]
        public void Width_OverridesBlockStretch()
        {
            _document.Build(ui => ui.Column(null).Id("fixed").Width(120f));

            var le = _document.Find("fixed").GameObject.GetComponent<LayoutElement>();
            Assert.AreEqual(120f, le.preferredWidth);
            Assert.AreEqual(0f, le.flexibleWidth);
        }

        [Test]
        public void RefreshRegion_RebuildsOnlyThatRegion()
        {
            int rootBuilds = 0;
            int regionBuilds = 0;
            _document.Build(ui =>
            {
                rootBuilds++;
                ui.Column(null).Id("static");
                ui.Region("region", region =>
                {
                    regionBuilds++;
                    region.Column(null).Id("inside");
                });
            });

            GameObject staticBefore = _document.Find("static").GameObject;
            GameObject insideBefore = _document.Find("inside").GameObject;

            Assert.IsTrue(_document.Refresh("region"));

            Assert.AreEqual(1, rootBuilds);
            Assert.AreEqual(2, regionBuilds);
            Assert.AreSame(staticBefore, _document.Find("static").GameObject);
            Assert.IsTrue(_document.Find("inside").IsValid);
            Assert.AreNotSame(insideBefore, _document.Find("inside").GameObject);
        }

        [Test]
        public void Refresh_UnknownRegion_ReturnsFalse()
        {
            _document.Build(ui => ui.Column(null));

            Assert.IsFalse(_document.Refresh("missing"));
        }

        [Test]
        public void Pages_ShowPage_SwitchesContent_AndSurvivesFullRefresh()
        {
            _document.Build(ui => ui.Pages("pages", set => set
                .Add("A", page => page.Column(null).Id("a"))
                .Add("B", page => page.Column(null).Id("b")), UIPageNav.SideMenu));

            Assert.IsTrue(_document.Find("a").IsValid);
            Assert.IsFalse(_document.Find("b").IsValid);

            _document.ShowPage("pages", "B");

            Assert.AreEqual(1, _document.GetPage("pages"));
            Assert.IsFalse(_document.Find("a").IsValid);
            Assert.IsTrue(_document.Find("b").IsValid);

            _document.Refresh();

            Assert.IsTrue(_document.Find("b").IsValid);
        }

        [Test]
        public void Foldout_ToggleKeepsStateAndShowsContent()
        {
            _document.Build(ui => ui.Foldout("fold", "Section", body => body.Column(null).Id("content")));

            Assert.IsFalse(_document.Find("content").IsValid);

            _document.Find("fold").GameObject.GetComponentInChildren<Button>().onClick.Invoke();

            Assert.IsTrue(_document.GetState("fold.open", false));
            Assert.IsTrue(_document.Find("content").IsValid);

            _document.Refresh();

            Assert.IsTrue(_document.Find("content").IsValid);
        }
        #endregion
    }
}
