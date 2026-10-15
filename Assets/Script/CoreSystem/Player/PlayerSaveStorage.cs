using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.UI.Hotbar;
using UnityEngine;

namespace CoreSystem.Player
{
    public static class PlayerSaveStorage
    {
        #region [Public Methods]
        public static PlayerDataModel Capture(Character character)
        {
            PlayerDataModel model = new PlayerDataModel();

            if (character.attributeSystem != null)
            {
                var baseLvlAttr = character.attributeSystem.GetAttribute(AttributeType.BaseLevel);
                if (baseLvlAttr != null) model.Stats.BaseLevel = (int)baseLvlAttr.BaseValue;

                var jobLvlAttr = character.attributeSystem.GetAttribute(AttributeType.JobLevel);
                if (jobLvlAttr != null) model.Stats.JobLevel = (int)jobLvlAttr.BaseValue;

                var strAttr = character.attributeSystem.GetAttribute(AttributeType.STR);
                if (strAttr != null) model.Stats.BaseSTR = strAttr.BaseValue;

                var agiAttr = character.attributeSystem.GetAttribute(AttributeType.AGI);
                if (agiAttr != null) model.Stats.BaseAGI = agiAttr.BaseValue;

                var vitAttr = character.attributeSystem.GetAttribute(AttributeType.VIT);
                if (vitAttr != null) model.Stats.BaseVIT = vitAttr.BaseValue;

                var intAttr = character.attributeSystem.GetAttribute(AttributeType.INT);
                if (intAttr != null) model.Stats.BaseINT = intAttr.BaseValue;

                var dexAttr = character.attributeSystem.GetAttribute(AttributeType.DEX);
                if (dexAttr != null) model.Stats.BaseDEX = dexAttr.BaseValue;

                var lukAttr = character.attributeSystem.GetAttribute(AttributeType.LUK);
                if (lukAttr != null) model.Stats.BaseLUK = lukAttr.BaseValue;
            }

            if (character.expSystem != null)
            {
                model.Stats.CurrentBaseExp = character.expSystem.CurrentBaseExp;
                model.Stats.CurrentJobExp = character.expSystem.CurrentJobExp;
            }

            model.Stats.PassivePoints = character.PassivePoints;

            var allInstances = GameData.Player.Inventory.GetAllItems();
            foreach (var inst in allInstances)
            {
                if (inst == null) continue;

                var record = new InventoryItemRecord
                {
                    InstanceId = inst.Id,
                    ItemId = inst.ItemId,
                    Quantity = inst.Quantity
                };

                if (inst is EquipmentInstance eqInst)
                {
                    record.Refine = eqInst.Refine;
                    record.Reforge = eqInst.Reforge;
                    if (eqInst.SocketedFragments != null)
                    {
                        foreach (var f in eqInst.SocketedFragments)
                        {
                            if (f != null) record.SocketedFragmentIds.Add(f.Id);
                        }
                    }
                }

                for (int i = 0; i < character.EquipmentSlots.Length; i++)
                {
                    if (character.EquipmentSlots[i] == inst)
                    {
                        record.EquippedSlotIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < character.CostumeSlots.Length; i++)
                {
                    if (character.CostumeSlots[i] == inst)
                    {
                        record.CostumeSlotIndex = i;
                        break;
                    }
                }

                model.InventoryItems.Add(record);
            }

            if (character.skillCaster != null)
            {
                foreach (var granted in character.skillCaster.GrantedSkills)
                {
                    if (granted == null || granted.SkillData == null) continue;

                    var skillRec = new SkillRecord
                    {
                        SkillId = granted.SkillData.Id,
                        EquippedSlotIndex = character.skillCaster.EquippedSkills.IndexOf(granted)
                    };

                    if (granted.LinkedModifiers != null)
                    {
                        foreach (var mod in granted.LinkedModifiers)
                        {
                            if (mod != null) skillRec.LinkedSupportSkillIds.Add(mod.ID);
                        }
                    }

                    model.Skills.Add(skillRec);
                }
            }

            if (character.PassiveNodes != null)
            {
                foreach (var p in character.PassiveNodes)
                {
                    if (p != null) model.PassiveNodes.Add(new PassiveNodeRecord(p.NodeId, p.AttributeId, p.Value));
                }
            }

            if (character.UniquePassives != null)
            {
                foreach (var id in character.UniquePassives.ActivePassiveIds)
                {
                    model.ActiveUniquePassiveIds.Add(id);
                }
            }

            if (HotbarManager.Instance != null)
            {
                model.Hotbars = HotbarManager.Instance.SaveData();
            }

            return model;
        }

        public static void Restore(Character character, PlayerDataModel data)
        {
            if (character == null || data == null) return;

            GameData.Player.Data = data;

            if (data.Stats != null && character.attributeSystem != null)
            {
                var baseLvl = character.attributeSystem.GetAttribute(AttributeType.BaseLevel);
                if (baseLvl != null) baseLvl.BaseValue = data.Stats.BaseLevel;

                var jobLvl = character.attributeSystem.GetAttribute(AttributeType.JobLevel);
                if (jobLvl != null) jobLvl.BaseValue = data.Stats.JobLevel;

                var str = character.attributeSystem.GetAttribute(AttributeType.STR);
                if (str != null) str.BaseValue = data.Stats.BaseSTR;

                var agi = character.attributeSystem.GetAttribute(AttributeType.AGI);
                if (agi != null) agi.BaseValue = data.Stats.BaseAGI;

                var vit = character.attributeSystem.GetAttribute(AttributeType.VIT);
                if (vit != null) vit.BaseValue = data.Stats.BaseVIT;

                var intAttr = character.attributeSystem.GetAttribute(AttributeType.INT);
                if (intAttr != null) intAttr.BaseValue = data.Stats.BaseINT;

                var dex = character.attributeSystem.GetAttribute(AttributeType.DEX);
                if (dex != null) dex.BaseValue = data.Stats.BaseDEX;

                var luk = character.attributeSystem.GetAttribute(AttributeType.LUK);
                if (luk != null) luk.BaseValue = data.Stats.BaseLUK;

                character.attributeSystem.UpdateDerivedAttributes();

                if (character.expSystem != null)
                {
                    character.expSystem.SetExp(data.Stats.CurrentBaseExp, data.Stats.CurrentJobExp);
                }

                character.PassivePoints = data.Stats.PassivePoints;
            }

            if (character.PassiveNodes != null)
            {
                character.PassiveNodes.Clear();
                if (data.PassiveNodes != null)
                    character.PassiveNodes.AddRange(data.PassiveNodes);
            }

            if (character.UniquePassives != null && data.ActiveUniquePassiveIds != null)
            {
                character.UniquePassives.Clear();
                foreach (var id in data.ActiveUniquePassiveIds)
                {
                    character.UniquePassives.AddPassive(id);
                }
            }

            character.RecalculateStats();

            var inventory = GameData.Player.Inventory;
            inventory.Clear();

            for (int i = 0; i < character.EquipmentSlots.Length; i++) character.EquipmentSlots[i] = null;
            for (int i = 0; i < character.CostumeSlots.Length; i++) character.CostumeSlots[i] = null;

            if (data.InventoryItems != null)
            {
                foreach (var rec in data.InventoryItems)
                {
                    Item template = Database.GetItem(rec.ItemId);
                    if (template == null) continue;

                    ItemInstance instance = null;
                    if (template is Equipment)
                    {
                        var eq = new EquipmentInstance(rec.InstanceId, rec.ItemId, rec.Quantity)
                        {
                            Refine = rec.Refine,
                            Reforge = rec.Reforge
                        };

                        if (rec.SocketedFragmentIds != null)
                        {
                            foreach (var fragId in rec.SocketedFragmentIds)
                            {
                                Item fragItem = Database.GetItem(fragId);
                                if (fragItem is Fragment fragment)
                                {
                                    eq.SocketedFragments.Add(fragment);
                                }
                            }
                        }

                        instance = eq;
                    }
                    else if (template is Costume)
                    {
                        instance = new CostumeInstance(rec.InstanceId, rec.ItemId, rec.Quantity);
                    }
                    else
                    {
                        instance = new ItemInstance(rec.InstanceId, rec.ItemId, rec.Quantity);
                    }

                    inventory.AddDirectInstance(instance);

                    if (rec.EquippedSlotIndex >= 0 && instance is EquipmentInstance eqInst)
                    {
                        GameData.Player.Equipment.Equip(rec.EquippedSlotIndex, eqInst);
                    }
                    else if (rec.CostumeSlotIndex >= 0 && instance is CostumeInstance costumeInst)
                    {
                        GameData.Player.Costumes.Equip(costumeInst);
                    }
                }
            }

            if (character.skillCaster != null && data.Skills != null)
            {
                character.skillCaster.RevokeAllSkills();
                foreach (var sRec in data.Skills)
                {
                    ActiveSkill skillData = Database.GetSynchronizedSkill(sRec.SkillId);
                    if (skillData == null) continue;

                    SkillInstance granted = character.skillCaster.GrantSkill(skillData);
                    if (granted != null && sRec.LinkedSupportSkillIds != null)
                    {
                        foreach (var suppId in sRec.LinkedSupportSkillIds)
                        {
                            var suppMod = Database.GetSynchronizedSupport(suppId);
                            if (suppMod != null)
                            {
                                granted.LinkModifier(suppMod);
                            }
                        }
                    }

                    if (granted != null && sRec.EquippedSlotIndex >= 0)
                    {
                        character.skillCaster.EquipSkill(granted, sRec.EquippedSlotIndex);
                    }
                }
            }

            if (HotbarManager.Instance != null && data.Hotbars != null)
            {
                HotbarManager.Instance.LoadData(data.Hotbars);
            }

            Debug.Log($"[PlayerSaveStorage] Successfully loaded PlayerDataModel (Slot: {data.Meta.SlotIndex})");
        }
        #endregion
    }
}
