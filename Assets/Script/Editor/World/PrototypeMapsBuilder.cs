using System.Collections.Generic;
using System.Linq;
using CoreSystem.Agent;
using CoreSystem.Enums;
using CoreSystem.Entity;
using CoreSystem.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoreSystem.Editor.World
{
    public static class PrototypeMapsBuilder
    {
        #region [Variables/Fields]
        private const string SourceScene = "Assets/Scenes/SandBox.unity";
        private const string TownScene = "Assets/Scenes/Town.unity";
        private const string FieldScene = "Assets/Scenes/Field.unity";
        private const string TownSceneName = "Town";
        private const string FieldSceneName = "Field";
        private const string TownArrival = "town_gate";
        private const string FieldArrival = "field_gate";
        private const string NpcFolder = "Assets/Addressable/Prefab/Entity/NPC/";
        private static readonly string[] Npcs = { "Alchemist", "Blacksmith", "Shopkeeper", "OldMan" };
        private static readonly Vector3 PortalOffset = new Vector3(4f, 0f, 0f);
        private static readonly Vector3 ArrivalOffset = new Vector3(2.5f, 0f, 0f);
        private static readonly Vector3 SpawnerOffset = new Vector3(-8f, 0f, 0f);
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/World/Build Prototype Town and Field")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TownScene) != null || AssetDatabase.LoadAssetAtPath<SceneAsset>(FieldScene) != null)
            {
                Debug.LogError("Town.unity or Field.unity already exists. Delete it first to rebuild.");
                return;
            }

            AssetDatabase.CopyAsset(SourceScene, TownScene);
            AssetDatabase.CopyAsset(SourceScene, FieldScene);

            BuildTown();
            BuildField();
            AddToBuildSettings();
            EditorSceneManager.OpenScene(TownScene);
            Debug.Log("Town and Field built. Move the NPCs, portals and spawner onto walkable ground.");
        }
        #endregion

        #region [Private Methods]
        private static void BuildTown()
        {
            var scene = EditorSceneManager.OpenScene(TownScene);
            Vector3 origin = PlayerPosition();

            foreach (EntitySpawner spawner in Object.FindObjectsByType<EntitySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                spawner.gameObject.SetActive(false);
            }

            for (int i = 0; i < Npcs.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NpcFolder + Npcs[i] + ".prefab");
                if (prefab == null)
                {
                    Debug.LogWarning("NPC prefab missing: " + Npcs[i]);
                    continue;
                }

                var npc = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                npc.transform.position = origin + new Vector3(-3f + i * 2f, 3f, 0f);
            }

            CreatePortal("Portal To Field", origin + PortalOffset, FieldSceneName, FieldArrival);
            CreateArrival(TownArrival, origin + PortalOffset - ArrivalOffset);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildField()
        {
            var scene = EditorSceneManager.OpenScene(FieldScene);
            Vector3 origin = PlayerPosition();

            CreatePortal("Portal To Town", origin + PortalOffset, TownSceneName, TownArrival);
            CreateArrival(FieldArrival, origin + PortalOffset - ArrivalOffset);

            var go = new GameObject("Field Spawner");
            go.transform.position = origin + SpawnerOffset;
            var spawner = go.AddComponent<EntitySpawner>();
            spawner.MaxAlive = 8;
            spawner.RespawnDelay = 8f;
            spawner.SpawnTable = new List<SpawnEntry>
            {
                new SpawnEntry { CharacterId = 1001, Label = "Melee", Weight = 100f, MinCount = 4, MaxCount = 4 },
                new SpawnEntry
                {
                    CharacterId = 1002, Label = "Ranged", Weight = 100f, MinCount = 3, MaxCount = 3,
                    OverrideCombatStyle = true, Combat = CombatStyle.Ranged,
                    OverrideAttackRange = true, AttackRange = 6f
                },
                new SpawnEntry
                {
                    CharacterId = 1003, Label = "Mini Boss", Weight = 100f, MinCount = 1, MaxCount = 1,
                    OverrideLevel = true, Level = 15
                }
            };
            EditorSceneManager.SaveScene(scene);
        }

        private static Vector3 PlayerPosition()
        {
            Character player = Object.FindObjectsByType<Character>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c.DefaultController == CharacterControllerType.Player);
            return player != null ? player.transform.position : Vector3.zero;
        }

        private static void CreatePortal(string name, Vector3 position, string targetScene, string targetSpawn)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 8f;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.color = new Color(0.45f, 0.75f, 1f);
            go.AddComponent<CircleCollider2D>().isTrigger = true;

            var serialized = new SerializedObject(go.AddComponent<MapPortal>());
            serialized.FindProperty("_targetScene").stringValue = targetScene;
            serialized.FindProperty("_targetSpawnId").stringValue = targetSpawn;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateArrival(string id, Vector3 position)
        {
            var go = new GameObject("Arrival " + id);
            go.transform.position = position;
            var serialized = new SerializedObject(go.AddComponent<MapSpawnPoint>());
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            foreach (string path in new[] { TownScene, FieldScene })
            {
                if (scenes.All(s => s.path != path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        #endregion
    }
}
