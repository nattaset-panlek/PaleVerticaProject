using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using TMPro;
using CoreSystem.Player;
using CoreSystem.UI.Visualizer;
using GlobalVisualizer;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillManageWindow : UIWindow
    {
        #region [Variables/Fields]
        [SerializeField] private GameObject _skillItemPrefab;
        [SerializeField] private Transform _itemContainer;

        [Header("Skill Details Display")]
        [SerializeField] private TMP_Text _selectedSkillNameText;
        [SerializeField] private Image _selectedSkillIconImage;
        [SerializeField] private GameObject _skillTagPrefab;
        [SerializeField] private Transform _tagContainer;

        [Header("Skill Tag Colours")]
        [SerializeField] private Color _castTagColor = new Color(0.314f, 0.314f, 0.314f, 1f);
        [SerializeField] private Color _castTagText = Color.white;
        [SerializeField] private Color _neutralTagColor = new Color(0.718f, 0.718f, 0.718f, 1f);
        [SerializeField] private Color _neutralTagText = new Color(0.314f, 0.314f, 0.314f, 1f);
        [SerializeField] private Color _elementTagText = Color.white;
        [SerializeField] private List<ElementTagColor> _elementTagColors = new List<ElementTagColor>
        {
            new ElementTagColor(ElementType.Fire, new Color(1f, 0.486f, 0.263f, 1f)),
            new ElementTagColor(ElementType.Water, new Color(0.239f, 0.608f, 0.914f, 1f)),
            new ElementTagColor(ElementType.Earth, new Color(0.722f, 0.525f, 0.231f, 1f)),
            new ElementTagColor(ElementType.Electric, new Color(0.851f, 0.643f, 0f, 1f)),
            new ElementTagColor(ElementType.Ghost, new Color(0.557f, 0.424f, 0.788f, 1f)),
            new ElementTagColor(ElementType.Holy, new Color(0.788f, 0.635f, 0.153f, 1f))
        };

        [Header("Support Skill Slots")]
        [SerializeField] private SupportSkillSlotComponent[] _supportSkillSlots = new SupportSkillSlotComponent[5];

        [Header("Modifier Fields Container")]
        [SerializeField] private GameObject _modifierFieldPrefab;
        [SerializeField] private Transform _modifierFieldContainer;
        [SerializeField] private GameObject _modifierPlaceholder;

        [Header("Skill List Filter")]
        [SerializeField] private SkillListFilterView _filter;

        private readonly List<SkillInstance> _listedSkills = new List<SkillInstance>();
        private GlobalVisualizerView _skillListView;
        private List<GameObject> _spawnedTagList = new List<GameObject>();
        private readonly HashSet<string> _spawnedTagTexts = new HashSet<string>();
        private List<ModelModifierFieldContainerComponent> _spawnedModifierFieldList = new List<ModelModifierFieldContainerComponent>();
        private SkillInstance _selectedSkillInstance;

        public event Action<SkillInstance> OnSelectedSkillChanged;
        #endregion

        #region [Properties]
        private Character _playerCharacter;
        public Character TargetPlayer => _playerCharacter;
        public SkillInstance SelectedSkillInstance => _selectedSkillInstance;
        public TMP_Text SelectedSkillNameText => _selectedSkillNameText;
        public Image SelectedSkillIconImage => _selectedSkillIconImage;
        public SupportSkillSlotComponent[] SupportSkillSlots => _supportSkillSlots;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();

            if (_playerCharacter == null)
            {
                _playerCharacter = PlayerCore.GetPlayer();
            }
        }

        private void OnEnable()
        {
            if (_filter != null) _filter.OnFilterChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            if (_filter != null) _filter.OnFilterChanged -= RefreshUI;
            UnsubscribeFromSkillCaster();
            UnsubscribeFromSelectedSkill();
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);

            if (data is Character player)
            {
                _playerCharacter = player;
            }

            RefreshUI();
            SignalAction?.Invoke(data);
        }

        public override void OnClose()
        {
            UnsubscribeFromSkillCaster();
            UnsubscribeFromSelectedSkill();
            base.OnClose();
        }

        public void Initialize(Character player)
        {
            _playerCharacter = player;
            RefreshUI();
        }

        public void SelectSkill(SkillInstance instance)
        {
            UnsubscribeFromSelectedSkill();
            _selectedSkillInstance = instance;
            SubscribeToSelectedSkill();

            UpdateListedSelection();

            UpdateSelectedSkillDetails(instance);
            OnSelectedSkillChanged?.Invoke(_selectedSkillInstance);
        }

        public void RefreshUI()
        {
            ClearTags();
            _listedSkills.Clear();
            EnsureSkillListView();

            if (_playerCharacter == null)
            {
                _playerCharacter = PlayerCore.GetPlayer();
            }

            SubscribeToSkillCaster();

            List<SkillInstance> grantedSkills = _playerCharacter != null && _playerCharacter.skillCaster != null ? _playerCharacter.skillCaster.GrantedSkills : null;
            if (grantedSkills == null || grantedSkills.Count == 0 || _skillListView == null)
            {
                ShowListedSkills();
                return;
            }

            for (int i = 0; i < grantedSkills.Count; i++)
            {
                SkillInstance instance = grantedSkills[i];
                if (instance == null || instance.SkillData == null) continue;
                if (_filter != null && !_filter.Matches(instance, _playerCharacter)) continue;

                _listedSkills.Add(instance);
            }
            ShowListedSkills();

            if (!grantedSkills.Contains(_selectedSkillInstance))
            {
                _selectedSkillInstance = grantedSkills[0];
            }

            SelectSkill(_selectedSkillInstance);
        }

        public bool LinkSupportSkillToSelected(SupportModifier modifier, int slotIndex, ItemInstance sourceItemInstance)
        {
            if (_selectedSkillInstance == null || modifier == null || sourceItemInstance == null) return false;

            if (GameData.Player?.Inventory == null) return false;
            ItemInstance invItem = GameData.Player.Inventory.GetByInstanceId(sourceItemInstance.Id);
            if (invItem == null) return false;

            SupportModifier oldMod = _selectedSkillInstance.GetModifierAt(slotIndex);

            bool success = _selectedSkillInstance.SetModifierAt(slotIndex, modifier);
            if (success)
            {
                if (oldMod != null)
                {
                    int oldItemId = 7000 + (oldMod.ID % 1000);
                    GameData.Player.Inventory.AddItem(oldItemId, 1);
                }

                GameData.Player.Inventory.RemoveItemByInstance(invItem.Id, 1);

                UpdateSupportSkillSlots(_selectedSkillInstance);
            }
            return success;
        }

        public bool UnlinkSupportSkillFromSelected(int slotIndex)
        {
            if (_selectedSkillInstance == null) return false;

            SupportModifier oldMod = _selectedSkillInstance.GetModifierAt(slotIndex);
            if (oldMod == null) return false;

            _selectedSkillInstance.UnlinkModifierAt(slotIndex);

            if (GameData.Player?.Inventory != null)
            {
                int oldItemId = 7000 + (oldMod.ID % 1000);
                GameData.Player.Inventory.AddItem(oldItemId, 1);
            }

            UpdateSupportSkillSlots(_selectedSkillInstance);
            return true;
        }
        #endregion

        #region [Private Methods]
        private void EnsureSkillListView()
        {
            if (_skillListView != null || _skillItemPrefab == null || _itemContainer == null) return;
            _skillListView = VisualizerViewFactory.AttachTo(_itemContainer, (RectTransform)_skillItemPrefab.transform);
            if (_skillListView != null) _skillListView.SetItems(_listedSkills, BindSkillItem);
        }

        private void ShowListedSkills()
        {
            if (_skillListView != null) _skillListView.NotifyReset();
        }

        private void BindSkillItem(GameObject view, SkillInstance instance, int index)
        {
            SkillItemListUI itemUI = view.GetComponent<SkillItemListUI>();
            if (itemUI == null) itemUI = view.AddComponent<SkillItemListUI>();
            itemUI.Setup(instance, _playerCharacter, this, RefreshUI);
            itemUI.SetSelected(instance == _selectedSkillInstance);
        }

        private void UpdateListedSelection()
        {
            if (_skillListView == null) return;
            VisualRange range = _skillListView.VisibleRange;
            for (int i = range.First; i <= range.Last; i++)
            {
                RectTransform view = _skillListView.GetView(i);
                SkillItemListUI itemUI = view != null ? view.GetComponent<SkillItemListUI>() : null;
                if (itemUI != null) itemUI.SetSelected(itemUI.SkillInstance == _selectedSkillInstance);
            }
        }

        private void UpdateSelectedSkillDetails(SkillInstance instance)
        {
            if (_selectedSkillNameText != null)
            {
                _selectedSkillNameText.text = (instance != null && instance.SkillData != null) ? instance.SkillData.Name : "";
            }

            if (_selectedSkillIconImage != null)
            {
                if (instance != null && instance.SkillData != null && instance.SkillData.Icon != null)
                {
                    _selectedSkillIconImage.sprite = instance.SkillData.Icon;
                    _selectedSkillIconImage.enabled = true;
                }
                else
                {
                    _selectedSkillIconImage.sprite = null;
                    _selectedSkillIconImage.enabled = false;
                }
            }

            ClearTags();

            if (instance == null || instance.SkillData == null || _skillTagPrefab == null || _tagContainer == null) return;

            ActiveSkill data = instance.SkillData;

            SpawnTag(data.IsToggle ? "Toggle" : "Active", _castTagColor, _castTagText);

            if (data.SkillElement != ElementType.Neutral)
            {
                SpawnTag(data.SkillElement.ToString(), ElementColor(data.SkillElement), _elementTagText);
            }

            SpawnTag(data.SkillDamageType.ToString(), _neutralTagColor, _neutralTagText);

            if (data.Tags != null)
            {
                for (int i = 0; i < data.Tags.Count; i++)
                {
                    SpawnTag(data.Tags[i].ToString(), _neutralTagColor, _neutralTagText);
                }
            }

            UpdateSupportSkillSlots(instance);
        }

        private void SpawnTag(string text, Color background, Color ink)
        {
            if (string.IsNullOrEmpty(text) || _skillTagPrefab == null || _tagContainer == null) return;
            if (_spawnedTagTexts.Contains(text.ToUpperInvariant())) return;
            _spawnedTagTexts.Add(text.ToUpperInvariant());

            GameObject tagObj = Instantiate(_skillTagPrefab, _tagContainer);
            tagObj.SetActive(true);

            SkillTagComponent tagComponent = tagObj.GetComponent<SkillTagComponent>();
            if (tagComponent != null)
            {
                tagComponent.Setup(text, background, ink);
            }
            else
            {
                TMP_Text tmp = tagObj.GetComponentInChildren<TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = text;
                    tmp.color = ink;
                }
            }

            _spawnedTagList.Add(tagObj);
        }

        private void ClearTags()
        {
            for (int i = 0; i < _spawnedTagList.Count; i++)
            {
                if (_spawnedTagList[i] != null)
                {
                    Destroy(_spawnedTagList[i]);
                }
            }
            _spawnedTagList.Clear();
            _spawnedTagTexts.Clear();
        }

        private Color ElementColor(ElementType element)
        {
            for (int i = 0; i < _elementTagColors.Count; i++)
            {
                if (_elementTagColors[i].Element == element) return _elementTagColors[i].Color;
            }
            return _neutralTagColor;
        }

        private void UpdateSupportSkillSlots(SkillInstance instance)
        {
            if (_supportSkillSlots == null) return;

            int maxSlots = (instance != null && instance.SkillData != null) ? instance.SkillData.MaxSupportSlots : 0;

            for (int i = 0; i < _supportSkillSlots.Length; i++)
            {
                if (_supportSkillSlots[i] != null)
                {
                    bool isUnlocked = instance != null && i < maxSlots;
                    SupportModifier mod = (instance != null && instance.LinkedModifiers != null) ? instance.GetModifierAt(i) : null;
                    _supportSkillSlots[i].Setup(i, mod, isUnlocked, OnSupportSkillSlotClicked);
                }
            }

            UpdateModifierFields(instance);
        }

        private void OnSupportSkillSlotClicked(int slotIndex)
        {
            if (_selectedSkillInstance == null || _selectedSkillInstance.SkillData == null) return;
            if (slotIndex < 0 || slotIndex >= _selectedSkillInstance.SkillData.MaxSupportSlots) return;
        }

        private void SubscribeToSkillCaster()
        {
            UnsubscribeFromSkillCaster();
            if (_playerCharacter != null && _playerCharacter.skillCaster != null)
            {
                _playerCharacter.skillCaster.OnGrantedSkillsChanged += RefreshUI;
            }
        }

        private void UnsubscribeFromSkillCaster()
        {
            if (_playerCharacter != null && _playerCharacter.skillCaster != null)
            {
                _playerCharacter.skillCaster.OnGrantedSkillsChanged -= RefreshUI;
            }
        }

        private void SubscribeToSelectedSkill()
        {
            if (_selectedSkillInstance != null)
            {
                _selectedSkillInstance.OnSupportSkillsChanged += OnSelectedSkillSupportChanged;
            }
        }

        private void UnsubscribeFromSelectedSkill()
        {
            if (_selectedSkillInstance != null)
            {
                _selectedSkillInstance.OnSupportSkillsChanged -= OnSelectedSkillSupportChanged;
            }
        }

        private void OnSelectedSkillSupportChanged()
        {
            UpdateSupportSkillSlots(_selectedSkillInstance);
            OnSelectedSkillChanged?.Invoke(_selectedSkillInstance);
        }

        private void UpdateModifierFields(SkillInstance instance)
        {
            if (_modifierPlaceholder != null) _modifierPlaceholder.SetActive(!HasLinkedModifier(instance));
            if (_modifierFieldContainer == null || _modifierFieldPrefab == null) return;

            List<SupportModifier> activeModifiers = new List<SupportModifier>();
            if (instance != null && instance.LinkedModifiers != null)
            {
                for (int i = 0; i < instance.LinkedModifiers.Count; i++)
                {
                    SupportModifier mod = instance.LinkedModifiers[i];
                    if (mod != null)
                    {
                        activeModifiers.Add(mod);
                    }
                }
            }

            int requiredCount = activeModifiers.Count;

            for (int i = 0; i < requiredCount; i++)
            {
                ModelModifierFieldContainerComponent fieldUI;
                if (i < _spawnedModifierFieldList.Count)
                {
                    fieldUI = _spawnedModifierFieldList[i];
                }
                else
                {
                    GameObject obj = Instantiate(_modifierFieldPrefab, _modifierFieldContainer);
                    fieldUI = obj.GetComponent<ModelModifierFieldContainerComponent>();
                    if (fieldUI == null)
                    {
                        fieldUI = obj.AddComponent<ModelModifierFieldContainerComponent>();
                    }
                    _spawnedModifierFieldList.Add(fieldUI);
                }

                if (fieldUI != null)
                {
                    fieldUI.gameObject.SetActive(true);
                    fieldUI.Setup(activeModifiers[i]);
                }
            }

            for (int i = requiredCount; i < _spawnedModifierFieldList.Count; i++)
            {
                if (_spawnedModifierFieldList[i] != null)
                {
                    _spawnedModifierFieldList[i].gameObject.SetActive(false);
                }
            }
        }

        private static bool HasLinkedModifier(SkillInstance instance)
        {
            if (instance == null || instance.LinkedModifiers == null) return false;
            for (int i = 0; i < instance.LinkedModifiers.Count; i++)
            {
                if (instance.LinkedModifiers[i] != null) return true;
            }
            return false;
        }
        #endregion
    }
}
