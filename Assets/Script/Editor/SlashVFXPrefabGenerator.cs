using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor
{
    [InitializeOnLoad]
    public static class SlashVFXPrefabGenerator
    {
        #region [Variables/Fields]
        private const string ResourcesPrefabDir = "Assets/Resources/Prefab/VFX";
        private const string MaterialPath = "Assets/Materials/Slash_VFX.mat";
        private const string SlashVFXResourcesPrefabPath = "Assets/Resources/Prefab/VFX/SlashVFX.prefab";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        static SlashVFXPrefabGenerator()
        {
            EditorApplication.delayCall += GenerateSlashVFXPrefabIfMissing;
        }
        #endregion

        #region [Public Methods]
        [MenuItem("Tools/VFX/Generate Slash VFX Prefab")]
        public static void GenerateSlashVFXPrefab()
        {
            EnsureDirectories();
            CreateSlashVFXPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region [Private Methods]
        private static void GenerateSlashVFXPrefabIfMissing()
        {
            if (!File.Exists(SlashVFXResourcesPrefabPath))
            {
                GenerateSlashVFXPrefab();
            }
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(ResourcesPrefabDir))
            {
                Directory.CreateDirectory(ResourcesPrefabDir);
            }
        }

        private static void CreateSlashVFXPrefab()
        {
            GameObject go = new GameObject("SlashVFX");

            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.533f;
            main.loop = false;
            main.startLifetime = 0.533f;
            main.startSpeed = 0f;
            main.startSize = 2.5f;
            main.startColor = Color.white;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.playOnAwake = true;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var tsa = ps.textureSheetAnimation;
            tsa.enabled = true;
            tsa.numTilesX = 4;
            tsa.numTilesY = 4;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.cycleCount = 1;
            tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat != null)
            {
                renderer.material = mat;
            }
            renderer.sortingOrder = 50;
            renderer.maxParticleSize = 2f;
            SerializedObject rendererSo = new SerializedObject(renderer);
            SerializedProperty alignProp = rendererSo.FindProperty("m_RenderAlignment");
            if (alignProp != null)
            {
                alignProp.intValue = 2;
                rendererSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(go, SlashVFXResourcesPrefabPath);

            Object.DestroyImmediate(go);
        }
        #endregion
    }
}
