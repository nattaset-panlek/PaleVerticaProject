using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Visuals;
using UnityEngine;

namespace CoreSystem.Skills
{
    [CreateAssetMenu(fileName = "NewActiveSkill", menuName = "Skill System/Active Skill")]
    public class ActiveSkill : ScriptableObject
    {
        #region [Variables/Fields]
        public int Id;
        public string Name;
        public Sprite Icon;
        public ElementType SkillElement = ElementType.Neutral;
        public DamageType SkillDamageType = DamageType.Physical;
        
        [SerializeReference, SubclassSelector]
        public TargetingLogic BaseTargeting;
        
        [SerializeReference, SubclassSelector]
        public List<EffectLogic> Effects = new List<EffectLogic>();

        [Header("Cast Phase Visuals")]
        public GameObject CastVfxPrefab;
        public VisualAppearance CastVfxSpawnPoint = VisualAppearance.FollowCaster;

        [Header("Skill Visual Category")]
        public VisualCategory VisualType = VisualCategory.InstantHit;

        [Header("Visual Prefabs")]
        public GameObject SkillIndicatorPrefab;
        public GameObject EruptionVfxPrefab;
        public GameObject ProjectilePrefab;
        public GameObject ImpactVfxPrefab;
        public GameObject AuraVfxPrefab;
        public GameObject BeamVfxPrefab;
        
        public float BaseDamage;
        public float BaseAOERadius;
        public int BaseProjectileCount;
        public float BaseManaCost;
        public List<SkillTag> Tags = new List<SkillTag>();
        
        [Header("Skill Type")]
        public bool IsToggle;
        
        [Header("Continuous Area Setup")]
        public bool IsContinuousArea = false;
        public float AreaDuration = 5f;
        public float AreaTickRate = 0.5f;
        
        [Header("Cast & Delays")]
        public float BaseVCT; 
        public float BaseFCT; 
        public float BaseCastDelay; 
        public float BaseCooldown; 
        
        [Header("Skill Chain & Combo")]
        public List<SkillChainOverride> ChainOverrides = new List<SkillChainOverride>();

        public List<OutputScaling> Scalings = new List<OutputScaling>();
        public int MaxSupportSlots = 5;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static SkillChainOverride GetActiveChainOverride(CoreSystem.Entity.Character caster, ActiveSkill baseSkill)
        {
            if (baseSkill == null || baseSkill.ChainOverrides.Count == 0) return null;
            if (caster == null) return null;
            
            var buffManager = caster.GetComponent<Logic.BuffSystem>();
            if (buffManager == null) return null;

            for (int i = 0; i < baseSkill.ChainOverrides.Count; i++)
            {
                var over = baseSkill.ChainOverrides[i];
                if (buffManager.HasBuff(over.RequiredBuffCode))
                {
                    return over;
                }
            }
            return null;
        }

        public virtual void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            if (this.IsContinuousArea && this.BaseTargeting != null)
            {
                GameObject areaGo = new GameObject("ContinuousArea_" + this.Name);
                
                if (this.CastVfxSpawnPoint == VisualAppearance.FollowCaster)
                {
                    areaGo.transform.SetParent(caster.CharacterRef.transform);
                    areaGo.transform.localPosition = Vector3.zero;
                }
                else
                {
                    areaGo.transform.position = payload.IsTargetPositionSet ? payload.TargetPosition : payload.OriginPosition;
                }
                
                var areaBehaviour = areaGo.AddComponent<ContinuousAreaBehaviour>();
                areaBehaviour.Initialize(this, payload, this.BaseTargeting, this.Effects, this.AreaDuration, this.AreaTickRate);
                return;
            }

            if (this.BaseTargeting != null)
            {
                payload.Targets = this.BaseTargeting.GetTargets(payload);
                caster.LastHitTargets.Clear();
                for (int i = 0; i < payload.Targets.Count; i++)
                {
                    if (payload.Targets[i] is MonoBehaviour mb)
                    {
                        caster.LastHitTargets.Add(mb.gameObject);
                    }
                }
            }

            for (int i = 0; i < this.Effects.Count; i++)
            {
                EffectLogic effect = this.Effects[i];
                if (effect != null)
                {
                    if (effect.ApplyToCaster)
                    {
                        effect.Apply(payload, caster.CharacterRef);
                    }
                    else if (effect.ApplyOncePerCast)
                    {
                        effect.Apply(payload, payload.Targets.Count > 0 ? payload.Targets[0] : null);
                    }
                    else
                    {
                        for (int j = 0; j < payload.Targets.Count; j++)
                        {
                            effect.Apply(payload, payload.Targets[j]);
                        }
                    }
                }
            }

            if (this.IsToggle)
            {
                if (instance.ActiveVisual == null && this.AuraVfxPrefab != null)
                {
                    Vector3 spawnPos = caster.CharacterRef.transform.position;
                    Transform parent = caster.CharacterRef.transform;
                    GameObject vfxInstance = GameObject.Instantiate(this.AuraVfxPrefab, spawnPos, Quaternion.identity, parent);
                    instance.ActiveVisual = vfxInstance;
                    
                    var visual = vfxInstance.GetComponent<ISkillVisual>();
                    if (visual != null)
                    {
                        visual.Initialize(payload);
                    }
                    else
                    {
                        foreach (var v in vfxInstance.GetComponentsInChildren<ISkillVisual>())
                        {
                            v.Initialize(payload);
                        }
                    }
                    
                    var sve = vfxInstance.GetComponent<SkillVisualEffect>();
                    if (sve != null)
                    {
                        sve.destroyAfterDuration = false;
                    }
                }
            }
            else
            {
                if (Visuals.VisualManager.Instance != null)
                {
                    Visuals.VisualManager.Instance.PlayVisual(this, payload);
                }
            }

            if (this.IsToggle && this.CastVfxPrefab != null && instance.ActiveVisual == null)
            {
                Vector3 spawnPos = caster.CharacterRef.transform.position;
                Transform parent = null;

                if (this.CastVfxSpawnPoint == VisualAppearance.FollowCaster)
                {
                    parent = caster.CharacterRef.transform;
                }

                GameObject castVfx = GameObject.Instantiate(this.CastVfxPrefab, spawnPos, Quaternion.identity, parent);
                instance.ActiveVisual = castVfx;

                var visual = castVfx.GetComponent<ISkillVisual>();
                if (visual != null)
                {
                    visual.Initialize(payload);
                }
                else
                {
                    foreach (var v in castVfx.GetComponentsInChildren<ISkillVisual>())
                    {
                        v.Initialize(payload);
                    }
                }

                var sve = castVfx.GetComponent<SkillVisualEffect>();
                if (sve != null)
                {
                    sve.destroyAfterDuration = false;
                }
            }

            if (caster != null && caster.CharacterRef != null && caster.CharacterRef.UniquePassives != null)
            {
                caster.CharacterRef.UniquePassives.TriggerOnSkillExecuted(this, payload);
            }
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
