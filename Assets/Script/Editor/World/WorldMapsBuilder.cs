using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSystem.Agent;
using CoreSystem.Enums;
using CoreSystem.Pathfinding;
using CoreSystem.World;
using MapLab;
using MapLab.Editor;
using MapLab.Shapes;
using MapLab.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CoreSystem.Editor.World
{
    public static class WorldMapsBuilder
    {
        #region [Variables/Fields]
        private const string TownPath = "Assets/Scenes/Town.unity";
        private const string FieldPath = "Assets/Scenes/Field.unity";
        private const string ArchiveFolder = "Assets/Scenes/Archive";
        private const string NpcFolder = "Assets/Addressable/Prefab/Entity/NPC/";
        private const string DummyPrefab = "Assets/Addressable/Prefab/Entity/Enemy/Dummy.prefab";
        private const string SpriteLitGuid = "a97c105638bdf8b4a8650670310a4cd3";
        private const string TownSceneName = "Town";
        private const string FieldSceneName = "Field";
        private const string TownGate = "town_gate";
        private const string FieldGate = "field_gate";
        private const float CellSize = 0.5f;
        private const float RoadWidth = 1.8f;
        private static readonly Rect MapArea = new Rect(-30f, -20f, 60f, 40f);
        private static readonly Color Warm = new Color(1f, 0.74f, 0.46f);
        private static readonly Color Cool = new Color(0.5f, 0.66f, 1f);
        private static readonly Color PortalColor = new Color(0.45f, 0.75f, 1f);
        private static readonly Color Curb = new Color(0.7f, 0.68f, 0.77f);
        private static readonly Color[] MossTints =
        {
            new Color(0.53f, 0.78f, 0.4f, 0.4f),
            new Color(0.66f, 0.87f, 0.5f, 0.5f),
            new Color(0.88f, 0.8f, 0.6f, 0.3f)
        };

        private static Material _lit;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/World/Rebuild Town and Field")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Rebuild Town and Field",
                    "The current Town and Field scenes will be moved to Assets/Scenes/Archive and built again.", "Rebuild", "Cancel")) return;

            _lit = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(SpriteLitGuid));
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Archive(TownPath);
            Archive(FieldPath);

            BuildMap(TownPath, BuildTown, PlaceTownActors);
            BuildMap(FieldPath, BuildField, PlaceFieldActors);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(TownPath);
            Debug.Log("Town and Field rebuilt. Old scenes are in Assets/Scenes/Archive.");
        }
        #endregion

        #region [Private Methods]
        private static void BuildMap(string path, Action<Transform> buildWorld, Action<Scene> placeActors)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);

            var root = new GameObject(MapLabSceneBuilder.EnvironmentRootName);
            BuildGround(root.transform);
            buildWorld(root.transform);
            BuildAtmosphere(root.transform);

            MapLabMap map = AddMap(root);
            Physics2D.SyncTransforms();
            MapLabWalkableBaker.Bake(map);
            AddWalkableGrid(map);

            placeActors(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildTown(Transform root)
        {
            Transform roads = MapLabSceneBuilder.Set(root, "Roads", 2);
            Road(roads, "Road East", new Vector2(8f, 0f), new Vector2(14f, 1.2f), new Vector2(20f, 0.2f), new Vector2(31f, 0.6f));
            Road(roads, "Road West", new Vector2(-8f, 0f), new Vector2(-15f, -1.4f), new Vector2(-22f, -0.4f), new Vector2(-31f, -1f));
            Road(roads, "Road North", new Vector2(0f, 6f), new Vector2(-1f, 11f), new Vector2(0.6f, 21f));
            Road(roads, "Road South", new Vector2(0f, -6f), new Vector2(1.2f, -12f), new Vector2(-0.4f, -21f));

            Transform plaza = MapLabSceneBuilder.Set(root, "Plaza", 1);
            Flat(plaza, "Plaza", Blob(Vector2.zero, new Vector2(10f, 7.4f), 14, 5), MapLabArtPainter.Cobble, 4f, MapLabSceneBuilder.PlazaOrder, 0.22f);
            foreach (Vector2 position in new[] { new Vector2(-8f, 4.8f), new Vector2(8f, 4.8f), new Vector2(-8f, -4.8f), new Vector2(8f, -4.8f) })
            {
                Pillar(plaza, position);
            }

            Transform cliffs = MapLabSceneBuilder.Set(root, "Cliffs", 0);
            ExtrudedShape plateau = ExtrudedShapeMenu.Create(ExtrudedShapeType.Plateau, "North Hill", cliffs, Vector3.zero,
                Blob(new Vector2(-17f, 12f), new Vector2(8f, 4.5f), 12, 3), 1.75f, 0.4f, ExtrudedShapeMenu.CharacterOrder);
            ExtrudedShapeMenu.AddTier(plateau, 0.6f, 1.25f);

            Transform ruins = MapLabSceneBuilder.Set(root, "Ruins", 3);
            ExtrudedShapeMenu.Create(ExtrudedShapeType.Ring, "Old Wall", ruins, Vector3.zero,
                Blob(new Vector2(17f, -12f), new Vector2(5.5f, 3.6f), 10, 17), 1.2f, 0.9f, ExtrudedShapeMenu.CharacterOrder);

            Transform props = MapLabSceneBuilder.Set(root, "Props", MapLabSets.Palette.Length - 1);
            foreach (Vector2 position in new[]
                     {
                         new Vector2(-14f, 5.5f), new Vector2(14f, 6f), new Vector2(-15f, -7f), new Vector2(11f, 11f),
                         new Vector2(-6f, 13f), new Vector2(8f, -12f), new Vector2(-9f, -13f), new Vector2(23f, 7f),
                         new Vector2(-24f, 4f), new Vector2(-23f, -11f)
                     }) Tree(props, position);
            foreach (Vector2 position in new[]
                     {
                         new Vector2(-12f, 3f), new Vector2(13f, 3.6f), new Vector2(-12f, -4f), new Vector2(5f, 10f), new Vector2(-4f, -11f)
                     }) Boulder(props, position);

            Transform lights = MapLabSceneBuilder.Child(root, "Lights");
            Light(lights, "Global Light", Vector2.zero, Light2D.LightType.Global, Color.white, 1f, 0f);
            foreach (Vector2 position in new[] { new Vector2(-8f, 3.6f), new Vector2(8f, 3.6f), new Vector2(-8f, -6f), new Vector2(8f, -6f) })
            {
                Light(lights, "Torch", position, Light2D.LightType.Point, Warm, 0.45f, 5f);
            }
        }

        private static void BuildField(Transform root)
        {
            Transform roads = MapLabSceneBuilder.Set(root, "Roads", 2);
            Road(roads, "Main Road", new Vector2(-31f, 0f), new Vector2(-21f, 1.6f), new Vector2(-11f, -1.8f), new Vector2(0f, 0.6f), new Vector2(11f, -2.4f), new Vector2(31f, -1f));
            Road(roads, "North Trail", new Vector2(-2f, 0.6f), new Vector2(-3.6f, 7f), new Vector2(-1f, 13f), new Vector2(-2f, 21f));

            Transform clearing = MapLabSceneBuilder.Set(root, "Clearing", 1);
            Flat(clearing, "Dirt Clearing", Blob(new Vector2(12f, 8f), new Vector2(7f, 5f), 11, 7), MapLabArtPainter.Dirt, 2f, MapLabSceneBuilder.JunctionOrder, 0f);

            Transform cliffs = MapLabSceneBuilder.Set(root, "Cliffs", 0);
            ExtrudedShape plateau = ExtrudedShapeMenu.Create(ExtrudedShapeType.Plateau, "West Hill", cliffs, Vector3.zero,
                Blob(new Vector2(-18f, 11.5f), new Vector2(8.5f, 5f), 12, 9), 1.75f, 0.4f, ExtrudedShapeMenu.CharacterOrder);
            ExtrudedShape middle = ExtrudedShapeMenu.AddTier(plateau, 0.64f, 1.25f);
            ExtrudedShapeMenu.AddTier(middle, 0.55f, 1f);
            ExtrudedShapeMenu.Create(ExtrudedShapeType.Void, "Sinkhole", cliffs, Vector3.zero,
                Blob(new Vector2(-14f, -12f), new Vector2(6.5f, 3.6f), 9, 11), 2f, 0.35f, ExtrudedShapeMenu.CharacterOrder);

            Transform ruins = MapLabSceneBuilder.Set(root, "Ruins", 3);
            ExtrudedShapeMenu.Create(ExtrudedShapeType.Ring, "Ruined Arena", ruins, Vector3.zero,
                Blob(new Vector2(16f, -12f), new Vector2(7f, 4.6f), 12, 21), 1.2f, 0.9f, ExtrudedShapeMenu.CharacterOrder);

            Transform props = MapLabSceneBuilder.Set(root, "Props", MapLabSets.Palette.Length - 1);
            var rng = new Random(31);
            for (int i = 0; i < 26; i++)
            {
                Vector2 position = RandomPoint(rng);
                if (IsClear(position)) Tree(props, position);
            }
            for (int i = 0; i < 18; i++)
            {
                Vector2 position = RandomPoint(rng);
                if (IsClear(position)) Boulder(props, position);
            }

            Transform lights = MapLabSceneBuilder.Child(root, "Lights");
            Light(lights, "Global Light", Vector2.zero, Light2D.LightType.Global, new Color(0.86f, 0.9f, 1f), 0.9f, 0f);
            Light(lights, "Arena Glow", new Vector2(16f, -12f), Light2D.LightType.Point, Cool, 0.5f, 8f);
            Light(lights, "Road Lantern", new Vector2(-24f, 3f), Light2D.LightType.Point, Warm, 0.45f, 5f);
        }

        private static bool IsClear(Vector2 position)
        {
            bool nearRoad = Mathf.Abs(position.y) < 4.5f;
            bool nearHill = Inside(position, new Vector2(-18f, 11.5f), new Vector2(11f, 7f));
            bool nearHole = Inside(position, new Vector2(-14f, -12f), new Vector2(9f, 6f));
            bool nearArena = Inside(position, new Vector2(16f, -12f), new Vector2(10f, 7f));
            bool nearClearing = Inside(position, new Vector2(12f, 8f), new Vector2(9f, 7f));
            bool nearTrail = Mathf.Abs(position.x + 2f) < 4f && position.y > 0f;
            return !nearRoad && !nearHill && !nearHole && !nearArena && !nearClearing && !nearTrail;
        }

        private static bool Inside(Vector2 point, Vector2 center, Vector2 radii)
        {
            Vector2 offset = point - center;
            return offset.x * offset.x / (radii.x * radii.x) + offset.y * offset.y / (radii.y * radii.y) < 1f;
        }

        private static Vector2 RandomPoint(Random rng)
        {
            return new Vector2(MapArea.xMin + 2f + (float)rng.NextDouble() * (MapArea.width - 4f),
                MapArea.yMin + 2f + (float)rng.NextDouble() * (MapArea.height - 4f));
        }

        private static void PlaceTownActors(Scene scene)
        {
            PlacePrefab(NpcFolder + "Shopkeeper.prefab", scene, new Vector2(-5f, 3f));
            PlacePrefab(NpcFolder + "Blacksmith.prefab", scene, new Vector2(5f, 3f));
            PlacePrefab(NpcFolder + "Alchemist.prefab", scene, new Vector2(-5f, -3f));
            PlacePrefab(NpcFolder + "OldMan.prefab", scene, new Vector2(0f, 4.5f));
            PlacePrefab(DummyPrefab, scene, new Vector2(14f, -5f));

            SpawnPoint(GameCoreLoader.StartSpawnId, new Vector2(0f, -2f));
            SpawnPoint(TownGate, new Vector2(21f, 0.2f));
            Portal("Portal To Field", new Vector2(25f, 0.4f), FieldSceneName, FieldGate);
        }

        private static void PlaceFieldActors(Scene scene)
        {
            SpawnPoint(GameCoreLoader.StartSpawnId, new Vector2(-23f, 1f));
            SpawnPoint(FieldGate, new Vector2(-23f, 1f));
            Portal("Portal To Town", new Vector2(-27f, 0.6f), TownSceneName, TownGate);

            Spawner("Road Spawner", new Vector2(2f, 0f), 6f, 8, new SpawnEntry { CharacterId = 1001, Label = "Melee", MinCount = 6, MaxCount = 6 });
            Spawner("Clearing Spawner", new Vector2(12f, 8f), 4.5f, 5,
                new SpawnEntry
                {
                    CharacterId = 1002, Label = "Ranged", MinCount = 5, MaxCount = 5,
                    OverrideCombatStyle = true, Combat = CombatStyle.Ranged, OverrideAttackRange = true, AttackRange = 6f
                });
            Spawner("Arena Spawner", new Vector2(16f, -12f), 2f, 1,
                new SpawnEntry { CharacterId = 1003, Label = "Mini Boss", MinCount = 1, MaxCount = 1, OverrideLevel = true, Level = 15 });
        }

        private static void BuildGround(Transform root)
        {
            Transform ground = MapLabSceneBuilder.Child(root, "Ground");
            var go = new GameObject("Grass Base");
            go.transform.SetParent(ground, false);
            go.transform.position = MapArea.center;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Art(MapLabArtPainter.Grass);
            renderer.sharedMaterial = _lit;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = MapArea.size + new Vector2(30f, 20f);
            renderer.sortingOrder = MapLabSceneBuilder.GroundOrder;

            Transform patches = MapLabSceneBuilder.Child(ground, "Ground Patches");
            var rng = new Random(7);
            for (int i = 0; i < 44; i++)
            {
                var patch = new GameObject($"Patch {i:00}");
                patch.transform.SetParent(patches, false);
                patch.transform.position = RandomPoint(rng);
                patch.transform.rotation = Quaternion.Euler(0f, 0f, (float)rng.NextDouble() * 360f);
                float scale = 1.2f + (float)rng.NextDouble() * 1.4f;
                patch.transform.localScale = new Vector3(scale * (0.8f + (float)rng.NextDouble() * 0.5f), scale, 1f);
                var sprite = patch.AddComponent<SpriteRenderer>();
                sprite.sprite = Art(MapLabArtPainter.Moss);
                sprite.sharedMaterial = _lit;
                sprite.color = MossTints[rng.Next(MossTints.Length)];
                sprite.sortingOrder = MapLabSceneBuilder.MossOrder;
            }
        }

        private static void Road(Transform parent, string name, params Vector2[] points)
        {
            ExtrudedShapeMenu.CreatePath(name, parent, Vector3.zero, points, RoadWidth, MapLabSceneBuilder.PathOrder);
        }

        private static void Flat(Transform parent, string name, Vector2[] points, string texture, float textureSize, int order, float border)
        {
            ExtrudedShape shape = ExtrudedShapeMenu.Create(ExtrudedShapeType.Plateau, name, parent, Vector3.zero, points, 0f, 0.4f, order);
            Tune(shape, serialized =>
            {
                serialized.FindProperty("_ySort").boolValue = false;
                serialized.FindProperty("_generateCollision").boolValue = false;
                serialized.FindProperty("_topTexture").objectReferenceValue = ExtrudedShapeMenu.Art(texture);
                serialized.FindProperty("_topTextureSize").floatValue = textureSize;
                serialized.FindProperty("_topColor").colorValue = Color.white;
                serialized.FindProperty("_borderWidth").floatValue = border;
                serialized.FindProperty("_borderColor").colorValue = Curb;
            });
        }

        private static void Pillar(Transform parent, Vector2 position)
        {
            var half = new Vector2(0.42f, 0.26f);
            var box = new[] { new Vector2(-half.x, -half.y), new Vector2(half.x, -half.y), new Vector2(half.x, half.y), new Vector2(-half.x, half.y) };
            ExtrudedShape pillar = ExtrudedShapeMenu.Create(ExtrudedShapeType.Plateau, "Pillar", parent, position, box, 2.2f, 0.4f,
                ExtrudedShapeMenu.CharacterOrder, new[] { true, true, true, true });
            Tune(pillar, serialized =>
            {
                serialized.FindProperty("_topTexture").objectReferenceValue = ExtrudedShapeMenu.Art(MapLabArtPainter.TopStone);
                serialized.FindProperty("_topColor").colorValue = new Color(0.9f, 0.88f, 0.95f);
                serialized.FindProperty("_wallColor").colorValue = new Color(0.76f, 0.74f, 0.84f);
                serialized.FindProperty("_wallTextureSize").floatValue = 1f;
                serialized.FindProperty("_borderWidth").floatValue = 0f;
            });
        }

        private static void Tune(PointShape shape, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(shape);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            shape.Rebuild();
        }

        private static void Boulder(Transform parent, Vector2 position)
        {
            GameObject boulder = Prop(parent, "Boulder", position, Art(MapLabArtPainter.Boulder), new Vector2(1.6f, 0.6f));
            var circle = boulder.AddComponent<CircleCollider2D>();
            circle.radius = 0.42f;
            circle.offset = new Vector2(0f, 0.24f);
        }

        private static void Tree(Transform parent, Vector2 position)
        {
            GameObject tree = Prop(parent, "Tree", position, Art(MapLabArtPainter.TreeTrunk), new Vector2(1.2f, 0.45f));
            var circle = tree.AddComponent<CircleCollider2D>();
            circle.radius = 0.22f;
            circle.offset = new Vector2(0f, 0.08f);

            Sprite(tree.transform, "Canopy Shadow", Art(MapLabArtPainter.SoftShadow), MapLabSceneBuilder.ShadowOrder,
                new Color(0f, 0f, 0f, 0.24f), new Vector3(0.35f, 0.15f, 0f), new Vector3(3.3f, 1.5f, 1f));
            SpriteRenderer canopy = Sprite(tree.transform, "Canopy", Art(MapLabArtPainter.TreeCanopy), MapLabSceneBuilder.CanopyOrder,
                Color.white, new Vector3(0f, 2.05f, 0f), Vector3.one);
            canopy.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            var trigger = canopy.gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(2.6f, 3.05f);
            trigger.offset = new Vector2(0f, -0.575f);

            var serialized = new SerializedObject(canopy.gameObject.AddComponent<OccluderFadeController>());
            SerializedProperty renderers = serialized.FindProperty("_renderers");
            renderers.arraySize = 1;
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = canopy;
            serialized.FindProperty("_viewerLayers").intValue = 1 << LayerMask.NameToLayer("Player");
            serialized.FindProperty("_fadedAlpha").floatValue = 0.35f;
            serialized.FindProperty("_fadeDuration").floatValue = 0.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Prop(Transform parent, string name, Vector2 position, Sprite sprite, Vector2 shadowSize)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            SpriteRenderer body = Sprite(root.transform, "Sprite", sprite, MapLabSceneBuilder.ObjectOrder, Color.white, Vector3.zero, Vector3.one);
            body.spriteSortPoint = SpriteSortPoint.Pivot;
            Sprite shadow = Art(MapLabArtPainter.SoftShadow);
            Sprite(root.transform, "Shadow", shadow, MapLabSceneBuilder.ShadowOrder, new Color(0f, 0f, 0f, 0.2f),
                new Vector3(0.12f, -0.03f, 0f), new Vector3(shadowSize.x, shadowSize.y, 1f));
            Sprite(root.transform, "Contact Shadow", shadow, MapLabSceneBuilder.ContactShadowOrder, new Color(0f, 0f, 0f, 0.24f),
                new Vector3(0.02f, 0.01f, 0f), new Vector3(shadowSize.x * 0.55f, shadowSize.y * 0.5f, 1f));
            return root;
        }

        private static SpriteRenderer Sprite(Transform parent, string name, Sprite sprite, int order, Color color, Vector3 localPosition, Vector3 localScale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = _lit;
            renderer.sortingOrder = order;
            renderer.color = color;
            return renderer;
        }

        private static void Light(Transform parent, string name, Vector2 position, Light2D.LightType type, Color color, float intensity, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var light = go.AddComponent<Light2D>();
            light.lightType = type;
            light.color = color;
            light.intensity = intensity;
            light.shadowsEnabled = false;
            light.targetSortingLayers = SortingLayer.layers.Select(layer => layer.id).ToArray();
            if (type != Light2D.LightType.Point) return;
            light.pointLightOuterRadius = radius;
            light.pointLightInnerRadius = radius * 0.2f;
            light.falloffIntensity = 0.6f;
        }

        private static void BuildAtmosphere(Transform root)
        {
            Transform atmosphere = MapLabSceneBuilder.Child(root, "Atmosphere");
            var volume = MapLabSceneBuilder.Child(atmosphere, "Global Volume").gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(MapLabSceneBuilder.DemoFolder + "/MapLab_Volume.asset");

            Material fog = AssetDatabase.LoadAssetAtPath<Material>(MapLabSceneBuilder.GeneratedFolder + "/MapLab_Fog.mat");
            Material dust = AssetDatabase.LoadAssetAtPath<Material>(MapLabSceneBuilder.GeneratedFolder + "/MapLab_Dust.mat");

            ParticleSystem mist = Particles(atmosphere, "Ground Mist", fog, MapLabSceneBuilder.MistOrder, MapArea.size);
            ParticleSystem.MainModule mistMain = mist.main;
            mistMain.startLifetime = new ParticleSystem.MinMaxCurve(22f, 30f);
            mistMain.startSize = new ParticleSystem.MinMaxCurve(5f, 9f);
            mistMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.04f), new Color(1f, 1f, 1f, 0.08f));
            mistMain.maxParticles = 80;
            ParticleSystem.EmissionModule mistEmission = mist.emission;
            mistEmission.rateOverTime = 2f;
            FadeInOut(mist);

            ParticleSystem motes = Particles(atmosphere, "Dust Motes", dust, MapLabSceneBuilder.DustOrder, MapArea.size * 0.7f);
            ParticleSystem.MainModule motesMain = motes.main;
            motesMain.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            motesMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
            motesMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.93f, 0.75f, 0.5f), new Color(1f, 0.9f, 0.7f, 0.85f));
            motesMain.maxParticles = 160;
            ParticleSystem.EmissionModule motesEmission = motes.emission;
            motesEmission.rateOverTime = 7f;
            FadeInOut(motes);
        }

        private static ParticleSystem Particles(Transform parent, string name, Material material, int order, Vector2 area)
        {
            GameObject go = MapLabSceneBuilder.Child(parent, name).gameObject;
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.prewarm = true;
            main.playOnAwake = true;
            main.startSpeed = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(area.x, area.y, 0f);
            return system;
        }

        private static void FadeInOut(ParticleSystem system)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static MapLabMap AddMap(GameObject root)
        {
            var map = root.AddComponent<MapLabMap>();
            var serialized = new SerializedObject(map);
            serialized.FindProperty("_bounds").rectValue = new Rect(MapArea.xMin + 1f, MapArea.yMin + 1f, MapArea.width - 2f, MapArea.height - 2f);
            serialized.FindProperty("_cellSize").floatValue = CellSize;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return map;
        }

        private static void AddWalkableGrid(MapLabMap map)
        {
            var go = new GameObject("Walkable Grid");
            var serialized = new SerializedObject(go.AddComponent<WalkableGridManager>());
            SerializedProperty data = serialized.FindProperty("_walkableData");
            data.arraySize = 1;
            data.GetArrayElementAtIndex(0).objectReferenceValue = map.WalkableData;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void PlacePrefab(string path, Scene scene, Vector2 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("Prefab missing: " + path);
                return;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = position;
        }

        private static void SpawnPoint(string id, Vector2 position)
        {
            var go = new GameObject("Spawn Point " + id);
            go.transform.position = position;
            var serialized = new SerializedObject(go.AddComponent<MapSpawnPoint>());
            serialized.FindProperty("_id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void Portal(string name, Vector2 position, string targetScene, string targetSpawn)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 8f;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.color = PortalColor;
            renderer.sortingOrder = MapLabSceneBuilder.ObjectOrder;
            go.AddComponent<CircleCollider2D>().isTrigger = true;

            var serialized = new SerializedObject(go.AddComponent<MapPortal>());
            serialized.FindProperty("_targetScene").stringValue = targetScene;
            serialized.FindProperty("_targetSpawnId").stringValue = targetSpawn;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var glow = new GameObject("Glow");
            glow.transform.SetParent(go.transform, false);
            var light = glow.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = PortalColor;
            light.intensity = 0.8f;
            light.pointLightOuterRadius = 4f;
            light.pointLightInnerRadius = 0.6f;
            light.targetSortingLayers = SortingLayer.layers.Select(layer => layer.id).ToArray();
        }

        internal static void Spawner(string name, Vector2 position, float radius, int maxAlive, SpawnEntry entry)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var spawner = go.AddComponent<EntitySpawner>();
            spawner.Radius = radius;
            spawner.MaxAlive = maxAlive;
            spawner.RespawnDelay = 8f;
            entry.Weight = 100f;
            spawner.SpawnTable = new List<SpawnEntry> { entry };
        }

        private static Sprite Art(string name)
        {
            return MapLabArtPainter.Get(MapLabSceneBuilder.GeneratedFolder, name);
        }

        private static Vector2[] Blob(Vector2 center, Vector2 radii, int count, int seed)
        {
            var rng = new Random(seed);
            var points = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                float jitter = 1f + ((float)rng.NextDouble() - 0.5f) * 0.16f;
                points[i] = center + new Vector2(Mathf.Cos(angle) * radii.x, Mathf.Sin(angle) * radii.y) * jitter;
            }
            return points;
        }

        private static void Archive(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            if (!AssetDatabase.IsValidFolder(ArchiveFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Archive");
            string target = AssetDatabase.GenerateUniqueAssetPath($"{ArchiveFolder}/{Path.GetFileNameWithoutExtension(path)}_Old.unity");
            string error = AssetDatabase.MoveAsset(path, target);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }

        private static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => !s.path.StartsWith(ArchiveFolder) && s.path != TownPath && s.path != FieldPath).ToList();
            scenes.Add(new EditorBuildSettingsScene(TownPath, true));
            scenes.Add(new EditorBuildSettingsScene(FieldPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        #endregion
    }
}
