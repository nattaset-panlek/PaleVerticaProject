using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Shop;
using CoreSystem.Skills;
using CoreSystem.SkillTree;

namespace CoreSystem.UI.DevTools
{
    public class GameDevToolUI
    {
        #region [Variables/Fields]
        private const string WindowTitle = "Dev Tool - Debug Menu";
        private readonly DevToolCharacterSpawnPage _characterSpawnPage = new DevToolCharacterSpawnPage();
        private const int WorldCreditAmount = 100000;
        private const string DisplayTitle = "Dev Tool";
        public const string PagesId = "devtool";
        private const float DetailWidth = 300f;
        private const float NavWidth = 150f;
        private const float ChipRadius = 10f;
        private const float LightColorLimit = 0.8f;
        private const string NavButtonStyle = "Nav Button";
        private const string ChipStyle = "Chip";
        private const string ItemRowStyle = "Item Row";
        private const string ItemCardStyle = "Item Card";
        private const string SearchPlaceholder = "search items...";
        private const string DatabaseListId = "db.list";
        private const string DatabaseCardId = "db.card";
        private const float DatabaseRowEstimate = 60f;
        public const int MinAddAmount = 1;
        public const int MaxAddAmount = 9999;
        private const float AmountInputWidth = 80f;

        private static readonly Vector2 WindowSize = new Vector2(1000f, 680f);
        private static readonly Vector2 MinWindowSize = new Vector2(760f, 440f);
        private static readonly Color SlotColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        private static readonly Color SlotTextColor = new Color(0.79f, 0.79f, 0.79f, 1f);
        private static readonly AttributeType[] PrimaryAttributes =
        {
            AttributeType.STR, AttributeType.AGI, AttributeType.VIT,
            AttributeType.INT, AttributeType.DEX, AttributeType.LUK
        };

        private readonly Character _player;
        private string _worldStatus = "Spawns a Shopkeeper with a random shop where the player stands.";
        private UIGlobalModal _modal;
        private string _searchQuery = "";

        private PassiveNodeRecord _pendingPassive = new PassiveNodeRecord();
        private EquipmentInstance _selectedUpgradeItem;
        private EquipmentInstance _selectedSocketItem;
        private ItemInstance _selectedFragment;
        private SkillInstance _selectedSkillForSupports;
        private Item _selectedDatabaseItem;
        private readonly List<DevToolDatabaseRow> _databaseRows = new List<DevToolDatabaseRow>();
        private GlobalVisualizer.GlobalVisualizerView _databaseView;
        private ItemInstance _selectedInventoryItem;
        private int _addAmount = MinAddAmount;
        #endregion

        #region [Properties]
        public bool IsOpen => _modal != null;
        #endregion

        #region [Public Methods]
        public GameDevToolUI(Character player)
        {
            _player = player;
        }

        public static int ParseAddAmount(string text, int fallback)
        {
            if (!int.TryParse(text, out int value)) return Mathf.Clamp(fallback, MinAddAmount, MaxAddAmount);
            return Mathf.Clamp(value, MinAddAmount, MaxAddAmount);
        }

        public void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        public void Open()
        {
            if (_modal != null) return;

            _modal = UIBuilder.BuildGlobalModal(WindowTitle, null, WindowSize);
            if (_modal == null) return;

            Setup(_modal);
            _modal.OnClosed += HandleClosed;
        }

        public void Close()
        {
            if (_modal == null) return;
            if (UIManager.Instance != null) UIManager.Instance.Close(_modal.WindowName);
            HandleClosed();
        }

        public void Toggle()
        {
            if (_modal != null) Close();
            else Open();
        }

        public void RequestRebuild()
        {
            _modal?.RefreshPage(PagesId);
        }

        public void SelectDatabaseItem(Item item)
        {
            _selectedDatabaseItem = item;
            RequestRebuild();
        }

        public void Setup(UIGlobalModal modal)
        {
            modal.SetTitle(DisplayTitle)
                 .SetMinSize(MinWindowSize)
                 .SetResizable(true)
                 .SetScrollable(false)
                 .Build(BuildUI);
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

        private void HandleClosed()
        {
            if (_modal == null) return;
            _modal.OnClosed -= HandleClosed;
            _modal = null;
        }

        private bool Matches(string text)
        {
            return string.IsNullOrEmpty(_searchQuery)
                || (!string.IsNullOrEmpty(text) && text.IndexOf(_searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void BuildCharactersPage(UIBuilderContext ui)
        {
            _characterSpawnPage.Build(ui, _player != null ? _player.transform : null, _searchQuery, RequestRebuild);
        }

        private void BuildWorldPage(UIBuilderContext ui)
        {
            ui.Heading("Merchant");
            ui.Label(_worldStatus);
            ui.Row(row =>
            {
                row.Button("Spawn Random Merchant Here", SpawnRandomMerchant, UIVariant.Primary).Fill();
                row.Button("Open Random Shop", () => MerchantFactory.OpenShop(MerchantFactory.CreateRandomShop(new System.Random())), UIVariant.Secondary).Fill();
                row.Button($"Add {WorldCreditAmount:N0} Credit", () => GameData.Player.Inventory.AddCredit(WorldCreditAmount), UIVariant.Secondary).Fill();
            });
        }

        private void SpawnRandomMerchant()
        {
            MerchantFactory.SpawnRandom(_player.transform.position, merchant =>
            {
                _worldStatus = merchant != null
                    ? $"{merchant.gameObject.name} is ready. Move away, then right-click it and choose Shop."
                    : "Spawn failed. See the Console.";
                RequestRebuild();
            });
        }

        private void BuildUI(UIBuilderContext ui)
        {
            ui.Pages(PagesId, pages =>
            {
                pages.NavWidth = NavWidth;
                pages.ScrollPages = true;
                pages.NavButtonStyle = NavButtonStyle;
                pages.WithHeader(BuildSearchBar)
                     .WithNavStyle(button => button.TextAlign(TextAlignmentOptions.Center))
                     .Add("Database", BuildDatabasePage, false)
                     .Add("Inventory", BuildInventoryPage, false)
                     .Add("Equipment", BuildEquipmentPage)
                     .Add("Socketing", BuildSocketingPage)
                     .Add("Attributes", BuildAttributesPage)
                     .Add("Skills", BuildSkillsPage)
                     .Add("Upgrade", BuildUpgradePage)
                     .Add("Passive", BuildPassivePage)
                     .Add("Unique Passive", BuildUniquePassivesPage)
                     .Add("Characters", BuildCharactersPage)
                     .Add("World", BuildWorldPage);
            }, UIPageNav.SideMenu);
        }

        private void BuildSearchBar(UIBuilderContext ui)
        {
            ui.Row(bar =>
            {
                var search = bar.Input(_searchQuery, value =>
                {
                    _searchQuery = value;
                    RequestRebuild();
                }).Fill();
                SetPlaceholder(search, SearchPlaceholder);
                bar.Button(ChipStyle, "Clear", () =>
                {
                    _searchQuery = "";
                    ui.Document.Refresh();
                });
                bool dark = UIGlobalModal.GlobalThemeMode == UIThemeMode.Dark;
                bar.Toggle("Dark mode", dark, isOn => UIGlobalModal.SetGlobalThemeMode(isOn ? UIThemeMode.Dark : UIThemeMode.Light));
                bar.Button("Snapshot", () => Diagnostics.UIDebugCapture.Capture(_modal), UIVariant.Secondary);
            }).Gap(10f);
        }

        private static void SetPlaceholder(UIElementRef input, string text)
        {
            var field = input.GameObject != null ? input.GameObject.GetComponentInChildren<TMP_InputField>(true) : null;
            if (field != null && field.placeholder is TMP_Text placeholder) placeholder.text = text;
        }

        private static Color Legible(Color color, UITheme theme)
        {
            return color.grayscale > LightColorLimit ? theme.Text : color;
        }

        private static void Chip(UIBuilderContext ui, string label, Color? color)
        {
            ui.Badge(label.ToUpperInvariant(), color).Padding(10, 10, 3, 3).CornerRadius(ChipRadius);
        }

        private static UIElementRef ChipButton(UIBuilderContext ui, string label, System.Action onClick, UIVariant variant = UIVariant.Default)
        {
            var chip = ui.Button(ChipStyle, label, onClick);
            return variant == UIVariant.Selected ? chip.Variant(variant) : chip.LabelVariant(variant);
        }

        private static UIElementRef ListButton(UIBuilderContext ui, string label, System.Action onClick, bool selected)
        {
            return ui.Button(NavButtonStyle, label, onClick)
                     .Variant(selected ? UIVariant.Selected : UIVariant.Default)
                     .TextAlign(TextAlignmentOptions.MidlineLeft)
                     .Fill();
        }

        private void BuildDatabasePage(UIBuilderContext ui)
        {
            ui.Row(layout =>
            {
                layout.Column(list =>
                {
                    var allItems = Database.GetItems;
                    list.Row(header =>
                    {
                        header.Column(title =>
                        {
                            title.Heading("ITEM DATABASE");
                            title.Caption($"{allItems.Count} items  ·  {allItems.Select(i => i.Type).Distinct().Count()} types");
                        }).Fill().Gap(2f);
                        header.Button("Add All", () =>
                        {
                            GameData.Player.Inventory.AddItems(allItems);
                            RequestRebuild();
                        }, UIVariant.Danger);
                    }).Gap(12f);

                    FillDatabaseRows(list.Document);
                    var rows = GlobalVisualizer.VisualLayoutSettings.Vertical(DatabaseRowEstimate, 8f);
                    rows.Padding = new GlobalVisualizer.VisualPadding(2f, 2f, 2f, 8f);
                    var listRef = list.Visualizer(_databaseRows, rows, BuildDatabaseCell, 0f, GlobalVisualizer.VisualSizeMode.Measure).Id(DatabaseListId).Elastic(true);
                    _databaseView = UIVisualizerFactory.Find(listRef);
                }).Fill().FillHeight().Gap(10f);

                var card = layout.Div(ItemCardStyle, BuildDatabaseDetail).Width(DetailWidth).FillHeight();
                layout.Document.RegisterRegion(DatabaseCardId, card.GameObject.transform, BuildDatabaseDetail);
            }).Align(TextAnchor.UpperLeft).Gap(16f).FillHeight();
        }

        private void BuildItemRow(UIBuilderContext ui, Item item)
        {
            var row = ui.Custom(ItemRowStyle)
                        .SetText("Name", item.Name)
                        .SetText("Sub", $"{item.Type}  ·  {item.Rarity}")
                        .SetText("ValueLabel", "ID")
                        .SetText("Value", item.Id.ToString())
                        .SetSprite("Icon", item.Icon);

            if (item == _selectedDatabaseItem)
            {
                Color rarity = Legible(item.Rarity.GetColor(), ui.Theme);
                row.SetColor("Outline", rarity).SetColor("Name", rarity).SetColor("Body", ui.Theme.PanelAlt);
            }

            UIDocument document = ui.Document;
            new UIElementRef(row.gameObject, document).OnClick(() => SelectFromDatabaseList(document, item));
        }

        private void FillDatabaseRows(UIDocument document)
        {
            bool searching = !string.IsNullOrEmpty(_searchQuery);
            DevToolDatabaseRow.Fill(_databaseRows, Database.GetItems, Matches, group => document.GetState(DevToolDatabaseRow.StateKey(group), searching));
        }

        private void BuildDatabaseCell(UIBuilderContext cell, DevToolDatabaseRow row, int index)
        {
            if (!row.IsHeader)
            {
                BuildItemRow(cell, row.Item);
                return;
            }

            UIDocument document = cell.Document;
            string label = row.IsOpen ? row.Title : $"{row.Title}  +";
            System.Action toggle = () => ToggleDatabaseGroup(document, row.Group);
            var header = document.Factory.HasStyle(UIElementType.Button, UIBuilderContext.FoldoutHeaderStyle)
                ? cell.Button(UIBuilderContext.FoldoutHeaderStyle, label, toggle)
                : cell.Button(label, toggle, UIVariant.Secondary);
            header.TextAlign(TextAlignmentOptions.MidlineLeft).Fill();
        }

        private void ToggleDatabaseGroup(UIDocument document, string group)
        {
            string key = DevToolDatabaseRow.StateKey(group);
            document.SetState(key, !document.GetState(key, !string.IsNullOrEmpty(_searchQuery)));
            if (_databaseView == null)
            {
                RequestRebuild();
                return;
            }

            int before = _databaseRows.Count;
            FillDatabaseRows(document);
            int header = DevToolDatabaseRow.IndexOfHeader(_databaseRows, group);
            int delta = _databaseRows.Count - before;
            if (header < 0) _databaseView.NotifyReset();
            else if (delta > 0) _databaseView.NotifyInserted(header + 1, delta);
            else if (delta < 0) _databaseView.NotifyRemoved(header + 1, -delta);
            else _databaseView.Refresh(true);
        }

        private void SelectFromDatabaseList(UIDocument document, Item item)
        {
            _selectedDatabaseItem = item;
            if (_databaseView == null || !document.Refresh(DatabaseCardId))
            {
                RequestRebuild();
                return;
            }
            _databaseView.Refresh(true);
        }

        private void BuildDatabaseDetail(UIBuilderContext ui)
        {
            var item = _selectedDatabaseItem;
            if (item == null)
            {
                ui.Caption("Select an item to see its details.");
                return;
            }

            ui.Scroll(details => BuildDatabaseDetailBody(details, item), 0f).Id("db.detail").Gap(10f);
            ui.Row(actions =>
            {
                actions.Button("Add One", () => AddToInventory(item, MinAddAmount), UIVariant.Secondary).Fill();
            });
            ui.Row(actions =>
            {
                actions.Input(_addAmount.ToString(), value => _addAmount = ParseAddAmount(value, _addAmount)).Width(AmountInputWidth);
                actions.Button("Add Amount", () => AddToInventory(item, _addAmount), UIVariant.Primary).Fill();
            }).Gap(8f);
        }

        private void AddToInventory(Item item, int amount)
        {
            GameData.Player.Inventory.AddItem(item, Mathf.Clamp(amount, MinAddAmount, MaxAddAmount));
            RequestRebuild();
        }

        private void BuildDatabaseDetailBody(UIBuilderContext ui, Item item)
        {
            Color rarity = item.Rarity.GetColor();
            ui.Row(head =>
            {
                head.Column(ring =>
                {
                    ring.Column(box =>
                    {
                        if (item.Icon != null) box.Image(item.Icon).Size(40f, 40f);
                    }).Size(60f, 60f).Background(ring.Theme.Selected).CornerRadius(8f).Align(TextAnchor.MiddleCenter);
                }).Size(64f, 64f).Padding(2).Background(rarity).CornerRadius(10f);

                head.Column(info =>
                {
                    info.Heading(item.Name.ToUpperInvariant());
                    info.Row(tags =>
                    {
                        Chip(tags, item.Rarity.ToString(), rarity);
                        Chip(tags, item.Type.ToString(), null);
                    }).Gap(6f);
                }).Fill().Gap(4f);
            }).Gap(12f);

            ui.SectionHeader("DETAILS");
            ui.KeyValue("ID", item.Id.ToString());
            ui.KeyValue("Type", item.Type.ToString());
            ui.KeyValue("Rarity", item.Rarity.ToString());
            int maxSockets = (item as Equipment)?.MaxSockets ?? 0;
            if (maxSockets > 0) ui.KeyValue("Sockets", $"0 / {maxSockets}");

            if (!string.IsNullOrEmpty(item.Description))
            {
                ui.SectionHeader("DESCRIPTION");
                ui.Label(item.Description);
            }
        }

        private void BuildInventoryPage(UIBuilderContext ui)
        {
            var instances = GameData.Player.Inventory.GetAllItems();

            ui.Row(layout =>
            {
                layout.Column(list =>
                {
                    list.Row(header =>
                    {
                        header.Heading($"Inventory  ({instances.Count})").Fill();
                        if (instances.Count > 0)
                        {
                            header.Button("Discard All", () =>
                            {
                                GameData.Player.Inventory.BeginBatch();
                                foreach (var inst in new List<ItemInstance>(instances))
                                {
                                    GameData.Player.Inventory.RemoveItemByInstance(inst.Id, inst.Quantity);
                                }
                                GameData.Player.Inventory.EndBatch();
                                _selectedInventoryItem = null;
                                RequestRebuild();
                            }, UIVariant.Danger);
                        }
                    });

                    if (instances.Count == 0)
                    {
                        list.Scroll(scroll => scroll.Caption("Inventory is empty."), 0f).Id("inv.list");
                    }
                    else
                    {
                        var shown = instances.Where(instance => Matches(instance.Name)).ToList();
                        var cells = GlobalVisualizer.VisualLayoutSettings.Grid(new Vector2(118f, 122f), 8f);
                        list.Visualizer(shown, cells, (cell, instance, index) => BuildInventorySlot(cell, instance), 0f).Id("inv.list");
                    }
                }).Fill().FillHeight();

                layout.Panel(BuildInventoryDetail).Width(DetailWidth);
            }).Align(TextAnchor.UpperLeft).Gap(12f).FillHeight();
        }

        private void BuildInventorySlot(UIBuilderContext grid, ItemInstance instance)
        {
            bool isSelected = instance == _selectedInventoryItem;
            Color rarityColor = instance.Item.Rarity.GetColor();

            grid.Column(slot =>
            {
                slot.Label(SlotHeader(instance, rarityColor)).FontSize(11f).Bold().TextColor(SlotTextColor);
                if (instance.Item.Icon != null) slot.Image(instance.Item.Icon).Size(44f, 44f);
                slot.Label(instance.Name).FontSize(11f).Bold().TextColor(rarityColor).TextAlign(TextAlignmentOptions.Center);
                slot.Label(SlotFooter(instance)).FontSize(10f).TextColor(SlotTextColor).TextAlign(TextAlignmentOptions.Center);
            }).Background(isSelected ? grid.Theme.Selected : SlotColor)
              .Padding(6)
              .Gap(2f)
              .Align(TextAnchor.UpperCenter)
              .OnClick(() =>
              {
                  _selectedInventoryItem = instance;
                  RequestRebuild();
              });
        }

        private static string SlotHeader(ItemInstance instance, Color rarityColor)
        {
            string header = $"<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{instance.Item.Rarity.ToString().Substring(0, 1)}</color>";
            if (instance is EquipmentInstance eq)
            {
                if (eq.IsEquipped) header += "  <color=#33FF66>E</color>";
                if (eq.Refine > 0) header += $"  <color=#FFD94D>+{eq.Refine}</color>";
            }
            else if (instance is CostumeInstance costume && costume.IsEquipped)
            {
                header += "  <color=#33FF66>E</color>";
            }
            else if (instance.Quantity > 1)
            {
                header += $"  x{instance.Quantity}";
            }
            return header;
        }

        private static string SlotFooter(ItemInstance instance)
        {
            if (!(instance is EquipmentInstance eq)) return instance.Item.Type.ToString();

            string footer = "";
            int maxSockets = (eq.Item as Equipment)?.MaxSockets ?? 0;
            if (maxSockets > 0)
            {
                int used = eq.SocketedFragments != null ? eq.SocketedFragments.Count(f => f != null) : 0;
                footer = $"S {used}/{maxSockets}";
            }
            if (eq.Reforge > 0) footer += $"  R{eq.Reforge}";
            return footer;
        }

        private void BuildInventoryDetail(UIBuilderContext ui)
        {
            var selected = _selectedInventoryItem;
            if (selected == null || selected.Item == null)
            {
                ui.Caption("Select an item slot to see details and actions.");
                return;
            }

            var item = selected.Item;
            ui.Caption($"UID {selected.Id}");
            ui.Heading(selected.Name).TextColor(item.Rarity.GetColor());
            ui.KeyValue("Rarity", item.Rarity.ToString());
            ui.KeyValue("Type", item.Type.ToString());

            if (selected is EquipmentInstance eq)
            {
                ui.KeyValue("Status", eq.IsEquipped ? "<color=#1E9E4F>Equipped</color>" : "In bag");
                ui.KeyValue("Refine", $"+{eq.Refine}");
                ui.KeyValue("Advancement", $"Tier {eq.Reforge}");

                int maxSockets = (eq.Item as Equipment)?.MaxSockets ?? 0;
                if (maxSockets > 0)
                {
                    int used = eq.SocketedFragments != null ? eq.SocketedFragments.Count(f => f != null) : 0;
                    ui.KeyValue("Sockets", $"{used} / {maxSockets}");
                    if (eq.SocketedFragments != null)
                    {
                        for (int s = 0; s < eq.SocketedFragments.Count; s++)
                        {
                            var fragment = eq.SocketedFragments[s];
                            if (fragment != null) ui.Caption($"  [{s + 1}] {fragment.Name}").TextColor(fragment.Rarity.GetColor());
                        }
                    }
                }
            }
            else if (selected is CostumeInstance costume)
            {
                ui.KeyValue("Status", costume.IsEquipped ? "<color=#1E9E4F>Equipped</color>" : "In wardrobe");
            }

            if (selected.Quantity > 1) ui.KeyValue("Quantity", $"x{selected.Quantity}");
            if (!string.IsNullOrEmpty(item.Description)) ui.Label(item.Description);

            ui.Separator();

            if (item is Equipment)
            {
                bool isEquipped = selected is EquipmentInstance equipped && equipped.IsEquipped;
                ui.Button(isEquipped ? "Unequip" : "Equip", () =>
                {
                    if (isEquipped) UnequipFromSlot(selected);
                    else GameData.Player.Equipment.SmartEquip(selected);
                    RequestRebuild();
                }, UIVariant.Primary).Fill();
            }
            else if (item is Costume)
            {
                bool isEquipped = selected is CostumeInstance worn && worn.IsEquipped;
                ui.Button(isEquipped ? "Take Off" : "Wear", () =>
                {
                    GameData.Player.Costumes.Equip(selected);
                    RequestRebuild();
                }, UIVariant.Primary).Fill();
            }

            ui.Row(actions =>
            {
                ChipButton(actions, "Discard", () =>
                {
                    GameData.Player.Inventory.RemoveItemByInstance(selected.Id, 1);
                    if (selected.Quantity <= 1) _selectedInventoryItem = null;
                    RequestRebuild();
                }, UIVariant.Danger).Fill();

                if (selected.Quantity > 1)
                {
                    ChipButton(actions, "Discard All", () =>
                    {
                        GameData.Player.Inventory.RemoveItemByInstance(selected.Id, selected.Quantity);
                        _selectedInventoryItem = null;
                        RequestRebuild();
                    }, UIVariant.Danger).Fill();
                }
            }).Gap(6f);
        }

        private void UnequipFromSlot(ItemInstance instance)
        {
            for (int i = 0; i < _player.EquipmentSlots.Length; i++)
            {
                if (_player.EquipmentSlots[i] == instance)
                {
                    GameData.Player.Equipment.Equip(i, null);
                    return;
                }
            }
        }

        private void BuildEquipmentPage(UIBuilderContext ui)
        {
            ui.Heading("Equipment");
            ui.Panel(panel =>
            {
                for (int i = 0; i < _player.EquipmentSlots.Length; i++)
                {
                    var instance = _player.EquipmentSlots[i];
                    int slotIndex = i;
                    SlotRow(panel, $"Slot {i}", instance != null ? $"[{instance.Id}] {instance.Name}" : null, "Unequip", () =>
                    {
                        GameData.Player.Equipment.Equip(slotIndex, null);
                        RequestRebuild();
                    });
                }
            });

            ui.Heading("Costumes");
            ui.Panel(panel =>
            {
                for (int i = 0; i < _player.CostumeSlots.Length; i++)
                {
                    var instance = _player.CostumeSlots[i];
                    int slotIndex = i;
                    SlotRow(panel, ((CostumeSlot)i).ToString(), instance != null ? $"[{instance.Id}] {instance.Name}" : null, "Take Off", () =>
                    {
                        GameData.Player.Costumes.Unequip(slotIndex);
                        RequestRebuild();
                    });
                }
            });

            if (_player.skillCaster == null) return;

            ui.Heading("Equipped Skills");
            ui.Panel(panel =>
            {
                for (int i = 0; i < _player.skillCaster.EquippedSkills.Count; i++)
                {
                    var skill = _player.skillCaster.EquippedSkills[i];
                    int slotIndex = i;
                    string name = skill == null ? null : skill.SkillData != null ? skill.SkillData.Name : "Data Missing";
                    SlotRow(panel, $"Slot {i + 1}", name, "Unequip", () =>
                    {
                        _player.skillCaster.EquipSkill((SkillInstance)null, slotIndex);
                        RequestRebuild();
                    });
                }
            });
        }

        private static void SlotRow(UIBuilderContext ui, string slot, string content, string actionLabel, System.Action action)
        {
            ui.Row(row =>
            {
                row.Caption(slot).Width(90f);
                if (content != null)
                {
                    row.Label(content).Fill();
                    row.Button(actionLabel, action, UIVariant.Secondary).MinWidth(90f);
                }
                else
                {
                    row.Caption("Empty").Fill();
                }
            }).Gap(8f);
        }

        private void BuildSocketingPage(UIBuilderContext ui)
        {
            ui.Heading("Socketing");
            ui.Row(columns =>
            {
                columns.Panel(left =>
                {
                    left.Caption("Equipped item");
                    for (int i = 0; i < _player.EquipmentSlots.Length; i++)
                    {
                        var inst = _player.EquipmentSlots[i] as EquipmentInstance;
                        if (inst == null) continue;

                        ListButton(left, $"[{i}] {inst.Name}", () =>
                        {
                            _selectedSocketItem = inst;
                            RequestRebuild();
                        }, inst == _selectedSocketItem);
                    }
                }).Fill();

                columns.Panel(right =>
                {
                    right.Caption("Fragment from inventory");
                    var fragments = GameData.Player.Inventory.GetAllItems().Where(it => it.Item is Fragment).ToList();
                    if (fragments.Count == 0)
                    {
                        right.Caption("No fragments in inventory.");
                        return;
                    }

                    foreach (var fragment in fragments)
                    {
                        ListButton(right, $"{fragment.Name}  (x{fragment.Quantity})", () =>
                        {
                            _selectedFragment = fragment;
                            RequestRebuild();
                        }, fragment == _selectedFragment);
                    }
                }).Fill();
            }).Align(TextAnchor.UpperLeft).Gap(12f);

            if (_selectedSocketItem == null) return;

            int maxSockets = (_selectedSocketItem.Item as Equipment)?.MaxSockets ?? 0;
            ui.Panel(action =>
            {
                action.Heading($"Applying to {_selectedSocketItem.Name}");
                action.KeyValue("Sockets", $"{_selectedSocketItem.SocketedFragments.Count} / {maxSockets}");
                foreach (var socketed in _selectedSocketItem.SocketedFragments)
                {
                    action.Caption($"  - {socketed.Name}");
                }

                if (_selectedFragment != null)
                {
                    action.Button($"Socket {_selectedFragment.Name} into {_selectedSocketItem.Name}", () =>
                    {
                        GameData.Player.Equipment.SocketFragment(_selectedSocketItem, _selectedFragment);
                        if (_selectedFragment.Quantity <= 0) _selectedFragment = null;
                        RequestRebuild();
                    }, UIVariant.Primary);
                }
            });
        }

        private void BuildAttributesPage(UIBuilderContext ui)
        {
            if (_player.attributeSystem == null) return;

            ui.Heading("Primary Attributes");
            ui.Panel(panel =>
            {
                foreach (var type in PrimaryAttributes)
                {
                    var attr = _player.attributeSystem.GetAttribute(type);
                    if (attr == null) continue;

                    panel.Row(row =>
                    {
                        row.Label(type.ToString()).Bold().Width(60f);
                        row.Label($"{attr.CurrentValue:F0}").Bold().Width(60f);
                        row.Caption($"base {attr.BaseValue:F0}").Fill();
                        ChipButton(row, "-", () => ChangeAttribute(attr, -1)).MinWidth(44f);
                        ChipButton(row, "+", () => ChangeAttribute(attr, 1)).MinWidth(44f);
                        ChipButton(row, "+10", () => ChangeAttribute(attr, 10)).MinWidth(56f);
                    }).Gap(6f);
                }
            });

            ui.Heading("Experience");
            ui.Panel(panel =>
            {
                int baseLevel = (int)_player.attributeSystem.GetAttribute(AttributeType.BaseLevel).BaseValue;
                long nextBase = _player.expSystem.GetNextBaseExp(baseLevel);
                ExperienceRow(panel, $"Base Lv {baseLevel}", _player.expSystem.CurrentBaseExp, nextBase, amount => _player.expSystem.AddBaseExp(amount));

                int jobLevel = (int)_player.attributeSystem.GetAttribute(AttributeType.JobLevel).BaseValue;
                long nextJob = _player.expSystem.GetNextJobExp(jobLevel);
                ExperienceRow(panel, $"Job Lv {jobLevel}", _player.expSystem.CurrentJobExp, nextJob, amount => _player.expSystem.AddJobExp(amount));
            });
        }

        private void ChangeAttribute(Attribute attr, int delta)
        {
            attr.BaseValue = Mathf.Max(1, attr.BaseValue + delta);
            _player.attributeSystem.UpdateDerivedAttributes();
            RequestRebuild();
        }

        private void ExperienceRow(UIBuilderContext ui, string label, long current, long next, System.Action<int> addExp)
        {
            ui.Row(row =>
            {
                row.Label(label).Bold().Width(90f);
                row.Caption($"{current} / {next}").Fill();
                ChipButton(row, "+100", () => { addExp(100); RequestRebuild(); }).MinWidth(64f);
                ChipButton(row, "+1000", () => { addExp(1000); RequestRebuild(); }).MinWidth(72f);
            }).Gap(6f);
            ui.Progress(next > 0 ? (float)current / next : 0f);
        }

        private void BuildSkillsPage(UIBuilderContext ui)
        {
            ui.Heading("Granted Skills");
            ui.Panel(panel =>
            {
                if (_player.skillCaster.GrantedSkills.Count == 0) panel.Caption("No skills granted.");
                foreach (var granted in _player.skillCaster.GrantedSkills)
                {
                    bool isSelected = _selectedSkillForSupports == granted;
                    panel.Row(row =>
                    {
                        row.Label($"[{granted.SkillData.Id}] {granted.SkillData.Name}").Fill();
                        for (int slot = 0; slot < 3; slot++)
                        {
                            int slotIndex = slot;
                            ChipButton(row, $"Eq {slot + 1}", () =>
                            {
                                _player.skillCaster.EquipSkill(granted, slotIndex);
                                RequestRebuild();
                            }).MinWidth(56f);
                        }
                        ChipButton(row, "Select", () =>
                        {
                            _selectedSkillForSupports = isSelected ? null : granted;
                            RequestRebuild();
                        }, isSelected ? UIVariant.Selected : UIVariant.Default).MinWidth(70f);
                        ChipButton(row, "Revoke", () =>
                        {
                            _player.skillCaster.RevokeSkill(granted.SkillData);
                            if (_selectedSkillForSupports == granted) _selectedSkillForSupports = null;
                            RequestRebuild();
                        }, UIVariant.Danger).MinWidth(70f);
                    }).Gap(6f);
                }
            });

            ui.Heading("All Skills");
            ui.Panel(panel =>
            {
                foreach (var skill in Database.Skills)
                {
                    if (!Matches(skill.Name)) continue;
                    panel.Row(row =>
                    {
                        row.Label($"[{skill.Id}] {skill.Name}").Fill();
                        row.Caption(skill.SkillElement.ToString()).Width(80f);
                        row.Button("Grant", () =>
                        {
                            _player.skillCaster.GrantSkill(skill);
                            RequestRebuild();
                        }, UIVariant.Primary).MinWidth(70f);
                    }).Gap(6f);
                }
            });
        }

        private void BuildUpgradePage(UIBuilderContext ui)
        {
            ui.Heading("Equipment Upgrade");
            ui.Row(columns =>
            {
                columns.Panel(left =>
                {
                    left.Caption("Your equipment");
                    var equipment = GameData.Player.Inventory.GetAllItems().OfType<EquipmentInstance>().ToList();
                    foreach (var inst in equipment)
                    {
                        if (!Matches(inst.Name)) continue;
                        ListButton(left, $"+{inst.Refine}  {inst.Name}", () =>
                        {
                            _selectedUpgradeItem = inst;
                            RequestRebuild();
                        }, inst == _selectedUpgradeItem);
                    }
                }).Fill();

                columns.Panel(BuildUpgradeDetail).Width(DetailWidth + 40f);
            }).Align(TextAnchor.UpperLeft).Gap(12f);
        }

        private void BuildUpgradeDetail(UIBuilderContext ui)
        {
            var target = _selectedUpgradeItem;
            if (target == null)
            {
                ui.Caption("Select a piece of equipment to upgrade.");
                return;
            }

            int maxRefine = EquipmentDatabase.RefineTable.Length - 1;
            float bonus = EquipmentDatabase.GetRefineMultiplier(target.Refine) * 100f;

            ui.Heading(target.Name);
            ui.Row(row =>
            {
                row.KeyValue("Refine", $"{target.Refine} / {maxRefine}").Fill();
                ChipButton(row, "Reset", () =>
                {
                    target.Refine = 0;
                    RequestRebuild();
                }).MinWidth(70f);
            }).Gap(6f);
            ui.Progress(maxRefine > 0 ? (float)target.Refine / maxRefine : 0f, UIVariant.Warning);
            ui.KeyValue("Bonus", $"+{bonus:F0}% base stats");
            ui.KeyValue("Tier", target.Reforge.ToString());
            ui.Separator();

            ui.Button("Refine +1", () =>
            {
                target.Refine = Mathf.Min(maxRefine, target.Refine + 1);
                RefreshEquipped(target);
            }, UIVariant.Primary).Fill();

            ui.Button("Reforge / Advance Tier +1", () =>
            {
                target.Reforge += 1;
                RefreshEquipped(target);
            }, UIVariant.Success).Fill();
        }

        private void RefreshEquipped(EquipmentInstance target)
        {
            if (target.IsEquipped) (target.Item as Equipment)?.ForceRefresh(_player, target);
            RequestRebuild();
        }

        private void BuildPassivePage(UIBuilderContext ui)
        {
            ui.Heading("Passive Points");
            ui.Panel(panel =>
            {
                panel.KeyValue("Available", _player.PassivePoints.ToString());
                panel.Row(row =>
                {
                    ChipButton(row, "+1 Point", () => { _player.PassivePoints++; RequestRebuild(); });
                    ChipButton(row, "+10 Points", () => { _player.PassivePoints += 10; RequestRebuild(); });
                    row.Spacer();
                    row.Button("Reset All", () =>
                    {
                        _player.PassivePoints += _player.PassiveNodes.Count;
                        _player.PassiveNodes.Clear();
                        _player.RecalculateStats();
                        PassiveTreeWindow.Instance?.LoadFromCharacter(_player);
                        RequestRebuild();
                    }, UIVariant.Danger);
                }).Gap(6f);
            });

            ui.Heading("Add Passive Record");
            ui.Panel(panel =>
            {
                panel.Row(row =>
                {
                    row.Caption("Node");
                    row.Input(_pendingPassive.NodeId.ToString(), value => { if (int.TryParse(value, out int v)) _pendingPassive.NodeId = v; }).Width(80f);
                    row.Caption("Attribute");
                    row.Input(_pendingPassive.AttributeId.ToString(), value => { if (int.TryParse(value, out int v)) _pendingPassive.AttributeId = v; }).Width(80f);
                    row.Caption("Value");
                    row.Input(_pendingPassive.Value.ToString(), value => { if (float.TryParse(value, out float v)) _pendingPassive.Value = v; }).Width(80f);
                    row.Spacer();
                    row.Button("Unlock & Apply", () =>
                    {
                        if (_player.PassivePoints <= 0) return;
                        _player.PassiveNodes.Add(new PassiveNodeRecord(_pendingPassive.NodeId, _pendingPassive.AttributeId, _pendingPassive.Value));
                        _player.PassivePoints--;
                        _player.RecalculateStats();
                        RequestRebuild();
                    }, UIVariant.Primary);
                }).Gap(6f);
            });

            ui.Heading($"Unlocked Records  ({_player.PassiveNodes.Count})");
            ui.Panel(panel =>
            {
                if (_player.PassiveNodes.Count == 0) panel.Caption("No passive records.");
                foreach (var record in _player.PassiveNodes.ToList())
                {
                    panel.Row(row =>
                    {
                        row.Label($"Node {record.NodeId}").Width(90f);
                        row.Label(((AttributeType)record.AttributeId).ToString()).Fill();
                        row.Caption(record.Value.ToString()).Width(60f);
                        ChipButton(row, "X", () =>
                        {
                            _player.PassiveNodes.Remove(record);
                            _player.PassivePoints++;
                            _player.RecalculateStats();
                            RequestRebuild();
                        }, UIVariant.Danger).MinWidth(36f);
                    }).Gap(6f);
                }
            });
        }

        private void BuildUniquePassivesPage(UIBuilderContext ui)
        {
            ui.Heading("Unique Passives");
            ui.Panel(panel =>
            {
                foreach (var passive in Database.UniquePassives)
                {
                    if (!Matches(passive.Name) && !Matches(passive.Id)) continue;

                    bool isActive = _player.UniquePassives.HasPassive(passive.Id);
                    panel.Row(row =>
                    {
                        row.Label(passive.Name).Fill();
                        row.Caption(passive.Id).Width(160f);
                        row.Button(isActive ? "Remove" : "Add", () =>
                        {
                            if (isActive) _player.UniquePassives.RemovePassive(passive.Id);
                            else _player.UniquePassives.AddPassive(passive.Id);
                            _player.RecalculateStats();
                            RequestRebuild();
                        }, isActive ? UIVariant.Danger : UIVariant.Success).MinWidth(80f);
                    }).Gap(6f);
                }
            });
        }
        #endregion
    }
}
