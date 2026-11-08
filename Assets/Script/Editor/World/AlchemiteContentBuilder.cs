using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSystem.Agent;
using CoreSystem.Entity;
using CoreSystem.Visuals;
using MapLab;
using MapLab.Editor;
using MapLab.Visuals;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CoreSystem.Editor.World
{
    public static class AlchemiteContentBuilder
    {
        #region [Variables/Fields]
        private const string EnvFolder = "Assets/Image/Environment/";
        private const string SourceFolder = "Assets/Prefab/Imported/AlchemiteReturn/";
        private const string EnemyFolder = "Assets/Addressable/Prefab/Entity/Enemy/";
        private const string TemplatePrefab = EnemyFolder + "Skeletom.prefab";
        private const string TownPath = "Assets/Scenes/Town.unity";
        private const string FieldPath = "Assets/Scenes/Field.unity";
        private const string SpriteLitGuid = "a97c105638bdf8b4a8650670310a4cd3";
        private const string TreeName = "Env Tree";
        private const string RockName = "Env Rock";
        private const float TreeScale = 0.53f;
        private const float RockScale = 0.5f;
        private const float HouseScale = 0.55f;
        private const float HouseClearance = 4.5f;

        private static readonly string[] Rocks = { "Rock_Tall", "Rock_Stump", "Rock_Flat", "Rock_Crystal" };
        private static readonly string[] GrassTufts = { "Grass_Tuft_1", "Grass_Tuft_2", "Grass_Tuft_3" };
        private const string GrassRootName = "Grass Tufts";
        private const int GrassCount = 320;
        private const int GrassSortingOrder = MapLabSceneBuilder.MossOrder + 1;
        private static readonly Rect GrassArea = new Rect(-29f, -19f, 58f, 38f);
        private static readonly (string Name, int Id)[] Monsters = { ("Slime", 1009), ("Gas", 1010), ("Golem", 1011) };
        private static readonly (string Art, Vector2 Position)[] Houses =
        {
            ("House_Red", new Vector2(-17f, -8f)),
            ("House_Blue", new Vector2(13f, 10f))
        };
        private static Material _lit;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/World/Import Alchemite Monsters And Environment")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            _lit = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(SpriteLitGuid));
            ConfigureSprites();
            foreach ((string name, int id) in Monsters) BuildMonster(name, id);
            DressTown();
            DressField();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(TownPath);
            Debug.Log("Alchemite monsters built, and Town and Field dressed.");
        }
        #endregion

        #region [Private Methods]
        private static void ConfigureSprites()
        {
            foreach (string name in Rocks.Concat(GrassTufts).Concat(Houses.Select(h => h.Art)))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(EnvFolder + name + ".png");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.alphaIsTransparency = true;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        private static void BuildMonster(string name, int id)
        {
            string path = EnemyFolder + name + ".prefab";
            GameObject target = PrefabUtility.LoadPrefabContents(TemplatePrefab);
            GameObject source = PrefabUtility.LoadPrefabContents(SourceFolder + name + ".prefab");
            try
            {
                GameObject model = ReplaceModel(target, source);
                Wire(target, model, id);
                target.name = name;
                PrefabUtility.SaveAsPrefabAsset(target, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(source);
                PrefabUtility.UnloadPrefabContents(target);
            }
            MakeAddressable(path);
        }

        private static GameObject ReplaceModel(GameObject target, GameObject source)
        {
            Transform old = target.transform.Find("Model");
            int sibling = old.GetSiblingIndex();
            Vector3 position = old.localPosition;
            var oldGroup = old.GetComponent<SortingGroup>();
            int layer = oldGroup != null ? oldGroup.sortingLayerID : 0;
            int order = oldGroup != null ? oldGroup.sortingOrder : 0;
            Object.DestroyImmediate(old.gameObject);

            GameObject model = Object.Instantiate(source.transform.Find("Modeln").gameObject, target.transform);
            Transform sourceShadow = source.transform.Find("shadow");
            if (sourceShadow != null) Object.Instantiate(sourceShadow.gameObject, model.transform).name = "shadow";
            model.name = "Model";
            model.transform.localPosition = position;
            model.transform.SetSiblingIndex(sibling);
            foreach (Transform child in model.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
            }

            var group = model.GetComponent<SortingGroup>();
            if (group == null) group = model.AddComponent<SortingGroup>();
            group.sortingLayerID = layer;
            group.sortingOrder = order;
            return model;
        }

        private static void Wire(GameObject target, GameObject model, int id)
        {
            var character = new SerializedObject(target.GetComponent<Character>());
            character.FindProperty("characterId").intValue = id;
            character.FindProperty("Presenter.Anim").objectReferenceValue = model.GetComponentInChildren<Animator>();
            character.FindProperty("Presenter.ModelTransform").objectReferenceValue = model.transform;
            character.ApplyModifiedPropertiesWithoutUndo();

            Transform shadow = model.transform.Find("shadow");
            var visuals = new SerializedObject(target.GetComponent<EntityVisualController>());
            visuals.FindProperty("_modelRoot").objectReferenceValue = model.transform;
            visuals.FindProperty("_shadowRenderer").objectReferenceValue = shadow != null ? shadow.GetComponent<SpriteRenderer>() : null;
            visuals.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void MakeAddressable(string path)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry template = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(TemplatePrefab));
            AddressableAssetGroup group = template != null ? template.parentGroup : settings.DefaultGroup;
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group);
            entry.address = path;
            EditorUtility.SetDirty(settings);
        }

        private static void DressTown()
        {
            Scene scene = EditorSceneManager.OpenScene(TownPath);
            Transform props = FindProps(scene);
            foreach ((string art, Vector2 position) in Houses)
            {
                RemoveNear(props, position, HouseClearance);
                if (props.Find(art) == null) CreateHouse(props, art, position);
            }
            ReplaceProps(props);
            AddGrass(scene, 11);
            Rebake(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void DressField()
        {
            Scene scene = EditorSceneManager.OpenScene(FieldPath);
            ReplaceProps(FindProps(scene));
            AddGrass(scene, 23);
            AddSpawner(scene, "Slime Spawner", new Vector2(-8f, -5.5f), 3.5f, 5, 1009, 4);
            AddSpawner(scene, "Gas Spawner", new Vector2(23f, 6f), 3.5f, 4, 1010, 3);
            AddSpawner(scene, "Golem Spawner", new Vector2(4f, -13f), 2.5f, 2, 1011, 2);
            Rebake(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddGrass(Scene scene, int seed)
        {
            Transform ground = scene.GetRootGameObjects().First(root => root.GetComponent<MapLabMap>() != null).transform.Find("Ground");
            if (ground == null || ground.Find(GrassRootName) != null) return;

            var holder = new GameObject(GrassRootName).transform;
            holder.SetParent(ground, false);
            var rng = new System.Random(seed);
            for (int i = 0; i < GrassCount; i++)
            {
                var go = new GameObject("Grass");
                go.transform.SetParent(holder, false);
                go.transform.position = new Vector2(GrassArea.xMin + (float)rng.NextDouble() * GrassArea.width, GrassArea.yMin + (float)rng.NextDouble() * GrassArea.height);
                float scale = 0.8f + (float)rng.NextDouble() * 0.6f;
                go.transform.localScale = new Vector3(rng.Next(2) == 0 ? scale : -scale, scale, 1f);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(EnvFolder + GrassTufts[rng.Next(GrassTufts.Length)] + ".png");
                renderer.sharedMaterial = _lit;
                renderer.sortingOrder = GrassSortingOrder;
            }
        }

        private static Transform FindProps(Scene scene)
        {
            return scene.GetRootGameObjects().First(root => root.GetComponent<MapLabMap>() != null).transform.Find("Props");
        }

        private static void RemoveNear(Transform props, Vector2 position, float distance)
        {
            foreach (Transform child in props.Cast<Transform>().ToArray())
            {
                if (Vector2.Distance(child.position, position) < distance && !Houses.Any(h => h.Art == child.name))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void ReplaceProps(Transform props)
        {
            int index = 0;
            foreach (Transform child in props.Cast<Transform>().ToArray())
            {
                if (child.name == "Tree" || child.name == TreeName) CreateTree(props, child.position, index);
                else if (child.name == "Boulder") CreateRock(props, child.position, index);
                else continue;

                Object.DestroyImmediate(child.gameObject);
                index++;
            }
        }

        private static void CreateTree(Transform parent, Vector2 position, int index)
        {
            string art = MapLabArtPainter.Trees[index % MapLabArtPainter.Trees.Length];
            GameObject root = NewProp(parent, TreeName, position, TreeScale);
            AddRenderer(root.transform, MapLabArtPainter.Get(MapLabSceneBuilder.GeneratedFolder, art));

            var circle = root.AddComponent<CircleCollider2D>();
            circle.radius = 0.25f / TreeScale;
            AddOccluderFade(root);
        }

        private static void CreateRock(Transform parent, Vector2 position, int index)
        {
            string name = Rocks[index % Rocks.Length];
            GameObject root = NewProp(parent, RockName, position, RockScale);
            SpriteRenderer renderer = AddRenderer(root.transform, AssetDatabase.LoadAssetAtPath<Sprite>(EnvFolder + name + ".png"));
            var circle = root.AddComponent<CircleCollider2D>();
            circle.radius = renderer.sprite.bounds.extents.x * 0.7f;
            circle.offset = new Vector2(0f, circle.radius * 0.5f);
        }

        private static void CreateHouse(Transform parent, string art, Vector2 position)
        {
            GameObject root = NewProp(parent, art, position, HouseScale);
            SpriteRenderer renderer = AddRenderer(root.transform, AssetDatabase.LoadAssetAtPath<Sprite>(EnvFolder + art + ".png"));
            Vector2 size = renderer.sprite.bounds.size;
            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(size.x * 0.85f, size.y * 0.25f);
            box.offset = new Vector2(0f, size.y * 0.125f);
            AddOccluderFade(root);
        }

        private static GameObject NewProp(Transform parent, string name, Vector2 position, float scale)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
            root.AddComponent<SortingGroup>().sortingOrder = MapLabSceneBuilder.ObjectOrder;
            return root;
        }

        private static SpriteRenderer AddRenderer(Transform parent, Sprite sprite)
        {
            var go = new GameObject("Art");
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = _lit;
            return renderer;
        }

        private static void AddOccluderFade(GameObject root)
        {
            var bounds = new Bounds(root.transform.position, Vector3.zero);
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer renderer in renderers) bounds.Encapsulate(renderer.bounds);

            var holder = new GameObject("Fade Trigger") { layer = LayerMask.NameToLayer("Ignore Raycast") };
            holder.transform.SetParent(root.transform, false);
            var trigger = holder.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            float scale = root.transform.lossyScale.x;
            trigger.size = new Vector2(bounds.size.x / scale, bounds.size.y * 0.7f / scale);
            trigger.offset = new Vector2(0f, bounds.size.y * 0.65f / scale);

            var serialized = new SerializedObject(holder.AddComponent<OccluderFadeController>());
            SerializedProperty list = serialized.FindProperty("_renderers");
            list.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            serialized.FindProperty("_viewerLayers").intValue = 1 << LayerMask.NameToLayer("Player");
            serialized.FindProperty("_fadedAlpha").floatValue = 0.35f;
            serialized.FindProperty("_fadeDuration").floatValue = 0.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSpawner(Scene scene, string name, Vector2 position, float radius, int maxAlive, int characterId, int count)
        {
            if (scene.GetRootGameObjects().Any(root => root.name == name)) return;
            WorldMapsBuilder.Spawner(name, position, radius, maxAlive, new SpawnEntry { CharacterId = characterId, Label = name, MinCount = count, MaxCount = count });
        }

        private static void Rebake(Scene scene)
        {
            MapLabMap map = scene.GetRootGameObjects().Select(root => root.GetComponent<MapLabMap>()).First(found => found != null);
            Physics2D.SyncTransforms();
            MapLabWalkableBaker.Bake(map);
        }
        #endregion
    }
}
