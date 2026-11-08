using System.Collections.Generic;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.Visualizer;
using GlobalVisualizer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class InventoryVisualizerTests
    {
        #region [Variables/Fields]
        private const long FrameBudgetMilliseconds = 50;
        private readonly List<Object> _cleanup = new List<Object>();
        #endregion

        #region [Public Methods]
        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _cleanup.Count; i++)
            {
                if (_cleanup[i] != null) Object.DestroyImmediate(_cleanup[i]);
            }
            _cleanup.Clear();
        }

        [Test]
        public void AddItems_RaisesOneChangeEvent()
        {
            var inventory = new PlayerInventory();
            int events = 0;
            inventory.OnInventoryChanged += () => events++;
            List<Item> items = Database.GetItems;

            inventory.AddItems(items);

            Assert.AreEqual(1, events);
            Assert.AreEqual(InventoryItemCount(items), inventory.Count);
        }

        [Test]
        public void Batch_NestsAndStaysQuietWithoutChanges()
        {
            var inventory = new PlayerInventory();
            int events = 0;
            inventory.OnInventoryChanged += () => events++;
            List<Item> items = Database.GetItems;

            inventory.BeginBatch();
            inventory.BeginBatch();
            inventory.AddItem(items[0]);
            inventory.EndBatch();
            inventory.AddItem(items[1]);
            Assert.AreEqual(0, events);
            inventory.EndBatch();
            Assert.AreEqual(1, events);

            inventory.BeginBatch();
            inventory.EndBatch();
            inventory.EndBatch();
            Assert.AreEqual(1, events);
            Assert.IsFalse(inventory.IsBatching);
        }

        [Test]
        public void AttachTo_ConvertsTheGridLayoutGroup()
        {
            RectTransform content = CreateScrollContent(new Vector2(560f, 400f));
            var stale = new GameObject("Stale Slot", typeof(RectTransform));
            stale.transform.SetParent(content, false);

            GlobalVisualizerView view = VisualizerViewFactory.AttachTo(content, CreateTemplate());
            view.SetCount(199);

            VisualLayoutSettings settings = view.LayoutSettings;
            Assert.AreEqual(VisualLayoutKind.Grid, settings.Kind);
            Assert.AreEqual(new Vector2(80f, 80f), settings.ItemSize);
            Assert.AreEqual(new Vector2(25f, 15f), settings.Spacing);
            Assert.AreEqual(20f, settings.Padding.Left);
            Assert.AreEqual(10f, settings.Padding.Top);
            Assert.IsFalse(content.GetComponent<GridLayoutGroup>().enabled);
            Assert.IsFalse(content.GetComponent<ContentSizeFitter>().enabled);
            Assert.IsNull(content.Find("Stale Slot"));
            Assert.AreEqual(199, view.Count);
            Assert.LessOrEqual(view.CreatedViewCount, 35);
            Assert.AreSame(view, VisualizerViewFactory.AttachTo(content, CreateTemplate()));
        }

        [Test]
        public void FromLayoutGroup_KeepsFixedWidthRowsCentered()
        {
            var go = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup));
            _cleanup.Add(go);
            VerticalLayoutGroup group = go.GetComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.UpperCenter;
            group.childControlWidth = false;
            group.spacing = 7.8f;
            group.padding = new RectOffset(0, 0, 5, 0);
            RectTransform template = CreateTemplate();
            template.sizeDelta = new Vector2(320f, 64f);

            VisualLayoutSettings settings = VisualizerViewFactory.FromLayoutGroup(group, template);

            Assert.AreEqual(VisualLayoutKind.Vertical, settings.Kind);
            Assert.AreEqual(new Vector2(320f, 0f), settings.ItemSize);
            Assert.AreEqual(VisualAlignment.Center, settings.CrossAlignment);
            Assert.AreEqual(7.8f, settings.Spacing.y, 0.001f);
            Assert.AreEqual(5f, settings.Padding.Top);
        }

        [Test]
        public void AddAll_WithRealSlotsStaysInsideOneFrame()
        {
            GameObject managerPrefab = Resources.Load<GameObject>("Prefab/InventoryManager");
            InventoryManager manager = managerPrefab != null ? managerPrefab.GetComponent<InventoryManager>() : null;
            if (manager == null || manager.ItemSlotSquarePrefab == null) Assert.Ignore("Resources/Prefab/InventoryManager has no square slot prefab.");

            RectTransform content = CreateScrollContent(new Vector2(560f, 400f));
            GlobalVisualizerView view = VisualizerViewFactory.AttachTo(content, (RectTransform)manager.ItemSlotSquarePrefab.transform);
            var inventory = new PlayerInventory();
            var shown = new List<ItemInstance>();
            view.SetItems(shown, (slot, item, index) => slot.GetComponent<ItemSlotGlobal>().UpdateUI(item));
            List<Item> items = Database.GetItems;

            AddAndShow(inventory, items, shown, view);
            inventory.Clear();
            shown.Clear();
            view.NotifyReset();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            AddAndShow(inventory, items, shown, view);
            watch.Stop();

            Assert.AreEqual(InventoryItemCount(items), view.Count);
            Assert.LessOrEqual(view.CreatedViewCount, 35);
            Assert.Less(watch.ElapsedMilliseconds, FrameBudgetMilliseconds);
        }
        #endregion

        #region [Private Methods]
        private static int InventoryItemCount(List<Item> items)
        {
            int count = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type != ItemType.Progression) count++;
            }
            return count;
        }

        private static void AddAndShow(PlayerInventory inventory, List<Item> items, List<ItemInstance> shown, GlobalVisualizerView view)
        {
            inventory.AddItems(items);
            shown.Clear();
            foreach (ItemInstance instance in inventory.Items) shown.Add(instance);
            view.NotifyReset();
        }

        private RectTransform CreateScrollContent(Vector2 size)
        {
            var root = new GameObject("Scroll View", typeof(RectTransform));
            _cleanup.Add(root);
            var rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = size;

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.SetParent(rootRect, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            var contentRect = (RectTransform)content.transform;
            contentRect.SetParent(viewportRect, false);
            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80f, 80f);
            grid.spacing = new Vector2(25f, 15f);
            grid.padding = new RectOffset(20, 0, 10, 0);
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = root.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            return contentRect;
        }

        private RectTransform CreateTemplate()
        {
            var template = new GameObject("Slot", typeof(RectTransform));
            _cleanup.Add(template);
            var rect = (RectTransform)template.transform;
            rect.sizeDelta = new Vector2(80f, 80f);
            template.SetActive(false);
            return rect;
        }
        #endregion
    }
}
