using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Items;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;
using CoreSystem.Skills.Visuals;

public static class SkillConverter
{
    public static void ShowConverter(ActiveSkillJsonData data)
    {
        string path = EditorUtility.SaveFilePanelInProject("Save ActiveSkill Asset", data.SkillName, "asset", "Select location for the new skill asset", "Assets/Resources/Skill/ActiveSkill");
        if (string.IsNullOrEmpty(path)) return;

        ActiveSkill asset = ScriptableObject.CreateInstance<ActiveSkill>();
        asset.Id = data.SkillID;
        asset.Name = data.SkillName;
        
        if (System.Enum.TryParse(data.SkillElement, out ElementType element))
            asset.SkillElement = element;

        if (System.Enum.TryParse(data.DamageType, out DamageType damageType))
            asset.SkillDamageType = damageType;

        asset.BaseDamage = data.BaseDamage;
        asset.BaseManaCost = data.BaseManaCost;
        asset.BaseVCT = data.BaseVCT;
        asset.BaseFCT = data.BaseFCT;
        asset.BaseCastDelay = data.BaseCastDelay;
        asset.BaseCooldown = data.BaseCooldown;
        asset.BaseProjectileCount = data.BaseProjectileCount;
        asset.IsToggle = data.IsToggle;
        asset.BaseAOERadius = data.BaseAOERadius;
        
        if (!string.IsNullOrEmpty(data.VisualPrefabName))
            asset.VisualPrefab = Resources.Load<GameObject>(data.VisualPrefabName);
        if (!string.IsNullOrEmpty(data.CastVisualPrefabName))
            asset.CastVisualPrefab = Resources.Load<GameObject>(data.CastVisualPrefabName);
            
        if (System.Enum.TryParse(data.CastVisualSpawnPoint, out VisualAppearance castSpawn))
            asset.CastSpawnPoint = castSpawn;
            
        if (data.BaseTargeting != null)
        {
            TargetingLogic logic = null;
            switch (data.BaseTargeting.Type)
            {
                case "Single": logic = new SingleTargeting(); break;
                case "Sphere": logic = new SphereAOETargeting(); break;
                case "Cone": logic = new ConeTargeting { ConeAngle = data.BaseTargeting.Angle }; break;
                case "Box": logic = new BoxTargeting { BoxSize = data.BaseTargeting.BoxSize, Offset = data.BaseTargeting.Offset }; break;
                case "Line": logic = new LinePierceTargeting(); break;
                case "Self": logic = new SelfTargeting(); break;
                case "TargetLocation": logic = new TargetLocationTargeting { CastRange = data.BaseTargeting.CastRange > 0 ? data.BaseTargeting.CastRange : 7f }; break;
            }
            asset.BaseTargeting = logic;

            if (logic != null)
            {
                if (System.Enum.TryParse(data.BaseTargeting.Alignment, out TargetAlignment alignment))
                    logic.AlignmentAction = alignment;

                logic.IncludeSelf = data.BaseTargeting.IncludeSelf;
            }
        }

        if (data.Scalings != null)
            asset.Scalings = new List<OutputScaling>(data.Scalings);

        if (data.Effects != null)
        {
            asset.Effects = new List<EffectLogic>();
            foreach (var e in data.Effects)
            {
                EffectLogic effect = null;
                switch (e.EffectType)
                {
                    case "DamageEffect": effect = new DamageEffect(); break;
                    case "HealEffect": effect = new HealEffect { TargetAttribute = (AttributeType)e.AttributeID, BaseHealAmount = e.Value }; break;
                    case "ApplyBuffEffect": 
                        if (System.Enum.TryParse(e.ModType, out CoreSystem.ModifierType mType))
                            effect = new ApplyBuffEffect { TargetAttribute = (AttributeType)e.AttributeID, ModifierValue = e.Value, ModType = mType }; 
                        break;
                    case "ApplyTemporaryBuffEffect":
                        if (System.Enum.TryParse(e.ModType, out CoreSystem.ModifierType tempMType) && System.Enum.TryParse(e.Category, out BuffCategory bCat))
                        {
                            effect = new ApplyTemporaryBuffEffect { 
                                TargetAttribute = (AttributeType)e.AttributeID, 
                                ModifierValue = e.Value, 
                                ModType = tempMType,
                                Duration = e.Duration,
                                MaxStacks = e.MaxStacks,
                                BuffName = e.BuffName,
                                Category = bCat
                            };
                        }
                        break;
                    case "MovementEffectLogic":
                        if (System.Enum.TryParse(e.MovementType, out CoreSystem.Skills.Logic.Effects.MovementType movType))
                        {
                            effect = new MovementEffectLogic { 
                                Type = movType,
                                Distance = e.MovementDistance,
                                ApplyOncePerCast = e.ApplyOnce,
                                UseLerp = e.UseLerp,
                                LerpDuration = e.LerpDuration
                            };
                        }
                        break;
                    case "SummonObjectEffect":
                        effect = new CoreSystem.Skills.Logic.Effects.SummonObjectEffect { 
                            Lifetime = e.Duration, 
                            ParentToCaster = e.ParentToCaster,
                            Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + e.PrefabPath + ".prefab")
                        };
                        break;
                }
                if (effect != null) asset.Effects.Add(effect);
            }
        }

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }

    public static void BatchConvert(List<ActiveSkillJsonData> skills)
    {
        if (skills == null || skills.Count == 0) return;
        
        string folder = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets/Resources/Skill/ActiveSkill", "");
        if (string.IsNullOrEmpty(folder)) return;
        
        if (folder.StartsWith(Application.dataPath))
        {
            folder = "Assets" + folder.Substring(Application.dataPath.Length);
        }

        int count = 0;
        foreach (var data in skills)
        {
            string path = $"{folder}/{data.SkillName.Replace(" ", "_")}.asset";
            CreateOrUpdateAsset(data, path);
            count++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Batch Match", $"Converted {count} skills to assets in {folder}", "OK");
    }

    private static void CreateOrUpdateAsset(ActiveSkillJsonData data, string path)
    {
        ActiveSkill asset = AssetDatabase.LoadAssetAtPath<ActiveSkill>(path);
        bool isNew = false;
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ActiveSkill>();
            isNew = true;
        }

        asset.Id = data.SkillID;
        asset.Name = data.SkillName;
        
        if (System.Enum.TryParse(data.SkillElement, out ElementType element))
            asset.SkillElement = element;

        if (System.Enum.TryParse(data.DamageType, out DamageType damageType))
            asset.SkillDamageType = damageType;

        asset.BaseDamage = data.BaseDamage;
        asset.BaseManaCost = data.BaseManaCost;
        asset.BaseVCT = data.BaseVCT;
        asset.BaseFCT = data.BaseFCT;
        asset.BaseCastDelay = data.BaseCastDelay;
        asset.BaseCooldown = data.BaseCooldown;
        asset.BaseProjectileCount = data.BaseProjectileCount;
        asset.IsToggle = data.IsToggle;
        asset.BaseAOERadius = data.BaseAOERadius;
        
        if (!string.IsNullOrEmpty(data.VisualPrefabName))
            asset.VisualPrefab = Resources.Load<GameObject>(data.VisualPrefabName);
        if (!string.IsNullOrEmpty(data.CastVisualPrefabName))
            asset.CastVisualPrefab = Resources.Load<GameObject>(data.CastVisualPrefabName);
            
        if (System.Enum.TryParse(data.CastVisualSpawnPoint, out VisualAppearance castSpawn))
            asset.CastSpawnPoint = castSpawn;
            
        if (data.BaseTargeting != null)
        {
            TargetingLogic logic = null;
            switch (data.BaseTargeting.Type)
            {
                case "Single": logic = new SingleTargeting(); break;
                case "Sphere": logic = new SphereAOETargeting(); break;
                case "Cone": logic = new ConeTargeting { ConeAngle = data.BaseTargeting.Angle }; break;
                case "Box": logic = new BoxTargeting { BoxSize = data.BaseTargeting.BoxSize, Offset = data.BaseTargeting.Offset }; break;
                case "Line": logic = new LinePierceTargeting(); break;
                case "Self": logic = new SelfTargeting(); break;
                case "TargetLocation": logic = new TargetLocationTargeting { CastRange = data.BaseTargeting.CastRange > 0 ? data.BaseTargeting.CastRange : 7f }; break;
            }
            asset.BaseTargeting = logic;

            if (logic != null)
            {
                if (System.Enum.TryParse(data.BaseTargeting.Alignment, out TargetAlignment alignment))
                    logic.AlignmentAction = alignment;

                logic.IncludeSelf = data.BaseTargeting.IncludeSelf;
            }
        }

        if (data.Scalings != null)
            asset.Scalings = new List<OutputScaling>(data.Scalings);

        if (data.Effects != null)
        {
            asset.Effects = new List<EffectLogic>();
            foreach (var e in data.Effects)
            {
                EffectLogic effect = null;
                switch (e.EffectType)
                {
                    case "DamageEffect": effect = new DamageEffect(); break;
                    case "HealEffect": effect = new HealEffect { TargetAttribute = (AttributeType)e.AttributeID, BaseHealAmount = e.Value }; break;
                    case "ApplyBuffEffect": 
                        if (System.Enum.TryParse(e.ModType, out CoreSystem.ModifierType mType))
                            effect = new ApplyBuffEffect { TargetAttribute = (AttributeType)e.AttributeID, ModifierValue = e.Value, ModType = mType }; 
                        break;
                    case "ApplyTemporaryBuffEffect":
                        if (System.Enum.TryParse(e.ModType, out CoreSystem.ModifierType tempMType) && System.Enum.TryParse(e.Category, out BuffCategory bCat))
                        {
                            effect = new ApplyTemporaryBuffEffect { 
                                TargetAttribute = (AttributeType)e.AttributeID, 
                                ModifierValue = e.Value, 
                                ModType = tempMType,
                                Duration = e.Duration,
                                MaxStacks = e.MaxStacks,
                                BuffName = e.BuffName,
                                Category = bCat
                            };
                        }
                        break;
                    case "MovementEffectLogic":
                        if (System.Enum.TryParse(e.MovementType, out CoreSystem.Skills.Logic.Effects.MovementType movType))
                        {
                            effect = new MovementEffectLogic { 
                                Type = movType,
                                Distance = e.MovementDistance,
                                ApplyOncePerCast = e.ApplyOnce,
                                UseLerp = e.UseLerp,
                                LerpDuration = e.LerpDuration
                            };
                        }
                        break;
                    case "SummonObjectEffect":
                        effect = new CoreSystem.Skills.Logic.Effects.SummonObjectEffect { 
                            Lifetime = e.Duration, 
                            ParentToCaster = e.ParentToCaster,
                            Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + e.PrefabPath + ".prefab")
                        };
                        break;
                }
                if (effect != null) asset.Effects.Add(effect);
            }
        }

        if (isNew)
            AssetDatabase.CreateAsset(asset, path);
        else
            EditorUtility.SetDirty(asset);
    }
}
