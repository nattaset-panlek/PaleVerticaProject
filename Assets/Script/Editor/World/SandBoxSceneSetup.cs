using System.Linq;
using CoreSystem.Agent;
using CoreSystem.Enums;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.World
{
    public static class SandBoxSceneSetup
    {
        #region [Variables/Fields]
        private const string OldScenePath = "Assets/Scenes/SandBox.unity";
        private const string OldDataFolder = "Assets/Scenes/SandBox_MapLab";
        private const string DevFolder = "Assets/Scenes/Dev";
        private const string NewScenePath = DevFolder + "/SandBox.unity";
        private const string NewDataFolder = DevFolder + "/SandBox_MapLab";
        private const string NpcFolder = "Assets/Addressable/Prefab/Entity/NPC/";
        private const string StartPointName = "Start Point";
        private static readonly string[] MissingNpcs = { "Shopkeeper", "OldMan" };
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/World/Set Up SandBox Test Scene")]
        public static void SetUp()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(NewScenePath) != null)
            {
                Debug.LogError("SandBox is already in Assets/Scenes/Dev.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!Move()) return;

            Scene scene = EditorSceneManager.OpenScene(NewScenePath);
            AddTestContent(scene);
            EditorSceneManager.SaveScene(scene);
            RemoveFromBuild();
            AssetDatabase.SaveAssets();
            Debug.Log("SandBox moved to Assets/Scenes/Dev and left out of the build.");
        }
        #endregion

        #region [Private Methods]
        private static bool Move()
        {
            if (!AssetDatabase.IsValidFolder(DevFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Dev");

            string error = AssetDatabase.MoveAsset(OldScenePath, NewScenePath);
            if (string.IsNullOrEmpty(error) && AssetDatabase.IsValidFolder(OldDataFolder))
            {
                error = AssetDatabase.MoveAsset(OldDataFolder, NewDataFolder);
            }
            if (string.IsNullOrEmpty(error)) return true;

            Debug.LogError("Could not move SandBox: " + error);
            return false;
        }

        private static void AddTestContent(Scene scene)
        {
            GameObject start = scene.GetRootGameObjects().FirstOrDefault(root => root.name == StartPointName);
            Vector2 origin = start != null ? (Vector2)start.transform.position : Vector2.zero;

            for (int i = 0; i < MissingNpcs.Length; i++)
            {
                WorldMapsBuilder.PlacePrefab(NpcFolder + MissingNpcs[i] + ".prefab", scene, origin + new Vector2(-2f + i * 2f, 3f));
            }

            WorldMapsBuilder.Portal("Portal To Town", origin + new Vector2(5f, 1.5f), "Town", "town_gate");
            WorldMapsBuilder.Portal("Portal To Field", origin + new Vector2(5f, -1.5f), "Field", "field_gate");
            WorldMapsBuilder.Spawner("Test Melee", origin + new Vector2(-8f, 3f), 2f, 2, new SpawnEntry { CharacterId = 1001, Label = "Melee", MinCount = 2, MaxCount = 2 });
            WorldMapsBuilder.Spawner("Test Ranged", origin + new Vector2(-8f, 0f), 2f, 2,
                new SpawnEntry
                {
                    CharacterId = 1002, Label = "Ranged", MinCount = 2, MaxCount = 2,
                    OverrideCombatStyle = true, Combat = CombatStyle.Ranged, OverrideAttackRange = true, AttackRange = 6f
                });
            WorldMapsBuilder.Spawner("Test Mini Boss", origin + new Vector2(-8f, -3f), 1f, 1, new SpawnEntry { CharacterId = 1003, Label = "Mini Boss", MinCount = 1, MaxCount = 1 });
        }

        private static void RemoveFromBuild()
        {
            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(entry => entry.path != OldScenePath && entry.path != NewScenePath)
                .ToArray();
        }
        #endregion
    }
}
