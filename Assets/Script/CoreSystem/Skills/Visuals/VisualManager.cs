using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class VisualManager : MonoBehaviour
    {
        public static VisualManager Instance { get; private set; }

        private Dictionary<VisualCategory, IVisualHandler> handlers;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeHandlers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeHandlers()
        {
            handlers = new Dictionary<VisualCategory, IVisualHandler>
            {
                { VisualCategory.InstantHit, new InstantHitVisual() },
                { VisualCategory.Projectile, new ProjectileVisual() },
                { VisualCategory.RayBeam, new RayBeamVisual() },
                { VisualCategory.GroundEruption, new GroundEruptionVisual() },
                { VisualCategory.AuraBuff, new AuraBuffVisual() }
            };
        }

        public void PlayVisual(ActiveSkill skill, SkillPayload context)
        {
            if (skill == null || context == null) return;

            if (handlers.TryGetValue(skill.VisualType, out IVisualHandler handler))
            {
                GameObject prefab = null;
                switch (skill.VisualType)
                {
                    case VisualCategory.InstantHit:
                        prefab = skill.ImpactVfxPrefab;
                        break;
                    case VisualCategory.Projectile:
                        prefab = skill.ProjectilePrefab;
                        break;
                    case VisualCategory.RayBeam:
                        prefab = skill.BeamVfxPrefab;
                        break;
                    case VisualCategory.GroundEruption:
                        prefab = skill.EruptionVfxPrefab;
                        break;
                    case VisualCategory.AuraBuff:
                        prefab = skill.AuraVfxPrefab;
                        break;
                }

                if (prefab != null)
                {
                    handler.Execute(prefab, context);
                }
            }
        }
    }
}
