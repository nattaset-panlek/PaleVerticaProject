using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Data.Save;
using CoreSystem.UI.Utility;

namespace CoreSystem.UI.Hotbar
{
    public class HotbarManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private static HotbarManager _instance;

        [Header("UI Containers & Prefabs")]
        [SerializeField] private Transform _rowContainer;
        [SerializeField] private GameObject _hotbarRowPrefab;

        [Header("Settings")]
        [SerializeField] private int _maxRows = 10;
        [SerializeField] private int _defaultFirstRowSlots = 10;
        [SerializeField] private int _defaultOtherRowSlots = 10;

        private List<HotbarRowUI> _activeRows = new List<HotbarRowUI>();
        private bool _isDefaultLayout;
        #endregion

        #region [Properties]
        public static HotbarManager Instance => _instance;
        public List<HotbarRowUI> ActiveRows => _activeRows;
        public int MaxRows => _maxRows;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnEnable()
        {
            CoreSystem.Player.PlayerCore.OnPlayerChanged += HandlePlayerChanged;
        }

        private void OnDisable()
        {
            CoreSystem.Player.PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
        }

        private void Start()
        {
            if (_activeRows.Count == 0)
            {
                InitializeDefaultHotbar();
            }
            FillDefaultLayout(CoreSystem.Player.PlayerCore.GetPlayer());
        }

        private void Update()
        {
            HandleHotkeyInputs();
        }
        #endregion

        #region [Public Methods]
        public HotbarRowUI AddHotbarRow()
        {
            if (_activeRows.Count >= _maxRows)
            {
                Debug.LogWarning($"[HotbarManager] Cannot add more hotbars. Reached maximum limit of {_maxRows}.");
                return null;
            }

            if (_rowContainer == null || _hotbarRowPrefab == null)
            {
                Debug.LogError("[HotbarManager] RowContainer or HotbarRowPrefab is not assigned!");
                return null;
            }

            GameObject rowObj = Instantiate(_hotbarRowPrefab, _rowContainer);
            rowObj.SetActive(true);

            HotbarRowUI rowUI = rowObj.GetComponent<HotbarRowUI>();
            if (rowUI == null)
            {
                rowUI = rowObj.AddComponent<HotbarRowUI>();
            }

            int rowIndex = _activeRows.Count;
            int slotCount = (rowIndex == 0) ? _defaultFirstRowSlots : _defaultOtherRowSlots;
            Vector2 initialPos = new Vector2(0f, -rowIndex * 70f);

            rowUI.SetupRow(rowIndex, slotCount, initialPos, false);
            _activeRows.Add(rowUI);

            Debug.Log($"[HotbarManager] Added Hotbar Row {rowIndex} with {slotCount} slots.");
            return rowUI;
        }

        public void RemoveHotbarRow(HotbarRowUI row)
        {
            if (row == null) return;
            if (_activeRows.Count <= 1)
            {
                Debug.LogWarning("[HotbarManager] Cannot remove the last remaining hotbar row.");
                return;
            }

            _activeRows.Remove(row);
            Destroy(row.gameObject);
            ReindexRows();
            Debug.Log("[HotbarManager] Removed Hotbar Row.");
        }

        public void OpenContextMenu(HotbarRowUI row, Vector2 screenPos)
        {
            if (row == null || ContextMenuManager.Instance == null) return;

            List<ContextMenuItem> items = new List<ContextMenuItem>();

            items.Add(new ContextMenuItem("Add Hotbar", () => AddHotbarRow()));

            if (_activeRows.Count > 1)
            {
                items.Add(new ContextMenuItem("Remove Hotbar", () => RemoveHotbarRow(row)));
            }

            string lockLabel = row.IsLocked ? "Unlock Position" : "Lock Position";
            items.Add(new ContextMenuItem(lockLabel, () => row.ToggleLockPosition()));

            ContextMenuManager.Instance.Show(items, screenPos);
        }

        public List<HotbarRowRecord> SaveData()
        {
            List<HotbarRowRecord> records = new List<HotbarRowRecord>();

            for (int r = 0; r < _activeRows.Count; r++)
            {
                var row = _activeRows[r];
                if (row == null) continue;

                var slotData = new List<HotbarSlotData>(row.Slots.Count);
                for (int s = 0; s < row.Slots.Count; s++)
                {
                    slotData.Add(row.Slots[s] != null ? row.Slots[s].SlotData : null);
                }

                records.Add(new HotbarRowRecord
                {
                    RowIndex = records.Count,
                    IsLocked = row.IsLocked,
                    AnchoredPosition = row.RectTransform != null ? row.RectTransform.anchoredPosition : Vector2.zero,
                    SlotCount = row.Slots.Count,
                    Slots = HotbarLayoutFormula.ToRecords(slotData, records.Count)
                });
            }

            return records;
        }

        public void LoadData(List<HotbarRowRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                InitializeDefaultHotbar();
                FillDefaultLayout(CoreSystem.Player.PlayerCore.GetPlayer());
                return;
            }

            _isDefaultLayout = false;

            // Clear existing rows
            foreach (var row in _activeRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _activeRows.Clear();

            for (int r = 0; r < records.Count; r++)
            {
                var rec = records[r];
                if (rec == null) continue;

                GameObject rowObj = Instantiate(_hotbarRowPrefab, _rowContainer);
                rowObj.SetActive(true);

                HotbarRowUI rowUI = rowObj.GetComponent<HotbarRowUI>();
                if (rowUI == null) rowUI = rowObj.AddComponent<HotbarRowUI>();

                int rowIndex = _activeRows.Count;
                rowUI.SetupRow(rowIndex, rec.SlotCount, rec.AnchoredPosition, rec.IsLocked);
                _activeRows.Add(rowUI);

                HotbarSlotData[] slotData = HotbarLayoutFormula.FromRecords(rec.Slots, rowUI.Slots.Count);
                for (int s = 0; s < rowUI.Slots.Count; s++)
                {
                    if (rowUI.Slots[s] != null)
                    {
                        rowUI.Slots[s].SetupSlot(rowIndex, s, HotbarLayoutFormula.GetHotkeyLabel(rowIndex, s), slotData[s]);
                    }
                }
            }
        }
        #endregion

        #region [Private Methods]
        private void InitializeDefaultHotbar()
        {
            if (_activeRows.Count == 0)
            {
                AddHotbarRow();
                _isDefaultLayout = true;
            }
        }

        private void HandlePlayerChanged(CoreSystem.Entity.Character player)
        {
            FillDefaultLayout(player);
        }

        private void FillDefaultLayout(CoreSystem.Entity.Character player)
        {
            if (!_isDefaultLayout || _activeRows.Count == 0 || _activeRows[0] == null) return;
            if (player == null || player.skillCaster == null || player.skillCaster.EquippedSkills == null) return;

            HotbarRowUI row = _activeRows[0];
            var equipped = player.skillCaster.EquippedSkills;
            for (int i = 0; i < equipped.Count; i++)
            {
                var instance = equipped[i];
                if (instance == null || instance.SkillData == null) continue;
                HotbarSlotView slot = row.GetSlot(i);
                if (slot == null || (slot.SlotData != null && !slot.SlotData.IsEmpty)) continue;
                slot.AssignSkill(instance.SkillData.Id);
            }
        }

        private void ReindexRows()
        {
            for (int i = 0; i < _activeRows.Count; i++)
            {
                if (_activeRows[i] != null)
                {
                    _activeRows[i].ReindexRow(i);
                }
            }
        }

        private void HandleHotkeyInputs()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            bool isAlt = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
            bool isCtrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            bool isShift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

            int targetRow = 0;
            if (isAlt && !isCtrl && !isShift) targetRow = 1;
            else if (isCtrl && !isAlt && !isShift) targetRow = 2;
            else if (isShift && !isAlt && !isCtrl) targetRow = 3;
            else if (isAlt || isCtrl || isShift) return;

            if (targetRow >= _activeRows.Count) return;
            var row = _activeRows[targetRow];
            if (row == null) return;

            for (int s = 0; s < row.Slots.Count; s++)
            {
                if (IsSlotKeyPressed(keyboard, s))
                {
                    row.Slots[s].ExecuteAction();
                    break;
                }
            }
        }

        private static bool IsSlotKeyPressed(UnityEngine.InputSystem.Keyboard keyboard, int slotIndex)
        {
            for (int slot = 0; slot < CoreSystem.Settings.Keybinds.KeybindPair.SlotCount; slot++)
            {
                UnityEngine.InputSystem.Key key = CoreSystem.Settings.Keybinds.KeybindSetting.GetHotbarKey(slotIndex, slot);
                if (key != UnityEngine.InputSystem.Key.None && keyboard[key].wasPressedThisFrame) return true;
            }
            return false;
        }
        #endregion
    }
}
