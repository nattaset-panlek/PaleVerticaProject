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

public partial class GameDataWorkbench : EditorWindow
{
    #region [Variables/Fields]
    private bool _skillPreviewHover;
    #endregion

    #region [Private Methods]
    private void DrawSkillEditor()
    {
        DrawEditor(_skillData.ActiveSkills, _skillSelectedIndex, (idx) => _skillSelectedIndex = idx, (entry) => {
            if (entry.BaseTargeting == null) entry.BaseTargeting = new BaseTargetingData { Type = "Single", Radius = 2 };

            BeginInspectorColumns();
            if (BeginGroup("Basic Info", ThemeAccent))
            {
                entry.SkillID = EditorGUILayout.IntField("Skill ID", entry.SkillID);
                entry.SkillName = EditorGUILayout.TextField("Name", entry.SkillName);

                if (!System.Enum.TryParse(entry.SkillElement, out ElementType element)) element = ElementType.Neutral;
                entry.SkillElement = ((ElementType)EditorGUILayout.EnumPopup("Element", element)).ToString();

                if (!System.Enum.TryParse(entry.DamageType, out DamageType damageType)) damageType = DamageType.Physical;
                entry.DamageType = ((DamageType)EditorGUILayout.EnumPopup("Damage Type", damageType)).ToString();

                EditorGUILayout.LabelField("Description");
                entry.Description = EditorGUILayout.TextArea(entry.Description ?? string.Empty, EditorStyles.textArea, GUILayout.MinHeight(54f));

                DrawUniqueSkillScriptField(entry);

                EditorGUILayout.Space(4);
                if (GUILayout.Button("Convert to ScriptableObject", EditorStyles.miniButton))
                    SkillConverter.ShowConverter(entry);
            }
            EndGroup();

            DrawUniqueSkillSettings(entry);

            if (BeginGroup("Stats", ThemeAccent))
            {
                entry.BaseDamage = EditorGUILayout.FloatField("Base Damage", entry.BaseDamage);
                entry.BaseManaCost = EditorGUILayout.FloatField("Mana Cost", entry.BaseManaCost);
                entry.IsToggle = EditorGUILayout.Toggle("Is Toggle", entry.IsToggle);
                entry.BaseProjectileCount = EditorGUILayout.IntField("Projectiles", entry.BaseProjectileCount);
                entry.BaseAOERadius = EditorGUILayout.FloatField("Base AOE Radius", entry.BaseAOERadius);
            }
            EndGroup();

            if (BeginGroup("Cast Timing", ThemeAccent))
            {
                entry.BaseVCT = EditorGUILayout.FloatField("Base VCT", entry.BaseVCT);
                entry.BaseFCT = EditorGUILayout.FloatField("Base FCT", entry.BaseFCT);
                entry.BaseCastDelay = EditorGUILayout.FloatField("Cast Delay", entry.BaseCastDelay);
                entry.BaseCooldown = EditorGUILayout.FloatField("Cooldown", entry.BaseCooldown);
            }
            EndGroup();

            DrawEffectSection(entry.Effects ?? (entry.Effects = new List<EffectEntry>()), true);
            DrawChainOverridesSection(entry);
            DrawSkillTags(entry);

            NextInspectorColumn();
            DrawSkillTargeting(entry);
            DrawSkillScalings(entry);

            if (BeginGroup("Simulator", ThemeAccent, $"Lv {_simSkillLevel}"))
                DrawSkillSimulator(entry);
            EndGroup();

            if (BeginGroup("Skill Graphic Preview", ThemeAccent))
                DrawSkillPreview(entry);
            EndGroup();

            DrawSkillVisuals(entry);
            EndInspectorColumns();
        },
        (entry) => {
            Color elementColor = System.Enum.TryParse(entry.SkillElement, out ElementType el) ? GetElementColor(el) : ThemeMuted;
            var chips = new List<EntryChip> { new EntryChip(entry.SkillElement ?? "Neutral", elementColor) };
            if (entry.BaseTargeting != null && !string.IsNullOrEmpty(entry.BaseTargeting.Type)) chips.Add(new EntryChip(entry.BaseTargeting.Type, ThemeAccent));
            if (entry.IsToggle) chips.Add(new EntryChip("Toggle", ThemeMuted));
            if (entry.ChainOverrides != null && entry.ChainOverrides.Count > 0) chips.Add(new EntryChip("Chain", ThemeWarn));
            return new EntryView {
                Id = entry.SkillID.ToString(),
                Name = entry.SkillName,
                Meta = entry.SkillElement,
                MetaColor = elementColor,
                Accent = ThemeAccent,
                Icon = GetCachedSprite(entry.IconPath),
                Subtitle = entry.DamageType,
                Chips = chips.ToArray()
            };
        },
        () => {
            var s = new ActiveSkillJsonData {
                SkillElement = ElementType.Neutral.ToString(),
                DamageType = DamageType.Physical.ToString(),
                VisualType = VisualCategory.InstantHit.ToString(),
                CastVfxSpawnPoint = VisualAppearance.FollowCaster.ToString(),
                IconPath = "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png"
            };
            s.SkillID = _skillData.ActiveSkills.Count > 0 ? _skillData.ActiveSkills.Max(x => x.SkillID) + 1 : 101;
            s.BaseTargeting = new BaseTargetingData { Type = "Single", Radius = 2 };
            return s;
        },
        SkillFilter,
        () => {
            DrawSkillFilters();
            GUILayout.Space(4);
            if (GUILayout.Button("Convert ALL to ScriptableAssets", EditorStyles.miniButton))
                SkillConverter.BatchConvert(_skillData.ActiveSkills);
        }, sorts: GetSkillSorts());
    }

    private void DrawUniqueSkillScriptField(ActiveSkillJsonData entry)
    {
        MonoScript currentScript = null;
        if (!string.IsNullOrEmpty(entry.CustomClassName))
        {
            var t = System.Type.GetType(entry.CustomClassName + ", Assembly-CSharp");
            if (t != null)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:MonoScript " + t.Name))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                    if (script != null && script.GetClass() == t)
                    {
                        currentScript = script;
                        break;
                    }
                }
            }
        }

        MonoScript newScript = (MonoScript)EditorGUILayout.ObjectField(new GUIContent("Unique Skill Script", "Custom C# script class inheriting from ActiveSkill to override execution logic."), currentScript, typeof(MonoScript), false);
        if (newScript == currentScript) return;
        if (newScript == null)
        {
            entry.CustomClassName = "";
            return;
        }
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

    private void DrawUniqueSkillSettings(ActiveSkillJsonData entry)
    {
        if (string.IsNullOrEmpty(entry.CustomClassName)) return;
        System.Type t = System.Type.GetType(entry.CustomClassName + ", Assembly-CSharp");
        if (t == null) return;

        if (BeginGroup("Unique Skill Settings", ThemeWarn, t.Name))
        {
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
                    fData.StringValue = EditorGUILayout.FloatField(fVal).ToString();
                }
                else if (field.FieldType == typeof(int))
                {
                    int.TryParse(fData.StringValue, out int iVal);
                    fData.StringValue = EditorGUILayout.IntField(iVal).ToString();
                }
                else if (field.FieldType == typeof(string))
                {
                    fData.StringValue = EditorGUILayout.TextField(fData.StringValue);
                }
                else if (field.FieldType == typeof(bool))
                {
                    bool.TryParse(fData.StringValue, out bool bVal);
                    fData.StringValue = EditorGUILayout.Toggle(bVal).ToString();
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
        }
        EndGroup();
    }

    private void DrawSkillTags(ActiveSkillJsonData entry)
    {
        if (entry.Tags == null) entry.Tags = new List<string>();
        if (BeginGroup("Skill Tags", ThemeAccent, $"{entry.Tags.Count} tags"))
        {
            if (entry.Tags.Count == 0)
            {
                EditorGUILayout.LabelField("No tags assigned.", EditorStyles.miniLabel);
            }
            else
            {
                float currentWidth = 0;
                float maxWidth = Mathf.Max(200, (_twoColumns ? _columnWidth : position.width - NavigatorWidth - ListPaneWidth - 80) - 40);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < entry.Tags.Count; i++)
                {
                    GUIContent tagContent = new GUIContent($"{entry.Tags[i]}  x");
                    Vector2 size = EditorStyles.miniButton.CalcSize(tagContent);
                    if (currentWidth + size.x > maxWidth)
                    {
                        GUILayout.FlexibleSpace();
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
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Tag", EditorStyles.miniButton))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var tag in System.Enum.GetNames(typeof(SkillTag)))
                {
                    if (!entry.Tags.Contains(tag)) menu.AddItem(new GUIContent(tag), false, () => { entry.Tags.Add(tag); MarkDirty(); });
                    else menu.AddDisabledItem(new GUIContent(tag));
                }
                menu.ShowAsContext();
            }
        }
        EndGroup();
    }

    private void DrawSkillTargeting(ActiveSkillJsonData entry)
    {
        if (BeginGroup("Targeting", ThemeAccent, entry.BaseTargeting.Type))
        {
            string[] targetingTypes = { "Single", "SingleUnit", "Sphere", "Cone", "Box", "Self", "Line", "TargetLocation" };
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
                entry.BaseTargeting.LineWidth = EditorGUILayout.FloatField("Line Width", entry.BaseTargeting.LineWidth);
                EditorGUILayout.LabelField("Line Length (Linked to Stats)", entry.BaseAOERadius.ToString());
            }
            else if (entry.BaseTargeting.Type == "SingleUnit")
            {
                entry.BaseTargeting.CastRange = EditorGUILayout.FloatField("Cast Range (Fixed)", entry.BaseTargeting.CastRange);
            }
            else if (entry.BaseTargeting.Type == "TargetLocation")
            {
                entry.BaseTargeting.CastRange = EditorGUILayout.FloatField("Cast Range (Fixed)", entry.BaseTargeting.CastRange);
                entry.BaseTargeting.Radius = entry.BaseAOERadius;
                EditorGUILayout.LabelField("AOE Radius (Linked to Stats)", entry.BaseAOERadius.ToString());
            }
        }
        EndGroup();
    }

    private void DrawSkillScalings(ActiveSkillJsonData entry)
    {
        if (entry.Scalings == null) entry.Scalings = new List<OutputScaling>();
        if (BeginGroup("Scalings", ThemeAccent, $"{entry.Scalings.Count} rows"))
        {
            EditorGUILayout.HelpBox("Formula: (Stat x AttrMult + FlatBonus + LevelMult x Level) x GlobalMult", MessageType.None);
            for (int i = 0; i < entry.Scalings.Count; i++)
            {
                var scaling = entry.Scalings[i];
                EditorGUILayout.BeginVertical(SectionStyle);
                EditorGUILayout.BeginHorizontal();
                scaling.TargetField = (PayloadTarget)EditorGUILayout.EnumPopup("Target", scaling.TargetField);
                bool removed = GUILayout.Button("X", GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
                if (removed)
                {
                    entry.Scalings.RemoveAt(i);
                    MarkDirty();
                    EditorGUILayout.EndVertical();
                    break;
                }

                bool attrExists = _attributeData.Attributes.Any(a => a.Id == (int)scaling.ScalingAttribute);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Scaling Attr");
                string currentAttr = attrExists ? scaling.ScalingAttribute.ToString() : $"Attr ID {(int)scaling.ScalingAttribute} (not in Attribute.json)";
                Rect rect = GUILayoutUtility.GetRect(new GUIContent(currentAttr), EditorStyles.popup);
                Color prev = GUI.contentColor;
                if (!attrExists) GUI.contentColor = ThemeWarn;
                if (GUI.Button(rect, new GUIContent(currentAttr), EditorStyles.popup))
                {
                    var dropdown = new GenericAttributeDropdown(_attributeData.Attributes, (selected) => {
                        scaling.ScalingAttribute = (AttributeType)selected.Id;
                        MarkDirty();
                    });
                    dropdown.Show(rect);
                }
                GUI.contentColor = prev;
                EditorGUILayout.EndHorizontal();

                scaling.AttributeMultiplier = EditorGUILayout.FloatField("Attr Mult", scaling.AttributeMultiplier);
                scaling.FlatBonus = EditorGUILayout.FloatField("Flat Bonus", scaling.FlatBonus);
                scaling.LevelMultiplier = EditorGUILayout.FloatField("Level Mult", scaling.LevelMultiplier);
                scaling.GlobalMultiplier = EditorGUILayout.FloatField("Global Mult", scaling.GlobalMultiplier);
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Scaling")) entry.Scalings.Add(new OutputScaling());
        }
        EndGroup();
    }

    private void DrawSkillVisuals(ActiveSkillJsonData entry)
    {
        if (!BeginGroup("Visuals", ThemeAccent))
        {
            EndGroup();
            return;
        }

        EditorGUILayout.HelpBox("Drag a Sprite from Assets/Addressable/Sprite/Skill/Icon to auto-resolve the Addressable Key.", MessageType.None);
        string iconPathToResolve = string.IsNullOrEmpty(entry.IconPath) ? "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png" : entry.IconPath;
        Sprite currentIcon = GetCachedSprite(iconPathToResolve);
        Sprite newIcon = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Skill Icon", "Sprite loaded from Assets/Addressable/Sprite/Skill/Icon."), currentIcon, typeof(Sprite), false);
        if (newIcon != currentIcon)
        {
            if (newIcon == null)
            {
                entry.IconPath = "";
            }
            else if (!AssetDatabase.GetAssetPath(newIcon).StartsWith("Assets/Addressable/Sprite/Skill/Icon"))
            {
                EditorUtility.DisplayDialog("Invalid Folder", "Please select a Sprite from Assets/Addressable/Sprite/Skill/Icon", "OK");
            }
            else
            {
                entry.IconPath = GetAddressableKey(newIcon);
            }
        }

        EditorGUILayout.BeginHorizontal();
        entry.IconPath = EditorGUILayout.TextField("Addressable Key", entry.IconPath ?? "");
        if (GUILayout.Button("Default", GUILayout.Width(60))) entry.IconPath = "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png";
        EditorGUILayout.EndHorizontal();

        if (GetCachedSprite(string.IsNullOrEmpty(entry.IconPath) ? "Assets/Addressable/Sprite/Skill/Icon/Icon_Default.png" : entry.IconPath) == null && !string.IsNullOrEmpty(entry.IconPath))
            EditorGUILayout.HelpBox($"Key '{entry.IconPath}' not found in Addressables or Path", MessageType.Warning);

        EditorGUILayout.Space(5);
        if (!System.Enum.TryParse(entry.VisualType, out VisualCategory vCategory)) vCategory = VisualCategory.InstantHit;
        entry.VisualType = ((VisualCategory)EditorGUILayout.EnumPopup("Visual Type", vCategory)).ToString();

        DrawVfxPrefabField(ref entry.CastVfxPrefabName, "Cast VFX Prefab", "Played during cast phase of the skill (VCT/FCT).");
        if (!System.Enum.TryParse(entry.CastVfxSpawnPoint, out VisualAppearance castSpawn)) castSpawn = VisualAppearance.FollowCaster;
        entry.CastVfxSpawnPoint = ((VisualAppearance)EditorGUILayout.EnumPopup("Cast VFX Spawn Point", castSpawn)).ToString();
        DrawVfxPrefabField(ref entry.SkillIndicatorPrefabName, "Custom Ground Indicator", "Custom ground warning or targeting indicator prefab spawned at the target location during the casting phase.");

        EditorGUILayout.Space(5);
        GUILayout.Label("Visual Type Prefabs", EditorStyles.boldLabel);
        DrawVfxPrefabField(ref entry.EruptionVfxPrefabName, "Eruption VFX Prefab", "Visual effect prefab spawned at the target location when using the GroundEruption visual category.");
        DrawVfxPrefabField(ref entry.ProjectilePrefabName, "Projectile Prefab", "Visual prefab spawned for the Projectile visual category.");
        DrawVfxPrefabField(ref entry.ImpactVfxPrefabName, "Impact VFX Prefab", "Visual effect prefab spawned on top of each target when using the InstantHit visual category.");
        DrawVfxPrefabField(ref entry.AuraVfxPrefabName, "Aura VFX Prefab", "Visual effect prefab spawned and parented to each target when using the AuraBuff visual category.");
        DrawVfxPrefabField(ref entry.BeamVfxPrefabName, "Beam VFX Prefab", "Visual effect prefab utilizing a LineRenderer component, spawned for the RayBeam visual category.");
        EndGroup();
    }

    private void DrawSupportSkillEditor()
    {
        DrawEditor(_supportSkillData.SupportModifiers, _supportSelectedIndex, (idx) => _supportSelectedIndex = idx, (entry) => {
            BeginInspectorColumns();
            if (BeginGroup("Basic Info", ThemeAccent))
            {
                entry.SkillID = EditorGUILayout.IntField("Skill ID", entry.SkillID);
                entry.ModifierName = EditorGUILayout.TextField("Modifier Name", entry.ModifierName);
            }
            EndGroup();
            NextInspectorColumn();
            if (BeginGroup("Description", ThemeAccent))
            {
                entry.Description = EditorGUILayout.TextArea(entry.Description ?? string.Empty, EditorStyles.textArea, GUILayout.MinHeight(54f));
                EditorGUILayout.LabelField("{0}, {1} are filled with the modifier values at runtime.", MutedStyle);
            }
            EndGroup();
            EndInspectorColumns();
        },
        (entry) => new EntryView {
            Id = entry.SkillID.ToString(),
            Name = entry.ModifierName,
            Accent = ThemeAccent,
            Icon = GetCachedSprite(entry.IconPath),
            Subtitle = "Support Modifier"
        },
        () => new SupportSkillJsonData(),
        SupportFilter, DrawSupportFilters, sorts: GetSupportSorts(), columns: GetSupportColumns());
    }

    private void DrawChainOverridesSection(ActiveSkillJsonData entry)
    {
        if (entry.ChainOverrides == null) entry.ChainOverrides = new List<SkillChainOverrideJsonData>();
        if (BeginGroup("Skill Chain & Combo Overrides", ThemeWarn, $"{entry.ChainOverrides.Count} overrides"))
        {
            if (entry.ChainOverrides.Count == 0)
                EditorGUILayout.HelpBox("No chain overrides. Add one to enable combo/mutation for this skill.", MessageType.None);

            for (int i = 0; i < entry.ChainOverrides.Count; i++)
            {
                var chain = entry.ChainOverrides[i];
                EditorGUILayout.BeginVertical(SectionStyle);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Chain Override [{i}]", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                bool removed = GUILayout.Button("X", GUILayout.Width(22));
                EditorGUILayout.EndHorizontal();
                if (removed)
                {
                    entry.ChainOverrides.RemoveAt(i);
                    MarkDirty();
                    EditorGUILayout.EndVertical();
                    break;
                }

                chain.RequiredBuffCode = EditorGUILayout.TextField(
                    new GUIContent("Required Buff Code", "Code of the TemporaryBuff that must be active on the caster to trigger this override."),
                    chain.RequiredBuffCode);

                EditorGUILayout.BeginHorizontal();
                chain.ReplacementSkillID = EditorGUILayout.IntField(
                    new GUIContent("Replacement Skill ID", "The SkillID of the ActiveSkill that replaces this one when the buff is active."),
                    chain.ReplacementSkillID);
                var replacementSkill = _skillData.ActiveSkills.FirstOrDefault(s => s.SkillID == chain.ReplacementSkillID);
                if (replacementSkill != null)
                {
                    EditorGUILayout.LabelField(replacementSkill.SkillName, EditorStyles.miniLabel, GUILayout.Width(120));
                }
                else if (chain.ReplacementSkillID > 0)
                {
                    Color prev = GUI.contentColor;
                    GUI.contentColor = ThemeError;
                    EditorGUILayout.LabelField("NOT FOUND", EditorStyles.miniLabel, GUILayout.Width(80));
                    GUI.contentColor = prev;
                }
                EditorGUILayout.EndHorizontal();

                chain.ConsumeBuffOnCast = EditorGUILayout.Toggle(
                    new GUIContent("Consume Buff on Cast", "If true, the required buff is removed when the replacement skill is cast."),
                    chain.ConsumeBuffOnCast);

                Color hintColor = Color.white;
                if (!string.IsNullOrEmpty(chain.HintColorHex))
                    ColorUtility.TryParseHtmlString(chain.HintColorHex, out hintColor);
                hintColor = EditorGUILayout.ColorField(
                    new GUIContent("Hint Color", "Color shown on the Hotbar UI to indicate this combo is ready."),
                    hintColor);
                chain.HintColorHex = "#" + ColorUtility.ToHtmlStringRGBA(hintColor);

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Chain Override"))
                entry.ChainOverrides.Add(new SkillChainOverrideJsonData { ConsumeBuffOnCast = true, HintColorHex = "#FFFFFFFF" });
        }
        EndGroup();
    }

    private void DrawSkillEffectFields(EffectEntry effect)
    {
        effect.ApplyToCaster = EditorGUILayout.Toggle(
            new GUIContent("Apply to Caster", "If enabled, this effect targets the skill caster instead of the hit targets."),
            effect.ApplyToCaster);
        effect.ApplyOnce = EditorGUILayout.Toggle("Apply Once per Cast", effect.ApplyOnce);

        EditorGUILayout.Space(3);

        if (effect.EffectType == "SprintEffect")
        {
            effect.Value = EditorGUILayout.FloatField(new GUIContent("Sprint Bonus %", "Move speed bonus at full sprint. Jog is half of it, then it fades back to walking."), effect.Value);
            effect.Duration = EditorGUILayout.FloatField(new GUIContent("Duration (s)", "Sprint 40%, ease to jog 10%, jog 30%, fade to walk 20% of this time."), effect.Duration);
            effect.BuffName = EditorGUILayout.TextField("Buff Name", effect.BuffName);
            effect.BuffCode = EditorGUILayout.TextField("Buff Code", effect.BuffCode);
        }

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
                    effect.BuffCode = EditorGUILayout.TextField("Buff Code", effect.BuffCode);
                    effect.Duration = EditorGUILayout.FloatField("Duration", effect.Duration);
                    effect.MaxStacks = EditorGUILayout.IntField("Max Stacks", effect.MaxStacks);
                    if (!System.Enum.TryParse(effect.Category, out BuffCategory bCat)) bCat = BuffCategory.Default;
                    effect.Category = ((BuffCategory)EditorGUILayout.EnumPopup("Category", bCat)).ToString();

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
                effect.UseLerp = EditorGUILayout.Toggle("Smooth (Lerp)", effect.UseLerp);
                if (effect.UseLerp) effect.LerpDuration = EditorGUILayout.FloatField("Duration", effect.LerpDuration);
            }
        }
        else if (effect.EffectType == "SpawnProjectileEffect")
        {
            var go = string.IsNullOrEmpty(effect.PrefabPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(effect.PrefabPath);
            var newGo = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Projectile Prefab", "Prefab with a ProjectileBehaviour; it carries the skill's DamageEffect to whatever it hits."), go, typeof(GameObject), false);
            if (newGo != go) effect.PrefabPath = newGo != null ? AssetDatabase.GetAssetPath(newGo) : "";
            effect.ApplyOnce = true;
            EditorGUILayout.HelpBox("Spawns BaseProjectileCount projectiles once per cast toward the aim point. Use Self targeting.", MessageType.None);
        }
        else if (effect.EffectType == "SummonObjectEffect")
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + effect.PrefabPath + ".prefab");
            var newGo = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab (in Resources)", "GameObject prefab to spawn which must reside under a Resources/ directory."), go, typeof(GameObject), false);
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

    private void DrawVfxPrefabField(ref string prefabName, string label, string tooltip)
    {
        GameObject vfxObj = null;
        if (!string.IsNullOrEmpty(prefabName))
        {
            if (prefabName.StartsWith("Assets/")) vfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabName);
            else vfxObj = Resources.Load<GameObject>(prefabName);
        }
        var newVfx = (GameObject)EditorGUILayout.ObjectField(new GUIContent(label, tooltip), vfxObj, typeof(GameObject), false);
        if (newVfx != vfxObj) prefabName = newVfx == null ? "" : AssetDatabase.GetAssetPath(newVfx);
    }

    private void DrawSkillPreview(ActiveSkillJsonData skill)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Drag to pan, scroll to zoom. Caster is at the center.", MutedStyle);
        GUILayout.FlexibleSpace();
        _previewZoom = EditorGUILayout.Slider(_previewZoom, 5, 200, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        Rect previewRect = GUILayoutUtility.GetRect(0, 260, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(previewRect, ThemeField);
        Event e = Event.current;
        bool inside = previewRect.Contains(e.mousePosition);

        if (e.type == EventType.MouseMove && inside)
        {
            _previewTargetPoint = e.mousePosition - previewRect.position;
            _skillPreviewHover = true;
        }
        else if (e.type == EventType.MouseMove && _skillPreviewHover)
        {
            _skillPreviewHover = false;
        }

        if (e.type == EventType.Repaint)
        {
            GUI.BeginGroup(previewRect);
            Rect localRect = new Rect(0, 0, previewRect.width, previewRect.height);
            DrawGrid(localRect, _previewZoom);
            DrawTargetingGraphics(localRect, skill, _previewZoom);
            GUI.EndGroup();
        }

        if (e.type == EventType.MouseDrag && inside)
        {
            _previewOffset += e.delta;
            e.Use();
            Repaint();
        }

        if (e.type == EventType.ScrollWheel && inside)
        {
            _previewZoom = Mathf.Clamp(_previewZoom - e.delta.y, 5, 200);
            e.Use();
            Repaint();
        }
    }

    private void DrawSkillSimulator(ActiveSkillJsonData skill)
    {
        _simSkillLevel = EditorGUILayout.IntSlider("Skill Level", _simSkillLevel, 1, 10);

        HashSet<int> usedAttrs = new HashSet<int>();
        if (skill.Scalings != null)
            foreach (var s in skill.Scalings)
                if (s.ScalingAttribute > 0) usedAttrs.Add((int)s.ScalingAttribute);

        if (usedAttrs.Count > 0)
        {
            EditorGUILayout.Space(4);
            GUILayout.Label("Simulation Stats", EditorStyles.boldLabel);
            foreach (int attrId in usedAttrs)
            {
                var attr = _attributeData.Attributes.FirstOrDefault(a => a.Id == attrId);
                string label = attr != null ? $"{attr.FullName} ({attrId})" : $"Attr {attrId}";
                if (!_mockStats.ContainsKey(attrId)) _mockStats[attrId] = 0;
                _mockStats[attrId] = EditorGUILayout.IntField(label, _mockStats[attrId]);
            }
        }

        float finalDamage = CalculateSimulatedValue(skill, PayloadTarget.BaseDamage, skill.BaseDamage, _simSkillLevel);
        float finalAOE = CalculateSimulatedValue(skill, PayloadTarget.AOERadius, skill.BaseAOERadius, _simSkillLevel);
        float finalVCT = CalculateSimulatedValue(skill, PayloadTarget.VCT, skill.BaseVCT, _simSkillLevel);

        EditorGUILayout.Space(6);
        GUILayout.Label("Calculated Results", EditorStyles.boldLabel);
        DrawStatTiles(("Final Base Damage", finalDamage.ToString("0.##")), ("Final Cast Time", finalVCT.ToString("0.##") + " s"), ("Final AOE Radius", finalAOE.ToString("0.##")));
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
        
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
        float startX = (center.x % zoom);
        for (float x = startX; x < rect.width; x += zoom)
            Handles.DrawLine(new Vector2(rect.x + x, rect.y), new Vector2(rect.x + x, rect.yMax));
        
        float startY = (center.y % zoom);
        for (float y = startY; y < rect.height; y += zoom)
            Handles.DrawLine(new Vector2(rect.x, rect.y + y), new Vector2(rect.xMax, rect.y + y));

        Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        if (center.x >= rect.x && center.x <= rect.xMax)
            Handles.DrawLine(new Vector2(center.x, rect.y), new Vector2(center.x, rect.yMax));
        if (center.y >= rect.y && center.y <= rect.yMax)
            Handles.DrawLine(new Vector2(rect.x, center.y), new Vector2(rect.xMax, center.y));
        
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
        
        float simulatedAOE = CalculateSimulatedValue(skill, PayloadTarget.AOERadius, skill.BaseAOERadius, _simSkillLevel);
        float radius = (simulatedAOE > 0 ? simulatedAOE : skill.BaseTargeting.Radius) * zoom;

        Handles.BeginGUI();
        
        Handles.color = Color.white;
        Handles.DrawSolidDisc(center, Vector3.forward, 4);
        Handles.color = Color.black;
        Handles.DrawWireDisc(center, Vector3.forward, 4);

        Vector2 forward = new Vector2(0, -1);
        Vector2 arrowTip = center + forward * 20f;
        Handles.color = Color.yellow;
        Handles.DrawLine(center, arrowTip);
        Handles.DrawLine(arrowTip, (Vector3)arrowTip + new Vector3(-5, 5, 0));
        Handles.DrawLine(arrowTip, (Vector3)arrowTip + new Vector3(5, 5, 0));
        Handles.Label(arrowTip + new Vector2(5, -15), "Facing", EditorStyles.miniBoldLabel);

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
                Handles.DrawSolidArc(center, Vector3.forward, Vector3.down, angle/2, radius);
                Handles.DrawSolidArc(center, Vector3.forward, Vector3.down, -angle/2, radius);
                Handles.color = new Color(1f, 0.6f, 0.1f, 0.8f);
                Handles.DrawWireArc(center, Vector3.forward, Vector3.down, angle/2, radius);
                Handles.DrawWireArc(center, Vector3.forward, Vector3.down, -angle/2, radius);
                Vector3 leftLine = Quaternion.Euler(0, 0, angle/2) * Vector3.down * radius;
                Vector3 rightLine = Quaternion.Euler(0, 0, -angle/2) * Vector3.down * radius;
                Handles.DrawLine(center, (Vector3)center + leftLine);
                Handles.DrawLine(center, (Vector3)center + rightLine);
                break;
            case "Box":
                Vector2 boxSize = skill.BaseTargeting.BoxSize * zoom;
                Vector2 boxOffset = skill.BaseTargeting.Offset * zoom;
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
                float lineWidth = skill.BaseTargeting.LineWidth * zoom;
                Vector3[] lineVerts = new Vector3[] {
                    new Vector3(center.x - lineWidth/2, center.y, 0),
                    new Vector3(center.x + lineWidth/2, center.y, 0),
                    new Vector3(center.x + lineWidth/2, center.y - radius, 0),
                    new Vector3(center.x - lineWidth/2, center.y - radius, 0)
                };
                Handles.color = new Color(1f, 0.1f, 0.1f, 0.25f);
                Handles.DrawSolidRectangleWithOutline(lineVerts, new Color(1f, 0.1f, 0.1f, 0.25f), new Color(1f, 0.1f, 0.1f, 0.8f));
                break;
            case "TargetLocation":
                float castRange = skill.BaseTargeting.CastRange * zoom;
                float aoeRadius = skill.BaseAOERadius * zoom; 
                
                Handles.color = new Color(1f, 1f, 1f, 0.1f);
                Handles.DrawSolidDisc(center, Vector3.forward, castRange);
                Handles.color = new Color(1f, 1f, 1f, 0.4f);
                Handles.DrawWireDisc(center, Vector3.forward, castRange);
                
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
            case ElementType.Fire: return Hex(0xFF7A45);
            case ElementType.Water: return Hex(0x5AB4FF);
            case ElementType.Earth: return Hex(0xC49A5A);
            case ElementType.Electric: return Hex(0xFFD84D);
            case ElementType.Holy: return Hex(0xF2E08C);
            case ElementType.Ghost: return Hex(0xA58CFF);
            case ElementType.Neutral: return Hex(0xB8BCC4);
            default: return ThemeMuted;
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
        string relative = assetPath.Substring(resIndex + 10);
        return relative.Replace(".prefab", "");
    }
    #endregion
}
