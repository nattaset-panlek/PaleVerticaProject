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
                { VisualCategory.AOE, new AOEVisual() },
                { VisualCategory.AuraBuff, new AuraBuffVisual() }
            };
        }

        public void PlayVisual(VisualPayload data, SkillPayload context)
        {
            if (data == null || context == null) return;

            if (handlers.TryGetValue(data.Category, out IVisualHandler handler))
            {
                handler.Execute(data, context);
            }
        }
    }
}
