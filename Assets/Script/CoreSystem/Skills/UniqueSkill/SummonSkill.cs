using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "SummonSkill", menuName = "Skill System/Unique/Summon Skill")]
    public class SummonSkill : ActiveSkill
    {
        public GameObject MinionPrefab;
        public bool IsPermanent = false;
        public float Lifetime = 10f;
        public int MaxMinions = 3;

        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            // Calculate spawn position (e.g., slightly in front of caster)
            Vector3 spawnPos = caster.CharacterRef.transform.position + caster.CharacterRef.transform.up * 1.5f;
            
            GameObject minion = GameObject.Instantiate(MinionPrefab, spawnPos, Quaternion.identity);
            
            if (!IsPermanent)
            {
                GameObject.Destroy(minion, Lifetime);
            }

            Debug.Log($"[Summon] Spawned {(IsPermanent ? "Permanent" : "Temporary")} minion.");
            
            // Note: You could track minions here or on the Character component
        }
    }
}
