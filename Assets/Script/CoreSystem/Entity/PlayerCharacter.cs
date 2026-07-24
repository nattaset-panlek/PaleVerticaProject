using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
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
            GameData.Player.CharacterManager.Bind(this.attributeSystem);
            GameData.Player.SkillManager.Bind(this.skillCaster);
            GameData.Player.EquipmentManager.Bind(this);
            GameData.Player.CostumeManager.Bind(this);

            // Screen Shake on Take Damage
            OnTakeDamage += (attacker, damage) => {
                CoreSystem.Controllers.CameraController.Instance?.ScreenShake(0.15f, 0.4f);
            };
        }

        [Header("Debug UI")]
        public bool ShowDebugUI = true;
        private Rect _debugWindowRect = new Rect(20, 45, 450, 600);
        private int _selectedTab = 0;
        private Vector2 _scrollPos;
        private string[] _tabLabels = { "DB", "Inv", "Equip", "Socket", "Attr", "Skill", "Upgrade", "Passive", "U-Passive" };
        private bool _showFullMenu = true;
        private string _searchQuery = "";
        private EquipmentInstance _selectedUpgradeItem;
        private SkillInstance _selectedSkillForSupports;
        private const int MaxLinkedSupportsLimit = -1;

        [Header("Passive Skill Tree")]
        public List<PassiveNodeRecord> PassiveNodes = new List<PassiveNodeRecord>();
        public int PassivePoints = 0;

        private void OnGUI()
        {
            if (!ShowDebugUI) return;
            
            // Draw a toggle button for min/max window
            bool newShowFullMenu = GUI.Toggle(new Rect(20, 20, 150, 20), _showFullMenu, " Show Debug Menu");
            if (newShowFullMenu != _showFullMenu)
            {
                _showFullMenu = newShowFullMenu;
            }

            if (_showFullMenu)
            {
                _debugWindowRect = GUI.Window(0, _debugWindowRect, DrawDebugWindow, "Debug Menu - Player Character");
            }
        }

        private void DrawDebugWindow(int windowID)
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabLabels);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            _searchQuery = GUILayout.TextField(_searchQuery);
            if (GUILayout.Button("Clear", GUILayout.Width(50))) _searchQuery = "";
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0: DrawDatabaseTab(); break;
                case 1: DrawInventoryTab(); break;
                case 2: DrawEquipmentTab(); break;
                case 3: DrawSocketingTab(); break;
                case 4: DrawAttributesTab(); break;
                case 5: DrawSkillsTab(); break;
                case 6: DrawUpgradeTab(); break;
                case 7: DrawPassiveTab(); break;
                case 8: DrawUniquePassivesTab(); break;
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        private PassiveNodeRecord _pendingPassive = new PassiveNodeRecord();

        private void DrawPassiveTab()
        {
            GUILayout.Label("--- Passive Point Settings ---", GUI.skin.box);
            GUILayout.Label($"Current Passive Points: <color=yellow><b>{PassivePoints}</b></color>", new GUIStyle(GUI.skin.label) { richText = true });

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1 PT")) PassivePoints++;
            if (GUILayout.Button("+10 PT")) PassivePoints += 10;
            if (GUILayout.Button("RESET ALL", GUILayout.Width(90)))
            {
                PassivePoints += PassiveNodes.Count;
                PassiveNodes.Clear();
                RecalculateStats();
                FindObjectOfType<PassiveSkillLoader>()?.LoadFromPlayer(this);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("--- Add Passive Record ---", GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("NodeId", GUILayout.Width(55));
            string nidStr = GUILayout.TextField(_pendingPassive.NodeId.ToString(), GUILayout.Width(50));
            if (int.TryParse(nidStr, out int pnid)) _pendingPassive.NodeId = pnid;

            GUILayout.Label("AttrId", GUILayout.Width(50));
            string aidStr = GUILayout.TextField(_pendingPassive.AttributeId.ToString(), GUILayout.Width(50));
            if (int.TryParse(aidStr, out int paid)) _pendingPassive.AttributeId = paid;

            GUILayout.Label("Val", GUILayout.Width(30));
            string valStr = GUILayout.TextField(_pendingPassive.Value.ToString("F1"), GUILayout.Width(55));
            if (float.TryParse(valStr, out float pval)) _pendingPassive.Value = pval;
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Unlock & Apply", GUILayout.Height(28)))
            {
                if (PassivePoints > 0)
                {
                    PassiveNodes.Add(new PassiveNodeRecord(_pendingPassive.NodeId, _pendingPassive.AttributeId, _pendingPassive.Value));
                    PassivePoints--;
                    RecalculateStats();
                }
                else
                {
                    Debug.LogWarning("[PassiveTab] No passive points remaining.");
                }
            }

            GUILayout.Space(10);
            GUILayout.Label($"Unlocked Records: {PassiveNodes.Count}", GUI.skin.box);

            PassiveNodeRecord toRemove = null;
            foreach (var rec in PassiveNodes)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Node:{rec.NodeId}  Attr:{(AttributeType)rec.AttributeId} ({rec.AttributeId})  Val:{rec.Value:+0.##;-0.##;0}", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("X", GUILayout.Width(25))) toRemove = rec;
                GUILayout.EndHorizontal();
            }
            if (toRemove != null)
            {
                PassiveNodes.Remove(toRemove);
                PassivePoints++;
                RecalculateStats();
            }
        }

        private void DrawUniquePassivesTab()
        {
            GUILayout.Label("Unique Passive Settings", GUI.skin.box);
            var passives = Database.UniquePassives;
            if (passives == null || passives.Count == 0)
            {
                GUILayout.Label("No unique passives in database.");
                return;
            }
            foreach (var passive in passives)
            {
                if (!string.IsNullOrEmpty(_searchQuery) && 
                    !passive.Name.ToLower().Contains(_searchQuery.ToLower()) && 
                    !passive.Id.ToLower().Contains(_searchQuery.ToLower()))
                {
                    continue;
                }
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.BeginVertical();
                GUILayout.Label($"<b>{passive.Name}</b> ({passive.Id})", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Label(passive.Description, new GUIStyle(GUI.skin.label) { wordWrap = true });
                GUILayout.EndVertical();
                bool isActive = UniquePassives.HasPassive(passive.Id);
                if (isActive)
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Remove", GUILayout.Width(80), GUILayout.Height(30)))
                    {
                        UniquePassives.RemovePassive(passive.Id);
                        RecalculateStats();
                    }
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Add", GUILayout.Width(80), GUILayout.Height(30)))
                    {
                        UniquePassives.AddPassive(passive.Id);
                        RecalculateStats();
                    }
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
            }
        }

        private Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();

        private void DrawDatabaseTab()
        {
            GUILayout.Label("--- All Possible Items (Models) ---", GUI.skin.box);

            var allItems = Database.GetItems;

            if (GUILayout.Button("DEBUG: ADD ALL ITEMS (GLOBAL)", GUILayout.Height(30)))
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorUtility.DisplayDialog("Confirm", "Do you want to add every item in the database?", "Yes", "No"))
                {
                    foreach (var item in allItems) GameData.Player.Inventory.AddItem(item);
                }
#else
                foreach (var item in allItems) GameData.Player.Inventory.AddItem(item);
#endif
            }

            Dictionary<ItemType, List<Item>> groupedItems = new Dictionary<ItemType, List<Item>>();
            foreach (var item in allItems)
            {
                if (!groupedItems.ContainsKey(item.Type)) groupedItems[item.Type] = new List<Item>();
                groupedItems[item.Type].Add(item);
            }

            foreach (var group in groupedItems)
            {
                string typeName = group.Key.ToString();
                if (!_foldouts.ContainsKey(typeName)) _foldouts[typeName] = false;

                GUILayout.BeginHorizontal();
                _foldouts[typeName] = GUILayout.Toggle(_foldouts[typeName], $" {typeName} ({group.Value.Count})", "Button", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Add All", GUILayout.Width(70)))
                {
                    foreach (var item in group.Value) GameData.Player.Inventory.AddItem(item);
                }
                GUILayout.EndHorizontal();

                if (_foldouts[typeName])
                {
                    if (group.Key == ItemType.Equipment)
                    {
                        Dictionary<EquipmentSlot, List<Equipment>> slotGroups = new Dictionary<EquipmentSlot, List<Equipment>>();
                        foreach (var item in group.Value)
                        {
                            var e = item as Equipment;
                            if (e != null)
                            {
                                if (!slotGroups.ContainsKey(e.Slot)) slotGroups[e.Slot] = new List<Equipment>();
                                slotGroups[e.Slot].Add(e);
                            }
                        }

                        foreach (var slotGroup in slotGroups)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"  [ {slotGroup.Key} ]", GUI.skin.box, GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("Add All", GUILayout.Width(70)))
                            {
                                foreach (var item in slotGroup.Value) GameData.Player.Inventory.AddItem(item);
                            }
                            GUILayout.EndHorizontal();

                            foreach (var equip in slotGroup.Value)
                            {
                                if (!string.IsNullOrEmpty(_searchQuery) && !equip.Name.ToLower().Contains(_searchQuery.ToLower())) continue;
                                DrawItemRow(equip);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in group.Value)
                        {
                            if (!string.IsNullOrEmpty(_searchQuery) && !item.Name.ToLower().Contains(_searchQuery.ToLower())) continue;
                            DrawItemRow(item);
                        }
                    }
                    GUILayout.Space(5);
                }
            }
        }

        private void DrawInventoryTab()
        {
            GUILayout.Label("--- Player Held Items (Instances) ---", GUI.skin.box);
            var instances = GameData.Player.Inventory.GetAllItems();
            if (instances.Count == 0)
            {
                GUILayout.Label("Inventory is empty.");
                return;
            }

            foreach (var instance in instances)
            {
                if (!string.IsNullOrEmpty(_searchQuery) && !instance.Name.ToLower().Contains(_searchQuery.ToLower())) continue;

                GUILayout.BeginHorizontal();
                GUILayout.Label($"[{instance.Id}] {instance.Name} x{instance.Quantity}");

                if (instance.Item is Equipment equip)
                {
                    if (GUILayout.Button("Equip", GUILayout.Width(60)))
                    {
                        GameData.Player.EquipmentManager.SmartEquip(instance);
                    }
                }
                else if (instance.Item is Costume costume)
                {
                    if (GUILayout.Button("Wear", GUILayout.Width(60)))
                    {
                        GameData.Player.CostumeManager.Equip(instance);
                    }
                }

                if (GUILayout.Button("Discard", GUILayout.Width(70)))
                {
                    GameData.Player.Inventory.RemoveItemByInstance(instance.Id, instance.Quantity);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawUpgradeTab()
        {
            GUILayout.Label("--- Equipment Upgrade System ---", GUI.skin.box);
            GUILayout.BeginHorizontal();

            // คอลัมน์ซ้าย: รายชื่ออุปกรณ์ในตัว
            GUILayout.BeginVertical(GUILayout.Width(180));
            GUILayout.Label("Your Equipments:");
            var invItems = GameData.Player.Inventory.GetAllItems();
            var equipInstances = invItems.Where(it => it is EquipmentInstance).Cast<EquipmentInstance>().ToList();

            foreach (var inst in equipInstances)
            {
                // ระบบ Search Filter
                if (!string.IsNullOrEmpty(_searchQuery) && !inst.Name.ToLower().Contains(_searchQuery.ToLower()))
                    continue;

                bool isSelected = (_selectedUpgradeItem == inst);
                GUI.color = isSelected ? Color.yellow : Color.white;

                if (GUILayout.Button($"[{inst.Refine}] {inst.Name}"))
                {
                    _selectedUpgradeItem = inst;
                }
            }
            GUI.color = Color.white;
            GUILayout.EndVertical();

            // คอลัมน์ขวา: ส่วนควบคุมการตีบวก
            GUILayout.BeginVertical(GUI.skin.box);
            if (_selectedUpgradeItem != null)
            {
                GUILayout.Label($"UPGRADING: <color=yellow>{_selectedUpgradeItem.Name}</color>", new GUIStyle(GUI.skin.label) { richText = true, fontStyle = FontStyle.Bold });
                GUILayout.Space(10);

                // --- ส่วน Refine ---
                int maxRefine = EquipmentDatabase.RefineTable.Length - 1;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Refine Level: <b>{_selectedUpgradeItem.Refine} / {maxRefine}</b>", new GUIStyle(GUI.skin.label) { richText = true });
                if (GUILayout.Button("Reset", GUILayout.Width(50))) _selectedUpgradeItem.Refine = 0;
                GUILayout.EndHorizontal();

                float currentMult = EquipmentDatabase.GetRefineMultiplier(_selectedUpgradeItem.Refine) * 100f;
                GUILayout.Label($"Current Bonus: <color=#006FC0>+{currentMult:F0}% Base Stats</color>", new GUIStyle(GUI.skin.label) { richText = true });

                if (GUILayout.Button("REFINE (+1)", GUILayout.Height(35)))
                {
                    _selectedUpgradeItem.Refine = Mathf.Min(maxRefine, _selectedUpgradeItem.Refine + 1);

                    if (_selectedUpgradeItem.IsEquipped)
                    {
                        (_selectedUpgradeItem.Item as Equipment)?.ForceRefresh(this, _selectedUpgradeItem);
                    }
                }

                GUILayout.Space(20);
                GUI.backgroundColor = Color.cyan;

                // --- ส่วน Reforge ---
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Reforge Level: <b>{_selectedUpgradeItem.Reforge} / 100</b>", new GUIStyle(GUI.skin.label) { richText = true });
                if (GUILayout.Button("Reset", GUILayout.Width(50))) _selectedUpgradeItem.Reforge = 0;
                GUILayout.EndHorizontal();

                float currentFlat = _selectedUpgradeItem.Reforge * EquipmentDatabase.ReforgeValuePerLevel;
                GUILayout.Label($"Current Bonus: <color=#006FC0>+{currentFlat:F0} Flat Stats</color>", new GUIStyle(GUI.skin.label) { richText = true });

                if (GUILayout.Button("REFORGE (+1)", GUILayout.Height(35)))
                {
                    _selectedUpgradeItem.Reforge = Mathf.Min(100, _selectedUpgradeItem.Reforge + 1);

                    if (_selectedUpgradeItem.IsEquipped)
                    {
                        (_selectedUpgradeItem.Item as Equipment)?.ForceRefresh(this, _selectedUpgradeItem);
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Select an item to upgrade");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        private void DrawItemRow(Item item)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label($"[{item.Id}] {item.Name}", GUILayout.ExpandWidth(true));
            GUILayout.Label($"{item.Rarity}", GUILayout.Width(80));

            if (GUILayout.Button("Collect", GUILayout.Width(70)))
            {
                GameData.Player.Inventory.AddItem(item);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawEquipmentTab()
        {
            GUILayout.Label("--- Current Equipment ---", GUI.skin.box);
            for (int i = 0; i < EquipmentSlots.Length; i++)
            {
                var instance = EquipmentSlots[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Slot {i}: {(instance != null ? $"[{instance.Id}] {instance.Name}" : "Empty")}");
                if (instance != null)
                {
                    if (GUILayout.Button("X", GUILayout.Width(30))) GameData.Player.EquipmentManager.Equip(i, null);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("--- Current Costumes ---", GUI.skin.box);
            for (int i = 0; i < CostumeSlots.Length; i++)
            {
                var instance = CostumeSlots[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Slot {(CostumeSlot)i}: {(instance != null ? $"[{instance.Id}] {instance.Name}" : "Empty")}");
                if (instance != null)
                {
                    if (GUILayout.Button("X", GUILayout.Width(30))) GameData.Player.CostumeManager.Unequip(i);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("--- Equipped Skills ---", GUI.skin.box);
            if (skillCaster != null)
            {
                for (int i = 0; i < skillCaster.EquippedSkills.Count; i++)
                {
                    var skill = skillCaster.EquippedSkills[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Slot {i + 1}: {(skill != null ? (skill.SkillData != null ? skill.SkillData.Name : "Data Missing") : "Empty")}");
                    if (skill != null)
                    {
                        if (GUILayout.Button("X", GUILayout.Width(30)))
                        {
                            skillCaster.EquipSkill((SkillInstance)null, i);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        private EquipmentInstance _selectedSocketItem;
        private ItemInstance _selectedFragment;

        private void DrawSocketingTab()
        {
            GUILayout.Label("--- Socketing System ---", GUI.skin.box);

            GUILayout.BeginHorizontal();
            
            // Left Column: Equipped Items
            GUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Label("Select Equipped Item:");
            for (int i = 0; i < EquipmentSlots.Length; i++)
            {
                var inst = EquipmentSlots[i] as EquipmentInstance;
                if (inst == null) continue;
                
                GUI.color = (_selectedSocketItem == inst) ? Color.green : Color.white;
                if (GUILayout.Button($"[{i}] {inst.Name}"))
                {
                    _selectedSocketItem = inst;
                }
            }
            GUI.color = Color.white;
            GUILayout.EndVertical();

            // Right Column: Fragments in Inventory
            GUILayout.BeginVertical();
            GUILayout.Label("Select Fragment from Inventory:");
            var fragments = GameData.Player.Inventory.GetAllItems().Where(it => it.Item is Fragment).ToList();
            if (fragments.Count == 0)
            {
                GUILayout.Label("No fragments in inventory.");
            }
            else
            {
                foreach (var frag in fragments)
                {
                    GUI.color = (_selectedFragment == frag) ? Color.green : Color.white;
                    if (GUILayout.Button($"{frag.Name} (x{frag.Quantity})"))
                    {
                        _selectedFragment = frag;
                    }
                }
            }
            GUI.color = Color.white;
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Action Section
            if (_selectedSocketItem != null)
            {
                var equip = _selectedSocketItem.Item as Equipment;
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"Applying to: {_selectedSocketItem.Name}");
                GUILayout.Label($"Sockets: {_selectedSocketItem.SocketedFragments.Count} / {equip.MaxSockets}");
                
                foreach(var f in _selectedSocketItem.SocketedFragments)
                {
                    GUILayout.Label($"  - {f.Name}");
                }

                if (_selectedFragment != null)
                {
                    if (GUILayout.Button($"SOCKET {_selectedFragment.Name} into {_selectedSocketItem.Name}", GUILayout.Height(40)))
                    {
                        GameData.Player.EquipmentManager.SocketFragment(_selectedSocketItem, _selectedFragment);
                        if (_selectedFragment.Quantity <= 0) _selectedFragment = null;
                    }
                }
                else
                {
                    GUILayout.Label("Please select a fragment above.");
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("Please select an item from the left column.");
            }
        }

        private void DrawAttributesTab()
        {
            GUILayout.Label("--- Primary Attributes (Adjustable) ---", GUI.skin.box);
            if (attributeSystem != null)
            {
                AttributeType[] primaryTypes = { 
                    AttributeType.STR, AttributeType.AGI, AttributeType.VIT, 
                    AttributeType.INT, AttributeType.DEX, AttributeType.LUK 
                };

                foreach (var type in primaryTypes)
                {
                    var attr = attributeSystem.GetAttribute(type);
                    if (attr != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{type}: <b>{attr.CurrentValue:F0}</b> (Base: {attr.BaseValue:F0})", new GUIStyle(GUI.skin.label) { richText = true }, GUILayout.Width(200));
                        
                        if (GUILayout.Button("-", GUILayout.Width(30)))
                        {
                            attr.BaseValue = Mathf.Max(1, attr.BaseValue - 1);
                            attributeSystem.UpdateDerivedAttributes();
                        }
                        if (GUILayout.Button("+", GUILayout.Width(35)))
                        {
                            attr.BaseValue += 1;
                            Debug.Log($"[PlayerCharacter] Increased {type} to {attr.BaseValue}");
                            attributeSystem.UpdateDerivedAttributes();
                        }
                        if (GUILayout.Button("+10", GUILayout.Width(45)))
                        {
                            attr.BaseValue += 10;
                            attributeSystem.UpdateDerivedAttributes();
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(10);
                GUILayout.Label("--- All Active Attributes ---", GUI.skin.box);
                foreach (var kvp in attributeSystem.ActiveAttributes)
                {
                    GUILayout.Label($"{kvp.Key}: {kvp.Value.CurrentValue:F2} (Base: {kvp.Value.BaseValue:F2} + Init: {kvp.Value.InitialValue:F2})");
                }

                GUILayout.Space(20);
                GUILayout.Label("--- Experience Controls ---", GUI.skin.box);
                
                // Base EXP
                int blvl = (int)attributeSystem.GetAttribute(AttributeType.BaseLevel).BaseValue;
                long nextBase = expSystem.GetNextBaseExp(blvl);
                GUILayout.Label($"Base Exp: {expSystem.CurrentBaseExp} / {nextBase} (Lvl {blvl})");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+100 Base Exp")) expSystem.AddBaseExp(100);
                if (GUILayout.Button("+1000 Base Exp")) expSystem.AddBaseExp(1000);
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                // Job EXP
                int jlvl = (int)attributeSystem.GetAttribute(AttributeType.JobLevel).BaseValue;
                long nextJob = expSystem.GetNextJobExp(jlvl);
                GUILayout.Label($"Job Exp: {expSystem.CurrentJobExp} / {nextJob} (Lvl {jlvl})");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+100 Job Exp")) expSystem.AddJobExp(100);
                if (GUILayout.Button("+1000 Job Exp")) expSystem.AddJobExp(1000);
                GUILayout.EndHorizontal();
            }
        }


        private void DrawSkillsTab()
        {
            GUILayout.Label("--- All Possible Skills ---", GUI.skin.box);
            var skills = Database.Skills;
            
            foreach (var skill in skills)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUILayout.Label($"[{skill.Id}] {skill.Name} ({skill.SkillElement})", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Grant", GUILayout.Width(70)))
                {
                    skillCaster.GrantSkill(skill);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("--- Player Held Skills (Granted) ---", GUI.skin.box);
            
            SkillInstance skillToRevoke = null;

            foreach (var granted in skillCaster.GrantedSkills)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                
                bool isSelected = (_selectedSkillForSupports == granted);
                string labelPrefix = isSelected ? "<color=yellow>▶ </color>" : "";
                GUILayout.Label($"{labelPrefix}[{granted.SkillData.Id}] {granted.SkillData.Name}", new GUIStyle(GUI.skin.label) { richText = true }, GUILayout.ExpandWidth(true));
                
                if (GUILayout.Button("Eq 1", GUILayout.Width(40))) skillCaster.EquipSkill(granted, 0);
                if (GUILayout.Button("Eq 2", GUILayout.Width(40))) skillCaster.EquipSkill(granted, 1);
                if (GUILayout.Button("Eq 3", GUILayout.Width(40))) skillCaster.EquipSkill(granted, 2);
                
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    _selectedSkillForSupports = isSelected ? null : granted;
                }
                
                if (GUILayout.Button("Revoke", GUILayout.Width(60)))
                {
                    skillToRevoke = granted;
                    if (_selectedSkillForSupports == granted)
                    {
                        _selectedSkillForSupports = null;
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (skillToRevoke != null)
            {
                skillCaster.RevokeSkill(skillToRevoke.SkillData);
            }

            if (_selectedSkillForSupports != null)
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"<b>Support Skills for: {_selectedSkillForSupports.SkillData.Name}</b>", new GUIStyle(GUI.skin.label) { richText = true });
                
                string limitStr = MaxLinkedSupportsLimit > 0 ? MaxLinkedSupportsLimit.ToString() : "Unlimited";
                GUILayout.Label($"Linked Support Skills ({_selectedSkillForSupports.LinkedModifiers.Count} / {limitStr})");

                if (_selectedSkillForSupports.LinkedModifiers.Count == 0)
                {
                    GUILayout.Label("No support skills linked.");
                }
                else
                {
                    SupportModifier toRemove = null;
                    foreach (var mod in _selectedSkillForSupports.LinkedModifiers)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"  - [{mod.ID}] {mod.Name} ({mod.Chroma} - {mod.Mineral})");
                        if (GUILayout.Button("Remove", GUILayout.Width(70)))
                        {
                            toRemove = mod;
                        }
                        GUILayout.EndHorizontal();
                    }
                    if (toRemove != null)
                    {
                        _selectedSkillForSupports.LinkedModifiers.Remove(toRemove);
                    }
                }

                GUILayout.Space(5);
                GUILayout.Label("<b>Add Support Skill:</b>", new GUIStyle(GUI.skin.label) { richText = true });
                
                bool reachedLimit = MaxLinkedSupportsLimit > 0 && _selectedSkillForSupports.LinkedModifiers.Count >= MaxLinkedSupportsLimit;
                if (reachedLimit)
                {
                    GUILayout.Label("<color=orange>Limit reached. Cannot add more support skills.</color>", new GUIStyle(GUI.skin.label) { richText = true });
                }
                else
                {
                    var allSupportJson = Database.SupportSkills;
                    if (allSupportJson != null && allSupportJson.Count > 0)
                    {
                        foreach (var supportJson in allSupportJson)
                        {
                            if (_selectedSkillForSupports.LinkedModifiers.Any(m => m.ID == supportJson.SkillID))
                            {
                                continue;
                            }

                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"  [{supportJson.SkillID}] {supportJson.ModifierName}");
                            
                            SupportModifier tempMod = Database.GetSynchronizedSupport(supportJson.SkillID);
                            bool isCompatible = tempMod != null && tempMod.IsCompatible(_selectedSkillForSupports.SkillData);
                            
                            if (!isCompatible)
                            {
                                GUI.enabled = false;
                                GUILayout.Button("Incompatible", GUILayout.Width(100));
                                GUI.enabled = true;
                            }
                            else
                            {
                                if (GUILayout.Button("Add", GUILayout.Width(60)))
                                {
                                    _selectedSkillForSupports.LinkModifier(tempMod);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        GUILayout.Label("No support skills available in database.");
                    }
                }

                GUILayout.EndVertical();
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

        [ContextMenu("Call LoadData")]
        public void LoadData()
        {
            Debug.Log(GameData.Player.CharacterManager.GetAttribute(AttributeType.HP));

            foreach (var item in Database.GetItems)
            {
                Debug.Log($"{item.Name} : {item.Description} : {item.Id}");
            }


            foreach (var item in Database.Characters)
            {
                Debug.Log($"{item.Name} : {item.BaseHP} : {item.BaseMP}");
            }
        }
    }
}
