using System.Collections.Generic;
using CoreSystem.Combat;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Entity;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Loot
{
    public sealed class LootManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private const float ScatterRadius = 0.7f;
        private const string LabelsWindowName = "LootLabels";
        private static LootManager _instance;
        private bool _labelsOpened;

        public static event System.Action<GroundItem> OnPickupRequested;
        #endregion

        #region [Properties]
        public static LootManager Instance => _instance;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            CombatEvents.OnKilled += HandleKilled;
        }

        private void OnDestroy()
        {
            CombatEvents.OnKilled -= HandleKilled;
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static LootManager Create()
        {
            return RuntimeRoot.CreateChild("LootManager").AddComponent<LootManager>();
        }

        public static LootManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public static void RequestPickup(GroundItem item)
        {
            if (item != null) OnPickupRequested?.Invoke(item);
        }
        #endregion

        #region [Private Methods]
        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!(victim is Character character)) return;

            CharacterModel model = Database.GetCharacter(character.CharacterId);
            if (model == null || model.DropTableId <= 0) return;

            List<LootRoll> rolls = LootFormula.Roll(DropTableDatabase.Get(model.DropTableId), () => Random.value);
            if (rolls.Count == 0) return;

            OpenLabelsOnce();
            IEntity owner = KillCreditRegistry.TopAttacker(victim);
            foreach (LootRoll roll in rolls)
            {
                Item item = Database.GetItem(roll.ItemId);
                if (item == null)
                {
                    Debug.LogWarning($"LootManager: item {roll.ItemId} in drop table {model.DropTableId} does not exist.");
                    continue;
                }

                Vector2 offset = Random.insideUnitCircle * ScatterRadius;
                GroundItem.Create(item, roll.Quantity, character.transform.position + (Vector3)offset, owner);
            }
        }

        private void OpenLabelsOnce()
        {
            if (_labelsOpened || CoreSystem.UI.UIManager.Instance == null) return;
            _labelsOpened = true;
            CoreSystem.UI.UIManager.Instance.Open(LabelsWindowName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            OnPickupRequested = null;
        }
        #endregion
    }
}
