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
        
        string defaultFileName = $"{data.SkillName.Replace(" ", "_")}_{data.SkillID}";
        string path = EditorUtility.SaveFilePanelInProject("Save ActiveSkill Asset", defaultFileName, "asset", "Select location for the new skill asset", defaultPath);
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
        string iconPathToLoad = string.IsNullOrEmpty(data.IconPath) ? "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png" : data.IconPath;
        asset.Icon = GameDataWorkbench.ResolveEditorSprite(iconPathToLoad);
        
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
        
        if (System.Enum.TryParse(data.VisualType, out VisualCategory vType))
            asset.VisualType = vType;

        if (!string.IsNullOrEmpty(data.CastVfxPrefabName))
        {
            if (data.CastVfxPrefabName.StartsWith("Assets/")) asset.CastVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.CastVfxPrefabName);
            else asset.CastVfxPrefab = Resources.Load<GameObject>(data.CastVfxPrefabName);
        }
            
        if (System.Enum.TryParse(data.CastVfxSpawnPoint, out VisualAppearance castSpawn))
            asset.CastVfxSpawnPoint = castSpawn;

        if (!string.IsNullOrEmpty(data.SkillIndicatorPrefabName))
        {
            if (data.SkillIndicatorPrefabName.StartsWith("Assets/")) asset.SkillIndicatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.SkillIndicatorPrefabName);
            else asset.SkillIndicatorPrefab = Resources.Load<GameObject>(data.SkillIndicatorPrefabName);
        }

        if (!string.IsNullOrEmpty(data.EruptionVfxPrefabName))
        {
            if (data.EruptionVfxPrefabName.StartsWith("Assets/")) asset.EruptionVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.EruptionVfxPrefabName);
            else asset.EruptionVfxPrefab = Resources.Load<GameObject>(data.EruptionVfxPrefabName);
        }

        if (!string.IsNullOrEmpty(data.ProjectilePrefabName))
        {
            if (data.ProjectilePrefabName.StartsWith("Assets/")) asset.ProjectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.ProjectilePrefabName);
            else asset.ProjectilePrefab = Resources.Load<GameObject>(data.ProjectilePrefabName);
        }

        if (!string.IsNullOrEmpty(data.ImpactVfxPrefabName))
        {
            if (data.ImpactVfxPrefabName.StartsWith("Assets/")) asset.ImpactVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.ImpactVfxPrefabName);
            else asset.ImpactVfxPrefab = Resources.Load<GameObject>(data.ImpactVfxPrefabName);
        }

        if (!string.IsNullOrEmpty(data.AuraVfxPrefabName))
        {
            if (data.AuraVfxPrefabName.StartsWith("Assets/")) asset.AuraVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.AuraVfxPrefabName);
            else asset.AuraVfxPrefab = Resources.Load<GameObject>(data.AuraVfxPrefabName);
        }

        if (!string.IsNullOrEmpty(data.BeamVfxPrefabName))
        {
            if (data.BeamVfxPrefabName.StartsWith("Assets/")) asset.BeamVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.BeamVfxPrefabName);
            else asset.BeamVfxPrefab = Resources.Load<GameObject>(data.BeamVfxPrefabName);
        }
            
        if (data.BaseTargeting != null)
        {
            TargetingLogic logic = null;
            switch (data.BaseTargeting.Type)
            {
                case "Single": logic = new SingleTargeting(); break;
                case "SingleUnit": logic = new SingleUnitTargeting { CastRange = data.BaseTargeting.CastRange > 0 ? data.BaseTargeting.CastRange : 7f }; break;
                case "Sphere": logic = new SphereAOETargeting(); break;
                case "Cone": logic = new ConeTargeting { ConeAngle = data.BaseTargeting.Angle }; break;
                case "Box": logic = new BoxTargeting { BoxSize = data.BaseTargeting.BoxSize, Offset = data.BaseTargeting.Offset }; break;
                case "Line": logic = new LinePierceTargeting { LineWidth = data.BaseTargeting.LineWidth }; break;
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

        asset.Tags = new List<SkillTag>();
        if (data.Tags != null)
        {
            foreach (var tagStr in data.Tags)
            {
                if (System.Enum.TryParse(tagStr, out SkillTag tag))
                {
                    asset.Tags.Add(tag);
                }
            }
        }

        if (data.Effects != null && string.IsNullOrEmpty(data.CustomClassName))
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
                                BuffCode = e.BuffCode,
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
            
        if (data.ChainOverrides != null && data.ChainOverrides.Count > 0)
        {
            ApplyChainOverrides(asset, data);
            EditorUtility.SetDirty(asset);
        }

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
            
            string path = $"{subFolder}/{data.SkillName.Replace(" ", "_")}_{data.SkillID}.asset";
            CreateOrUpdateAsset(data, path);
            count++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Second pass: Link ChainOverrides
        int linkCount = 0;
        foreach (var data in skills)
        {
            if (data.ChainOverrides == null || data.ChainOverrides.Count == 0) continue;
            
            string subFolder = folder;
            if (!string.IsNullOrEmpty(data.CustomClassName)) subFolder = folder + "/Unique";
            string path = $"{subFolder}/{data.SkillName.Replace(" ", "_")}_{data.SkillID}.asset";
            
            ActiveSkill asset = AssetDatabase.LoadAssetAtPath<ActiveSkill>(path);
            if (asset != null)
            {
                ApplyChainOverrides(asset, data);
                EditorUtility.SetDirty(asset);
                linkCount++;
            }
        }

        if (linkCount > 0)
        {
            AssetDatabase.SaveAssets();
        }

        EditorUtility.DisplayDialog("Batch Match", $"Converted {count} skills to assets in {folder}\nLinked {linkCount} skill chains.", "OK");
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
        string iconPathToLoad = string.IsNullOrEmpty(data.IconPath) ? "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png" : data.IconPath;
        asset.Icon = GameDataWorkbench.ResolveEditorSprite(iconPathToLoad);
        
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
        
        if (System.Enum.TryParse(data.VisualType, out VisualCategory vType))
            asset.VisualType = vType;

        if (!string.IsNullOrEmpty(data.CastVfxPrefabName))
        {
            if (data.CastVfxPrefabName.StartsWith("Assets/")) asset.CastVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.CastVfxPrefabName);
            else asset.CastVfxPrefab = Resources.Load<GameObject>(data.CastVfxPrefabName);
        }
            
        if (System.Enum.TryParse(data.CastVfxSpawnPoint, out VisualAppearance castSpawn))
            asset.CastVfxSpawnPoint = castSpawn;

        if (!string.IsNullOrEmpty(data.SkillIndicatorPrefabName))
        {
            if (data.SkillIndicatorPrefabName.StartsWith("Assets/")) asset.SkillIndicatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.SkillIndicatorPrefabName);
            else asset.SkillIndicatorPrefab = Resources.Load<GameObject>(data.SkillIndicatorPrefabName);
        }

        if (!string.IsNullOrEmpty(data.EruptionVfxPrefabName))
        {
            if (data.EruptionVfxPrefabName.StartsWith("Assets/")) asset.EruptionVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.EruptionVfxPrefabName);
            else asset.EruptionVfxPrefab = Resources.Load<GameObject>(data.EruptionVfxPrefabName);
        }

        if (!string.IsNullOrEmpty(data.ProjectilePrefabName))
        {
            if (data.ProjectilePrefabName.StartsWith("Assets/")) asset.ProjectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.ProjectilePrefabName);
            else asset.ProjectilePrefab = Resources.Load<GameObject>(data.ProjectilePrefabName);
        }

        if (!string.IsNullOrEmpty(data.ImpactVfxPrefabName))
        {
            if (data.ImpactVfxPrefabName.StartsWith("Assets/")) asset.ImpactVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.ImpactVfxPrefabName);
            else asset.ImpactVfxPrefab = Resources.Load<GameObject>(data.ImpactVfxPrefabName);
        }

        if (!string.IsNullOrEmpty(data.AuraVfxPrefabName))
        {
            if (data.AuraVfxPrefabName.StartsWith("Assets/")) asset.AuraVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.AuraVfxPrefabName);
            else asset.AuraVfxPrefab = Resources.Load<GameObject>(data.AuraVfxPrefabName);
        }

        if (!string.IsNullOrEmpty(data.BeamVfxPrefabName))
        {
            if (data.BeamVfxPrefabName.StartsWith("Assets/")) asset.BeamVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.BeamVfxPrefabName);
            else asset.BeamVfxPrefab = Resources.Load<GameObject>(data.BeamVfxPrefabName);
        }
            
        if (data.BaseTargeting != null)
        {
            TargetingLogic logic = null;
            switch (data.BaseTargeting.Type)
            {
                case "Single": logic = new SingleTargeting(); break;
                case "SingleUnit": logic = new SingleUnitTargeting { CastRange = data.BaseTargeting.CastRange > 0 ? data.BaseTargeting.CastRange : 7f }; break;
                case "Sphere": logic = new SphereAOETargeting(); break;
                case "Cone": logic = new ConeTargeting { ConeAngle = data.BaseTargeting.Angle }; break;
                case "Box": logic = new BoxTargeting { BoxSize = data.BaseTargeting.BoxSize, Offset = data.BaseTargeting.Offset }; break;
                case "Line": logic = new LinePierceTargeting { LineWidth = data.BaseTargeting.LineWidth }; break;
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

        asset.Tags = new List<SkillTag>();
        if (data.Tags != null)
        {
            foreach (var tagStr in data.Tags)
            {
                if (System.Enum.TryParse(tagStr, out SkillTag tag))
                {
                    asset.Tags.Add(tag);
                }
            }
        }

        if (data.Effects != null && string.IsNullOrEmpty(data.CustomClassName))
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
                        if (!System.Enum.TryParse(e.ModType, out CoreSystem.ModifierType mType)) mType = CoreSystem.ModifierType.Flat;
                        effect = new ApplyBuffEffect { TargetAttribute = (AttributeType)e.AttributeID, ModifierValue = e.Value, ModType = mType }; 
                        break;
                    case "ApplyTemporaryBuffEffect":
                        if (!System.Enum.TryParse(e.ModType, out CoreSystem.ModifierType tempMType)) tempMType = CoreSystem.ModifierType.Flat;
                        if (!System.Enum.TryParse(e.Category, out BuffCategory bCat)) bCat = BuffCategory.Default;
                        
                        effect = new ApplyTemporaryBuffEffect { 
                            TargetAttribute = (AttributeType)e.AttributeID, 
                            ModifierValue = e.Value, 
                            ModType = tempMType,
                            Duration = e.Duration,
                            MaxStacks = e.MaxStacks,
                            BuffName = e.BuffName,
                            BuffCode = e.BuffCode,
                            Category = bCat,
                            BuffIconPath = e.BuffIconPath
                        };
                        break;
                    case "MovementEffectLogic":
                        if (!System.Enum.TryParse(e.MovementType, out CoreSystem.Skills.Logic.Effects.MovementType movType)) movType = CoreSystem.Skills.Logic.Effects.MovementType.DashForward;
                        
                        effect = new MovementEffectLogic { 
                                Type = movType,
                                Distance = e.MovementDistance,
                                ApplyOncePerCast = e.ApplyOnce,
                                UseLerp = e.UseLerp,
                                LerpDuration = e.LerpDuration,
                                StunDuration = e.Duration
                            };
                        break;
                    case "SummonObjectEffect":
                        effect = new CoreSystem.Skills.Logic.Effects.SummonObjectEffect { 
                            Lifetime = e.Duration, 
                            ParentToCaster = e.ParentToCaster,
                            Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + e.PrefabPath + ".prefab")
                        };
                        break;
                }
                if (effect != null)
                {
                    effect.ApplyToCaster = e.ApplyToCaster;
                    asset.Effects.Add(effect);
                }
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
    private static void ApplyChainOverrides(ActiveSkill asset, ActiveSkillJsonData data)
    {
        if (data.ChainOverrides == null) return;
        asset.ChainOverrides = new List<SkillChainOverride>();
        
        // Find all ActiveSkills once to optimize search
        string[] allGuids = AssetDatabase.FindAssets("t:ActiveSkill");
        var allSkills = new List<ActiveSkill>();
        foreach(string guid in allGuids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var s = AssetDatabase.LoadAssetAtPath<ActiveSkill>(p);
            if (s != null) allSkills.Add(s);
        }

        foreach(var overrideData in data.ChainOverrides)
        {
            var chain = new SkillChainOverride
            {
                RequiredBuffCode = overrideData.RequiredBuffCode,
                ConsumeBuffOnCast = overrideData.ConsumeBuffOnCast
            };
            Color col;
            if (ColorUtility.TryParseHtmlString(overrideData.HintColorHex, out col))
            {
                chain.HintColor = col;
            }

            // Find replacement skill by ID
            ActiveSkill replacement = allSkills.Find(s => s.Id == overrideData.ReplacementSkillID);
            if (replacement != null)
            {
                chain.ReplacementSkill = replacement;
            }
            else
            {
                Debug.LogWarning($"[SkillConverter] Could not find Replacement Skill with ID {overrideData.ReplacementSkillID} for {asset.Name}");
            }
            
            asset.ChainOverrides.Add(chain);
        }
    }
}
