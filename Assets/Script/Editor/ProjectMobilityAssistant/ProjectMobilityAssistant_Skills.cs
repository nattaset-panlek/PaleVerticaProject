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
    private void DrawSkillEditor()
    {
        DrawEditor(_skillData.ActiveSkills, _skillSelectedIndex, (idx) => _skillSelectedIndex = idx, (entry) => {
            
            EditorGUILayout.BeginHorizontal();
            
            // --- Left Column: Fields ---
            EditorGUILayout.BeginVertical(SectionStyle, GUILayout.Width(350));
            
            GUILayout.Label("Basic Info", EditorStyles.boldLabel);
            entry.SkillID = EditorGUILayout.IntField("Skill ID", entry.SkillID);
            entry.SkillName = EditorGUILayout.TextField("Name", entry.SkillName);
            
            // Enum for Element
            if (!System.Enum.TryParse(entry.SkillElement, out ElementType element)) element = ElementType.Neutral;
            entry.SkillElement = ((ElementType)EditorGUILayout.EnumPopup("Element", element)).ToString();

            // Enum for DamageType
            if (!System.Enum.TryParse(entry.DamageType, out DamageType damageType)) damageType = DamageType.Physical;
            entry.DamageType = ((DamageType)EditorGUILayout.EnumPopup("Damage Type", damageType)).ToString();

            EditorGUILayout.Space();
            GUILayout.Label("Stats & Timing", EditorStyles.boldLabel);
            entry.BaseDamage = EditorGUILayout.FloatField("Base Damage", entry.BaseDamage);
            entry.BaseManaCost = EditorGUILayout.FloatField("Mana Cost", entry.BaseManaCost);
            entry.IsToggle = EditorGUILayout.Toggle("Is Toggle", entry.IsToggle);
            entry.BaseProjectileCount = EditorGUILayout.IntField("Projectiles", entry.BaseProjectileCount);
            entry.BaseAOERadius = EditorGUILayout.FloatField("Base AOE Radius", entry.BaseAOERadius);
            
            entry.BaseVCT = EditorGUILayout.FloatField("Base VCT", entry.BaseVCT);
            entry.BaseFCT = EditorGUILayout.FloatField("Base FCT", entry.BaseFCT);
            entry.BaseCastDelay = EditorGUILayout.FloatField("Cast Delay", entry.BaseCastDelay);
            entry.BaseCooldown = EditorGUILayout.FloatField("Cooldown", entry.BaseCooldown);

            EditorGUILayout.Space();
            GUILayout.Label("Visuals", EditorStyles.boldLabel);
            
            // Execution Prefab
            var vfxObj = Resources.Load<GameObject>(entry.VisualPrefabName);
            var newVfx = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Visual Prefab (Resources)", "Shown when skill activates. MUST be in a Resources folder."), vfxObj, typeof(GameObject), false);
            if (newVfx != vfxObj) entry.VisualPrefabName = GetResourcesPath(newVfx);
            
            // Cast Prefab
            var castVfxObj = Resources.Load<GameObject>(entry.CastVisualPrefabName);
            var newCastVfx = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Cast Visual Prefab (Resources)", "Shown when casting starts. MUST be in a Resources folder."), castVfxObj, typeof(GameObject), false);
            if (newCastVfx != castVfxObj) entry.CastVisualPrefabName = GetResourcesPath(newCastVfx);

            if (!System.Enum.TryParse(entry.CastVisualSpawnPoint, out VisualAppearance castSpawn)) castSpawn = VisualAppearance.TargetPosition;
            entry.CastVisualSpawnPoint = ((VisualAppearance)EditorGUILayout.EnumPopup("Cast Spawn Point", castSpawn)).ToString();

            EditorGUILayout.Space();
            GUILayout.Label("Targeting", EditorStyles.boldLabel);
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
                // Sync targeting radius with BaseAOERadius for TargetLocation
                entry.BaseTargeting.Radius = entry.BaseAOERadius;
                EditorGUILayout.LabelField("AOE Radius (Linked to Stats)", entry.BaseAOERadius.ToString());
            }

            EditorGUILayout.Space();
            GUILayout.Label("Scalings", EditorStyles.boldLabel);
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

            EditorGUILayout.Space();
            DrawEffectSection(entry.Effects ?? (entry.Effects = new List<EffectEntry>()), true);

            EditorGUILayout.Space(20);
            GUI.color = Color.cyan;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Convert to ScriptableObject", GUILayout.Height(30)))
            {
                SkillConverter.ShowConverter(entry);
            }
            if (GUILayout.Button("Convert ALL to ScriptableAssets", GUILayout.Height(30)))
            {
                SkillConverter.BatchConvert(_skillData.ActiveSkills);
            }
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();

            // --- Right Column: Preview ---
            EditorGUILayout.BeginVertical();
            DrawSkillPreview(entry);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }, (entry) => $"[{entry.SkillID}] {entry.SkillName}", 
        () => {
            var s = new ActiveSkillJsonData { SkillElement = ElementType.Neutral.ToString(), DamageType = DamageType.Physical.ToString() };
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
        });
    }

    private void DrawSupportSkillEditor()
    {
        EditorGUILayout.HelpBox("Available Modifier Scripts: " + string.Join(", ", _modifierTypeNames), MessageType.None);
        
        DrawEditor(_supportSkillData.SupportModifiers, _supportSelectedIndex, (idx) => _supportSelectedIndex = idx, (entry) => {
            entry.SkillID = EditorGUILayout.IntField("Skill ID", entry.SkillID);
            entry.ModifierName = EditorGUILayout.TextField("Modifier Name", entry.ModifierName);
            
            EditorGUILayout.Space();
            GUILayout.Label("Conditions (Modifier applies if ALL match)", EditorStyles.boldLabel);
            if (entry.Conditions == null) entry.Conditions = new List<ModifierConditionData>();
            
            for (int i = 0; i < entry.Conditions.Count; i++)
            {
                var cond = entry.Conditions[i];
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                if (!System.Enum.TryParse(cond.Type, out SupportConditionType condType)) condType = SupportConditionType.None;
                var newType = (SupportConditionType)EditorGUILayout.EnumPopup("Type", condType);
                cond.Type = newType.ToString();
                
                if (GUILayout.Button("X", GUILayout.Width(20))) { entry.Conditions.RemoveAt(i); EditorGUILayout.EndHorizontal(); EditorGUILayout.EndVertical(); break; }
                EditorGUILayout.EndHorizontal();

                switch (newType)
                {
                    case SupportConditionType.RequireElement:
                        if (!System.Enum.TryParse(cond.RequiredElement, out ElementType el)) el = ElementType.Neutral;
                        cond.RequiredElement = ((ElementType)EditorGUILayout.EnumPopup("Element", el)).ToString();
                        break;
                    case SupportConditionType.RequireSkillID:
                        cond.RequiredSkillID = EditorGUILayout.IntField("Skill ID", cond.RequiredSkillID);
                        break;
                    case SupportConditionType.MinSkillLevel:
                        cond.MinSkillLevel = EditorGUILayout.IntField("Min Level", cond.MinSkillLevel);
                        break;
                }
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Condition")) entry.Conditions.Add(new ModifierConditionData { Type = "RequireElement" });

            EditorGUILayout.Space();
            GUILayout.Label("Effects", EditorStyles.boldLabel);
            if (entry.Effects == null) entry.Effects = new List<ModifierEffectData>();
            
            for (int i = 0; i < entry.Effects.Count; i++)
            {
                var effect = entry.Effects[i];
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Effect #{i + 1}", EditorStyles.miniBoldLabel);
                if (GUILayout.Button("X", GUILayout.Width(20))) { entry.Effects.RemoveAt(i); EditorGUILayout.EndHorizontal(); EditorGUILayout.EndVertical(); break; }
                EditorGUILayout.EndHorizontal();

                // Target Field
                if (!System.Enum.TryParse(effect.TargetField, out PayloadTarget target)) target = PayloadTarget.BaseDamage;
                effect.TargetField = ((PayloadTarget)EditorGUILayout.EnumPopup("Target Field", target)).ToString();

                // Operation
                if (!System.Enum.TryParse(effect.Operation, out ModifierOperation op)) op = ModifierOperation.Add;
                effect.Operation = ((ModifierOperation)EditorGUILayout.EnumPopup("Operation", op)).ToString();
                
                effect.FlatValue = EditorGUILayout.FloatField("Flat Value", effect.FlatValue);
                effect.PercentValue = EditorGUILayout.FloatField("Percent Value (%)", effect.PercentValue);

                effect.UsePayloadReference = EditorGUILayout.Toggle("Use Payload Ref", effect.UsePayloadReference);
                if (effect.UsePayloadReference)
                {
                    if (!System.Enum.TryParse(effect.ReferencePayloadField, out PayloadTarget refPayload)) refPayload = PayloadTarget.BaseDamage;
                    effect.ReferencePayloadField = ((PayloadTarget)EditorGUILayout.EnumPopup("Ref Field", refPayload)).ToString();
                    effect.PayloadMultiplier = EditorGUILayout.FloatField("Mult", effect.PayloadMultiplier);
                }

                effect.UseAttributeReference = EditorGUILayout.Toggle("Use Attribute Ref", effect.UseAttributeReference);
                if (effect.UseAttributeReference)
                {
                    if (!System.Enum.TryParse(effect.ReferenceAttribute, out AttributeType attrType)) attrType = AttributeType.STR;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Attribute");
                    string currentTypeName = attrType.ToString();
                    Rect typeRect = GUILayoutUtility.GetRect(new GUIContent(currentTypeName), EditorStyles.popup);
                    if (GUI.Button(typeRect, new GUIContent(currentTypeName), EditorStyles.popup))
                    {
                        var dropdown = new GenericAttributeDropdown(_attributeData.Attributes, (selected) => {
                            effect.ReferenceAttribute = ((AttributeType)selected.Id).ToString();
                        });
                        dropdown.Show(typeRect);
                    }
                    EditorGUILayout.EndHorizontal();
                    effect.AttributeMultiplier = EditorGUILayout.FloatField("Mult", effect.AttributeMultiplier);
                }
                EditorGUILayout.EndVertical();
            }
            
            if (GUILayout.Button("Add Effect")) entry.Effects.Add(new ModifierEffectData { TargetField = "BaseDamage", Operation = "Add" });
            
        }, (entry) => $"[{entry.SkillID}] {entry.ModifierName}", () => new SupportSkillJsonData { Effects = new List<ModifierEffectData>(), Conditions = new List<ModifierConditionData>() });
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
            effect.MovementDistance = EditorGUILayout.FloatField("Distance", effect.MovementDistance);
            effect.ApplyOnce = EditorGUILayout.Toggle("Apply Once per Cast", effect.ApplyOnce);
            effect.UseLerp = EditorGUILayout.Toggle("Smooth (Lerp)", effect.UseLerp);
            if (effect.UseLerp) effect.LerpDuration = EditorGUILayout.FloatField("Duration", effect.LerpDuration);
        }
        else if (effect.EffectType == "SummonObjectEffect")
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + effect.PrefabPath + ".prefab");
            var newGo = (GameObject)EditorGUILayout.ObjectField("Prefab (in Resources)", go, typeof(GameObject), false);
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
        GUILayout.Label("\uD83C\uDFAF Skill Graphic Preview", EditorStyles.boldLabel);
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
        _showSimulator = EditorGUILayout.BeginFoldoutHeaderGroup(_showSimulator, "\uD83E\uDDEE Skill Calculator / Simulator");
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
