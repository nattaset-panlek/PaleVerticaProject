using System;
using UnityEngine;
using CoreSystem.Entity;
using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data
{
    public class PlayerExperience
    {
        #region [Variables/Fields]
        private ExperienceSystem _expSystem;

        public event Action<long, long> OnBaseExpChanged;
        public event Action<long, long> OnJobExpChanged;
        public event Action<int> OnBaseLevelUp;
        public event Action<int> OnJobLevelUp;
        #endregion

        #region [Properties]
        public long CurrentBaseExp => _expSystem != null ? _expSystem.CurrentBaseExp : (GameData.Player?.Data?.Stats?.CurrentBaseExp ?? 0);
        public long CurrentJobExp => _expSystem != null ? _expSystem.CurrentJobExp : (GameData.Player?.Data?.Stats?.CurrentJobExp ?? 0);
        public long NextBaseExp => _expSystem != null ? _expSystem.NextBaseExp : 0;
        public long NextJobExp => _expSystem != null ? _expSystem.NextJobExp : 0;
        public float BaseExpNormalized => _expSystem != null ? _expSystem.BaseExpNormalized : 0f;
        public float JobExpNormalized => _expSystem != null ? _expSystem.JobExpNormalized : 0f;
        public int BaseLevel => _expSystem != null ? _expSystem.CurrentBaseLevel : (GameData.Player?.Data?.Stats?.BaseLevel ?? 1);
        public int JobLevel => _expSystem != null ? _expSystem.CurrentJobLevel : (GameData.Player?.Data?.Stats?.JobLevel ?? 1);
        #endregion

        #region [Public Methods]
        public void Bind(ExperienceSystem expSystem)
        {
            if (_expSystem != null)
            {
                _expSystem.OnBaseExpChanged -= HandleBaseExpChanged;
                _expSystem.OnJobExpChanged -= HandleJobExpChanged;
                _expSystem.OnBaseLevelUp -= HandleBaseLevelUp;
                _expSystem.OnJobLevelUp -= HandleJobLevelUp;
            }

            _expSystem = expSystem;

            if (_expSystem != null)
            {
                _expSystem.OnBaseExpChanged += HandleBaseExpChanged;
                _expSystem.OnJobExpChanged += HandleJobExpChanged;
                _expSystem.OnBaseLevelUp += HandleBaseLevelUp;
                _expSystem.OnJobLevelUp += HandleJobLevelUp;
            }
        }

        public void AddBaseExp(long amount)
        {
            if (_expSystem != null)
            {
                _expSystem.AddBaseExp(amount);
            }
            else if (GameData.Player?.Data?.Stats != null)
            {
                GameData.Player.Data.Stats.CurrentBaseExp += amount;
                OnBaseExpChanged?.Invoke(GameData.Player.Data.Stats.CurrentBaseExp, 0);
            }
        }

        public void AddJobExp(long amount)
        {
            if (_expSystem != null)
            {
                _expSystem.AddJobExp(amount);
            }
            else if (GameData.Player?.Data?.Stats != null)
            {
                GameData.Player.Data.Stats.CurrentJobExp += amount;
                OnJobExpChanged?.Invoke(GameData.Player.Data.Stats.CurrentJobExp, 0);
            }
        }

        public void SetExp(long baseExp, long jobExp)
        {
            if (_expSystem != null)
            {
                _expSystem.SetExp(baseExp, jobExp);
            }
            else if (GameData.Player?.Data?.Stats != null)
            {
                GameData.Player.Data.Stats.CurrentBaseExp = baseExp;
                GameData.Player.Data.Stats.CurrentJobExp = jobExp;
                OnBaseExpChanged?.Invoke(baseExp, 0);
                OnJobExpChanged?.Invoke(jobExp, 0);
            }
        }

        public void ReceiveProgressionItem(Item item, int amount)
        {
            if (item == null || amount <= 0) return;

            if (_expSystem != null)
            {
                _expSystem.ReceiveProgressionItem(item, amount);
                return;
            }

            if (item is Progression progression)
            {
                if (progression.ProgressionKind == ProgressionKind.JobExp) AddJobExp(amount);
                else AddBaseExp(amount);
            }
            else if (!string.IsNullOrEmpty(item.Name) && item.Name.IndexOf("Job", StringComparison.OrdinalIgnoreCase) >= 0)
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
        private void HandleBaseExpChanged(long current, long next) => OnBaseExpChanged?.Invoke(current, next);
        private void HandleJobExpChanged(long current, long next) => OnJobExpChanged?.Invoke(current, next);
        private void HandleBaseLevelUp(int level) => OnBaseLevelUp?.Invoke(level);
        private void HandleJobLevelUp(int level) => OnJobLevelUp?.Invoke(level);
        #endregion
    }
}
