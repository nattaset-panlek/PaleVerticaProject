using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using TMPro;
using CoreSystem.Player;

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

        [Header("Support Skill Slots")]
        [SerializeField] private SupportSkillSlotComponent[] _supportSkillSlots = new SupportSkillSlotComponent[5];

        [Header("Modifier Fields Container")]
        [SerializeField] private GameObject _modifierFieldPrefab;
        [SerializeField] private Transform _modifierFieldContainer;

        private List<SkillItemListUI> _spawnedItemUIList = new List<SkillItemListUI>();
        private List<GameObject> _spawnedTagList = new List<GameObject>();
        private List<ModelModifierFieldContainerComponent> _spawnedModifierFieldList = new List<ModelModifierFieldContainerComponent>();
        private SkillInstance _selectedSkillInstance;
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
            RefreshUI();
        }

        private void OnDisable()
        {
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

            for (int i = 0; i < _spawnedItemUIList.Count; i++)
            {
                if (_spawnedItemUIList[i] != null)
                {
                    _spawnedItemUIList[i].SetSelected(_spawnedItemUIList[i].SkillInstance == _selectedSkillInstance);
                }
            }

            UpdateSelectedSkillDetails(instance);
        }

        public void RefreshUI()
        {
            ClearItems();

            if (_playerCharacter == null)
            {
                _playerCharacter = PlayerCore.GetPlayer();
            }

            SubscribeToSkillCaster();

            if (_playerCharacter == null || _playerCharacter.skillCaster == null) return;
            if (_skillItemPrefab == null || _itemContainer == null) return;

            List<SkillInstance> grantedSkills = _playerCharacter.skillCaster.GrantedSkills;
            if (grantedSkills == null || grantedSkills.Count == 0) return;

            for (int i = 0; i < grantedSkills.Count; i++)
            {
                SkillInstance instance = grantedSkills[i];
                if (instance == null || instance.SkillData == null) continue;

                GameObject itemObj = Instantiate(_skillItemPrefab, _itemContainer);
                itemObj.SetActive(true);

                SkillItemListUI itemUI = itemObj.GetComponent<SkillItemListUI>();
                if (itemUI == null)
                {
                    itemUI = itemObj.AddComponent<SkillItemListUI>();
                }

                itemUI.Setup(instance, _playerCharacter, this, RefreshUI);
                _spawnedItemUIList.Add(itemUI);
            }

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
        private void ClearItems()
        {
            for (int i = 0; i < _spawnedItemUIList.Count; i++)
            {
                if (_spawnedItemUIList[i] != null)
                {
                    Destroy(_spawnedItemUIList[i].gameObject);
                }
            }
            _spawnedItemUIList.Clear();
            ClearTags();
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

            SpawnTag(data.SkillDamageType.ToString());

            if (data.SkillElement != ElementType.Neutral)
            {
                SpawnTag(data.SkillElement.ToString());
            }

            SpawnTag(data.IsToggle ? "Toggle" : "Active");

            if (data.Tags != null)
            {
                for (int i = 0; i < data.Tags.Count; i++)
                {
                    SpawnTag(data.Tags[i].ToString());
                }
            }

            UpdateSupportSkillSlots(instance);
        }

        private void SpawnTag(string text)
        {
            if (string.IsNullOrEmpty(text) || _skillTagPrefab == null || _tagContainer == null) return;

            GameObject tagObj = Instantiate(_skillTagPrefab, _tagContainer);
            tagObj.SetActive(true);

            SkillTagComponent tagComponent = tagObj.GetComponent<SkillTagComponent>();
            if (tagComponent != null)
            {
                tagComponent.Setup(text);
            }
            else
            {
                TMP_Text tmp = tagObj.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = text;
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
        }

        private void UpdateModifierFields(SkillInstance instance)
        {
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
        #endregion
    }
}
