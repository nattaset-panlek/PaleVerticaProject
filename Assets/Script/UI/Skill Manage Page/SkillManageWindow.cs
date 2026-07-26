using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Data;

namespace CoreSystem.UI.SkillManagePage
{
    public class SkillManageWindow : UIWindow
    {
        #region [Variables/Fields]
        [SerializeField] private GameObject _skillItemPrefab;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private PlayerCharacter _playerCharacter;

        private List<SkillItemListUI> _spawnedItemUIList = new List<SkillItemListUI>();
        private SkillInstance _selectedSkillInstance;
        #endregion

        #region [Properties]
        public PlayerCharacter TargetPlayer => _playerCharacter;
        public SkillInstance SelectedSkillInstance => _selectedSkillInstance;
        #endregion

        #region [Unity Methods]
        protected override void Awake()
        {
            base.Awake();

            if (_playerCharacter == null)
            {
                _playerCharacter = GameData.Player.PlayerCharacterModel;
            }
        }

        private void OnEnable()
        {
            RefreshUI();
        }
        #endregion

        #region [Public Methods]
        public override void OnOpen(object data)
        {
            base.OnOpen(data);

            if (data is PlayerCharacter player)
            {
                _playerCharacter = player;
            }

            RefreshUI();
            SignalAction?.Invoke(data);
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public void Initialize(PlayerCharacter player)
        {
            _playerCharacter = player;
            RefreshUI();
        }

        public void SelectSkill(SkillInstance instance)
        {
            _selectedSkillInstance = instance;

            for (int i = 0; i < _spawnedItemUIList.Count; i++)
            {
                if (_spawnedItemUIList[i] != null)
                {
                    _spawnedItemUIList[i].SetSelected(_spawnedItemUIList[i].SkillInstance == _selectedSkillInstance);
                }
            }
        }

        public void RefreshUI()
        {
            ClearItems();

            if (_playerCharacter == null)
            {
                _playerCharacter = GameData.Player.PlayerCharacterModel;
            }

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
        }
        #endregion
    }
}
