using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.SkillTree;

namespace CoreSystem.UI.DevTools
{
    public class GameDevToolUI
    {
        #region [Variables/Fields]
        private readonly Character _player;
        private bool _isOpen = false;
        private UIGlobalModal _modalInstance;

        private int _selectedTab = 0;
        private string _searchQuery = "";

        private PassiveNodeRecord _pendingPassive = new PassiveNodeRecord();

        private EquipmentInstance _selectedUpgradeItem;
        private EquipmentInstance _selectedSocketItem;
        private ItemInstance _selectedFragment;

        private SkillInstance _selectedSkillForSupports;
        private const int MaxLinkedSupportsLimit = -1;

        private Dictionary<string, bool> _dbCategoryFoldout = new Dictionary<string, bool>();
        private Item _selectedDatabaseItem;
        private ItemInstance _selectedInventoryItem;
        #endregion

        #region [Properties]
        public bool IsOpen => _isOpen;
        #endregion

        #region [Public Methods]
        public GameDevToolUI(Character player)
        {
            _player = player;
        }

        public void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
            {
                if (_isOpen) Close();
                else Open();
            }
        }

        public void Open()
        {
            if (_isOpen) return;

            _modalInstance = UIBuilder.BuildGlobalModal("Dev Tool - Debug Menu", modal =>
            {
                BuildTabs(modal);
                BuildContent(modal);
            }, new Vector2(800, 600));

            if (_modalInstance != null)
            {
                _modalInstance.OnClosed += () =>
                {
                    _modalInstance = null;
                    _isOpen = false;
                };
            }

            _isOpen = _modalInstance != null;
        }

        public void Close()
        {
            if (_modalInstance != null)
            {
                UIManager.Instance.Close(_modalInstance.WindowName);
                _modalInstance = null;
            }
            _isOpen = false;
        }

        public void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        public void RequestRebuild()
        {
            if (_isOpen && _modalInstance != null)
            {
                _modalInstance.Build(context =>
                {
                    BuildTabs(context);
                    BuildContent(context);
                });
            }
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AttachToPlayerCore()
        {
            CoreSystem.Player.PlayerCore.OnReady += core =>
            {
                GameDevToolUI tool = core.Controlled != null ? new GameDevToolUI(core.Controlled) : null;
                core.OnPossessed += character => tool = new GameDevToolUI(character);
                core.OnReleased += _ =>
                {
                    tool?.Close();
                    tool = null;
                };
                core.OnTick += _ =>
                {
                    if (core.ShowDebugUI) tool?.Update();
                };
            };
        }

        private void BuildTabs(UIBuilderContext modal)
        {
            modal.HorizontalScrollView("TabsScrollView", row =>
            {
                string[] tabs = { "DB", "Inv", "Equip", "Socket", "Attr", "Skill", "Upgrade", "Passive", "U-Passive" };
                for (int i = 0; i < tabs.Length; i++)
                {
                    int index = i;
                    string style = _selectedTab == i ? "Button" : "Button";
                    var btn = row.Button(style, tabs[i], () =>
                    {
                        _selectedTab = index;
                        RequestRebuild();
                    }).SetPreferredSize(130, 50).SetFontSize(14);

                    if (_selectedTab == i)
                    {
                        btn.SetBackgroundColor(new Color(0.2f, 0.45f, 0.8f, 1f));
                    }
                }
            }, UnityEngine.UI.ScrollRect.MovementType.Elastic)
            .SetPreferredSize(-1, 50)
            .SetSpacing(8);

            modal.Horizontal("Div", row =>
            {
                row.Text("Text", "Search:").SetPreferredSize(80, 40).SetTextAlignment(TextAlignmentOptions.MidlineLeft);
                row.InputField("InputField", _searchQuery, val => _searchQuery = val).SetPreferredSize(300, 40).SetFlexibleSize(0, -1);
                row.Button("Primary Button", "Clear", () =>
                {
                    _searchQuery = "";
                    RequestRebuild();
                }).SetPreferredSize(80, -1).SetFontSize(14);
                row.Button("Primary Button", "Search", RequestRebuild).SetPreferredSize(120, -1).SetFontSize(14);
            }).SetSpacing(10);
        }

        private void BuildContent(UIBuilderContext modal)
        {
            modal.Vertical("Div", content =>
            {
                switch (_selectedTab)
                {
                    case 0: BuildDatabaseTab(content); break;
                    case 1: BuildInventoryTab(content); break;
                    case 2: BuildEquipmentTab(content); break;
                    case 3: BuildSocketingTab(content); break;
                    case 4: BuildAttributesTab(content); break;
                    case 5: BuildSkillsTab(content); break;
                    case 6: BuildUpgradeTab(content); break;
                    case 7: BuildPassiveTab(content); break;
                    case 8: BuildUniquePassivesTab(content); break;
                }
            }).SetFitParent(stretchWidth: true, stretchHeight: false);
        }

        private void BuildDatabaseTab(UIBuilderContext ctx)
        {
            ctx.Horizontal("Div", mainRow =>
            {
                mainRow.Vertical("Div", leftCol =>
                {
                    leftCol.Horizontal("Div", headerRow =>
                    {
                        headerRow.Text("Text", "All Possible Items (Database)")
                                 .SetFlexibleSize(1, -1)
                                 .SetFontStyle(FontStyles.Bold)
                                 .SetFontSize(20);

                        headerRow.Button("Primary Button", "DEBUG: ADD ALL", () =>
                        {
                            var allItems = Database.GetItems;
                            foreach (var item in allItems) GameData.Player.Inventory.AddItem(item);
                            RequestRebuild();
                        }).SetPreferredSize(200, -1).SetFontSize(14).SetColor(new Color(0.8f, 0.2f, 0.2f)).SetTextColor(Color.white);
                    });

                    var allItemsDb = Database.GetItems;
                    var groupedItems = allItemsDb.GroupBy(i => i.Type.ToString());

                    foreach (var group in groupedItems)
                    {
                        string catName = group.Key;
                        if (!_dbCategoryFoldout.ContainsKey(catName)) _dbCategoryFoldout[catName] = false;

                        bool isExpanded = _dbCategoryFoldout[catName];

                        leftCol.Button("Button", $"{(isExpanded ? "[-]" : "[+]")} {catName} ({group.Count()})", () =>
                        {
                            _dbCategoryFoldout[catName] = !isExpanded;
                            RequestRebuild();
                        }).SetBackgroundColor(new Color(0.15f, 0.15f, 0.2f, 1f))
                          .SetTextAlignment(TextAlignmentOptions.MidlineLeft)
                          .SetPreferredSize(270, 50).SetFontSize(14).SetTextColor(Color.white);

                        if (isExpanded)
                        {
                            leftCol.Grid("Grid", grid =>
                            {
                                foreach (var item in group)
                                {
                                    if (!string.IsNullOrEmpty(_searchQuery) && !item.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                                    bool isSelected = (_selectedDatabaseItem == item);
                                    var btn = grid.Button("Button", item.Name, () =>
                                    {
                                        _selectedDatabaseItem = item;
                                        RequestRebuild();
                                    }).SetPreferredSize(95, 95).SetFontSize(13);

                                    if (isSelected)
                                    {
                                        btn.SetBackgroundColor(new Color(0.2f, 0.45f, 0.8f, 1f));
                                    }
                                }
                            }).SetGridCellSize(new Vector2(95, 95))
                              .SetGridSpacing(new Vector2(8, 8))
                              .SetGridConstraint(GridLayoutGroup.Constraint.FixedColumnCount, 4)
                              .SetPadding(8, 8, 8, 8);
                        }
                    }
                }).SetFlexibleSize(1, -1);

                mainRow.Vertical("Div", rightCol =>
                {
                    if (_selectedDatabaseItem != null)
                    {
                        rightCol.Text("Text", $"[{_selectedDatabaseItem.Id}]")
                                .SetFontSize(14).SetTextColor(Color.gray);
                        rightCol.Text("Text", _selectedDatabaseItem.Name)
                                .SetFontSize(20).SetFontStyle(FontStyles.Bold).SetColor(Color.white);
                        rightCol.Text("Text", $"Type: {_selectedDatabaseItem.Type}")
                                .SetFontSize(14).SetTextColor(Color.cyan);
                        rightCol.Text("Text", _selectedDatabaseItem.Description)
                                .SetFontSize(14).SetColor(Color.white);

                        rightCol.Button("Primary Button", "Collect Item", () =>
                        {
                            GameData.Player.Inventory.AddItem(_selectedDatabaseItem);
                            RequestRebuild();
                        }).SetPreferredSize(200, -1).SetFontSize(14);
                    }
                    else
                    {
                        rightCol.Text("Text", "Select an item to view info.")
                                .SetTextAlignment(TextAlignmentOptions.TopLeft)
                                .SetTextColor(Color.gray);
                    }
                }).SetPreferredSize(260, -1)
                  .SetBackgroundColor(new Color(0.12f, 0.12f, 0.16f, 1f))
                  .SetPadding(15, 15, 15, 15)
                  .SetChildAlignment(TextAnchor.UpperLeft);

            }).SetFitParent(stretchWidth: true, stretchHeight: false)
              .SetChildAlignment(TextAnchor.UpperRight);
        }

        private void BuildInventoryTab(UIBuilderContext ctx)
        {
            var instances = GameData.Player.Inventory.GetAllItems();

            ctx.Horizontal("Div", mainRow =>
            {
                mainRow.Vertical("Div", leftCol =>
                {
                    leftCol.Horizontal("Div", headerRow =>
                    {
                        headerRow.Text("Text", $"Player Held Items ({instances.Count})")
                                 .SetFlexibleSize(1, -1)
                                 .SetFontStyle(FontStyles.Bold)
                                 .SetFontSize(20);

                        if (instances.Count > 0)
                        {
                            headerRow.Button("Primary Button", "Discard All", () =>
                            {
                                var copy = new List<ItemInstance>(instances);
                                foreach (var inst in copy)
                                    GameData.Player.Inventory.RemoveItemByInstance(inst.Id, inst.Quantity);
                                _selectedInventoryItem = null;
                                RequestRebuild();
                            }).SetPreferredSize(170, -1).SetFontSize(14).SetColor(new Color(0.7f, 0.2f, 0.2f)).SetTextColor(Color.white);
                        }
                    });

                    if (instances.Count == 0)
                    {
                        leftCol.Text("Text", "Inventory is empty.")
                               .SetTextColor(Color.gray)
                               .SetFontSize(16);
                    }
                    else
                    {
                        leftCol.Grid("Grid", grid =>
                        {
                            foreach (var instance in instances)
                            {
                                if (!string.IsNullOrEmpty(_searchQuery) && !instance.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                                bool isSelected = (_selectedInventoryItem == instance);
                                Color rarityColor = instance.Item.Rarity.GetColor();

                                grid.Vertical("Div", slot =>
                                {
                                    slot.Horizontal("Div", topRow =>
                                    {
                                        string rTag = instance.Item.Rarity.ToString().Substring(0, 1);
                                        topRow.Text("Text", $"[{rTag}]")
                                               .SetFontSize(11)
                                               .SetFontStyle(FontStyles.Bold)
                                               .SetTextColor(rarityColor)
                                               .SetFlexibleSize(1, -1);

                                        if (instance is EquipmentInstance eq)
                                        {
                                            if (eq.IsEquipped)
                                            {
                                                topRow.Text("Text", "[E]")
                                                       .SetFontSize(11)
                                                       .SetFontStyle(FontStyles.Bold)
                                                       .SetTextColor(new Color(0.2f, 1f, 0.4f));
                                            }
                                            if (eq.Refine > 0)
                                            {
                                                topRow.Text("Text", $"+{eq.Refine}")
                                                       .SetFontSize(11)
                                                       .SetFontStyle(FontStyles.Bold)
                                                       .SetTextColor(new Color(1f, 0.85f, 0.2f));
                                            }
                                        }
                                        else if (instance is CostumeInstance cost && cost.IsEquipped)
                                        {
                                            topRow.Text("Text", "[E]")
                                                   .SetFontSize(11)
                                                   .SetFontStyle(FontStyles.Bold)
                                                   .SetTextColor(new Color(0.2f, 1f, 0.4f));
                                        }
                                        else if (instance.Quantity > 1)
                                        {
                                            topRow.Text("Text", $"x{instance.Quantity}")
                                                   .SetFontSize(11)
                                                   .SetTextColor(Color.white);
                                        }
                                    }).SetPadding(4, 4, 2, 0);

                                    if (instance.Item.Icon != null)
                                    {
                                        slot.Image("Image", instance.Item.Icon)
                                            .SetPreferredSize(50, 50);
                                    }

                                    slot.Text("Text", instance.Name)
                                        .SetFontSize(11)
                                        .SetFontStyle(FontStyles.Bold)
                                        .SetTextColor(rarityColor)
                                        .SetTextAlignment(TextAlignmentOptions.Center);

                                    slot.Horizontal("Div", botRow =>
                                    {
                                        if (instance is EquipmentInstance eq)
                                        {
                                            int maxSock = (eq.Item as Equipment)?.MaxSockets ?? 0;
                                            if (maxSock > 0)
                                            {
                                                int used = eq.SocketedFragments != null ? eq.SocketedFragments.Count(f => f != null) : 0;
                                                botRow.Text("Text", $"S:{used}/{maxSock}")
                                                       .SetFontSize(10)
                                                       .SetTextColor(Color.gray)
                                                       .SetFlexibleSize(1, -1);
                                            }
                                            if (eq.Reforge > 0)
                                            {
                                                botRow.Text("Text", $"R:{eq.Reforge}")
                                                       .SetFontSize(10)
                                                       .SetTextColor(new Color(0.4f, 0.9f, 1f));
                                            }
                                        }
                                        else
                                        {
                                            botRow.Text("Text", instance.Item.Type.ToString())
                                                   .SetFontSize(10)
                                                   .SetTextColor(Color.gray)
                                                   .SetFlexibleSize(1, -1);
                                        }
                                    }).SetPadding(4, 4, 0, 2);

                                }).SetBackgroundColor(isSelected ? new Color(0.2f, 0.4f, 0.65f, 1f) : new Color(0.12f, 0.12f, 0.16f, 0.95f))
                                  .SetPadding(4, 4, 4, 4)
                                  .SetChildAlignment(TextAnchor.MiddleCenter)
                                  .SetOnClick(() =>
                                  {
                                      _selectedInventoryItem = instance;
                                      RequestRebuild();
                                  });
                            }
                        }).SetGridCellSize(new Vector2(100, 115))
                          .SetGridSpacing(new Vector2(8, 8))
                          .SetGridConstraint(GridLayoutGroup.Constraint.FixedColumnCount, 4)
                          .SetPadding(8, 8, 8, 8);
                    }
                }).SetFlexibleSize(1, -1);

                mainRow.Vertical("Div", rightCol =>
                {
                    if (_selectedInventoryItem != null && _selectedInventoryItem.Item != null)
                    {
                        var item = _selectedInventoryItem.Item;
                        Color rColor = item.Rarity.GetColor();

                        rightCol.Text("Text", $"[UID: {_selectedInventoryItem.Id}]")
                                .SetFontSize(12).SetTextColor(Color.gray);

                        rightCol.Text("Text", _selectedInventoryItem.Name)
                                .SetFontSize(18).SetFontStyle(FontStyles.Bold).SetTextColor(rColor);

                        rightCol.Text("Text", $"{item.Rarity} • {item.Type}")
                                .SetFontSize(13).SetTextColor(Color.cyan);

                        if (_selectedInventoryItem is EquipmentInstance eq)
                        {
                            string equipStatus = eq.IsEquipped ? "<color=#00FF66>● EQUIPPED</color>" : "<color=#888888>○ IN BAG</color>";
                            rightCol.Text("Text", equipStatus).SetFontSize(13);

                            rightCol.Text("Text", $"Refine: +{eq.Refine}  |  Advancement: Tier {eq.Reforge}")
                                    .SetFontSize(13).SetTextColor(new Color(1f, 0.85f, 0.3f));

                            int maxSock = (eq.Item as Equipment)?.MaxSockets ?? 0;
                            if (maxSock > 0)
                            {
                                int used = eq.SocketedFragments != null ? eq.SocketedFragments.Count(f => f != null) : 0;
                                rightCol.Text("Text", $"Sockets ({used}/{maxSock}):").SetFontSize(12).SetTextColor(Color.gray);
                                if (eq.SocketedFragments != null)
                                {
                                    for (int s = 0; s < eq.SocketedFragments.Count; s++)
                                    {
                                        var frag = eq.SocketedFragments[s];
                                        if (frag != null)
                                        {
                                            rightCol.Text("Text", $"  [{s + 1}] {frag.Name}").SetFontSize(11).SetTextColor(frag.Rarity.GetColor());
                                        }
                                    }
                                }
                            }
                        }
                        else if (_selectedInventoryItem is CostumeInstance cost)
                        {
                            string equipStatus = cost.IsEquipped ? "<color=#00FF66>● EQUIPPED</color>" : "<color=#888888>○ IN WARDROBE</color>";
                            rightCol.Text("Text", equipStatus).SetFontSize(13);
                        }

                        if (_selectedInventoryItem.Quantity > 1)
                        {
                            rightCol.Text("Text", $"Quantity: x{_selectedInventoryItem.Quantity}")
                                    .SetFontSize(13).SetTextColor(Color.white);
                        }

                        if (!string.IsNullOrEmpty(item.Description))
                        {
                            rightCol.Text("Text", item.Description)
                                    .SetFontSize(13).SetTextColor(new Color(0.85f, 0.85f, 0.85f));
                        }

                        rightCol.Text("Text", "--- Actions ---").SetFontSize(12).SetTextColor(Color.gray);

                        if (_selectedInventoryItem.Item is Equipment)
                        {
                            bool isEquipped = (_selectedInventoryItem is EquipmentInstance eqInst && eqInst.IsEquipped);
                            string btnText = isEquipped ? "Unequip" : "Equip";
                            rightCol.Button("Primary Button", btnText, () =>
                            {
                                if (isEquipped)
                                {
                                    for (int i = 0; i < _player.EquipmentSlots.Length; i++)
                                    {
                                        if (_player.EquipmentSlots[i] == _selectedInventoryItem)
                                        {
                                            GameData.Player.Equipment.Equip(i, null);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    GameData.Player.Equipment.SmartEquip(_selectedInventoryItem);
                                }
                                RequestRebuild();
                            }).SetPreferredSize(200, -1).SetFontSize(14);
                        }
                        else if (_selectedInventoryItem.Item is Costume)
                        {
                            bool isEquipped = (_selectedInventoryItem is CostumeInstance costInst && costInst.IsEquipped);
                            rightCol.Button("Primary Button", isEquipped ? "Take Off" : "Wear", () =>
                            {
                                GameData.Player.Costumes.Equip(_selectedInventoryItem);
                                RequestRebuild();
                            }).SetPreferredSize(200, -1).SetFontSize(14);
                        }

                        rightCol.Horizontal("Div", actRow =>
                        {
                            actRow.Button("Primary Button", "Discard", () =>
                            {
                                GameData.Player.Inventory.RemoveItemByInstance(_selectedInventoryItem.Id, 1);
                                if (_selectedInventoryItem.Quantity <= 1) _selectedInventoryItem = null;
                                RequestRebuild();
                            }).SetPreferredSize(200, -1).SetFontSize(14).SetColor(Color.red).SetTextColor(Color.white);

                            if (_selectedInventoryItem.Quantity > 1)
                            {
                                actRow.Button("Primary Button", "Discard All", () =>
                                {
                                    GameData.Player.Inventory.RemoveItemByInstance(_selectedInventoryItem.Id, _selectedInventoryItem.Quantity);
                                    _selectedInventoryItem = null;
                                    RequestRebuild();
                                }).SetPreferredSize(110, -1).SetFontSize(14).SetColor(new Color(0.7f, 0.2f, 0.2f));
                            }
                        }).SetSpacing(6);
                    }
                    else
                    {
                        rightCol.Text("Text", "Select an item slot to view details & actions.")
                                .SetTextAlignment(TextAlignmentOptions.TopLeft)
                                .SetTextColor(Color.gray);
                    }
                }).SetPreferredSize(260, -1)
                  .SetBackgroundColor(new Color(0.12f, 0.12f, 0.16f, 1f))
                  .SetPadding(15, 15, 15, 15)
                  .SetChildAlignment(TextAnchor.UpperLeft);

            }).SetFitParent(stretchWidth: true, stretchHeight: false)
              .SetChildAlignment(TextAnchor.UpperRight);
        }

        private void BuildEquipmentTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "--- Current Equipment ---").SetFontSize(14).SetFontStyle(FontStyles.Bold);
            for (int i = 0; i < _player.EquipmentSlots.Length; i++)
            {
                var instance = _player.EquipmentSlots[i];
                int slotIndex = i;
                ctx.Horizontal("Div", row =>
                {
                    row.Text("Text", $"Slot {i}: {(instance != null ? $"[{instance.Id}] {instance.Name}" : "Empty")}")
                       .SetFlexibleSize(1, -1).SetFontSize(13);
                    if (instance != null)
                    {
                        row.Button("Primary Button", "Unequip", () =>
                        {
                            GameData.Player.Equipment.Equip(slotIndex, null);
                            RequestRebuild();
                        }).SetPreferredSize(80, -1).SetFontSize(12);
                    }
                }).SetSpacing(8);
            }

            ctx.Text("Text", "--- Current Costumes ---").SetFontSize(14).SetFontStyle(FontStyles.Bold).SetPadding(0, 0, 10, 0);
            for (int i = 0; i < _player.CostumeSlots.Length; i++)
            {
                var instance = _player.CostumeSlots[i];
                int slotIndex = i;
                ctx.Horizontal("Div", row =>
                {
                    row.Text("Text", $"Slot {(CostumeSlot)i}: {(instance != null ? $"[{instance.Id}] {instance.Name}" : "Empty")}")
                       .SetFlexibleSize(1, -1).SetFontSize(13);
                    if (instance != null)
                    {
                        row.Button("Primary Button", "Take Off", () =>
                        {
                            GameData.Player.Costumes.Unequip(slotIndex);
                            RequestRebuild();
                        }).SetPreferredSize(80, -1).SetFontSize(12);
                    }
                }).SetSpacing(8);
            }

            ctx.Text("Text", "--- Equipped Skills ---").SetFontSize(14).SetFontStyle(FontStyles.Bold).SetPadding(0, 0, 10, 0);
            if (_player.skillCaster != null)
            {
                for (int i = 0; i < _player.skillCaster.EquippedSkills.Count; i++)
                {
                    var skill = _player.skillCaster.EquippedSkills[i];
                    int slotIndex = i;
                    ctx.Horizontal("Div", row =>
                    {
                        row.Text("Text", $"Slot {i + 1}: {(skill != null ? (skill.SkillData != null ? skill.SkillData.Name : "Data Missing") : "Empty")}")
                           .SetFlexibleSize(1, -1).SetFontSize(13);
                        if (skill != null)
                        {
                            row.Button("Primary Button", "Unequip", () =>
                            {
                                _player.skillCaster.EquipSkill((SkillInstance)null, slotIndex);
                                RequestRebuild();
                            }).SetPreferredSize(80, -1).SetFontSize(12);
                        }
                    }).SetSpacing(8);
                }
            }
        }

        private void BuildSocketingTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "--- Socketing System ---").SetFontSize(14).SetFontStyle(FontStyles.Bold);

            ctx.Horizontal("Div", row =>
            {
                row.Vertical("Div", leftCol =>
                {
                    leftCol.Text("Text", "Select Equipped Item:").SetFontSize(13);
                    for (int i = 0; i < _player.EquipmentSlots.Length; i++)
                    {
                        var inst = _player.EquipmentSlots[i] as EquipmentInstance;
                        if (inst == null) continue;

                        string prefix = _selectedSocketItem == inst ? ">> " : "";
                        leftCol.Button("Button", $"{prefix}[{i}] {inst.Name}", () =>
                        {
                            _selectedSocketItem = inst;
                            RequestRebuild();
                        }).SetPreferredSize(200, 50).SetFontSize(12);
                    }
                });

                row.Vertical("Div", rightCol =>
                {
                    rightCol.Text("Text", "Select Fragment from Inventory:").SetFontSize(13);
                    var fragments = GameData.Player.Inventory.GetAllItems().Where(it => it.Item is Fragment).ToList();
                    if (fragments.Count == 0)
                    {
                        rightCol.Text("Text", "No fragments in inventory.").SetFontSize(12).SetTextColor(Color.gray);
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
                            }).SetPreferredSize(200, 50).SetFontSize(12);
                        }
                    }
                });
            });

            if (_selectedSocketItem != null)
            {
                var equip = _selectedSocketItem.Item as Equipment;
                ctx.Vertical("Div", actionCol =>
                {
                    actionCol.Text("Text", $"Applying to: {_selectedSocketItem.Name}").SetFontSize(14).SetFontStyle(FontStyles.Bold);
                    actionCol.Text("Text", $"Sockets: {_selectedSocketItem.SocketedFragments.Count} / {equip.MaxSockets}").SetFontSize(13);

                    foreach (var f in _selectedSocketItem.SocketedFragments)
                    {
                        actionCol.Text("Text", $"  - {f.Name}").SetFontSize(12);
                    }

                    if (_selectedFragment != null)
                    {
                        actionCol.Button("Primary Button", $"SOCKET {_selectedFragment.Name} into {_selectedSocketItem.Name}", () =>
                        {
                            GameData.Player.Equipment.SocketFragment(_selectedSocketItem, _selectedFragment);
                            if (_selectedFragment.Quantity <= 0) _selectedFragment = null;
                            RequestRebuild();
                        }).SetPreferredSize(300, -1).SetFontSize(13);
                    }
                }).SetPadding(10, 10, 10, 10);
            }
        }

        private void BuildAttributesTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "--- Primary Attributes (Adjustable) ---").SetFontSize(14).SetFontStyle(FontStyles.Bold);
            if (_player.attributeSystem != null)
            {
                AttributeType[] primaryTypes = {
                    AttributeType.STR, AttributeType.AGI, AttributeType.VIT,
                    AttributeType.INT, AttributeType.DEX, AttributeType.LUK
                };

                foreach (var type in primaryTypes)
                {
                    var attr = _player.attributeSystem.GetAttribute(type);
                    if (attr != null)
                    {
                        ctx.Horizontal("Div", row =>
                        {
                            row.Text("Text", $"{type}: {attr.CurrentValue:F0} (Base: {attr.BaseValue:F0})")
                               .SetFlexibleSize(1, -1).SetFontSize(13);
                            row.Button("Primary Button", "-", () =>
                            {
                                attr.BaseValue = Mathf.Max(1, attr.BaseValue - 1);
                                _player.attributeSystem.UpdateDerivedAttributes();
                                RequestRebuild();
                            }).SetPreferredSize(45, -1).SetFontSize(14);
                            row.Button("Primary Button", "+", () =>
                            {
                                attr.BaseValue += 1;
                                _player.attributeSystem.UpdateDerivedAttributes();
                                RequestRebuild();
                            }).SetPreferredSize(45, -1).SetFontSize(14);
                            row.Button("Primary Button", "+10", () =>
                            {
                                attr.BaseValue += 10;
                                _player.attributeSystem.UpdateDerivedAttributes();
                                RequestRebuild();
                            }).SetPreferredSize(60, -1).SetFontSize(14);
                        }).SetSpacing(6);
                    }
                }

                ctx.Text("Text", "--- Experience Controls ---").SetFontSize(14).SetFontStyle(FontStyles.Bold).SetPadding(0, 0, 10, 0);
                int blvl = (int)_player.attributeSystem.GetAttribute(AttributeType.BaseLevel).BaseValue;
                long nextBase = _player.expSystem.GetNextBaseExp(blvl);
                ctx.Text("Text", $"Base Exp: {_player.expSystem.CurrentBaseExp} / {nextBase} (Lvl {blvl})").SetFontSize(13);

                ctx.Horizontal("Div", row =>
                {
                    row.Button("Primary Button", "+100 Base Exp", () => { _player.expSystem.AddBaseExp(100); RequestRebuild(); }).SetPreferredSize(160, -1).SetFontSize(12);
                    row.Button("Primary Button", "+1000 Base Exp", () => { _player.expSystem.AddBaseExp(1000); RequestRebuild(); }).SetPreferredSize(160, -1).SetFontSize(12);
                }).SetSpacing(8);

                int jlvl = (int)_player.attributeSystem.GetAttribute(AttributeType.JobLevel).BaseValue;
                long nextJob = _player.expSystem.GetNextJobExp(jlvl);
                ctx.Text("Text", $"Job Exp: {_player.expSystem.CurrentJobExp} / {nextJob} (Lvl {jlvl})").SetFontSize(13);

                ctx.Horizontal("Div", row =>
                {
                    row.Button("Primary Button", "+100 Job Exp", () => { _player.expSystem.AddJobExp(100); RequestRebuild(); }).SetPreferredSize(160, -1).SetFontSize(12);
                    row.Button("Primary Button", "+1000 Job Exp", () => { _player.expSystem.AddJobExp(1000); RequestRebuild(); }).SetPreferredSize(160, -1).SetFontSize(12);
                }).SetSpacing(8);
            }
        }

        private void BuildSkillsTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "--- Player Held Skills (Granted) ---").SetFontSize(14).SetFontStyle(FontStyles.Bold);
            foreach (var granted in _player.skillCaster.GrantedSkills)
            {
                ctx.Horizontal("Div", row =>
                {
                    bool isSelected = (_selectedSkillForSupports == granted);
                    string prefix = isSelected ? ">> " : "";
                    row.Text("Text", $"{prefix}[{granted.SkillData.Id}] {granted.SkillData.Name}").SetFlexibleSize(1, -1).SetFontSize(13);

                    row.Button("Primary Button", "Eq 1", () => { _player.skillCaster.EquipSkill(granted, 0); RequestRebuild(); }).SetPreferredSize(80, -1).SetFontSize(12);
                    row.Button("Primary Button", "Eq 2", () => { _player.skillCaster.EquipSkill(granted, 1); RequestRebuild(); }).SetPreferredSize(80, -1).SetFontSize(12);
                    row.Button("Primary Button", "Eq 3", () => { _player.skillCaster.EquipSkill(granted, 2); RequestRebuild(); }).SetPreferredSize(80, -1).SetFontSize(12);

                    row.Button("Primary Button", "Select", () =>
                    {
                        _selectedSkillForSupports = isSelected ? null : granted;
                        RequestRebuild();
                    }).SetPreferredSize(90, -1).SetFontSize(12);

                    row.Button("Primary Button", "Revoke", () =>
                    {
                        _player.skillCaster.RevokeSkill(granted.SkillData);
                        if (_selectedSkillForSupports == granted) _selectedSkillForSupports = null;
                        RequestRebuild();
                    }).SetPreferredSize(90, -1).SetFontSize(12);
                }).SetSpacing(6);
            }

            ctx.Text("Text", "--- All Possible Skills ---").SetFontSize(14).SetFontStyle(FontStyles.Bold).SetPadding(0, 0, 10, 0);
            var skills = Database.Skills;
            foreach (var skill in skills)
            {
                if (!string.IsNullOrEmpty(_searchQuery) && !skill.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                ctx.Horizontal("Div", row =>
                {
                    row.Text("Text", $"[{skill.Id}] {skill.Name} ({skill.SkillElement})").SetFlexibleSize(1, -1).SetFontSize(13);
                    row.Button("Primary Button", "Grant", () =>
                    {
                        _player.skillCaster.GrantSkill(skill);
                        RequestRebuild();
                    }).SetPreferredSize(100, -1).SetFontSize(12);
                }).SetSpacing(6);
            }
        }

        private void BuildUpgradeTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "--- Equipment Upgrade System ---").SetFontSize(14).SetFontStyle(FontStyles.Bold);
            ctx.Horizontal("Div", row =>
            {
                row.Vertical("Div", leftCol =>
                {
                    leftCol.Text("Text", "Your Equipments:").SetFontSize(13);
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
                        }).SetPreferredSize(200, 50).SetFontSize(12);
                    }
                });

                row.Vertical("Div", rightCol =>
                {
                    if (_selectedUpgradeItem != null)
                    {
                        rightCol.Text("Text", $"UPGRADING: {_selectedUpgradeItem.Name}").SetFontSize(14).SetFontStyle(FontStyles.Bold);

                        int maxRefine = EquipmentDatabase.RefineTable.Length - 1;
                        rightCol.Horizontal("Div", statRow =>
                        {
                            statRow.Text("Text", $"Refine Level: {_selectedUpgradeItem.Refine} / {maxRefine}").SetFontSize(13);
                            statRow.Button("Button", "Reset", () => { _selectedUpgradeItem.Refine = 0; RequestRebuild(); })
                                   .SetPreferredSize(70, 50).SetFontSize(12);
                        }).SetSpacing(8);

                        float currentMult = EquipmentDatabase.GetRefineMultiplier(_selectedUpgradeItem.Refine) * 100f;
                        rightCol.Text("Text", $"Current Bonus: +{currentMult:F0}% Base Stats").SetFontSize(13);

                        rightCol.Button("Primary Button", "REFINE (+1)", () =>
                        {
                            _selectedUpgradeItem.Refine = Mathf.Min(maxRefine, _selectedUpgradeItem.Refine + 1);
                            if (_selectedUpgradeItem.IsEquipped)
                            {
                                (_selectedUpgradeItem.Item as Equipment)?.ForceRefresh(_player, _selectedUpgradeItem);
                            }
                            RequestRebuild();
                        }).SetPreferredSize(200, -1).SetFontSize(14);

                        rightCol.Button("Primary Button", "REFORGE / ADVANCE TIER (+1)", () =>
                        {
                            _selectedUpgradeItem.Reforge += 1;
                            if (_selectedUpgradeItem.IsEquipped)
                            {
                                (_selectedUpgradeItem.Item as Equipment)?.ForceRefresh(_player, _selectedUpgradeItem);
                            }
                            RequestRebuild();
                        }).SetColor(new Color(0.2f, 0.6f, 0.6f)).SetPreferredSize(300, -1).SetFontSize(14);
                    }
                });
            });
        }

        private void BuildPassiveTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "--- Passive Point Settings ---").SetFontSize(14).SetFontStyle(FontStyles.Bold);
            ctx.Text("Text", $"Current Passive Points: {_player.PassivePoints}").SetFontSize(13);

            ctx.Horizontal("Div", row =>
            {
                row.Button("Button", "+1 PT", () => { _player.PassivePoints++; RequestRebuild(); }).SetPreferredSize(80, 50).SetFontSize(12);
                row.Button("Button", "+10 PT", () => { _player.PassivePoints += 10; RequestRebuild(); }).SetPreferredSize(90, 50).SetFontSize(12);
                row.Button("Button", "RESET ALL", () =>
                {
                    _player.PassivePoints += _player.PassiveNodes.Count;
                    _player.PassiveNodes.Clear();
                    _player.RecalculateStats();
                    PassiveTreeWindow.Instance?.LoadFromCharacter(_player);
                    RequestRebuild();
                }).SetPreferredSize(110, 50).SetFontSize(12);
            }).SetSpacing(8);

            ctx.Text("Text", "--- Add Passive Record ---").SetFontSize(14).SetFontStyle(FontStyles.Bold).SetPadding(0, 0, 10, 0);
            ctx.Horizontal("Div", row =>
            {
                row.Text("Text", "NodeId").SetFontSize(12);
                row.InputField("InputField", _pendingPassive.NodeId.ToString(), val => { if (int.TryParse(val, out int v)) _pendingPassive.NodeId = v; }).SetPreferredSize(80, 50);
                row.Text("Text", "AttrId").SetFontSize(12);
                row.InputField("InputField", _pendingPassive.AttributeId.ToString(), val => { if (int.TryParse(val, out int v)) _pendingPassive.AttributeId = v; }).SetPreferredSize(80, 50);
                row.Text("Text", "Val").SetFontSize(12);
                row.InputField("InputField", _pendingPassive.Value.ToString(), val => { if (float.TryParse(val, out float v)) _pendingPassive.Value = v; }).SetPreferredSize(80, 50);
            }).SetSpacing(8);

            ctx.Button("Button", "Unlock & Apply", () =>
            {
                if (_player.PassivePoints > 0)
                {
                    _player.PassiveNodes.Add(new PassiveNodeRecord(_pendingPassive.NodeId, _pendingPassive.AttributeId, _pendingPassive.Value));
                    _player.PassivePoints--;
                    _player.RecalculateStats();
                    RequestRebuild();
                }
            }).SetPreferredSize(140, -1).SetFontSize(12);

            ctx.Text("Text", $"Unlocked Records: {_player.PassiveNodes.Count}").SetFontSize(13).SetPadding(0, 0, 10, 0);
            foreach (var rec in _player.PassiveNodes.ToList())
            {
                ctx.Horizontal("Div", row =>
                {
                    row.Text("Text", $"Node:{rec.NodeId} Attr:{(AttributeType)rec.AttributeId} Val:{rec.Value}").SetFlexibleSize(1, -1).SetFontSize(12);
                    row.Button("Button", "X", () =>
                    {
                        _player.PassiveNodes.Remove(rec);
                        _player.PassivePoints++;
                        _player.RecalculateStats();
                        RequestRebuild();
                    }).SetPreferredSize(40, 30).SetFontSize(12);
                }).SetSpacing(6);
            }
        }

        private void BuildUniquePassivesTab(UIBuilderContext ctx)
        {
            ctx.Text("Text", "Unique Passive Settings").SetFontSize(14).SetFontStyle(FontStyles.Bold);
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
                    row.Text("Text", $"{passive.Name} ({passive.Id})").SetFlexibleSize(1, -1).SetFontSize(13);
                    bool isActive = _player.UniquePassives.HasPassive(passive.Id);

                    if (isActive)
                    {
                        row.Button("Button", "Remove", () =>
                        {
                            _player.UniquePassives.RemovePassive(passive.Id);
                            _player.RecalculateStats();
                            RequestRebuild();
                        }).SetPreferredSize(90, 50).SetFontSize(12);
                    }
                    else
                    {
                        row.Button("Button", "Add", () =>
                        {
                            _player.UniquePassives.AddPassive(passive.Id);
                            _player.RecalculateStats();
                            RequestRebuild();
                        }).SetPreferredSize(90, 50).SetFontSize(12);
                    }
                }).SetSpacing(6);
            }
        }
        #endregion
    }
}