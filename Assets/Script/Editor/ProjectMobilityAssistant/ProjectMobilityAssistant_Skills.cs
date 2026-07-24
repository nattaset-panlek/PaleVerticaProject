using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CoreSystem;
using CoreSystem.Attributes;
using CoreSystem.Data;
using CoreSystem.Data.Skills;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;
using CoreSystem.Skills.Logic.Effects;
using CoreSystem.Skills.Modifiers;
using CoreSystem.Skills.Visuals;

public partial class ProjectMobilityAssistant : EditorWindow
{
    private static bool _showBasicInfo = true;
    private static bool _showUniqueSettings = true;
    private static bool _showStatsTiming = true;
    private static bool _showVisualsContext = true;
    private static bool _showVisualPrefabs = true;
    private static bool _showSkillTags = true;
    private static bool _showTargeting = true;
    private static bool _showScalings = true;
    private static bool _showEffects = true;
    private static bool _showSupportConditions = true;
    private static bool _showSupportEffects = true;

    private void DrawSkillEditor()
    {
        DrawEditor(_skillData.ActiveSkills, _skillSelectedIndex, (idx) => _skillSelectedIndex = idx, (entry) => {
            
            EditorGUILayout.BeginHorizontal();
            
            // --- Left Column: Fields ---
            EditorGUILayout.BeginVertical(SectionStyle, GUILayout.Width(350));
            
            _showBasicInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showBasicInfo, "Basic Info");
            if (_showBasicInfo)
            {
                entry.SkillID = EditorGUILayout.IntField("Skill ID", entry.SkillID);
                entry.SkillName = EditorGUILayout.TextField("Name", entry.SkillName);

                EditorGUILayout.Space(5);
                GUI.color = new Color(1f, 0.9f, 0.6f);
                MonoScript currentScript = null;
                if (!string.IsNullOrEmpty(entry.CustomClassName))
                {
                    var t = System.Type.GetType(entry.CustomClassName + ", Assembly-CSharp");
                    if (t != null)
                    {
                        var componentNames = AssetDatabase.FindAssets("t:MonoScript " + t.Name);
                        foreach (var guid in componentNames)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                            if (script != null && script.GetClass() == t)
                            {
                                currentScript = script;
                                break;
                            }
                        }
                    }
                }

                MonoScript newScript = (MonoScript)EditorGUILayout.ObjectField(new GUIContent("Unique Skill Script", "Custom C# script class inheriting from ActiveSkill to override execution logic."), currentScript, typeof(MonoScript), false);
                if (newScript != currentScript)
                {
                    if (newScript != null)
                    {
                        System.Type scriptClass = newScript.GetClass();
                        if (scriptClass != null && typeof(ActiveSkill).IsAssignableFrom(scriptClass))
                        {
                            entry.CustomClassName = scriptClass.FullName;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Invalid Script", "The script must inherit from ActiveSkill to be used here.", "OK");
                            entry.CustomClassName = "";
                        }
                    }
                    else
                    {
                        entry.CustomClassName = "";
                    }
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // --- Render Custom Fields via Reflection ---
            if (!string.IsNullOrEmpty(entry.CustomClassName))
            {
                System.Type t = System.Type.GetType(entry.CustomClassName + ", Assembly-CSharp");
                if (t != null)
                {
                    _showUniqueSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showUniqueSettings, "Unique Skill Settings");
                    if (_showUniqueSettings)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        
                        if (entry.UniqueFields == null) entry.UniqueFields = new List<UniqueSkillFieldData>();
                        
                        var fields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                        foreach (var field in fields)
                        {
                            var fData = entry.UniqueFields.FirstOrDefault(x => x.FieldName == field.Name);
                            if (fData == null)
                            {
                                fData = new UniqueSkillFieldData { FieldName = field.Name, FieldType = field.FieldType.FullName, StringValue = "" };
                                entry.UniqueFields.Add(fData);
                            }
                            
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(field.Name, GUILayout.Width(120));
                            
                            if (field.FieldType == typeof(float))
                            {
                                float.TryParse(fData.StringValue, out float fVal);
                                fVal = EditorGUILayout.FloatField(fVal);
                                fData.StringValue = fVal.ToString();
                            }
                            else if (field.FieldType == typeof(int))
                            {
                                int.TryParse(fData.StringValue, out int iVal);
                                iVal = EditorGUILayout.IntField(iVal);
                                fData.StringValue = iVal.ToString();
                            }
                            else if (field.FieldType == typeof(string))
                            {
                                fData.StringValue = EditorGUILayout.TextField(fData.StringValue);
                            }
                            else if (field.FieldType == typeof(bool))
                            {
                                bool.TryParse(fData.StringValue, out bool bVal);
                                bVal = EditorGUILayout.Toggle(bVal);
                                fData.StringValue = bVal.ToString();
                            }
                            else if (field.FieldType == typeof(Vector2))
                            {
                                string[] parts = fData.StringValue.Split(',');
                                Vector2 vVal = Vector2.zero;
                                if (parts.Length == 2)
                                {
                                    float.TryParse(parts[0], out vVal.x);
                                    float.TryParse(parts[1], out vVal.y);
                                }
                                vVal = EditorGUILayout.Vector2Field("", vVal);
                                fData.StringValue = $"{vVal.x},{vVal.y}";
                            }
                            else if (field.FieldType == typeof(Vector3))
                            {
                                string[] parts = fData.StringValue.Split(',');
                                Vector3 vVal = Vector3.zero;
                                if (parts.Length == 3)
                                {
                                    float.TryParse(parts[0], out vVal.x);
                                    float.TryParse(parts[1], out vVal.y);
                                    float.TryParse(parts[2], out vVal.z);
                                }
                                vVal = EditorGUILayout.Vector3Field("", vVal);
                                fData.StringValue = $"{vVal.x},{vVal.y},{vVal.z}";
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"Unsupported Type: {field.FieldType.Name}", EditorStyles.miniLabel);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.Space(5);
                }
            }
            
            if (!System.Enum.TryParse(entry.SkillElement, out ElementType element)) element = ElementType.Neutral;
            entry.SkillElement = ((ElementType)EditorGUILayout.EnumPopup("Element", element)).ToString();

            if (!System.Enum.TryParse(entry.DamageType, out DamageType damageType)) damageType = DamageType.Physical;
            entry.DamageType = ((DamageType)EditorGUILayout.EnumPopup("Damage Type", damageType)).ToString();

            EditorGUILayout.Space();
            _showStatsTiming = EditorGUILayout.BeginFoldoutHeaderGroup(_showStatsTiming, "Stats & Timing");
            if (_showStatsTiming)
            {
                entry.BaseDamage = EditorGUILayout.FloatField("Base Damage", entry.BaseDamage);
                entry.BaseManaCost = EditorGUILayout.FloatField("Mana Cost", entry.BaseManaCost);
                entry.IsToggle = EditorGUILayout.Toggle("Is Toggle", entry.IsToggle);
                entry.BaseProjectileCount = EditorGUILayout.IntField("Projectiles", entry.BaseProjectileCount);
                entry.BaseAOERadius = EditorGUILayout.FloatField("Base AOE Radius", entry.BaseAOERadius);
                
                entry.BaseVCT = EditorGUILayout.FloatField("Base VCT", entry.BaseVCT);
                entry.BaseFCT = EditorGUILayout.FloatField("Base FCT", entry.BaseFCT);
                entry.BaseCastDelay = EditorGUILayout.FloatField("Cast Delay", entry.BaseCastDelay);
                entry.BaseCooldown = EditorGUILayout.FloatField("Cooldown", entry.BaseCooldown);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            _showVisualsContext = EditorGUILayout.BeginFoldoutHeaderGroup(_showVisualsContext, "Visuals & Context Configuration");
            if (_showVisualsContext)
            {
                if (!System.Enum.TryParse(entry.VisualType, out VisualCategory vCategory)) vCategory = VisualCategory.InstantHit;
                entry.VisualType = ((VisualCategory)EditorGUILayout.EnumPopup("Visual Type", vCategory)).ToString();

                GameObject castVfxObj = null;
                if (!string.IsNullOrEmpty(entry.CastVfxPrefabName))
                {
                    if (entry.CastVfxPrefabName.StartsWith("Assets/")) castVfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.CastVfxPrefabName);
                    else castVfxObj = Resources.Load<GameObject>(entry.CastVfxPrefabName);
                }
                var newCastVfx = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Cast VFX Prefab", "Played during cast phase of the skill (VCT/FCT)."), castVfxObj, typeof(GameObject), false);
                if (newCastVfx != castVfxObj) entry.CastVfxPrefabName = newCastVfx == null ? "" : AssetDatabase.GetAssetPath(newCastVfx);

                if (!System.Enum.TryParse(entry.CastVfxSpawnPoint, out VisualAppearance castSpawn)) castSpawn = VisualAppearance.FollowCaster;
                entry.CastVfxSpawnPoint = ((VisualAppearance)EditorGUILayout.EnumPopup("Cast VFX Spawn Point", castSpawn)).ToString();

                GameObject indicatorObj = null;
                if (!string.IsNullOrEmpty(entry.SkillIndicatorPrefabName))
                {
                    if (entry.SkillIndicatorPrefabName.StartsWith("Assets/")) indicatorObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.SkillIndicatorPrefabName);
                    else indicatorObj = Resources.Load<GameObject>(entry.SkillIndicatorPrefabName);
                }
                var newIndicator = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Custom Ground Indicator", "Custom ground warning or targeting indicator prefab spawned at the target location during the casting phase."), indicatorObj, typeof(GameObject), false);
                if (newIndicator != indicatorObj) entry.SkillIndicatorPrefabName = newIndicator == null ? "" : AssetDatabase.GetAssetPath(newIndicator);

                EditorGUILayout.Space(5);
                _showVisualPrefabs = EditorGUILayout.BeginFoldoutHeaderGroup(_showVisualPrefabs, "Visual Type Prefabs");
                if (_showVisualPrefabs)
                {
                    GameObject eruptionVfxObj = null;
                    if (!string.IsNullOrEmpty(entry.EruptionVfxPrefabName))
                    {
                        if (entry.EruptionVfxPrefabName.StartsWith("Assets/")) eruptionVfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.EruptionVfxPrefabName);
                        else eruptionVfxObj = Resources.Load<GameObject>(entry.EruptionVfxPrefabName);
                    }
                    var newEruption = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Eruption VFX Prefab", "Visual effect prefab spawned at the target location when using the GroundEruption visual category."), eruptionVfxObj, typeof(GameObject), false);
                    if (newEruption != eruptionVfxObj) entry.EruptionVfxPrefabName = newEruption == null ? "" : AssetDatabase.GetAssetPath(newEruption);

                    GameObject projVfxObj = null;
                    if (!string.IsNullOrEmpty(entry.ProjectilePrefabName))
                    {
                        if (entry.ProjectilePrefabName.StartsWith("Assets/")) projVfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.ProjectilePrefabName);
                        else projVfxObj = Resources.Load<GameObject>(entry.ProjectilePrefabName);
                    }
                    var newProj = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Projectile Prefab", "Visual prefab spawned for the Projectile visual category. Sprites/particles must point Up (Y+) in local space."), projVfxObj, typeof(GameObject), false);
                    if (newProj != projVfxObj) entry.ProjectilePrefabName = newProj == null ? "" : AssetDatabase.GetAssetPath(newProj);

                    GameObject impactVfxObj = null;
                    if (!string.IsNullOrEmpty(entry.ImpactVfxPrefabName))
                    {
                        if (entry.ImpactVfxPrefabName.StartsWith("Assets/")) impactVfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.ImpactVfxPrefabName);
                        else impactVfxObj = Resources.Load<GameObject>(entry.ImpactVfxPrefabName);
                    }
                    var newImpact = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Impact VFX Prefab", "Visual effect prefab spawned on top of each target when using the InstantHit visual category."), impactVfxObj, typeof(GameObject), false);
                    if (newImpact != impactVfxObj) entry.ImpactVfxPrefabName = newImpact == null ? "" : AssetDatabase.GetAssetPath(newImpact);

                    GameObject auraVfxObj = null;
                    if (!string.IsNullOrEmpty(entry.AuraVfxPrefabName))
                    {
                        if (entry.AuraVfxPrefabName.StartsWith("Assets/")) auraVfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.AuraVfxPrefabName);
                        else auraVfxObj = Resources.Load<GameObject>(entry.AuraVfxPrefabName);
                    }
                    var newAura = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Aura VFX Prefab", "Visual effect prefab spawned and parented to each target when using the AuraBuff visual category."), auraVfxObj, typeof(GameObject), false);
                    if (newAura != auraVfxObj) entry.AuraVfxPrefabName = newAura == null ? "" : AssetDatabase.GetAssetPath(newAura);

                    GameObject beamVfxObj = null;
                    if (!string.IsNullOrEmpty(entry.BeamVfxPrefabName))
                    {
                        if (entry.BeamVfxPrefabName.StartsWith("Assets/")) beamVfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(entry.BeamVfxPrefabName);
                        else beamVfxObj = Resources.Load<GameObject>(entry.BeamVfxPrefabName);
                    }
                    var newBeam = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Beam VFX Prefab", "Visual effect prefab utilizing a LineRenderer component, spawned for the RayBeam visual category."), beamVfxObj, typeof(GameObject), false);
                    if (newBeam != beamVfxObj) entry.BeamVfxPrefabName = newBeam == null ? "" : AssetDatabase.GetAssetPath(newBeam);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            _showSkillTags = EditorGUILayout.BeginFoldoutHeaderGroup(_showSkillTags, "Skill Tags");
            if (_showSkillTags)
            {
                if (entry.Tags == null) entry.Tags = new List<string>();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                if (entry.Tags.Count == 0)
                {
                    EditorGUILayout.LabelField("No tags assigned.", EditorStyles.miniLabel);
                }
                else
                {
                    float currentWidth = 0;
                    float maxWidth = 330;
                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < entry.Tags.Count; i++)
                    {
                        string tagStr = entry.Tags[i];
                        GUIContent tagContent = new GUIContent($"{tagStr} x");
                        Vector2 size = EditorStyles.miniButton.CalcSize(tagContent);
                        if (currentWidth + size.x > maxWidth)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            currentWidth = 0;
                        }
                        if (GUILayout.Button(tagContent, EditorStyles.miniButton, GUILayout.Width(size.x)))
                        {
                            entry.Tags.RemoveAt(i);
                            i--;
                            continue;
                        }
                        currentWidth += size.x + 4;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Tag", EditorStyles.miniButton))
                {
                    GenericMenu menu = new GenericMenu();
                    var allTags = System.Enum.GetNames(typeof(SkillTag));
                    foreach (var tag in allTags)
                    {
                        if (!entry.Tags.Contains(tag))
                        {
                            menu.AddItem(new GUIContent(tag), false, () => {
                                entry.Tags.Add(tag);
                            });
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(tag));
                        }
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            _showTargeting = EditorGUILayout.BeginFoldoutHeaderGroup(_showTargeting, "Targeting");
            if (_showTargeting)
            {
                if (entry.BaseTargeting == null) entry.BaseTargeting = new BaseTargetingData { Type = "Single", Radius = 2 };
                
                string[] targetingTypes = { "Single", "Sphere", "Cone", "Box", "Self", "Line", "TargetLocation" };
                int typeIdx = Mathf.Max(0, System.Array.IndexOf(targetingTypes, entry.BaseTargeting.Type));
                entry.BaseTargeting.Type = targetingTypes[EditorGUILayout.Popup("Target Type", typeIdx, targetingTypes)];
                
                entry.BaseTargeting.Radius = EditorGUILayout.FloatField("Radius/Range", entry.BaseTargeting.Radius);

                if (entry.BaseTargeting.Type == "Cone")
                {
                    entry.BaseTargeting.Angle = EditorGUILayout.Slider("Cone Angle", entry.BaseTargeting.Angle, 0, 360);
                }
                else if (entry.BaseTargeting.Type == "Box")
                {
                    entry.BaseTargeting.BoxSize = EditorGUILayout.Vector2Field("Box Size", entry.BaseTargeting.BoxSize);
                    entry.BaseTargeting.Offset = EditorGUILayout.Vector2Field("Offset", entry.BaseTargeting.Offset);
                }
                else if (entry.BaseTargeting.Type == "Line")
                {
                    entry.BaseTargeting.Radius = EditorGUILayout.FloatField("Line Length", entry.BaseTargeting.Radius);
                }
                else if (entry.BaseTargeting.Type == "TargetLocation")
                {
                    entry.BaseTargeting.CastRange = EditorGUILayout.FloatField("Cast Range (Fixed)", entry.BaseTargeting.CastRange);
                    entry.BaseTargeting.Radius = entry.BaseAOERadius;
                    EditorGUILayout.LabelField("AOE Radius (Linked to Stats)", entry.BaseAOERadius.ToString());
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            _showScalings = EditorGUILayout.BeginFoldoutHeaderGroup(_showScalings, "Scalings");
            if (_showScalings)
            {
                EditorGUILayout.HelpBox("Formula: (Stat x AttrMult + FlatBonus + LevelMult x Level) x GlobalMult", MessageType.Info);
                if (entry.Scalings == null) entry.Scalings = new List<OutputScaling>();
                for (int i = 0; i < entry.Scalings.Count; i++)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    var scaling = entry.Scalings[i];
                    EditorGUILayout.BeginHorizontal();
                    scaling.TargetField = (PayloadTarget)EditorGUILayout.EnumPopup("Target", scaling.TargetField);
                    if (GUILayout.Button("X", GUILayout.Width(20))) { entry.Scalings.RemoveAt(i); break; }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Scaling Attr");
                    string currentAttr = scaling.ScalingAttribute.ToString();
                    Rect rect = GUILayoutUtility.GetRect(new GUIContent(currentAttr), EditorStyles.popup);
                    if (GUI.Button(rect, new GUIContent(currentAttr), EditorStyles.popup))
                    {
                        var dropdown = new GenericAttributeDropdown(_attributeData.Attributes, (selected) => {
                            scaling.ScalingAttribute = (AttributeType)selected.Id;
                        });
                        dropdown.Show(rect);
                    }
                    EditorGUILayout.EndHorizontal();
                    scaling.AttributeMultiplier = EditorGUILayout.FloatField("Attr Mult", scaling.AttributeMultiplier);
                    scaling.FlatBonus = EditorGUILayout.FloatField("Flat Bonus", scaling.FlatBonus);
                    scaling.LevelMultiplier = EditorGUILayout.FloatField("Level Mult", scaling.LevelMultiplier);
                    scaling.GlobalMultiplier = EditorGUILayout.FloatField("Global Mult", scaling.GlobalMultiplier);
                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("Add Scaling")) entry.Scalings.Add(new OutputScaling());
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            DrawEffectSection(entry.Effects ?? (entry.Effects = new List<EffectEntry>()), true);

            EditorGUILayout.Space(20);
            GUI.color = Color.cyan;
            if (GUILayout.Button("Convert to ScriptableObject", GUILayout.Height(30)))
            {
                SkillConverter.ShowConverter(entry);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();

            // --- Right Column: Preview ---
            EditorGUILayout.BeginVertical();
            DrawSkillPreview(entry);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }, (entry) => $"[{entry.SkillID}] {entry.SkillName}", 
        () => {
            var s = new ActiveSkillJsonData { 
                SkillElement = ElementType.Neutral.ToString(), 
                DamageType = DamageType.Physical.ToString(),
                VisualType = VisualCategory.InstantHit.ToString(),
                CastVfxSpawnPoint = VisualAppearance.FollowCaster.ToString()
            };
            s.SkillID = _skillData.ActiveSkills.Count > 0 ? _skillData.ActiveSkills.Max(x => x.SkillID) + 1 : 101;
            s.BaseTargeting = new BaseTargetingData { Type = "Single", Radius = 2 };
            return s;
        },
        (rect, entry) => {
            if (System.Enum.TryParse(entry.SkillElement, out ElementType element)) {
                 Rect colorRect = new Rect(rect.x + 2, rect.y + 2, 4, rect.height - 4);
                 EditorGUI.DrawRect(colorRect, GetElementColor(element));
            }
        },
        (entry) => {
            if (_skillElementFilterIndex == 0) return true;
            return entry.SkillElement == _skillElementFilterNames[_skillElementFilterIndex];
        },
        () => {
            _skillElementFilterIndex = EditorGUILayout.Popup("Element Filter", _skillElementFilterIndex, _skillElementFilterNames);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Convert ALL to ScriptableAssets", GUILayout.Height(28)))
            {
                SkillConverter.BatchConvert(_skillData.ActiveSkills);
            }
            EditorGUILayout.Space(5);
        });
    }

    private void DrawSupportSkillEditor()
    {
        DrawEditor(_supportSkillData.SupportModifiers, _supportSelectedIndex, (idx) => _supportSelectedIndex = idx, (entry) => {
            entry.SkillID = EditorGUILayout.IntField("Skill ID", entry.SkillID);
            entry.ModifierName = EditorGUILayout.TextField("Modifier Name", entry.ModifierName);
            entry.Description = EditorGUILayout.TextField("Description", entry.Description);
        }, (entry) => $"[{entry.SkillID}] {entry.ModifierName}", () => new SupportSkillJsonData());
    }

    private void DrawSkillEffectFields(EffectEntry effect)
    {
        if (effect.EffectType == "ApplyBuffEffect" || effect.EffectType == "ApplyTemporaryBuffEffect" || effect.EffectType == "HealEffect")
        {
            if (_useManualAttributeInput)
                effect.AttributeID = EditorGUILayout.IntField("Attr ID", effect.AttributeID);
            else
                DrawAttributeSelector(effect.AttributeID, (id) => effect.AttributeID = id);

            effect.Value = EditorGUILayout.FloatField("Amount/Mult", effect.Value);

            if (effect.EffectType == "ApplyBuffEffect" || effect.EffectType == "ApplyTemporaryBuffEffect")
            {
                if (!System.Enum.TryParse(effect.ModType, out CoreSystem.ModifierType mType)) mType = CoreSystem.ModifierType.Flat;
                effect.ModType = ((CoreSystem.ModifierType)EditorGUILayout.EnumPopup("Mod Type", mType)).ToString();
                
                if (effect.EffectType == "ApplyTemporaryBuffEffect")
                {
                    effect.BuffName = EditorGUILayout.TextField("Buff Name", effect.BuffName);
                    effect.Duration = EditorGUILayout.FloatField("Duration", effect.Duration);
                    effect.MaxStacks = EditorGUILayout.IntField("Max Stacks", effect.MaxStacks);
                    if (!System.Enum.TryParse(effect.Category, out BuffCategory bCat)) bCat = BuffCategory.Default;
                    effect.Category = ((BuffCategory)EditorGUILayout.EnumPopup("Category", bCat)).ToString();

                    // --- Buff Icon (Addressable) ---
                    EditorGUILayout.BeginHorizontal();
                    Sprite currentBuffIcon = ResolveEditorSprite(effect.BuffIconPath);
                    Sprite newBuffIcon = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Buff Icon (Addressable)", "Icon sprite used to represent the active temporary buff on the UI."), currentBuffIcon, typeof(Sprite), false);
                    if (newBuffIcon != currentBuffIcon)
                    {
                        effect.BuffIconPath = GetAddressableKey(newBuffIcon);
                    }
                    EditorGUILayout.EndHorizontal();
                    if (!string.IsNullOrEmpty(effect.BuffIconPath))
                        EditorGUILayout.LabelField("Icon Key:", effect.BuffIconPath, EditorStyles.miniLabel);
                }
            }
        }
        else if (effect.EffectType == "DamageEffect")
        {
            effect.Value = EditorGUILayout.FloatField("Damage %", effect.Value);
        }
        else if (effect.EffectType == "MovementEffectLogic")
        {
            if (!System.Enum.TryParse(effect.MovementType, out CoreSystem.Skills.Logic.Effects.MovementType mType)) mType = CoreSystem.Skills.Logic.Effects.MovementType.DashForward;
            effect.MovementType = ((CoreSystem.Skills.Logic.Effects.MovementType)EditorGUILayout.EnumPopup("Move Type", mType)).ToString();
            
            if (mType == CoreSystem.Skills.Logic.Effects.MovementType.StunTarget)
            {
                effect.Duration = EditorGUILayout.FloatField("Stun Duration", effect.Duration);
            }
            else
            {
                effect.MovementDistance = EditorGUILayout.FloatField("Distance", effect.MovementDistance);
                effect.ApplyOnce = EditorGUILayout.Toggle("Apply Once per Cast", effect.ApplyOnce);
                effect.UseLerp = EditorGUILayout.Toggle("Smooth (Lerp)", effect.UseLerp);
                if (effect.UseLerp) effect.LerpDuration = EditorGUILayout.FloatField("Duration", effect.LerpDuration);
            }
        }
        else if (effect.EffectType == "SummonObjectEffect")
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + effect.PrefabPath + ".prefab");
            var newGo = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab (in Resources)", "GameObject prefab to spawn (e.g. projectiles, traps, obstacles) which must reside under a Resources/ directory."), go, typeof(GameObject), false);
            if (newGo != go)
            {
                if (newGo != null)
                {
                    string path = AssetDatabase.GetAssetPath(newGo);
                    if (path.Contains("Resources/"))
                    {
                        var parts = path.Split(new string[] { "Resources/" }, System.StringSplitOptions.None);
                        effect.PrefabPath = parts[parts.Length - 1].Replace(".prefab", "");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Prefab must be inside Resources folder", "OK");
                    }
                }
                else effect.PrefabPath = "";
            }
            effect.Duration = EditorGUILayout.FloatField("Lifetime (0=inf)", effect.Duration);
            effect.ParentToCaster = EditorGUILayout.Toggle("Parent to Caster", effect.ParentToCaster);
        }
    }

    private void DrawSkillPreview(ActiveSkillJsonData skill)
    {
        EditorGUILayout.BeginVertical(SectionStyle);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Skill Graphic Preview", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        _previewZoom = EditorGUILayout.Slider("Zoom", _previewZoom, 5, 200, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        Rect previewRect = GUILayoutUtility.GetRect(0, 450, GUILayout.ExpandWidth(true));
        GUI.Box(previewRect, "", EditorStyles.helpBox);
        
        // Background Grid & Graphic
        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove)
        {
            GUI.BeginGroup(previewRect);
            
            // Mouse is local within the group
            Vector2 mousePos = Event.current.mousePosition;
            if (mousePos.x >= 0 && mousePos.x <= previewRect.width && mousePos.y >= 0 && mousePos.y <= previewRect.height)
            {
                _previewTargetPoint = mousePos;
            }

            // Local rect for the group
            Rect localRect = new Rect(0, 0, previewRect.width, previewRect.height);
            DrawGrid(localRect, _previewZoom);
            DrawTargetingGraphics(localRect, skill, _previewZoom);
            GUI.EndGroup();

            Repaint();
        }

        // Interaction for offset
        if (Event.current.type == EventType.MouseDrag && previewRect.Contains(Event.current.mousePosition))
        {
            _previewOffset += Event.current.delta;
            Repaint();
        }
        
        if (previewRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
        {
            _previewZoom = Mathf.Clamp(_previewZoom - Event.current.delta.y, 5, 200);
            Event.current.Use();
        }

        EditorGUILayout.HelpBox("Drag in preview to move view. Scroll to zoom. Caster is at Center.", MessageType.None);
        
        EditorGUILayout.Space(10);
        DrawSkillSimulator(skill);

        EditorGUILayout.EndVertical();
    }

    private void DrawSkillSimulator(ActiveSkillJsonData skill)
    {
        _showSimulator = EditorGUILayout.BeginFoldoutHeaderGroup(_showSimulator, "Skill Calculator / Simulator");
        if (_showSimulator)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Simulation Stats", EditorStyles.boldLabel);
            
            _simSkillLevel = EditorGUILayout.IntSlider("Skill Level", _simSkillLevel, 1, 10);
            
            // Gather all attributes used in scalings
            HashSet<int> usedAttrs = new HashSet<int>();
            if (skill.Scalings != null)
            {
                foreach (var s in skill.Scalings)
                    if (s.ScalingAttribute > 0) usedAttrs.Add((int)s.ScalingAttribute);
            }

            foreach (int attrId in usedAttrs)
            {
                var attr = _attributeData.Attributes.FirstOrDefault(a => a.Id == attrId);
                string label = attr != null ? $"{attr.FullName} ({attrId})" : $"Attr {attrId}";
                
                if (!_mockStats.ContainsKey(attrId)) _mockStats[attrId] = 0;
                _mockStats[attrId] = EditorGUILayout.IntField(label, _mockStats[attrId]);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Calculated Results", EditorStyles.boldLabel);
            GUI.enabled = false;

            // Calculate simulated values
            float finalDamage = CalculateSimulatedValue(skill, PayloadTarget.BaseDamage, skill.BaseDamage, _simSkillLevel);
            float finalAOE = CalculateSimulatedValue(skill, PayloadTarget.AOERadius, skill.BaseAOERadius, _simSkillLevel);
            float finalVCT = CalculateSimulatedValue(skill, PayloadTarget.VCT, skill.BaseVCT, _simSkillLevel);

            EditorGUILayout.FloatField("Final Base Damage", finalDamage);
            EditorGUILayout.FloatField("Final AOE Radius", finalAOE);
            EditorGUILayout.FloatField("Final Cast Time", finalVCT);
            
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private float CalculateSimulatedValue(ActiveSkillJsonData skill, PayloadTarget target, float baseVal, int level)
    {
        float totalBonus = 0;
        if (skill.Scalings != null)
        {
            foreach (var s in skill.Scalings)
            {
                if (s.TargetField == target)
                {
                    int stat = _mockStats.ContainsKey((int)s.ScalingAttribute) ? _mockStats[(int)s.ScalingAttribute] : 0;
                    float bonus = (stat * s.AttributeMultiplier + s.FlatBonus + s.LevelMultiplier * level) * s.GlobalMultiplier;
                    totalBonus += bonus;
                }
            }
        }
        return baseVal + totalBonus;
    }

    private void DrawGrid(Rect rect, float zoom)
    {
        Vector2 center = rect.center + _previewOffset;
        Handles.BeginGUI();
        
        // Major Grid (1m)
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
        float startX = (center.x % zoom);
        for (float x = startX; x < rect.width; x += zoom)
            Handles.DrawLine(new Vector2(rect.x + x, rect.y), new Vector2(rect.x + x, rect.yMax));
        
        float startY = (center.y % zoom);
        for (float y = startY; y < rect.height; y += zoom)
            Handles.DrawLine(new Vector2(rect.x, rect.y + y), new Vector2(rect.xMax, rect.y + y));

        // Axes
        Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        if (center.x >= rect.x && center.x <= rect.xMax)
            Handles.DrawLine(new Vector2(center.x, rect.y), new Vector2(center.x, rect.yMax)); // Y Axis
        if (center.y >= rect.y && center.y <= rect.yMax)
            Handles.DrawLine(new Vector2(rect.x, center.y), new Vector2(rect.xMax, center.y)); // X Axis
        
        // Measurement markers
        Handles.color = Color.gray;
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.7f, 0.7f, 0.7f) } };
        for (int i = 1; i <= 20; i++)
        {
            float dist = i * zoom;
            if (center.x + dist < rect.xMax)
            {
                Handles.DrawLine(new Vector2(center.x + dist, center.y - 5), new Vector2(center.x + dist, center.y + 5));
                GUI.Label(new Rect(center.x + dist + 2, center.y + 2, 30, 20), $"{i}m", labelStyle);
            }
            if (center.y - dist > rect.y)
            {
                Handles.DrawLine(new Vector2(center.x - 5, center.y - dist), new Vector2(center.x + 5, center.y - dist));
                GUI.Label(new Rect(center.x + 8, center.y - dist - 15, 30, 20), $"{i}m", labelStyle);
            }
        }
        Handles.EndGUI();
    }

    private void DrawTargetingGraphics(Rect rect, ActiveSkillJsonData skill, float zoom)
    {
        if (skill == null || skill.BaseTargeting == null) return;
        
        Vector2 center = rect.center + _previewOffset;
        
        // Use simulated AOE radius if available, otherwise fallback to static targeting radius
        float simulatedAOE = CalculateSimulatedValue(skill, PayloadTarget.AOERadius, skill.BaseAOERadius, _simSkillLevel);
        float radius = (simulatedAOE > 0 ? simulatedAOE : skill.BaseTargeting.Radius) * zoom;

        Handles.BeginGUI();
        
        // Draw Origin (Caster)
        Handles.color = Color.white;
        Handles.DrawSolidDisc(center, Vector3.forward, 4);
        Handles.color = Color.black;
        Handles.DrawWireDisc(center, Vector3.forward, 4);

        // Draw Facing Direction Indicator
        Vector2 forward = new Vector2(0, -1); // Up the screen
        Vector2 arrowTip = center + forward * 20f;
        Handles.color = Color.yellow;
        Handles.DrawLine(center, arrowTip);
        Handles.DrawLine(arrowTip, (Vector3)arrowTip + new Vector3(-5, 5, 0));
        Handles.DrawLine(arrowTip, (Vector3)arrowTip + new Vector3(5, 5, 0));
        Handles.Label(arrowTip + new Vector2(5, -15), "Facing", EditorStyles.miniBoldLabel);

        // Draw Skill Hitbox based on type
        switch (skill.BaseTargeting.Type)
        {
            case "Single":
            case "Sphere":
                Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.25f);
                Handles.DrawSolidDisc(center, Vector3.forward, radius);
                Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                Handles.DrawWireDisc(center, Vector3.forward, radius);
                break;
            case "Cone":
                float angle = skill.BaseTargeting.Angle; 
                Handles.color = new Color(1f, 0.6f, 0.1f, 0.25f);
                // Vector3.down (0, -1, 0) is UP the screen in GUI
                Handles.DrawSolidArc(center, Vector3.forward, Vector3.down, angle/2, radius);
                Handles.DrawSolidArc(center, Vector3.forward, Vector3.down, -angle/2, radius);
                Handles.color = new Color(1f, 0.6f, 0.1f, 0.8f);
                Handles.DrawWireArc(center, Vector3.forward, Vector3.down, angle/2, radius);
                Handles.DrawWireArc(center, Vector3.forward, Vector3.down, -angle/2, radius);
                // Lines to connect
                Vector3 leftLine = Quaternion.Euler(0, 0, angle/2) * Vector3.down * radius;
                Vector3 rightLine = Quaternion.Euler(0, 0, -angle/2) * Vector3.down * radius;
                Handles.DrawLine(center, (Vector3)center + leftLine);
                Handles.DrawLine(center, (Vector3)center + rightLine);
                break;
            case "Box":
                Vector2 boxSize = skill.BaseTargeting.BoxSize * zoom;
                Vector2 boxOffset = skill.BaseTargeting.Offset * zoom;
                // use forward (0, -1) to point UP the screen
                Vector2 boxCenter = center + new Vector2(boxOffset.x, -boxOffset.y); 
                
                Vector3[] verts = new Vector3[] {
                    new Vector3(boxCenter.x - boxSize.x/2, boxCenter.y - boxSize.y/2, 0),
                    new Vector3(boxCenter.x + boxSize.x/2, boxCenter.y - boxSize.y/2, 0),
                    new Vector3(boxCenter.x + boxSize.x/2, boxCenter.y + boxSize.y/2, 0),
                    new Vector3(boxCenter.x - boxSize.x/2, boxCenter.y + boxSize.y/2, 0)
                };
                Handles.color = new Color(0.1f, 0.6f, 1f, 0.25f);
                Handles.DrawSolidRectangleWithOutline(verts, new Color(0.1f, 0.6f, 1f, 0.25f), new Color(0.1f, 0.6f, 1f, 0.8f));
                break;
            case "Line":
                Handles.color = new Color(1f, 0.1f, 0.1f, 0.8f);
                // use (0, -1) for forward line
                Handles.DrawLine(center, center + new Vector2(0, -1) * radius);
                Handles.DrawWireDisc(center + new Vector2(0, -1) * radius, Vector3.forward, 5);
                break;
            case "TargetLocation":
                float castRange = skill.BaseTargeting.CastRange * zoom;
                float aoeRadius = skill.BaseAOERadius * zoom; 
                
                // 1. Draw Cast Range circle (White/Gray)
                Handles.color = new Color(1f, 1f, 1f, 0.1f);
                Handles.DrawSolidDisc(center, Vector3.forward, castRange);
                Handles.color = new Color(1f, 1f, 1f, 0.4f);
                Handles.DrawWireDisc(center, Vector3.forward, castRange);
                
                // 2. Draw AOE circle (Target Point) - follows mouse and clamped to cast range
                Vector2 targetPoint = _previewTargetPoint;
                if (targetPoint == Vector2.zero) 
                    targetPoint = center + new Vector2(0, -1) * (castRange * 0.7f);

                Vector2 offset = targetPoint - center;
                if (offset.magnitude > castRange)
                {
                    targetPoint = center + offset.normalized * castRange;
                }

                Handles.color = new Color(0f, 0.7f, 1f, 0.25f);
                Handles.DrawSolidDisc(targetPoint, Vector3.forward, aoeRadius);
                Handles.color = new Color(0f, 0.7f, 1f, 0.8f);
                Handles.DrawWireDisc(targetPoint, Vector3.forward, aoeRadius);
                
                Handles.color = Color.white;
                Handles.Label(targetPoint + new Vector2(5, 5), "Target Point", EditorStyles.miniLabel);
                break;
        }
        
        Handles.color = Color.white;
        Handles.Label(center + new Vector2(10, 10), "Caster (0,0)", EditorStyles.miniBoldLabel);
        
        Handles.EndGUI();
    }

    private Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return Color.red;
            case ElementType.Water: return Color.blue;
            case ElementType.Earth: return new Color(0.4f, 0.2f, 0);
            case ElementType.Electric: return Color.yellow;
            case ElementType.Holy: return Color.white;
            case ElementType.Ghost: return new Color(0.5f, 0, 0.5f);
            case ElementType.Neutral: return Color.gray;
            default: return Color.white;
        }
    }

    private string GetResourcesPath(GameObject go)
    {
        if (go == null) return "";
        string assetPath = AssetDatabase.GetAssetPath(go);
        int resIndex = assetPath.IndexOf("Resources/");
        if (resIndex < 0)
        {
            EditorUtility.DisplayDialog("Resources Required", "Selected object MUST be inside a 'Resources' folder.", "OK");
            return "";
        }
        string relative = assetPath.Substring(resIndex + 10); // Skip "Resources/"
        return relative.Replace(".prefab", "");
    }
}
