using System.Collections.Generic;
using System.IO;
using CoreSystem.Editor.CharacterAsset;
using CoreSystem.Entity;
using CoreSystem.Refinement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoreSystem.Editor.NPC
{
    public static class NpcPrefabBuilder
    {
        #region [Variables/Fields]
        public const string DefaultBasePrefabPath = "Assets/Addressable/Prefab/Entity/Ally/Ally.prefab";
        public const int NeutralFaction = 3;
        public const float InteractRange = 1.5f;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static GameObject Build(NpcPrefabSpec spec, CharacterAssetSettings settings, List<string> log)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            if (existing != null)
            {
                log.Add($"{spec.Name}: {spec.PrefabPath} already exists, kept as is.");
                return existing;
            }
            if (!File.Exists(Path.GetFullPath(spec.PsbPath)))
            {
                log.Add($"{spec.Name}: {spec.PsbPath} not found. Build the PSB first (character_psb_tool.py port).");
                return null;
            }
            if (CharacterPsbLoader.GetImporter(spec.PsbPath) == null) AssetDatabase.ImportAsset(spec.PsbPath, ImportAssetOptions.ForceUpdate);
            CharacterPsbStandardizer.ApplyStandard(spec.PsbPath, settings, log);
            List<CharacterCheckResult> results = CharacterPsbValidator.Validate(spec.PsbPath, settings);
            foreach (CharacterCheckResult result in results)
            {
                if (result.Severity >= CharacterCheckSeverity.Warning) log.Add($"{spec.Name}: {result.Severity} {result.Message}");
            }
            if (CharacterPsbValidator.WorstSeverity(results) == CharacterCheckSeverity.Error)
            {
                log.Add($"{spec.Name}: validation has errors, no prefab created.");
                return null;
            }
            EnsureFolder(Path.GetDirectoryName(spec.PrefabPath).Replace('\\', '/'));
            if (CharacterPrefabGenerator.Create(spec.PsbPath, DefaultBasePrefabPath, spec.PrefabPath, settings, log) == null) return null;

            GameObject root = PrefabUtility.LoadPrefabContents(spec.PrefabPath);
            try
            {
                if (!ApplySettings(root, spec))
                {
                    log.Add($"{spec.Name}: base prefab has no Character component.");
                    return null;
                }
                PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            log.Add($"{spec.Name}: configured as {(spec.IsMerchant ? spec.MerchantWindow + " merchant" : "standing NPC")} (npcId '{spec.NpcId}', tags {spec.Tags}).");
            return AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
        }

        public static bool ApplySettings(GameObject root, NpcPrefabSpec spec)
        {
            Character character = root.GetComponent<Character>();
            if (character == null) return false;
            ConfigureCharacter(character, spec);
            if (!spec.IsMerchant) return true;
            RefinementMerchantSystem merchant = root.GetComponent<RefinementMerchantSystem>();
            if (merchant == null) merchant = root.AddComponent<RefinementMerchantSystem>();
            merchant.Configure(character, spec.MerchantLabel, InteractRange, spec.MerchantWindow);
            return true;
        }

        public static void ConfigureCharacter(Character character, NpcPrefabSpec spec)
        {
            SerializedObject so = new SerializedObject(character);
            SetInt(so, "defaultController", (int)CharacterControllerType.None);
            SetInt(so, "tags", (int)spec.Tags);
            SetInt(so, "faction", NeutralFaction);
            SetBool(so, "interactable", true);
            SetInt(so, "characterId", spec.CharacterId);
            SetString(so, "agentProfile.AgentName", spec.Name);
            SetBool(so, "agentProfile.CanRoam", false);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static GameObject PlaceInOpenScene(GameObject prefab, Vector3 position, string undoName)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = new Vector3(position.x, position.y, 0f);
            Undo.RegisterCreatedObjectUndo(instance, undoName);
            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(instance.scene);
            return instance;
        }

        public static bool PlaceInSceneFile(string scenePath, GameObject prefab, Vector3 position, List<string> log)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(root) != prefab) continue;
                log.Add($"{prefab.name} is already in {scenePath} at {root.transform.position}; scene not changed.");
                return true;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = new Vector3(position.x, position.y, 0f);
            bool saved = EditorSceneManager.SaveScene(scene);
            log.Add(saved ? $"Placed {prefab.name} in {scenePath} at {instance.transform.position}." : $"Could not save {scenePath}.");
            return saved;
        }
        #endregion

        #region [Private Methods]
        private static void SetInt(SerializedObject so, string name, int value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null)
            {
                Debug.LogWarning("[NpcPrefabBuilder] Missing Character field: " + name);
                return;
            }
            property.intValue = value;
        }

        private static void SetBool(SerializedObject so, string name, bool value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null)
            {
                Debug.LogWarning("[NpcPrefabBuilder] Missing Character field: " + name);
                return;
            }
            property.boolValue = value;
        }

        private static void SetString(SerializedObject so, string name, string value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null)
            {
                Debug.LogWarning("[NpcPrefabBuilder] Missing Character field: " + name);
                return;
            }
            property.stringValue = value;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
        #endregion
    }
}
