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
        public override void Initialize()
        {
            base.Initialize();
            
            // Connect global Player Managers to this specific instance
            GameData.Player.PlayerCharacterModel = this;
            GameData.Player.Stats.Bind(this.attributeSystem);
            GameData.Player.Skills.Bind(this.skillCaster);
            GameData.Player.Equipment.Bind(this);
            GameData.Player.Costumes.Bind(this);

            // Screen Shake on Take Damage
            OnTakeDamage += (attacker, damage) => {
                CoreSystem.Controllers.CameraController.Instance?.ScreenShake(0.15f, 0.4f);
            };
        }

        [Header("Debug UI")]
        public bool ShowDebugUI = true;
        private bool _isOpen = false;
        private CoreSystem.UI.UIGlobalModal _modalInstance;
        
        // State variables
        private int _selectedTab = 0;
        private string _searchQuery = "";
        
        // Passive Tab state
        private PassiveNodeRecord _pendingPassive = new PassiveNodeRecord();
        
        // Upgrade/Socket Tab state
        private EquipmentInstance _selectedUpgradeItem;
        private EquipmentInstance _selectedSocketItem;
        private ItemInstance _selectedFragment;
        
        // Skill Tab state
        private SkillInstance _selectedSkillForSupports;
        private const int MaxLinkedSupportsLimit = -1;

        private Dictionary<string, bool> _dbCategoryFoldout = new Dictionary<string, bool>();

        protected override void Update()
        {
            base.Update();
            
            if (!ShowDebugUI) return;
            
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
            {
                if (_isOpen) CloseModal();
                else OpenModal();
            }
        }

        private void OpenModal()
        {
            if (_isOpen) return;

            _modalInstance = CoreSystem.UI.UIBuilder.BuildGlobalModal("Dev Tool - Debug Menu", modal =>
            {
                BuildTabs(modal, this);
                BuildContent(modal, this);
            }, new Vector2(800, 600));

            _isOpen = _modalInstance != null;
        }

        private void CloseModal()
        {
            if (_modalInstance != null)
            {
                CoreSystem.UI.UIManager.Instance.Close(_modalInstance.WindowName);
                _modalInstance = null;
            }
            _isOpen = false;
        }

        private void RequestRebuild()
        {
            if (_isOpen && _modalInstance != null)
            {
                _modalInstance.ClearContent();
                var registry = CoreSystem.UI.UIManager.Instance.ElementRegistry;
                var context = new CoreSystem.UI.UIBuilderContext(_modalInstance.ContentContainer, registry);
                
                BuildTabs(context, this);
                BuildContent(context, this);
            }
        }

        private void BuildTabs(CoreSystem.UI.UIBuilderContext modal, PlayerCharacter player)
        {
            modal.Horizontal("Div", row => 
            {
                string[] tabs = { "DB", "Inv", "Equip", "Socket", "Attr", "Skill", "Upgrade", "Passive", "U-Passive" };
                for (int i = 0; i < tabs.Length; i++)
                {
                    int index = i;
                    string style = _selectedTab == i ? "Button" : "Button";
                    row.Button(style, tabs[i], () => 
                    {
                        _selectedTab = index;
                        RequestRebuild();
                    });
                }
            });

            modal.Horizontal("Div", row => 
            {
                row.Text("Text", "Search:").SetPreferredSize(80, 40).SetTextAlignment(TMPro.TextAlignmentOptions.MidlineLeft);
                row.InputField("InputField", _searchQuery, val => _searchQuery = val).SetPreferredSize(300, 40).SetFlexibleSize(0, -1);
                row.Button("Button", "Clear", () => 
                {
                    _searchQuery = "";
                    RequestRebuild();
                }).SetPreferredSize(80, 40);
                row.Button("Button", "Search", RequestRebuild).SetPreferredSize(120, 40);
            }).SetSpacing(10);
        }

        private void BuildContent(CoreSystem.UI.UIBuilderContext modal, PlayerCharacter player)
        {
            modal.Vertical("Div", content => 
            {
                switch (_selectedTab)
                {
                    case 0: BuildDatabaseTab(content, player); break;
                    case 1: BuildInventoryTab(content, player); break;
                    case 2: BuildEquipmentTab(content, player); break;
                    case 3: BuildSocketingTab(content, player); break;
                    case 4: BuildAttributesTab(content, player); break;
                    case 5: BuildSkillsTab(content, player); break;
                    case 6: BuildUpgradeTab(content, player); break;
                    case 7: BuildPassiveTab(content, player); break;
                    case 8: BuildUniquePassivesTab(content, player); break;
                }
            });
        }

        private void BuildDatabaseTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Horizontal("Div", headerRow => 
            {
                headerRow.Text("Text", "All Possible Items (Database)")
                         .SetFlexibleSize(1, -1)
                         .SetFontStyle(TMPro.FontStyles.Bold)
                         .SetFontSize(20);
                         
                headerRow.Button("Button", "DEBUG: ADD ALL", () => 
                {
                    var allItems = Database.GetItems;
                    foreach (var item in allItems) GameData.Player.Inventory.AddItem(item);
                    RequestRebuild();
                }).SetPreferredSize(200, 40).SetColor(new Color(0.8f, 0.2f, 0.2f));
            });

            var allItemsDb = Database.GetItems;
            var groupedItems = allItemsDb.GroupBy(i => i.Type.ToString());

            foreach (var group in groupedItems)
            {
                string catName = group.Key;
                if (!_dbCategoryFoldout.ContainsKey(catName)) _dbCategoryFoldout[catName] = false;

                bool isExpanded = _dbCategoryFoldout[catName];
                
                // Group Header (Foldout Button)
                ctx.Button("Button", $"{(isExpanded ? "[-]" : "[+]")} {catName} ({group.Count()})", () => 
                {
                    _dbCategoryFoldout[catName] = !isExpanded;
                    RequestRebuild();
                }).SetBackgroundColor(new Color(0.15f, 0.15f, 0.2f, 1f))
                  .SetTextAlignment(TMPro.TextAlignmentOptions.MidlineLeft)
                  .SetPreferredSize(-1, 40);

                if (isExpanded)
                {
                    ctx.Vertical("Div", contentRow => 
                    {
                        foreach (var item in group)
                        {
                            if (!string.IsNullOrEmpty(_searchQuery) && !item.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                            contentRow.Horizontal("Div", itemRow => 
                            {
                                // Label on the left taking all available space
                                itemRow.Text("Text", $"[{item.Id}] {item.Name}")
                                       .SetFlexibleSize(1, -1)
                                       .SetTextAlignment(TMPro.TextAlignmentOptions.MidlineLeft);
                                
                                // Button on the right
                                itemRow.Button("Button", "Collect", () => 
                                {
                                    GameData.Player.Inventory.AddItem(item);
                                    RequestRebuild();
                                }).SetPreferredSize(100, 35);
                                
                            }).SetBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 0.5f)).SetPadding(10, 10, 5, 5);
                        }
                    }).SetPadding(20, 0, 0, 0); // Indent child items to show hierarchy
                }
            }
        }

        private void BuildInventoryTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Player Held Items (Instances) ---");
            var instances = GameData.Player.Inventory.GetAllItems();
            if (instances.Count == 0)
            {
                ctx.Text("Text", "Inventory is empty.");
                return;
            }

            foreach (var instance in instances)
            {
                if (!string.IsNullOrEmpty(_searchQuery) && !instance.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                ctx.Horizontal("Div", row => 
                {
                    row.Text("Text", $"[{instance.Id}] {instance.Name} x{instance.Quantity}");

                    if (instance.Item is Equipment equip)
                    {
                        row.Button("Button", "Equip", () => 
                        {
                            GameData.Player.Equipment.SmartEquip(instance);
                            RequestRebuild();
                        });
                    }
                    else if (instance.Item is Costume costume)
                    {
                        row.Button("Button", "Wear", () => 
                        {
                            GameData.Player.Costumes.Equip(instance);
                            RequestRebuild();
                        });
                    }

                    row.Button("Button", "Discard", () => 
                    {
                        GameData.Player.Inventory.RemoveItemByInstance(instance.Id, instance.Quantity);
                        RequestRebuild();
                    });
                });
            }
        }

        private void BuildEquipmentTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Current Equipment ---");
            for (int i = 0; i < player.EquipmentSlots.Length; i++)
            {
                var instance = player.EquipmentSlots[i];
                int slotIndex = i;
                ctx.Horizontal("Div", row => 
                {
                    row.Text("Text", $"Slot {i}: {(instance != null ? $"[{instance.Id}] {instance.Name}" : "Empty")}");
                    if (instance != null)
                    {
                        row.Button("Button", "X", () => 
                        {
                            GameData.Player.Equipment.Equip(slotIndex, null);
                            RequestRebuild();
                        });
                    }
                });
            }

            ctx.Text("Text", "--- Current Costumes ---");
            for (int i = 0; i < player.CostumeSlots.Length; i++)
            {
                var instance = player.CostumeSlots[i];
                int slotIndex = i;
                ctx.Horizontal("Div", row => 
                {
                    row.Text("Text", $"Slot {(CostumeSlot)i}: {(instance != null ? $"[{instance.Id}] {instance.Name}" : "Empty")}");
                    if (instance != null)
                    {
                        row.Button("Button", "X", () => 
                        {
                            GameData.Player.Costumes.Unequip(slotIndex);
                            RequestRebuild();
                        });
                    }
                });
            }

            ctx.Text("Text", "--- Equipped Skills ---");
            if (player.skillCaster != null)
            {
                for (int i = 0; i < player.skillCaster.EquippedSkills.Count; i++)
                {
                    var skill = player.skillCaster.EquippedSkills[i];
                    int slotIndex = i;
                    ctx.Horizontal("Div", row => 
                    {
                        row.Text("Text", $"Slot {i + 1}: {(skill != null ? (skill.SkillData != null ? skill.SkillData.Name : "Data Missing") : "Empty")}");
                        if (skill != null)
                        {
                            row.Button("Button", "X", () => 
                            {
                                player.skillCaster.EquipSkill((SkillInstance)null, slotIndex);
                                RequestRebuild();
                            });
                        }
                    });
                }
            }
        }

        private void BuildSocketingTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Socketing System ---");

            ctx.Horizontal("Div", row => 
            {
                row.Vertical("Div", leftCol => 
                {
                    leftCol.Text("Text", "Select Equipped Item:");
                    for (int i = 0; i < player.EquipmentSlots.Length; i++)
                    {
                        var inst = player.EquipmentSlots[i] as EquipmentInstance;
                        if (inst == null) continue;
                        
                        string prefix = _selectedSocketItem == inst ? ">> " : "";
                        leftCol.Button("Button", $"{prefix}[{i}] {inst.Name}", () => 
                        {
                            _selectedSocketItem = inst;
                            RequestRebuild();
                        });
                    }
                });

                row.Vertical("Div", rightCol => 
                {
                    rightCol.Text("Text", "Select Fragment from Inventory:");
                    var fragments = GameData.Player.Inventory.GetAllItems().Where(it => it.Item is Fragment).ToList();
                    if (fragments.Count == 0)
                    {
                        rightCol.Text("Text", "No fragments in inventory.");
                    }
                    else
                    {
                        foreach (var frag in fragments)
                        {
                            string prefix = _selectedFragment == frag ? ">> " : "";
                            rightCol.Button("Button", $"{prefix}{frag.Name} (x{frag.Quantity})", () => 
                            {
                                _selectedFragment = frag;
                                RequestRebuild();
                            });
                        }
                    }
                });
            });

            if (_selectedSocketItem != null)
            {
                var equip = _selectedSocketItem.Item as Equipment;
                ctx.Vertical("Div", actionCol => 
                {
                    actionCol.Text("Text", $"Applying to: {_selectedSocketItem.Name}");
                    actionCol.Text("Text", $"Sockets: {_selectedSocketItem.SocketedFragments.Count} / {equip.MaxSockets}");
                    
                    foreach(var f in _selectedSocketItem.SocketedFragments)
                    {
                        actionCol.Text("Text", $"  - {f.Name}");
                    }

                    if (_selectedFragment != null)
                    {
                        actionCol.Button("Button", $"SOCKET {_selectedFragment.Name} into {_selectedSocketItem.Name}", () => 
                        {
                            GameData.Player.Equipment.SocketFragment(_selectedSocketItem, _selectedFragment);
                            if (_selectedFragment.Quantity <= 0) _selectedFragment = null;
                            RequestRebuild();
                        });
                    }
                });
            }
        }

        private void BuildAttributesTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Primary Attributes (Adjustable) ---");
            if (player.attributeSystem != null)
            {
                AttributeType[] primaryTypes = { 
                    AttributeType.STR, AttributeType.AGI, AttributeType.VIT, 
                    AttributeType.INT, AttributeType.DEX, AttributeType.LUK 
                };

                foreach (var type in primaryTypes)
                {
                    var attr = player.attributeSystem.GetAttribute(type);
                    if (attr != null)
                    {
                        ctx.Horizontal("Div", row => 
                        {
                            row.Text("Text", $"{type}: {attr.CurrentValue:F0} (Base: {attr.BaseValue:F0})");
                            row.Button("Button", "-", () => 
                            {
                                attr.BaseValue = Mathf.Max(1, attr.BaseValue - 1);
                                player.attributeSystem.UpdateDerivedAttributes();
                                RequestRebuild();
                            });
                            row.Button("Button", "+", () => 
                            {
                                attr.BaseValue += 1;
                                player.attributeSystem.UpdateDerivedAttributes();
                                RequestRebuild();
                            });
                            row.Button("Button", "+10", () => 
                            {
                                attr.BaseValue += 10;
                                player.attributeSystem.UpdateDerivedAttributes();
                                RequestRebuild();
                            });
                        });
                    }
                }

                ctx.Text("Text", "--- Experience Controls ---");
                int blvl = (int)player.attributeSystem.GetAttribute(AttributeType.BaseLevel).BaseValue;
                long nextBase = player.expSystem.GetNextBaseExp(blvl);
                ctx.Text("Text", $"Base Exp: {player.expSystem.CurrentBaseExp} / {nextBase} (Lvl {blvl})");
                
                ctx.Horizontal("Div", row => 
                {
                    row.Button("Button", "+100 Base Exp", () => { player.expSystem.AddBaseExp(100); RequestRebuild(); });
                    row.Button("Button", "+1000 Base Exp", () => { player.expSystem.AddBaseExp(1000); RequestRebuild(); });
                });

                int jlvl = (int)player.attributeSystem.GetAttribute(AttributeType.JobLevel).BaseValue;
                long nextJob = player.expSystem.GetNextJobExp(jlvl);
                ctx.Text("Text", $"Job Exp: {player.expSystem.CurrentJobExp} / {nextJob} (Lvl {jlvl})");
                
                ctx.Horizontal("Div", row => 
                {
                    row.Button("Button", "+100 Job Exp", () => { player.expSystem.AddJobExp(100); RequestRebuild(); });
                    row.Button("Button", "+1000 Job Exp", () => { player.expSystem.AddJobExp(1000); RequestRebuild(); });
                });
            }
        }

        private void BuildSkillsTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Player Held Skills (Granted) ---");
            foreach (var granted in player.skillCaster.GrantedSkills)
            {
                ctx.Horizontal("Div", row => 
                {
                    bool isSelected = (_selectedSkillForSupports == granted);
                    string prefix = isSelected ? ">> " : "";
                    row.Text("Text", $"{prefix}[{granted.SkillData.Id}] {granted.SkillData.Name}");
                    
                    row.Button("Button", "Eq 1", () => { player.skillCaster.EquipSkill(granted, 0); RequestRebuild(); });
                    row.Button("Button", "Eq 2", () => { player.skillCaster.EquipSkill(granted, 1); RequestRebuild(); });
                    row.Button("Button", "Eq 3", () => { player.skillCaster.EquipSkill(granted, 2); RequestRebuild(); });
                    
                    row.Button("Button", "Select", () => 
                    {
                        _selectedSkillForSupports = isSelected ? null : granted;
                        RequestRebuild();
                    });
                    
                    row.Button("Button", "Revoke", () => 
                    {
                        player.skillCaster.RevokeSkill(granted.SkillData);
                        if (_selectedSkillForSupports == granted) _selectedSkillForSupports = null;
                        RequestRebuild();
                    });
                });
            }

            ctx.Text("Text", "--- All Possible Skills ---");
            var skills = Database.Skills;
            foreach (var skill in skills)
            {
                if (!string.IsNullOrEmpty(_searchQuery) && !skill.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                ctx.Horizontal("Div", row => 
                {
                    row.Text("Text", $"[{skill.Id}] {skill.Name} ({skill.SkillElement})");
                    row.Button("Button", "Grant", () => 
                    {
                        player.skillCaster.GrantSkill(skill);
                        RequestRebuild();
                    });
                });
            }
        }

        private void BuildUpgradeTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Equipment Upgrade System ---");
            ctx.Horizontal("Div", row => 
            {
                row.Vertical("Div", leftCol => 
                {
                    leftCol.Text("Text", "Your Equipments:");
                    var invItems = GameData.Player.Inventory.GetAllItems();
                    var equipInstances = invItems.Where(it => it is EquipmentInstance).Cast<EquipmentInstance>().ToList();

                    foreach (var inst in equipInstances)
                    {
                        if (!string.IsNullOrEmpty(_searchQuery) && !inst.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                        string prefix = (_selectedUpgradeItem == inst) ? ">> " : "";
                        leftCol.Button("Button", $"{prefix}[{inst.Refine}] {inst.Name}", () => 
                        {
                            _selectedUpgradeItem = inst;
                            RequestRebuild();
                        });
                    }
                });

                row.Vertical("Div", rightCol => 
                {
                    if (_selectedUpgradeItem != null)
                    {
                        rightCol.Text("Text", $"UPGRADING: {_selectedUpgradeItem.Name}");
                        
                        int maxRefine = EquipmentDatabase.RefineTable.Length - 1;
                        rightCol.Horizontal("Div", statRow => 
                        {
                            statRow.Text("Text", $"Refine Level: {_selectedUpgradeItem.Refine} / {maxRefine}");
                            statRow.Button("Button", "Reset", () => { _selectedUpgradeItem.Refine = 0; RequestRebuild(); });
                        });

                        float currentMult = EquipmentDatabase.GetRefineMultiplier(_selectedUpgradeItem.Refine) * 100f;
                        rightCol.Text("Text", $"Current Bonus: +{currentMult:F0}% Base Stats");

                        rightCol.Button("Button", "REFINE (+1)", () => 
                        {
                            _selectedUpgradeItem.Refine = Mathf.Min(maxRefine, _selectedUpgradeItem.Refine + 1);
                            if (_selectedUpgradeItem.IsEquipped)
                            {
                                (_selectedUpgradeItem.Item as Equipment)?.ForceRefresh(player, _selectedUpgradeItem);
                            }
                            RequestRebuild();
                        });
                    }
                });
            });
        }

        private void BuildPassiveTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "--- Passive Point Settings ---");
            ctx.Text("Text", $"Current Passive Points: {player.PassivePoints}");
            
            ctx.Horizontal("Div", row => 
            {
                row.Button("Button", "+1 PT", () => { player.PassivePoints++; RequestRebuild(); });
                row.Button("Button", "+10 PT", () => { player.PassivePoints += 10; RequestRebuild(); });
                row.Button("Button", "RESET ALL", () => 
                {
                    player.PassivePoints += player.PassiveNodes.Count;
                    player.PassiveNodes.Clear();
                    player.RecalculateStats();
                    PassiveSkillLoader.Instance?.LoadFromCharacter(player);
                    RequestRebuild();
                });
            });

            ctx.Text("Text", "--- Add Passive Record ---");
            ctx.Horizontal("Div", row => 
            {
                row.Text("Text", "NodeId");
                row.InputField("InputField", _pendingPassive.NodeId.ToString(), val => { if(int.TryParse(val, out int v)) _pendingPassive.NodeId = v; });
                row.Text("Text", "AttrId");
                row.InputField("InputField", _pendingPassive.AttributeId.ToString(), val => { if(int.TryParse(val, out int v)) _pendingPassive.AttributeId = v; });
                row.Text("Text", "Val");
                row.InputField("InputField", _pendingPassive.Value.ToString(), val => { if(float.TryParse(val, out float v)) _pendingPassive.Value = v; });
            });

            ctx.Button("Button", "Unlock & Apply", () => 
            {
                if (player.PassivePoints > 0)
                {
                    player.PassiveNodes.Add(new PassiveNodeRecord(_pendingPassive.NodeId, _pendingPassive.AttributeId, _pendingPassive.Value));
                    player.PassivePoints--;
                    player.RecalculateStats();
                    RequestRebuild();
                }
            });

            ctx.Text("Text", $"Unlocked Records: {player.PassiveNodes.Count}");
            foreach (var rec in player.PassiveNodes.ToList())
            {
                ctx.Horizontal("Div", row => 
                {
                    row.Text("Text", $"Node:{rec.NodeId} Attr:{(AttributeType)rec.AttributeId} Val:{rec.Value}");
                    row.Button("Button", "X", () => 
                    {
                        player.PassiveNodes.Remove(rec);
                        player.PassivePoints++;
                        player.RecalculateStats();
                        RequestRebuild();
                    });
                });
            }
        }

        private void BuildUniquePassivesTab(CoreSystem.UI.UIBuilderContext ctx, PlayerCharacter player)
        {
            ctx.Text("Text", "Unique Passive Settings");
            var passives = Database.UniquePassives;
            foreach (var passive in passives)
            {
                if (!string.IsNullOrEmpty(_searchQuery) && 
                    !passive.Name.ToLower().Contains(_searchQuery.ToLower()) && 
                    !passive.Id.ToLower().Contains(_searchQuery.ToLower()))
                {
                    continue;
                }

                ctx.Horizontal("Div", row => 
                {
                    row.Text("Text", $"{passive.Name} ({passive.Id})");
                    bool isActive = player.UniquePassives.HasPassive(passive.Id);
                    
                    if (isActive)
                    {
                        row.Button("Button", "Remove", () => 
                        {
                            player.UniquePassives.RemovePassive(passive.Id);
                            player.RecalculateStats();
                            RequestRebuild();
                        });
                    }
                    else
                    {
                        row.Button("Button", "Add", () => 
                        {
                            player.UniquePassives.AddPassive(passive.Id);
                            player.RecalculateStats();
                            RequestRebuild();
                        });
                    }
                });
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
