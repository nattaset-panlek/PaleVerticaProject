using System.Collections.Generic;
using CoreSystem.Data.Save;
using CoreSystem.UI.Hotbar;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class HotbarSaveMappingTests
    {
        #region [Variables/Fields]
        private const int SlotCount = 10;
        private static readonly string[] ExpectedLabels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void HotkeyLabels_FollowSlotPosition()
        {
            for (int s = 0; s < SlotCount; s++)
            {
                Assert.AreEqual(ExpectedLabels[s], HotbarLayoutFormula.GetHotkeyLabel(0, s));
            }
        }

        [Test]
        public void SaveThenLoad_AfterSwap_KeepsSkillsAndLabelsAtTheirSlots()
        {
            var slots = CreateEmptyRow();
            slots[0].SetSkill(100);
            slots[6].SetSkill(700);
            slots[3].SetItem(55, 9);

            Swap(slots, 0, 6);

            HotbarRowRecord loaded = RoundTrip(slots);
            HotbarSlotData[] restored = HotbarLayoutFormula.FromRecords(loaded.Slots, loaded.SlotCount);

            Assert.AreEqual(SlotCount, restored.Length);
            Assert.AreEqual(700, restored[0].SkillId);
            Assert.AreEqual(100, restored[6].SkillId);
            Assert.AreEqual(HotbarActionType.Item, restored[3].ActionType);
            Assert.AreEqual(55, restored[3].ItemId);
            Assert.AreEqual(9L, restored[3].ItemInstanceId);
            Assert.IsTrue(restored[5].IsEmpty);

            for (int s = 0; s < SlotCount; s++)
            {
                Assert.AreEqual(ExpectedLabels[s], loaded.Slots[s].HotkeyText);
            }
        }

        [Test]
        public void Load_IgnoresStaleHotkeyTextInOldSave()
        {
            var record = new HotbarRowRecord { SlotCount = SlotCount };
            for (int s = 0; s < SlotCount; s++)
            {
                record.Slots.Add(new HotbarSlotRecord { HotkeyText = ExpectedLabels[s] });
            }
            record.Slots[0].HotkeyText = "7";
            record.Slots[0].ActionType = (int)HotbarActionType.Skill;
            record.Slots[0].SkillId = 700;
            record.Slots[6].HotkeyText = "1";

            HotbarSlotData[] restored = HotbarLayoutFormula.FromRecords(record.Slots, SlotCount);
            List<HotbarSlotRecord> resaved = HotbarLayoutFormula.ToRecords(restored, 0);

            Assert.AreEqual(700, restored[0].SkillId);
            Assert.AreEqual("1", resaved[0].HotkeyText);
            Assert.AreEqual("7", resaved[6].HotkeyText);
        }

        [Test]
        public void Load_PadsMissingSlotsWithEmpty()
        {
            var record = new HotbarRowRecord { SlotCount = SlotCount };
            record.Slots.Add(new HotbarSlotRecord { ActionType = (int)HotbarActionType.Skill, SkillId = 1 });

            HotbarSlotData[] restored = HotbarLayoutFormula.FromRecords(record.Slots, SlotCount);

            Assert.AreEqual(1, restored[0].SkillId);
            for (int s = 1; s < SlotCount; s++)
            {
                Assert.IsTrue(restored[s].IsEmpty);
            }
        }
        #endregion

        #region [Private Methods]
        private static HotbarSlotData[] CreateEmptyRow()
        {
            var slots = new HotbarSlotData[SlotCount];
            for (int s = 0; s < SlotCount; s++)
            {
                slots[s] = new HotbarSlotData();
            }
            return slots;
        }

        private static void Swap(HotbarSlotData[] slots, int a, int b)
        {
            HotbarSlotData temp = slots[a];
            slots[a] = slots[b];
            slots[b] = temp;
        }

        private static HotbarRowRecord RoundTrip(HotbarSlotData[] slots)
        {
            var record = new HotbarRowRecord
            {
                RowIndex = 0,
                SlotCount = slots.Length,
                Slots = HotbarLayoutFormula.ToRecords(slots, 0)
            };
            string json = JsonUtility.ToJson(record);
            return JsonUtility.FromJson<HotbarRowRecord>(json);
        }
        #endregion
    }
}
