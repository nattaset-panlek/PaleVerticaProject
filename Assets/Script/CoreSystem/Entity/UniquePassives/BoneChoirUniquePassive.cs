using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("BoneChoir")]
    public class BoneChoirUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private const int ChoirSkillId = -2;
        private const int MaxChoir = 3;
        #endregion

        #region [Public Methods]
        public override void OnEquip()
        {
            CombatEvents.OnKilled += HandleKilled;
        }

        public override void OnUnequip()
        {
            CombatEvents.OnKilled -= HandleKilled;
        }

        private void HandleKilled(IEntity killer, IEntity victim)
        {
            if (!(victim is Character body) || body.Race != RaceType.Undead || !TeamRegistry.IsHostile(Owner, victim)) return;
            if (Vector3.Distance(Owner.transform.position, victim.transform.position) > 6f) return;
            if (SummonRegistry.Count(Owner, ChoirSkillId) >= MaxChoir) return;
            var model = body.GetCharacterData();
            if (model == null || string.IsNullOrEmpty(model.PrefabPath)) return;
            GameObject prefab = LoadPrefab(model.PrefabPath);
            if (prefab != null) SummonFactory.Create(Owner, prefab, victim.transform.position, ChoirSkillId, 15f, 4f);
        }

        private static GameObject LoadPrefab(string path)
        {
            try
            {
                return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(path).WaitForCompletion();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[BoneChoir] Could not load {path}: {exception.Message}");
                return null;
            }
        }
        #endregion
    }
}
