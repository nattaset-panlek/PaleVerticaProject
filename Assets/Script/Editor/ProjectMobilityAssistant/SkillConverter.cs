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
        string defaultPath = "Assets/Resources/Skill/ActiveSkill";
        if (!string.IsNullOrEmpty(data.CustomClassName)) defaultPath += "/Unique";
        
        string path = EditorUtility.SaveFilePanelInProject("Save ActiveSkill Asset", data.SkillName, "asset", "Select location for the new skill asset", defaultPath);
        if (string.IsNullOrEmpty(path)) return;

        System.Type targetType = typeof(ActiveSkill);
        if (!string.IsNullOrEmpty(data.CustomClassName))
        {
            var t = System.Type.GetType(data.CustomClassName + ", Assembly-CSharp");
            if (t != null && typeof(ActiveSkill).IsAssignableFrom(t)) targetType = t;
        }

        ActiveSkill asset = AssetDatabase.LoadAssetAtPath<ActiveSkill>(path);
        bool isNew = false;
        
        if (asset != null && asset.GetType() != targetType)
        {
            AssetDatabase.DeleteAsset(path);
            asset = null;
        }

        if (asset == null)
        {
            asset = (ActiveSkill)ScriptableObject.CreateInstance(targetType);
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
        {
            if (data.VisualPrefabName.StartsWith("Assets/")) asset.VisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.VisualPrefabName);
            else asset.VisualPrefab = Resources.Load<GameObject>(data.VisualPrefabName);
        }
        if (!string.IsNullOrEmpty(data.CastVisualPrefabName))
        {
            if (data.CastVisualPrefabName.StartsWith("Assets/")) asset.CastVisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.CastVisualPrefabName);
            else asset.CastVisualPrefab = Resources.Load<GameObject>(data.CastVisualPrefabName);
        }
            
        if (System.Enum.TryParse(data.CastVisualSpawnPoint, out VisualAppearance castSpawn))
            asset.CastSpawnPoint = castSpawn;

        if (System.Enum.TryParse(data.VisualSpawnPoint, out VisualAppearance visualSpawn))
            asset.VisualSpawnPoint = visualSpawn;
            
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
                                Category = bCat,
                                BuffIconPath = e.BuffIconPath
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
                                LerpDuration = e.LerpDuration,
                                StunDuration = e.Duration
                            };
                        }
                        break;
                    case "SummonObjectEffect":
                        GameObject summonPrefab = null;
                        if (!string.IsNullOrEmpty(e.PrefabPath))
                        {
                            if (e.PrefabPath.StartsWith("Assets/")) summonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(e.PrefabPath);
                            else summonPrefab = Resources.Load<GameObject>(e.PrefabPath);
                        }
                        effect = new CoreSystem.Skills.Logic.Effects.SummonObjectEffect { 
                            Lifetime = e.Duration, 
                            ParentToCaster = e.ParentToCaster,
                            Prefab = summonPrefab
                        };
                        break;
                }
                if (effect != null) asset.Effects.Add(effect);
            }
        }

        ApplyUniqueFields(asset, data);

        if (isNew)
            AssetDatabase.CreateAsset(asset, path);
        else
            EditorUtility.SetDirty(asset);

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
            string subFolder = folder;
            if (!string.IsNullOrEmpty(data.CustomClassName))
            {
                subFolder = folder + "/Unique";
                if (!AssetDatabase.IsValidFolder(subFolder))
                {
                    AssetDatabase.CreateFolder(folder, "Unique");
                }
            }
            
            string path = $"{subFolder}/{data.SkillName.Replace(" ", "_")}.asset";
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

        System.Type targetType = typeof(ActiveSkill);
        if (!string.IsNullOrEmpty(data.CustomClassName))
        {
            var t = System.Type.GetType(data.CustomClassName + ", Assembly-CSharp");
            if (t != null && typeof(ActiveSkill).IsAssignableFrom(t)) targetType = t;
        }

        if (asset != null && asset.GetType() != targetType)
        {
            AssetDatabase.DeleteAsset(path);
            asset = null;
        }

        if (asset == null)
        {
            asset = (ActiveSkill)ScriptableObject.CreateInstance(targetType);
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
                                Category = bCat,
                                BuffIconPath = e.BuffIconPath
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
                                LerpDuration = e.LerpDuration,
                                StunDuration = e.Duration
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

        ApplyUniqueFields(asset, data);

        if (isNew)
            AssetDatabase.CreateAsset(asset, path);
        else
            EditorUtility.SetDirty(asset);
    }

    private static void ApplyUniqueFields(ActiveSkill asset, ActiveSkillJsonData data)
    {
        if (asset == null || data.UniqueFields == null || string.IsNullOrEmpty(data.CustomClassName)) return;
        
        System.Type t = asset.GetType();
        
        foreach (var fieldData in data.UniqueFields)
        {
            var fieldInfo = t.GetField(fieldData.FieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            if (fieldInfo != null)
            {
                try
                {
                    if (fieldInfo.FieldType == typeof(float))
                    {
                        if (float.TryParse(fieldData.StringValue, out float fVal)) fieldInfo.SetValue(asset, fVal);
                    }
                    else if (fieldInfo.FieldType == typeof(int))
                    {
                        if (int.TryParse(fieldData.StringValue, out int iVal)) fieldInfo.SetValue(asset, iVal);
                    }
                    else if (fieldInfo.FieldType == typeof(string))
                    {
                        fieldInfo.SetValue(asset, fieldData.StringValue);
                    }
                    else if (fieldInfo.FieldType == typeof(bool))
                    {
                        if (bool.TryParse(fieldData.StringValue, out bool bVal)) fieldInfo.SetValue(asset, bVal);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector2))
                    {
                        string[] parts = fieldData.StringValue.Split(',');
                        if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
                            fieldInfo.SetValue(asset, new Vector2(x, y));
                    }
                    else if (fieldInfo.FieldType == typeof(Vector3))
                    {
                        string[] parts = fieldData.StringValue.Split(',');
                        if (parts.Length == 3 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
                            fieldInfo.SetValue(asset, new Vector3(x, y, z));
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[SkillConverter] Failed to parse custom field {fieldData.FieldName} for skill {data.SkillName}: {ex.Message}");
                }
            }
        }
    }
}
