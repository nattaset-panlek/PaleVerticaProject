using System.Collections.Generic;
using CoreSystem.Data.Items;
using CoreSystem.UI.Socket;
using CoreSystem.UI.Utility;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class ItemInsertRegistryTests
    {
        #region [Variables/Fields]
        private readonly List<Object> _cleanup = new List<Object>();

        private class FakeInsertTarget : IItemInsertTarget
        {
            public bool Accept = true;
            public ItemInstance Inserted;
            public int InsertedSlot = int.MinValue;
            public int ClearedSlot = int.MinValue;
            public string Label = "Insert to Fake";

            public bool CanInsert(ItemInstance item, int slotIndex) => Accept && item != null;

            public bool TryInsert(ItemInstance item, int slotIndex)
            {
                if (!CanInsert(item, slotIndex)) return false;
                Inserted = item;
                InsertedSlot = slotIndex;
                return true;
            }

            public void ClearInsert(int slotIndex) => ClearedSlot = slotIndex;

            public string GetInsertLabel(ItemInstance item) => Label;
        }

        private class FakeProvider : IItemProvider
        {
            public ItemInstance Item;
            public ItemInstance GetItem() => Item;
            public Sprite GetIcon() => null;
        }
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            ItemInsertRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _cleanup.Count; i++)
            {
                if (_cleanup[i] != null) Object.DestroyImmediate(_cleanup[i]);
            }
            _cleanup.Clear();
            ItemInsertRegistry.Clear();
        }

        [Test]
        public void Register_IsIdempotentAndRaisesChange()
        {
            var target = new FakeInsertTarget();
            int changes = 0;
            System.Action onChange = () => changes++;
            ItemInsertRegistry.OnTargetsChanged += onChange;
            try
            {
                ItemInsertRegistry.Register(target);
                ItemInsertRegistry.Register(target);
                Assert.AreEqual(1, ItemInsertRegistry.Count);

                ItemInsertRegistry.Unregister(target);
                ItemInsertRegistry.Unregister(target);
                Assert.AreEqual(0, ItemInsertRegistry.Count);
                Assert.AreEqual(2, changes);
            }
            finally
            {
                ItemInsertRegistry.OnTargetsChanged -= onChange;
            }
        }

        [Test]
        public void GetTargetsFor_OnlyReturnsTargetsThatAccept()
        {
            var accepts = new FakeInsertTarget();
            var refuses = new FakeInsertTarget { Accept = false };
            ItemInsertRegistry.Register(accepts);
            ItemInsertRegistry.Register(refuses);

            var item = new ItemInstance(1, 0);
            List<IItemInsertTarget> targets = ItemInsertRegistry.GetTargetsFor(item);

            Assert.AreEqual(1, targets.Count);
            Assert.AreSame(accepts, targets[0]);
            Assert.AreEqual(0, ItemInsertRegistry.GetTargetsFor(null).Count);
        }

        [Test]
        public void BuildContextMenuItems_InsertsIntoAnySlot()
        {
            var target = new FakeInsertTarget();
            ItemInsertRegistry.Register(target);
            var item = new ItemInstance(2, 0);

            List<ContextMenuItem> menu = ItemInsertRegistry.BuildContextMenuItems(item);

            Assert.AreEqual(1, menu.Count);
            Assert.AreEqual(target.Label, menu[0].Label);
            menu[0].OnClick.Invoke();
            Assert.AreSame(item, target.Inserted);
            Assert.AreEqual(ItemInsertRegistry.AnySlot, target.InsertedSlot);
        }

        [Test]
        public void BuildContextMenuItems_EmptyWhenNoTargetOpen()
        {
            Assert.AreEqual(0, ItemInsertRegistry.BuildContextMenuItems(new ItemInstance(3, 0)).Count);
        }

        [Test]
        public void ResolveItem_ReadsInstanceAndProvider()
        {
            var item = new ItemInstance(4, 0);
            Assert.AreSame(item, ItemInsertSlot.ResolveItem(item));
            Assert.AreSame(item, ItemInsertSlot.ResolveItem(new FakeProvider { Item = item }));
            Assert.IsNull(ItemInsertSlot.ResolveItem("not an item"));
            Assert.IsNull(ItemInsertSlot.ResolveItem(null));
        }

        [Test]
        public void SocketWindow_RegistersWhileEnabledAndStartsEmpty()
        {
            var go = new GameObject("Socket Window Test");
            _cleanup.Add(go);
            var window = go.AddComponent<SocketWindow>();

            InvokeLifecycle(window, "OnEnable");
            Assert.AreEqual(1, ItemInsertRegistry.Count);
            Assert.IsNull(window.TargetEquipment);
            window.OnOpen(null);
            Assert.IsNull(window.TargetEquipment);

            InvokeLifecycle(window, "OnDisable");
            Assert.AreEqual(0, ItemInsertRegistry.Count);
        }

        [Test]
        public void SocketWindow_EquipmentGoesToCenterOnly()
        {
            var go = new GameObject("Socket Window Test");
            _cleanup.Add(go);
            var window = go.AddComponent<SocketWindow>();
            var equipment = new EquipmentInstance(5, 0);

            Assert.IsTrue(window.CanInsert(equipment, ItemInsertRegistry.AnySlot));
            Assert.IsTrue(window.CanInsert(equipment, SocketWindow.CenterSlot));
            Assert.IsFalse(window.CanInsert(equipment, 0));

            Assert.IsTrue(window.TryInsert(equipment, SocketWindow.CenterSlot));
            Assert.AreSame(equipment, window.TargetEquipment);

            window.ClearInsert(SocketWindow.CenterSlot);
            Assert.IsNull(window.TargetEquipment);
        }

        [Test]
        public void SocketWindow_NoSocketsWithoutTemplate()
        {
            Assert.AreEqual(0, SocketWindow.GetUnlockedSocketCount(null));
        }
        #endregion

        #region [Private Methods]
        private static void InvokeLifecycle(MonoBehaviour target, string methodName)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            target.GetType().GetMethod(methodName, flags).Invoke(target, null);
        }
        #endregion
    }
}
