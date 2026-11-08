using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor
{
    [InitializeOnLoad]
    public static class HitVFXPrefabGenerator
    {
        #region [Variables/Fields]
        private const string OutputDirectory = "Assets/Resources/Prefab/VFX";
        private const string MeleePrefabPath = "Assets/Resources/Prefab/VFX/HitMeleeVFX.prefab";
        private const string MagicPrefabPath = "Assets/Resources/Prefab/VFX/HitMagicVFX.prefab";

        private const string MeleeMaterialPath = "Assets/Materials/Hit_Melee_VFX.mat";
        private const string MagicMaterialPath = "Assets/Materials/Hit_Magic_VFX.mat";
        private const string MagicCoreMaterialPath = "Assets/Materials/Hit_Magic_Core_VFX.mat";

        private const string MeleeTexturePath = "Assets/Image/VFX/VFX_Hit_Melee.png";
        private const string MagicSparkTexturePath = "Assets/Image/VFX/VFX_Hit_Magic_Spark.png";
        private const string MagicCoreTexturePath = "Assets/Image/VFX/VFX_Hit_Magic_Core.png";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        static HitVFXPrefabGenerator()
        {
            EditorApplication.delayCall += GenerateIfMissing;
        }
        #endregion

        #region [Public Methods]
        [MenuItem("Tools/VFX/Setup Hit VFX Materials")]
        public static void SetupMaterials()
        {
            EnsureMaterial(MeleeMaterialPath, MeleeTexturePath);
            EnsureMaterial(MagicMaterialPath, MagicSparkTexturePath);
            EnsureMaterial(MagicCoreMaterialPath, MagicCoreTexturePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/VFX/Generate Hit VFX Prefabs")]
        public static void GenerateAll()
        {
            SetupMaterials();
            EnsureDirectory();
            CreateHitMeleePrefab();
            CreateHitMagicPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region [Private Methods]
        private static void GenerateIfMissing()
        {
            if (!File.Exists(MeleePrefabPath) || !File.Exists(MagicPrefabPath))
            {
                GenerateAll();
            }
        }

        private static void EnsureDirectory()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }

        private static void EnsureMaterial(string matPath, string texPath)
        {
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

            if (mat == null)
            {
                Shader shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Mobile/Particles/Alpha Blended");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }

            EditorUtility.SetDirty(mat);
            AssetDatabase.ImportAsset(matPath, ImportAssetOptions.ForceUpdate);
        }

        private static void CreateHitMeleePrefab()
        {
            GameObject go = new GameObject("HitMeleeVFX");

            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.35f;
            main.loop = false;
            main.startLifetime = 0.28f;
            main.startSpeed = 0f;
            main.startSize = 2.2f;
            main.startColor = Color.white;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.playOnAwake = true;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.75f),
                new Keyframe(0.25f, 1.05f),
                new Keyframe(1f, 1.0f)
            );
            sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MeleeMaterialPath);
            if (mat != null) renderer.material = mat;
            renderer.sortingOrder = 60;
            renderer.maxParticleSize = 3f;

            SetRendererAlignment(renderer, 2);

            PrefabUtility.SaveAsPrefabAsset(go, MeleePrefabPath);
            Object.DestroyImmediate(go);
        }

        private static void CreateHitMagicPrefab()
        {
            GameObject root = new GameObject("HitMagicVFX");

            ParticleSystem rootPs = root.AddComponent<ParticleSystem>();
            var rootMain = rootPs.main;
            rootMain.duration = 0.45f;
            rootMain.loop = false;
            rootMain.startLifetime = 0.22f;
            rootMain.startSpeed = 0f;
            rootMain.startSize = 1.6f;
            rootMain.startColor = new Color(0.85f, 0.95f, 1f, 1f);
            rootMain.stopAction = ParticleSystemStopAction.Destroy;
            rootMain.playOnAwake = true;
            rootMain.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var rootEmission = rootPs.emission;
            rootEmission.rateOverTime = 0f;
            rootEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var rootShape = rootPs.shape;
            rootShape.enabled = false;

            var rootCol = rootPs.colorOverLifetime;
            rootCol.enabled = true;
            Gradient rootGrad = new Gradient();
            rootGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.35f, 0.8f, 1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            rootCol.color = rootGrad;

            var rootSol = rootPs.sizeOverLifetime;
            rootSol.enabled = true;
            AnimationCurve rootSizeCurve = new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(0.2f, 1.2f), new Keyframe(1f, 1.4f));
            rootSol.size = new ParticleSystem.MinMaxCurve(1f, rootSizeCurve);

            var rootRenderer = root.GetComponent<ParticleSystemRenderer>();
            Material coreMat = AssetDatabase.LoadAssetAtPath<Material>(MagicCoreMaterialPath);
            if (coreMat != null) rootRenderer.material = coreMat;
            rootRenderer.sortingOrder = 60;
            rootRenderer.maxParticleSize = 3f;
            SetRendererAlignment(rootRenderer, 2);

            CreateDirectionalBurstChild(root.transform, "LeftBurst", new Vector3(-0.15f, 0f, 0f), 180f, 50f);
            CreateDirectionalBurstChild(root.transform, "RightBurst", new Vector3(0.15f, 0f, 0f), 0f, 50f);

            PrefabUtility.SaveAsPrefabAsset(root, MagicPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateDirectionalBurstChild(Transform parent, string childName, Vector3 localPos, float angleDeg, float spreadAngle)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);

            ParticleSystem ps = child.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.38f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 11f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 0.95f);
            main.startColor = new Color(0.9f, 0.95f, 1f, 1f);
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 6, 8) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = spreadAngle * 0.5f;
            shape.radius = 0.05f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.6f, 0.8f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.35f, 0.75f, 1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var renderer = child.GetComponent<ParticleSystemRenderer>();
            Material sparkMat = AssetDatabase.LoadAssetAtPath<Material>(MagicMaterialPath);
            if (sparkMat != null) renderer.material = sparkMat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 60;
            renderer.maxParticleSize = 3f;

            SetRendererAlignment(renderer, 4);
        }

        private static void SetRendererAlignment(ParticleSystemRenderer renderer, int alignmentValue)
        {
            SerializedObject so = new SerializedObject(renderer);
            SerializedProperty prop = so.FindProperty("m_RenderAlignment");
            if (prop != null)
            {
                prop.intValue = alignmentValue;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        #endregion
    }
}
