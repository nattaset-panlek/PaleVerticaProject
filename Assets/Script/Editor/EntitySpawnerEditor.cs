using UnityEngine;
using UnityEditor;
using CoreSystem.Agent;

namespace CoreSystem.EditorScripts
{
    [CustomEditor(typeof(EntitySpawner))]
    public class EntitySpawnerEditor : UnityEditor.Editor
    {
        private EntitySpawner _spawner;
        private bool _tableFoldout = true;
        private CoreSystem.Data.CharacterDataList _characterDataList;

        private void OnEnable()
        {
            _spawner = (EntitySpawner)target;
            LoadCharacters();
        }

        private void LoadCharacters()
        {
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Data/Character.json");
            if (json != null)
            {
                _characterDataList = JsonUtility.FromJson<CoreSystem.Data.CharacterDataList>(json.text);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty spawnTable = serializedObject.FindProperty("SpawnTable");

            // Header
            GUIStyle title = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal = { textColor = new Color(1f, 0.65f, 0.1f) }
            };
            EditorGUILayout.Space(4);
            GUILayout.Label("Entity Spawner", title);
            EditorGUILayout.Space(4);

            // Zone
            EditorGUILayout.LabelField("Zone", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ZoneShape"));
            int shapeIndex = serializedObject.FindProperty("ZoneShape").enumValueIndex;
            if (shapeIndex == (int)SpawnZoneShape.Circle)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Radius"));
            else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BoxSize"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // Timing
            EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InitialDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WaveCooldown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SpawnInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxWaves"), new GUIContent("Max Waves (0=infinite)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxAlive"),  new GUIContent("Max Alive (0=unlimited)"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // Spawn Table
            _tableFoldout = EditorGUILayout.Foldout(
                _tableFoldout,
                $"Spawn Table  [{spawnTable.arraySize} entries]",
                true, EditorStyles.foldoutHeader);

            if (_tableFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                int deleteIndex = -1;
                for (int i = 0; i < spawnTable.arraySize; i++)
                {
                    if (DrawSpawnEntry(spawnTable, i)) deleteIndex = i;
                    if (i < spawnTable.arraySize - 1) EditorGUILayout.Space(4);
                }

                if (deleteIndex >= 0)
                    spawnTable.DeleteArrayElementAtIndex(deleteIndex);

                if (GUILayout.Button("+ Add Entry"))
                    spawnTable.InsertArrayElementAtIndex(spawnTable.arraySize);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // Runtime Controls
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            GUI.enabled = Application.isPlaying;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Spawning", GUILayout.Height(28))) _spawner.StartSpawning();
            if (GUILayout.Button("Stop Spawning",  GUILayout.Height(28))) _spawner.StopSpawning();
            if (GUILayout.Button("Destroy All",    GUILayout.Height(28))) _spawner.DestroyAll();
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to use runtime controls.", MessageType.None);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawSpawnEntry(SerializedProperty table, int idx)
        {
            SerializedProperty entry = table.GetArrayElementAtIndex(idx);

            var charIdProp = entry.FindPropertyRelative("CharacterId");
            int charId = charIdProp.intValue;

            // In Editor, we assume characters have AgentCore and Character components.
            bool hasAgentCore = charId > 0;
            bool hasCharacter = charId > 0;

            string lbl = entry.FindPropertyRelative("Label").stringValue;
            if (string.IsNullOrEmpty(lbl)) lbl = charId > 0 ? $"Character {charId}" : $"Entry {idx}";
            float weight = entry.FindPropertyRelative("Weight").floatValue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Row header
            EditorGUILayout.BeginHorizontal();
            entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, lbl, true);
            GUILayout.Label($"{weight:F0}%", GUILayout.Width(38));
            GUIStyle del = new GUIStyle(EditorStyles.miniButton)
            { normal = { textColor = new Color(1f, 0.4f, 0.4f) } };
            bool wantsDelete = GUILayout.Button("X", del, GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();

            if (entry.isExpanded && !wantsDelete)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Character", "Character ID from Database"));
                string charName = "None";
                if (_characterDataList != null && _characterDataList.Characters != null)
                {
                    var c = _characterDataList.Characters.Find(x => x.Id == charId);
                    if (c != null) charName = $"[{c.Id}] {c.Name}";
                    else if (charId > 0) charName = $"Unknown ({charId})";
                }
                
                if (GUILayout.Button(charName, EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), charId == 0, () => 
                    {
                        charIdProp.intValue = 0;
                        charIdProp.serializedObject.ApplyModifiedProperties();
                    });
                    
                    if (_characterDataList != null && _characterDataList.Characters != null)
                    {
                        foreach (var c in _characterDataList.Characters)
                        {
                            int id = c.Id;
                            menu.AddItem(new GUIContent($"[{id}] {c.Name}"), charId == id, () => 
                            {
                                charIdProp.intValue = id;
                                charIdProp.serializedObject.ApplyModifiedProperties();
                            });
                        }
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("Label"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("Weight"), new GUIContent("Weight (%)"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("MinCount"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("MaxCount"));

                EditorGUILayout.Space(4);

                // Overrides section title with component hint
                if (charId <= 0)
                    EditorGUILayout.HelpBox("Assign a Character ID to enable overrides.", MessageType.Warning);
                else
                    EditorGUILayout.LabelField("Overrides", EditorStyles.boldLabel);

                // AgentCore-dependent overrides
                DrawOverride(entry, "OverrideBehavior",        "Behavior",        "Behavior",         hasAgentCore);
                DrawOverride(entry, "OverrideCombatStyle",     "Combat",          "Combat Style",     hasAgentCore);
                DrawOverride(entry, "OverrideGroupID",         "GroupID",         "Group ID",         hasAgentCore);
                DrawOverride(entry, "OverrideDetectionRadius", "DetectionRadius", "Detection Radius", hasAgentCore);
                DrawOverride(entry, "OverrideAttackRange",     "AttackRange",     "Attack Range",     hasAgentCore);

                // Character-dependent override
                DrawOverride(entry, "OverrideLevel", "Level", "Level", hasCharacter);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            return wantsDelete;
        }

        private void DrawOverride(SerializedProperty entry, string boolName, string valName, string label, bool componentPresent)
        {
            SerializedProperty toggle = entry.FindPropertyRelative(boolName);
            SerializedProperty value  = entry.FindPropertyRelative(valName);
            if (toggle == null || value == null) return;

            if (!componentPresent) toggle.boolValue = false;

            EditorGUILayout.BeginHorizontal();

            // Temporarily zero-out indent so the Toggle's hit-area matches its visual position
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            GUI.enabled = componentPresent;
            EditorGUI.BeginChangeCheck();
            bool newVal = EditorGUILayout.Toggle(toggle.boolValue, GUILayout.Width(18));
            if (EditorGUI.EndChangeCheck() && componentPresent)
                toggle.boolValue = newVal;

            EditorGUI.indentLevel = savedIndent;

            GUI.enabled = componentPresent && toggle.boolValue;
            EditorGUILayout.PropertyField(value, new GUIContent(label));
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
    }
}
