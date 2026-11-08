using System;
using UnityEngine;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Entity
{
    [Serializable]
    public class ExperienceSystem
    {
        #region [Variables/Fields]
        [Header("State")]
        public long CurrentBaseExp = 0;
        public long CurrentJobExp = 0;

        private Character _owner;
        private CoreSystem.Data.ExperienceTable _table;

        public event Action<int> OnBaseLevelUp;
        public event Action<int> OnJobLevelUp;
        public event Action<long, long> OnBaseExpChanged;
        public event Action<long, long> OnJobExpChanged;
        #endregion

        #region [Properties]
        public int CurrentBaseLevel
        {
            get
            {
                var levelAttr = _owner?.attributeSystem?.GetAttribute(AttributeType.BaseLevel);
                return levelAttr != null ? (int)levelAttr.BaseValue : 1;
            }
        }

        public int CurrentJobLevel
        {
            get
            {
                var levelAttr = _owner?.attributeSystem?.GetAttribute(AttributeType.JobLevel);
                return levelAttr != null ? (int)levelAttr.BaseValue : 1;
            }
        }

        public long NextBaseExp => GetNextBaseExp(CurrentBaseLevel);
        public long NextJobExp => GetNextJobExp(CurrentJobLevel);

        public float BaseExpNormalized => NextBaseExp > 0 ? Mathf.Clamp01((float)CurrentBaseExp / NextBaseExp) : 0f;
        public float JobExpNormalized => NextJobExp > 0 ? Mathf.Clamp01((float)CurrentJobExp / NextJobExp) : 0f;
        #endregion

        #region [Public Methods]
        public void Initialize(Character owner)
        {
            _owner = owner;
            _table = Resources.Load<CoreSystem.Data.ExperienceTable>("Data/ExperienceTable");
            if (_table == null)
            {
                Debug.LogWarning("[ExperienceSystem] ExperienceTable not found in Resources/Data/ExperienceTable! Falling back to formula.");
            }
        }

        public long GetNextBaseExp(int currentLevel)
        {
            if (_table != null) return _table.GetBaseExp(currentLevel);
            return ProgressionFormula.FallbackBaseExp(currentLevel);
        }

        public long GetNextJobExp(int currentLevel)
        {
            if (_table != null) return _table.GetJobExp(currentLevel);
            return ProgressionFormula.FallbackJobExp(currentLevel);
        }

        public void SetExp(long baseExp, long jobExp)
        {
            CurrentBaseExp = baseExp;
            CurrentJobExp = jobExp;
            OnBaseExpChanged?.Invoke(CurrentBaseExp, NextBaseExp);
            OnJobExpChanged?.Invoke(CurrentJobExp, NextJobExp);
        }

        public void AddBaseExp(long amount)
        {
            if (amount <= 0) return;
            CurrentBaseExp += amount;
            CheckBaseLevelUp();
            OnBaseExpChanged?.Invoke(CurrentBaseExp, NextBaseExp);
        }

        public void AddJobExp(long amount)
        {
            if (amount <= 0) return;
            CurrentJobExp += amount;
            CheckJobLevelUp();
            OnJobExpChanged?.Invoke(CurrentJobExp, NextJobExp);
        }

        public void ReceiveProgressionItem(Item item, int amount)
        {
            if (item == null || amount <= 0) return;

            if (item is Progression progression)
            {
                switch (progression.ProgressionKind)
                {
                    case ProgressionKind.JobExp:
                        AddJobExp(amount);
                        return;
                    case ProgressionKind.BaseExp:
                    default:
                        AddBaseExp(amount);
                        return;
                }
            }

            if (!string.IsNullOrEmpty(item.Name) && item.Name.IndexOf("Job", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                AddJobExp(amount);
            }
            else
            {
                AddBaseExp(amount);
            }
        }
        #endregion

        #region [Private Methods]
        private void CheckBaseLevelUp()
        {
            if (_owner == null || _owner.attributeSystem == null) return;
            var levelAttr = _owner.attributeSystem.GetAttribute(AttributeType.BaseLevel);
            if (levelAttr == null) return;

            int currentLevel = (int)levelAttr.BaseValue;
            long nextExp = GetNextBaseExp(currentLevel);

            while (CurrentBaseExp >= nextExp)
            {
                CurrentBaseExp -= nextExp;
                currentLevel++;
                levelAttr.BaseValue = currentLevel;
                
                Debug.Log($"[LevelUp] Character Base Level Up! {currentLevel-1} -> {currentLevel}");
                OnBaseLevelUp?.Invoke(currentLevel);
                
                nextExp = GetNextBaseExp(currentLevel);
            }
            
            _owner.attributeSystem.UpdateDerivedAttributes();
        }

        private void CheckJobLevelUp()
        {
            if (_owner == null || _owner.attributeSystem == null) return;
            var levelAttr = _owner.attributeSystem.GetAttribute(AttributeType.JobLevel);
            if (levelAttr == null) return;

            int currentLevel = (int)levelAttr.BaseValue;
            long nextExp = GetNextJobExp(currentLevel);

            while (CurrentJobExp >= nextExp)
            {
                CurrentJobExp -= nextExp;
                currentLevel++;
                levelAttr.BaseValue = currentLevel;
                
                Debug.Log($"[LevelUp] Character Job Level Up! {currentLevel-1} -> {currentLevel}");
                OnJobLevelUp?.Invoke(currentLevel);
                
                nextExp = GetNextJobExp(currentLevel);
            }
            
            _owner.attributeSystem.UpdateDerivedAttributes();
        }
        #endregion
    }
}
