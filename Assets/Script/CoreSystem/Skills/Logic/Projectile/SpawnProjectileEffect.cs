using CoreSystem.Entity;
using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class SpawnProjectileEffect : EffectLogic
    {
        public GameObject ProjectilePrefab;
        
        [SerializeReference, SubclassSelector]
        public EffectLogic DamageEffectToApply; 

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (ProjectilePrefab != null)
            {
                float spreadAngle = 90f; // Total arc angle, adjust as needed. Max 180.
                if (payload.ProjectileCount > 1)
                {
                    spreadAngle = Mathf.Min(180f, 45f * (payload.ProjectileCount - 1)); // 45 degrees between each, up to 180 max
                }

                float startAngle = -spreadAngle / 2f;
                float angleStep = payload.ProjectileCount > 1 ? spreadAngle / (payload.ProjectileCount - 1) : 0f;

                for (int i = 0; i < payload.ProjectileCount; i++)
                {
                    float currentAngle = payload.ProjectileCount == 1 ? 0f : startAngle + (angleStep * i);
                    // Default base rotation
                    Quaternion baseRotation = Quaternion.identity;
                    
                    if (payload.Caster != null)
                    {
                        // Aim based on caster's facing direction (up vector for 2D top-down)
                        baseRotation = payload.Caster.transform.rotation;
                    }

                    Quaternion rotation = baseRotation * Quaternion.Euler(0, 0, currentAngle);

                    GameObject projObj = UnityEngine.Object.Instantiate(ProjectilePrefab, payload.OriginPosition, rotation);
                    ProjectileBehaviour projBehaviour = projObj.GetComponent<ProjectileBehaviour>();
                    
                    if (projBehaviour != null)
                    {
                        projBehaviour.Initialize(payload, DamageEffectToApply);
                    }
                }
            }
        }
    }
}
