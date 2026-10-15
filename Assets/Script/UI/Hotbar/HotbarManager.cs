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

        private void Start()
        {
            if (_activeRows.Count == 0)
            {
                InitializeDefaultHotbar();
            }
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

                var rowRec = new HotbarRowRecord
                {
                    RowIndex = row.RowIndex,
                    IsLocked = row.IsLocked,
                    AnchoredPosition = row.RectTransform != null ? row.RectTransform.anchoredPosition : Vector2.zero,
                    SlotCount = row.Slots.Count
                };

                for (int s = 0; s < row.Slots.Count; s++)
                {
                    var slot = row.Slots[s];
                    if (slot != null && slot.SlotData != null)
                    {
                        rowRec.Slots.Add(new HotbarSlotRecord
                        {
                            ActionType = (int)slot.SlotData.ActionType,
                            SkillId = slot.SlotData.SkillId,
                            ItemId = slot.SlotData.ItemId,
                            ItemInstanceId = slot.SlotData.ItemInstanceId,
                            CustomActionId = slot.SlotData.CustomActionId,
                            HotkeyText = slot.SlotData.HotkeyText
                        });
                    }
                }

                records.Add(rowRec);
            }

            return records;
        }

        public void LoadData(List<HotbarRowRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                InitializeDefaultHotbar();
                return;
            }

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

                rowUI.SetupRow(rec.RowIndex, rec.SlotCount, rec.AnchoredPosition, rec.IsLocked);
                _activeRows.Add(rowUI);

                if (rec.Slots != null)
                {
                    for (int s = 0; s < rec.Slots.Count && s < rowUI.Slots.Count; s++)
                    {
                        var sRec = rec.Slots[s];
                        var slotUI = rowUI.Slots[s];
                        if (sRec != null && slotUI != null)
                        {
                            var sData = new HotbarSlotData
                            {
                                ActionType = (HotbarActionType)sRec.ActionType,
                                SkillId = sRec.SkillId,
                                ItemId = sRec.ItemId,
                                ItemInstanceId = sRec.ItemInstanceId,
                                CustomActionId = sRec.CustomActionId
                            };
                            slotUI.SetupSlot(rec.RowIndex, s, sRec.HotkeyText, sData);
                        }
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
                UnityEngine.InputSystem.Key key = GetKeyForSlotIndex(s);
                if (key != UnityEngine.InputSystem.Key.None && keyboard[key].wasPressedThisFrame)
                {
                    row.Slots[s].ExecuteAction();
                    break;
                }
            }
        }

        private UnityEngine.InputSystem.Key GetKeyForSlotIndex(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0: return UnityEngine.InputSystem.Key.Digit1;
                case 1: return UnityEngine.InputSystem.Key.Digit2;
                case 2: return UnityEngine.InputSystem.Key.Digit3;
                case 3: return UnityEngine.InputSystem.Key.Digit4;
                case 4: return UnityEngine.InputSystem.Key.Digit5;
                case 5: return UnityEngine.InputSystem.Key.Digit6;
                case 6: return UnityEngine.InputSystem.Key.Digit7;
                case 7: return UnityEngine.InputSystem.Key.Digit8;
                case 8: return UnityEngine.InputSystem.Key.Digit9;
                case 9: return UnityEngine.InputSystem.Key.Digit0;
                default: return UnityEngine.InputSystem.Key.None;
            }
        }
        #endregion
    }
}
