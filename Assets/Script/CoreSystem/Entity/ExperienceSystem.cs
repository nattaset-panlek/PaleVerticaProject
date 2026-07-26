using UnityEngine;
using CoreSystem.Attributes;

namespace CoreSystem.Entity
{
    [System.Serializable]
    public class ExperienceSystem
    {
        [Header("State")]
        public long CurrentBaseExp = 0;
        public long CurrentJobExp = 0;

        private Character _owner;
        private CoreSystem.Data.ExperienceTable _table;

        public event System.Action<int> OnBaseLevelUp;
        public event System.Action<int> OnJobLevelUp;

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
            return (long)(100f * Mathf.Pow(currentLevel, 1.5f));
        }

        public long GetNextJobExp(int currentLevel)
        {
            if (_table != null) return _table.GetJobExp(currentLevel);
            return (long)(50f * Mathf.Pow(currentLevel, 1.6f));
        }


        public void SetExp(long baseExp, long jobExp)
        {
            CurrentBaseExp = baseExp;
            CurrentJobExp = jobExp;
        }

        public void AddBaseExp(long amount)
        {
            CurrentBaseExp += amount;
            CheckBaseLevelUp();
        }

        public void AddJobExp(long amount)
        {
            CurrentJobExp += amount;
            CheckJobLevelUp();
        }

        private void CheckBaseLevelUp()
        {
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
    }
}
