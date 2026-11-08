using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Effects;
using CoreSystem.Data.Items.Model;
using CoreSystem.Refinement;
using CoreSystem.UI.ActionProgress;
using CoreSystem.UI.Utility;
using GlobalVisualizer;

namespace CoreSystem.UI.Refinement
{
    public class RefinementWindow : UIWindow, IItemInsertTarget
    {
        #region [Variables/Fields]
        [Header("Header & Info")]
        [SerializeField] private TMP_Text _headerTitle;
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemTypeText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private TMP_Text _levelText;

        [Header("Center Item Slot")]
        [SerializeField] private RefinementCenterSlot _centerSlot;
        [SerializeField] private Image _sunburstDial;
        [SerializeField, HideInInspector] private Image _itemIcon;

        [Header("Cost & Rate")]
        [SerializeField] private TMP_Text _successRateText;
        [SerializeField] private Slider _successRateSlider;
        [SerializeField, HideInInspector] private Image _successRateBar;
        [SerializeField] private TMP_Text _costText;

        [Header("Buttons")]
        [SerializeField] private Button _refineButton;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Button _closeButton;

        [Header("Stats Visualizer")]
        [SerializeField] private GlobalVisualizerView _statVisualizer;
        [SerializeField] private RectTransform _statRowPrefab;
        [SerializeField, HideInInspector] private Transform _statContainer;
        [SerializeField, HideInInspector] private List<TMP_Text> _statLabelList = new List<TMP_Text>();
        [SerializeField, HideInInspector] private List<TMP_Text> _statValueList = new List<TMP_Text>();

        [Header("Materials")]
        [SerializeField] private List<GameObject> _materialSlotObjects = new List<GameObject>();
        [SerializeField] private List<Image> _materialIcons = new List<Image>();
        [SerializeField] private List<TMP_Text> _materialCountTexts = new List<TMP_Text>();

        public const float RefineLoadingSeconds = 0.5f;
        public const string RefineActionType = "REFINE";
        public const string InsertLabel = "Insert to Refine";

        private EquipmentInstance _targetEquipment;
        #endregion

        #region [Properties]
        public EquipmentInstance TargetEquipment => _targetEquipment;
        public RefinementCenterSlot CenterSlot => _centerSlot;
        public Slider SuccessRateSlider => _successRateSlider;
        public GlobalVisualizerView StatVisualizer => _statVisualizer;
        public RectTransform StatRowPrefab => _statRowPrefab;
        public Button RefineButton => _refineButton;
        public Button CloseButton => _closeButton;
        public IReadOnlyList<GameObject> MaterialSlotObjects => _materialSlotObjects;
        public IReadOnlyList<Image> MaterialIcons => _materialIcons;
        public IReadOnlyList<TMP_Text> MaterialCountTexts => _materialCountTexts;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();

            if (_refineButton != null)
            {
                _refineButton.onClick.AddListener(ExecuteRefine);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(CloseWindow);
            }

            if (_statVisualizer != null && _statRowPrefab != null && _statVisualizer.ItemPrefab == null)
            {
                _statVisualizer.ItemPrefab = _statRowPrefab;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ItemInsertRegistry.Register(this);
            UpdateUI();
        }

        private void OnDisable()
        {
            ItemInsertRegistry.Unregister(this);
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            _targetEquipment = data as EquipmentInstance;
            UpdateUI();
        }

        public bool CanInsert(ItemInstance item, int slotIndex)
        {
            return item is EquipmentInstance && !ActionProgressWindow.IsRunningFor(this);
        }

        public bool TryInsert(ItemInstance item, int slotIndex)
        {
            if (!CanInsert(item, slotIndex)) return false;
            SetTarget((EquipmentInstance)item);
            return true;
        }

        public void ClearInsert(int slotIndex)
        {
            if (_targetEquipment == null || ActionProgressWindow.IsRunningFor(this)) return;
            SetTarget(null);
        }

        public string GetInsertLabel(ItemInstance item)
        {
            return InsertLabel;
        }

        public void SetTarget(EquipmentInstance equipment)
        {
            _targetEquipment = equipment;
            if (_centerSlot != null)
            {
                _centerSlot.UpdateDisplay(equipment);
            }
            UpdateUI();
        }

        public void ExecuteRefine()
        {
            if (_targetEquipment == null || ActionProgressWindow.IsRunningFor(this)) return;

            PlayerInventory inventory = GameData.Player != null ? GameData.Player.Inventory : null;
            if (!RefinementSystem.CanRefine(_targetEquipment, inventory, out _))
            {
                RefinementSystem.ExecuteRefine(_targetEquipment, inventory);
                UpdateUI();
                return;
            }

            EquipmentInstance target = _targetEquipment;
            if (_refineButton != null) _refineButton.interactable = false;
            ActionProgressWindow.Show(new ActionProgressRequest
            {
                ActionName = $"{(target.Item != null ? target.Item.Name : "Item")} +{target.Refine}",
                ActionType = RefineActionType,
                Icon = target.Item != null ? target.Item.Icon : null,
                Duration = RefineLoadingSeconds,
                UseUnscaledTime = true,
                Color = ActionProgressColor.Cyan,
                Owner = this,
                OnCompleted = () =>
                {
                    RefinementSystem.ExecuteRefine(target, GameData.Player != null ? GameData.Player.Inventory : null);
                    UpdateUI();
                },
                OnCancelled = UpdateUI
            });
        }

        public void CloseWindow()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Close(WindowName);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        #endregion

        #region [Private Methods]
        private void UpdateUI()
        {
            if (_targetEquipment == null)
            {
                ResetToEmptyState();
                return;
            }

            Equipment template = _targetEquipment.Item as Equipment;
            int currentRefine = _targetEquipment.Refine;

            if (_centerSlot != null)
            {
                _centerSlot.UpdateDisplay(_targetEquipment);
            }
            else if (_itemIcon != null && template != null)
            {
                _itemIcon.sprite = template.Icon;
                _itemIcon.enabled = _itemIcon.sprite != null;
            }

            if (_itemNameText != null)
            {
                _itemNameText.text = template != null ? template.Name.ToUpperInvariant() : "EQUIPMENT";
            }

            if (_itemTypeText != null)
            {
                _itemTypeText.text = template != null ? template.Slot.ToString().ToUpperInvariant() : "ITEM";
            }

            if (template != null)
            {
                if (_rarityText != null)
                {
                    _rarityText.text = template.Rarity.ToString().ToUpperInvariant();
                }

                if (_rarityBackground != null)
                {
                    _rarityBackground.color = template.Rarity.GetColor();
                }
            }

            if (_levelText != null)
            {
                _levelText.text = $"<size=32>Lv.{currentRefine}</size><#9F9F9F>/{RefinementSystem.MaxRefineLevel}</color>";
            }

            float rate = RefinementSystem.CalculateSuccessRate(currentRefine);
            if (_successRateText != null)
            {
                _successRateText.text = $"{Mathf.RoundToInt(rate)}%";
            }

            if (_successRateSlider != null)
            {
                _successRateSlider.value = Mathf.Clamp01(rate / 100f);
            }
            else if (_successRateBar != null)
            {
                _successRateBar.fillAmount = Mathf.Clamp01(rate / 100f);
            }

            int cost = template != null ? RefinementSystem.CalculateCost(template, currentRefine) : 0;
            if (_costText != null)
            {
                _costText.text = $"{cost:N0} CREDITS";
            }

            PlayerInventory inventory = GameData.Player != null ? GameData.Player.Inventory : null;
            UpdateMaterials(template, currentRefine, inventory);

            bool canRefine = RefinementSystem.CanRefine(_targetEquipment, inventory, out _);
            if (_refineButton != null)
            {
                _refineButton.interactable = canRefine;
            }

            if (_buttonText != null)
            {
                if (currentRefine >= RefinementSystem.MaxRefineLevel)
                {
                    _buttonText.gameObject.SetActive(true);
                    _buttonText.text = "MAX LEVEL";
                }
                else
                {
                    _buttonText.text = string.Empty;
                    _buttonText.gameObject.SetActive(false);
                }
            }

            UpdateStats(template, currentRefine);
        }

        private void ResetToEmptyState()
        {
            if (_centerSlot != null)
            {
                _centerSlot.UpdateDisplay(null);
            }
            else if (_itemIcon != null)
            {
                _itemIcon.sprite = null;
                _itemIcon.enabled = false;
            }

            if (_itemNameText != null) _itemNameText.text = "SELECT EQUIPMENT";
            if (_itemTypeText != null) _itemTypeText.text = string.Empty;
            if (_rarityText != null) _rarityText.text = "None";
            if (_levelText != null) _levelText.text = $"<size=32>Lv.0<#9F9F9F></size>/{RefinementSystem.MaxRefineLevel}</color>";
            if (_successRateText != null) _successRateText.text = "0%";
            if (_successRateSlider != null) _successRateSlider.value = 0f;
            if (_successRateBar != null) _successRateBar.fillAmount = 0f;
            if (_costText != null) _costText.text = "0 CREDITS";
            if (_refineButton != null) _refineButton.interactable = false;
            if (_buttonText != null)
            {
                _buttonText.text = string.Empty;
                _buttonText.gameObject.SetActive(false);
            }

            if (_materialSlotObjects != null)
            {
                for (int i = 0; i < _materialSlotObjects.Count; i++)
                {
                    if (_materialSlotObjects[i] != null)
                    {
                        _materialSlotObjects[i].SetActive(false);
                    }
                }
            }

            if (_statVisualizer != null)
            {
                _statVisualizer.Clear();
            }
        }

        private void UpdateMaterials(Equipment template, int currentRefine, PlayerInventory inventory)
        {
            var matStatus = template != null
                ? RefinementSystem.GetMaterialStatus(template, currentRefine, inventory)
                : new List<(int itemId, int required, int owned)>();

            int slotCount = _materialSlotObjects != null ? _materialSlotObjects.Count : 0;
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = _materialSlotObjects[i];
                if (slotObj == null) continue;

                if (i < matStatus.Count)
                {
                    slotObj.SetActive(true);
                    var status = matStatus[i];
                    Item matItem = ItemDatabase.GetItem(status.itemId);

                    if (_materialIcons != null && i < _materialIcons.Count && _materialIcons[i] != null)
                    {
                        if (matItem != null && matItem.Icon != null)
                        {
                            _materialIcons[i].sprite = matItem.Icon;
                            _materialIcons[i].enabled = true;
                        }
                    }

                    if (_materialCountTexts != null && i < _materialCountTexts.Count && _materialCountTexts[i] != null)
                    {
                        bool sufficient = status.owned >= status.required;
                        string colorHex = sufficient ? "#ffffff" : "#ef4444";
                        _materialCountTexts[i].text = $"<color={colorHex}>{status.owned}</color> / {status.required}";
                    }

                    HoverableUI hover = slotObj.GetComponent<HoverableUI>();
                    if (hover == null)
                    {
                        hover = slotObj.AddComponent<HoverableUI>();
                    }
                    string matName = matItem != null ? matItem.Name : $"Item {status.itemId}";
                    string matDesc = matItem != null ? matItem.Description : string.Empty;
                    hover.Setup(matName, matDesc);
                }
                else
                {
                    slotObj.SetActive(false);
                }
            }
        }

        private void UpdateStats(Equipment template, int refineLevel)
        {
            if (template == null)
            {
                if (_statVisualizer != null) _statVisualizer.Clear();
                return;
            }

            var stats = new List<RefinementStatRowData>();

            if (template.BasePhysical > 0)
            {
                float basePhy = template.BasePhysical;
                float curMul = ProgressionFormula.RefineMultiplier(refineLevel);
                float nextMul = ProgressionFormula.RefineMultiplier(Mathf.Min(refineLevel + 1, RefinementSystem.MaxRefineLevel));
                int curVal = Mathf.RoundToInt(basePhy * (1f + curMul));
                int nextVal = Mathf.RoundToInt(basePhy * (1f + nextMul));
                int diff = nextVal - curVal;
                string valText = diff > 0 && refineLevel < RefinementSystem.MaxRefineLevel
                    ? $"{curVal} <color=#38bdf8>+(+{diff})</color>"
                    : $"{curVal}";
                stats.Add(new RefinementStatRowData("PHYSICAL POWER", valText, false));
            }

            if (template.BaseMagic > 0)
            {
                float baseMag = template.BaseMagic;
                float curMul = ProgressionFormula.RefineMultiplier(refineLevel);
                float nextMul = ProgressionFormula.RefineMultiplier(Mathf.Min(refineLevel + 1, RefinementSystem.MaxRefineLevel));
                int curVal = Mathf.RoundToInt(baseMag * (1f + curMul));
                int nextVal = Mathf.RoundToInt(baseMag * (1f + nextMul));
                int diff = nextVal - curVal;
                string valText = diff > 0 && refineLevel < RefinementSystem.MaxRefineLevel
                    ? $"{curVal} <color=#38bdf8>+(+{diff})</color>"
                    : $"{curVal}";
                stats.Add(new RefinementStatRowData("MAGIC POWER", valText, false));
            }

            if (template.Effects != null)
            {
                for (int i = 0; i < template.Effects.Count; i++)
                {
                    var effect = template.Effects[i];
                    if (effect is AttributeBonusEffect attrEffect)
                    {
                        var attrTemplate = AttributeDatabase.GetAttribute(attrEffect.Attribute);
                        string attrName = attrTemplate != null && !string.IsNullOrEmpty(attrTemplate.FullName)
                            ? attrTemplate.FullName.ToUpperInvariant()
                            : attrEffect.Attribute.ToString().ToUpperInvariant();

                        bool isPercent = attrTemplate != null && attrTemplate.IsPercent;
                        float refineBonus = isPercent
                            ? refineLevel * (attrEffect.Value * 0.01f)
                            : refineLevel * 1.0f;
                        float nextBonus = isPercent
                            ? Mathf.Min(refineLevel + 1, RefinementSystem.MaxRefineLevel) * (attrEffect.Value * 0.01f)
                            : Mathf.Min(refineLevel + 1, RefinementSystem.MaxRefineLevel) * 1.0f;

                        float curVal = attrEffect.Value + refineBonus;
                        float nextVal = attrEffect.Value + nextBonus;
                        float diff = nextVal - curVal;

                        string sign = curVal >= 0 ? "+" : "";
                        string unit = isPercent ? "%" : "";
                        string valText = diff > 0 && refineLevel < RefinementSystem.MaxRefineLevel
                            ? $"{sign}{curVal:0.##}{unit} <color=#38bdf8>+(+{diff:0.##}{unit})</color>"
                            : $"{sign}{curVal:0.##}{unit}";

                        stats.Add(new RefinementStatRowData(attrName, valText, false));
                    }
                    else if (effect is CoreSystem.Data.Items.Effects.RefineBonusEffect refineBonus)
                    {
                        bool isActive = refineLevel >= refineBonus.RequiredRefine;
                        string label = $"REFINE +{refineBonus.RequiredRefine}";
                        string stateText = isActive
                            ? "<color=#22c55e>[ACTIVE]</color>"
                            : $"<color=#64748b>[REQ. +{refineBonus.RequiredRefine}]</color>";

                        string desc = refineBonus.BonusEffect != null ? refineBonus.BonusEffect.GetDescription() : "Perk Bonus";
                        string valText = $"{desc} {stateText}";
                        stats.Add(new RefinementStatRowData(label, valText, isActive));
                    }
                }
            }

            if (_statVisualizer != null)
            {
                _statVisualizer.SetItems(stats, (go, data, index) =>
                {
                    var rowView = go.GetComponent<RefinementStatRowView>();
                    if (rowView != null)
                    {
                        rowView.Bind(data);
                    }
                });
            }
            else
            {
                for (int i = 0; i < _statLabelList.Count; i++)
                {
                    if (i < stats.Count)
                    {
                        if (_statLabelList[i] != null) _statLabelList[i].text = stats[i].Label;
                        if (_statValueList != null && i < _statValueList.Count && _statValueList[i] != null)
                        {
                            _statValueList[i].text = stats[i].ValueFormatted;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
