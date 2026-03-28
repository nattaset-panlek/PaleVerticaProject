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
        public int Id;
        public string Name;
        public ElementType SkillElement = ElementType.Neutral;
        public DamageType SkillDamageType = DamageType.Physical;
        
        [SerializeReference, SubclassSelector]
        public TargetingLogic BaseTargeting;
        
        [SerializeReference, SubclassSelector]
        public List<EffectLogic> Effects = new List<EffectLogic>();
        public VisualPayload VisualData;
        public GameObject VisualPrefab;
        public VisualAppearance VisualSpawnPoint = VisualAppearance.TargetPosition;
        public GameObject CastVisualPrefab;
        public VisualAppearance CastSpawnPoint = VisualAppearance.TargetPosition;
        
        public float BaseDamage;
        public float BaseAOERadius;
        public int BaseProjectileCount;
        public float BaseManaCost;
        
        [Header("Skill Type")]
        public bool IsToggle;
        
        [Header("Continuous Area Setup")]
        [Tooltip("If checked, the targeting shape stays on the ground and repeatedly deals damage.")]
        public bool IsContinuousArea = false;
        public float AreaDuration = 5f;
        public float AreaTickRate = 0.5f;
        
        [Header("Cast & Delays")]
        public float BaseVCT; // Variable Cast Time
        public float BaseFCT; // Fixed Cast Time
        public float BaseCastDelay; // Global delay applied to ALL skills after casting
        public float BaseCooldown; // Skill-specific cooldown
        
        [Header("Dynamic Scaling (Stat x AttrMult + Flat + LevelMult x Lv) x Global")]
        [Tooltip("Formula: (Stat x AttrMult + FlatBonus + LevelMult x Level) x GlobalMult")]
        public List<OutputScaling> Scalings = new List<OutputScaling>();
        public virtual void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            if (this.IsContinuousArea && this.BaseTargeting != null)
            {
                GameObject areaGo = new GameObject("ContinuousArea_" + this.Name);
                
                // If it follows the caster, parent it
                if (this.VisualSpawnPoint == VisualAppearance.FollowCaster)
                {
                    areaGo.transform.SetParent(caster.CharacterRef.transform);
                    areaGo.transform.localPosition = Vector3.zero;
                }
                else
                {
                    areaGo.transform.position = payload.IsTargetPositionSet ? payload.TargetPosition : payload.OriginPosition;
                }
                
                var areaBehaviour = areaGo.AddComponent<ContinuousAreaBehaviour>();
                areaBehaviour.Initialize(payload, this.BaseTargeting, this.Effects, this.VisualData, this.AreaDuration, this.AreaTickRate);
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
                    if (effect.ApplyOncePerCast)
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

            if (this.VisualData != null)
            {
                Visuals.VisualManager.Instance?.PlayVisual(this.VisualData, payload);
            }

            // Spawn main VisualPrefab
            if (this.VisualPrefab != null)
            {
                // For toggle skills, only spawn the visual once
                if (this.IsToggle && instance.ActiveVisual != null)
                {
                    // Existing visual, follows automatically if parented
                }
                else
                {
                    Vector3 spawnPos = payload.IsTargetPositionSet ? payload.TargetPosition : payload.OriginPosition;
                    Transform parent = null;

                    if (this.VisualSpawnPoint == VisualAppearance.CasterPosition)
                    {
                        spawnPos = caster.CharacterRef.transform.position;
                    }
                    else if (this.VisualSpawnPoint == VisualAppearance.FollowCaster)
                    {
                        spawnPos = caster.CharacterRef.transform.position;
                        parent = caster.CharacterRef.transform;
                    }

                    GameObject vfxInstance = GameObject.Instantiate(this.VisualPrefab, spawnPos, Quaternion.identity, parent);
                    
                    if (this.IsToggle)
                    {
                        instance.ActiveVisual = vfxInstance;
                    }

                    SkillVisualEffect sve = vfxInstance.GetComponent<SkillVisualEffect>();
                    if (sve != null)
                    {
                        if (this.IsToggle) sve.destroyAfterDuration = false;
                        sve.Initialize(payload);
                    }
                }
            }

            // For toggle skills, also handle CastVisualPrefab if it should follow and hasn't been spawned yet
            if (this.IsToggle && this.CastVisualPrefab != null && instance.ActiveVisual == null)
            {
                Vector3 spawnPos = caster.CharacterRef.transform.position;
                Transform parent = null;

                if (this.CastSpawnPoint == VisualAppearance.FollowCaster)
                {
                    parent = caster.CharacterRef.transform;
                }

                GameObject castVfx = GameObject.Instantiate(this.CastVisualPrefab, spawnPos, Quaternion.identity, parent);
                instance.ActiveVisual = castVfx;

                SkillVisualEffect sve = castVfx.GetComponent<SkillVisualEffect>();
                if (sve != null)
                {
                    sve.destroyAfterDuration = false;
                    sve.Initialize(payload);
                }
            }
        }
    }
}
