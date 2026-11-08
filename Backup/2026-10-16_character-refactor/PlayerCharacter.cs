using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Save;
using CoreSystem.Skills;
using CoreSystem.UI.Hotbar;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace CoreSystem.Entity
{
    public class PlayerCharacter : Character
    {
        public string Name => "Player Character";
        [Header("Debug UI")]
        public bool ShowDebugUI = true;
        private CoreSystem.UI.DevTools.GameDevToolUI _devToolUI;

        public override void Initialize()
        {
            base.Initialize();
            
            // Connect global Player Managers to this specific instance
            GameData.Player.PlayerCharacterModel = this;
            GameData.Player.Stats.Bind(this.attributeSystem);
            GameData.Player.Skills.Bind(this.skillCaster);
            GameData.Player.Equipment.Bind(this);
            GameData.Player.Costumes.Bind(this);

            _devToolUI = new CoreSystem.UI.DevTools.GameDevToolUI(this);

            // Screen Shake on Take Damage
            OnTakeDamage += (attacker, damage) => {
                CoreSystem.Controllers.CameraController.Instance?.ScreenShake(0.15f, 0.4f);
            };
        }

        protected override void Update()
        {
            base.Update();
            
            if (ShowDebugUI)
            {
                _devToolUI?.Update();
            }
        }

        [ContextMenu("RecalculateStats")]
        public override void RecalculateStats()
        {
            base.RecalculateStats();
            ApplyPassiveRecords();
        }

        private void ApplyPassiveRecords()
        {
            foreach (var attr in attributeSystem.ActiveAttributes.Values)
                attr.Additional.RemoveByTag("Passive");

            foreach (var rec in PassiveNodes)
            {
                if (!System.Enum.IsDefined(typeof(AttributeType), rec.AttributeId)) continue;
                var type = (AttributeType)rec.AttributeId;
                attributeSystem.AddAdditional(type, 2, Attributes.AdditionalOp.Add, rec.Value, "Passive");
            }
        }

        #region Public Methods — Save & Load
        public PlayerDataModel SaveData()
        {
            PlayerDataModel model = new PlayerDataModel();

            if (attributeSystem != null)
            {
                var baseLvlAttr = attributeSystem.GetAttribute(AttributeType.BaseLevel);
                if (baseLvlAttr != null) model.Stats.BaseLevel = (int)baseLvlAttr.BaseValue;

                var jobLvlAttr = attributeSystem.GetAttribute(AttributeType.JobLevel);
                if (jobLvlAttr != null) model.Stats.JobLevel = (int)jobLvlAttr.BaseValue;

                var strAttr = attributeSystem.GetAttribute(AttributeType.STR);
                if (strAttr != null) model.Stats.BaseSTR = strAttr.BaseValue;

                var agiAttr = attributeSystem.GetAttribute(AttributeType.AGI);
                if (agiAttr != null) model.Stats.BaseAGI = agiAttr.BaseValue;

                var vitAttr = attributeSystem.GetAttribute(AttributeType.VIT);
                if (vitAttr != null) model.Stats.BaseVIT = vitAttr.BaseValue;

                var intAttr = attributeSystem.GetAttribute(AttributeType.INT);
                if (intAttr != null) model.Stats.BaseINT = intAttr.BaseValue;

                var dexAttr = attributeSystem.GetAttribute(AttributeType.DEX);
                if (dexAttr != null) model.Stats.BaseDEX = dexAttr.BaseValue;

                var lukAttr = attributeSystem.GetAttribute(AttributeType.LUK);
                if (lukAttr != null) model.Stats.BaseLUK = lukAttr.BaseValue;
            }

            if (expSystem != null)
            {
                model.Stats.CurrentBaseExp = expSystem.CurrentBaseExp;
                model.Stats.CurrentJobExp = expSystem.CurrentJobExp;
            }

            model.Stats.PassivePoints = PassivePoints;

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

                for (int i = 0; i < EquipmentSlots.Length; i++)
                {
                    if (EquipmentSlots[i] == inst)
                    {
                        record.EquippedSlotIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < CostumeSlots.Length; i++)
                {
                    if (CostumeSlots[i] == inst)
                    {
                        record.CostumeSlotIndex = i;
                        break;
                    }
                }

                model.InventoryItems.Add(record);
            }

            if (skillCaster != null)
            {
                foreach (var granted in skillCaster.GrantedSkills)
                {
                    if (granted == null || granted.SkillData == null) continue;

                    var skillRec = new SkillRecord
                    {
                        SkillId = granted.SkillData.Id,
                        EquippedSlotIndex = skillCaster.EquippedSkills.IndexOf(granted)
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

            if (PassiveNodes != null)
            {
                foreach (var p in PassiveNodes)
                {
                    if (p != null) model.PassiveNodes.Add(new PassiveNodeRecord(p.NodeId, p.AttributeId, p.Value));
                }
            }

            if (UniquePassives != null)
            {
                foreach (var id in UniquePassives.ActivePassiveIds)
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

        [ContextMenu("Call LoadData")]
        public void LoadData()
        {
            LoadData(GameData.Player.Data);
        }

        public void LoadData(PlayerDataModel data)
        {
            if (data == null) return;

            GameData.Player.Data = data;

            if (data.Stats != null && attributeSystem != null)
            {
                var baseLvl = attributeSystem.GetAttribute(AttributeType.BaseLevel);
                if (baseLvl != null) baseLvl.BaseValue = data.Stats.BaseLevel;

                var jobLvl = attributeSystem.GetAttribute(AttributeType.JobLevel);
                if (jobLvl != null) jobLvl.BaseValue = data.Stats.JobLevel;

                var str = attributeSystem.GetAttribute(AttributeType.STR);
                if (str != null) str.BaseValue = data.Stats.BaseSTR;

                var agi = attributeSystem.GetAttribute(AttributeType.AGI);
                if (agi != null) agi.BaseValue = data.Stats.BaseAGI;

                var vit = attributeSystem.GetAttribute(AttributeType.VIT);
                if (vit != null) vit.BaseValue = data.Stats.BaseVIT;

                var intAttr = attributeSystem.GetAttribute(AttributeType.INT);
                if (intAttr != null) intAttr.BaseValue = data.Stats.BaseINT;

                var dex = attributeSystem.GetAttribute(AttributeType.DEX);
                if (dex != null) dex.BaseValue = data.Stats.BaseDEX;

                var luk = attributeSystem.GetAttribute(AttributeType.LUK);
                if (luk != null) luk.BaseValue = data.Stats.BaseLUK;

                attributeSystem.UpdateDerivedAttributes();

                if (expSystem != null)
                {
                    expSystem.SetExp(data.Stats.CurrentBaseExp, data.Stats.CurrentJobExp);
                }

                PassivePoints = data.Stats.PassivePoints;
            }

            if (PassiveNodes != null)
            {
                PassiveNodes.Clear();
                if (data.PassiveNodes != null)
                    PassiveNodes.AddRange(data.PassiveNodes);
            }

            if (UniquePassives != null && data.ActiveUniquePassiveIds != null)
            {
                UniquePassives.Clear();
                foreach (var id in data.ActiveUniquePassiveIds)
                {
                    UniquePassives.AddPassive(id);
                }
            }

            RecalculateStats();

            var inventory = GameData.Player.Inventory;
            inventory.Clear();

            for (int i = 0; i < EquipmentSlots.Length; i++) EquipmentSlots[i] = null;
            for (int i = 0; i < CostumeSlots.Length; i++) CostumeSlots[i] = null;

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

            if (skillCaster != null && data.Skills != null)
            {
                skillCaster.RevokeAllSkills();
                foreach (var sRec in data.Skills)
                {
                    ActiveSkill skillData = Database.GetSynchronizedSkill(sRec.SkillId);
                    if (skillData == null) continue;

                    SkillInstance granted = skillCaster.GrantSkill(skillData);
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
                        skillCaster.EquipSkill(granted, sRec.EquippedSlotIndex);
                    }
                }
            }

            if (HotbarManager.Instance != null && data.Hotbars != null)
            {
                HotbarManager.Instance.LoadData(data.Hotbars);
            }

            Debug.Log($"[PlayerCharacter] Successfully loaded PlayerDataModel (Slot: {data.Meta.SlotIndex})");
        }
        #endregion
    }
}
