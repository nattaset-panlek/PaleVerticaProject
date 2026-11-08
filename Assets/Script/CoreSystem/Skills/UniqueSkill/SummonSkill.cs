using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "SummonSkill", menuName = "Skill System/Unique/Summon Skill")]
    public class SummonSkill : ActiveSkill
    {
        #region [Variables/Fields]
        [Header("Summon")]
        [Tooltip("The summoned body. Any prefab with a Character component works; swap this for the real art.")]
        public GameObject MinionPrefab;
        public bool IsPermanent = false;
        public float Lifetime = 20f;
        public int MaxMinions = 2;
        public int SummonsPerCast = 1;
        public float SpawnDistance = 1.5f;
        public float LeashRadius = 4f;
        #endregion

        #region [Public Methods]
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            Character owner = caster != null ? caster.CharacterRef : null;
            if (owner == null) return;
            if (MinionPrefab == null)
            {
                Debug.LogWarning($"[Summon] {Name} has no MinionPrefab set.");
                return;
            }

            float lifetime = IsPermanent ? 0f : SupportHitFormula.Duration(Lifetime, payload.DurationMultiplier);
            int count = Mathf.Max(1, SummonsPerCast);
            for (int i = 0; i < count; i++)
            {
                MakeRoom(owner);
                Vector3 position = SummonFactory.SpawnPoint(owner.transform.position, payload.GetAimDirection(), SpawnDistance, i);
                SummonFactory.Create(owner, MinionPrefab, position, Id, lifetime, LeashRadius);
            }

            if (owner.UniquePassives != null) owner.UniquePassives.TriggerOnSkillExecuted(this, payload);
        }
        #endregion

        #region [Private Methods]
        private void MakeRoom(Character owner)
        {
            int excess = SummonRegistry.ExcessCount(SummonRegistry.Count(owner, Id), MaxMinions);
            for (int i = 0; i < excess; i++)
            {
                SummonedUnit oldest = SummonRegistry.Oldest(owner, Id);
                if (oldest == null) return;
                oldest.Dismiss();
            }
        }
        #endregion
    }
}
