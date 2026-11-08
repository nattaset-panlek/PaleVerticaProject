using System.Collections.Generic;
using CoreSystem.Utility;
using UnityEngine;

namespace CoreSystem.Skills.Visuals
{
    public class VisualManager : MonoBehaviour
    {
        #region [Variables/Fields]
        private static VisualManager _instance;

        private readonly Dictionary<VisualCategory, IVisualHandler> _handlers = new Dictionary<VisualCategory, IVisualHandler>
        {
            { VisualCategory.InstantHit, new InstantHitVisual() },
            { VisualCategory.Projectile, new ProjectileVisual() },
            { VisualCategory.RayBeam, new RayBeamVisual() },
            { VisualCategory.GroundEruption, new GroundEruptionVisual() },
            { VisualCategory.AuraBuff, new AuraBuffVisual() }
        };
        #endregion

        #region [Properties]
        public static VisualManager Instance => _instance;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static VisualManager Create()
        {
            _instance = RuntimeRoot.CreateChild("VisualManager").AddComponent<VisualManager>();
            return _instance;
        }

        public static VisualManager Ensure()
        {
            return _instance != null ? _instance : Create();
        }

        public void PlayVisual(ActiveSkill skill, SkillPayload context)
        {
            if (skill == null || context == null) return;
            if (!_handlers.TryGetValue(skill.VisualType, out IVisualHandler handler)) return;

            GameObject prefab = GetPrefab(skill);
            if (prefab == null) return;
            if (prefab == skill.SkillIndicatorPrefab && (context.IsRepeatCast || context.CastIndicatorShown)) return;
            handler.Execute(prefab, context);
        }

        public static GameObject GetPrefab(ActiveSkill skill)
        {
            if (skill == null) return null;
            switch (skill.VisualType)
            {
                case VisualCategory.InstantHit: return skill.ImpactVfxPrefab;
                case VisualCategory.Projectile: return skill.ProjectilePrefab;
                case VisualCategory.RayBeam: return skill.BeamVfxPrefab;
                case VisualCategory.GroundEruption: return skill.EruptionVfxPrefab;
                case VisualCategory.AuraBuff: return skill.AuraVfxPrefab;
                default: return null;
            }
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
