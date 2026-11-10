using System.IO;
using CoreSystem.Visuals;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.Visuals
{
    public sealed class DirectionHintAssetBuilder : AssetPostprocessor
    {
        #region [Variables/Fields]
        public const string TextureAssetPath = "Assets/Resources/Sprite/World/DirectionHint_SDF.png";
        public const string MaterialAssetPath = "Assets/Resources/Materials/DirectionHint.mat";
        public const string SettingsAssetPath = "Assets/Resources/Settings/DirectionHintSettings.asset";
        private const string MenuPath = "Pale Vertica/Visuals/Build Direction Hint Assets";
        #endregion

        #region [Public Methods]
        [MenuItem(MenuPath)]
        public static void Build()
        {
            AssetDatabase.ImportAsset(TextureAssetPath, ImportAssetOptions.ForceUpdate);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialAssetPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Custom/World/DirectionHint");
                if (shader == null)
                {
                    Debug.LogError("[DirectionHint] Shader Custom/World/DirectionHint not found.");
                    return;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(MaterialAssetPath));
                material = new Material(shader) { name = "DirectionHint" };
                AssetDatabase.CreateAsset(material, MaterialAssetPath);
            }
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(TextureAssetPath));
            EditorUtility.SetDirty(material);

            DirectionHintSettings settings = AssetDatabase.LoadAssetAtPath<DirectionHintSettings>(SettingsAssetPath);
            if (settings == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsAssetPath));
                settings = ScriptableObject.CreateInstance<DirectionHintSettings>();
                AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            }
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("_material").objectReferenceValue = material;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            Debug.Log($"[DirectionHint] Assets ready: {MaterialAssetPath}, {SettingsAssetPath}");
        }

        public static void BuildBatch()
        {
            Build();
        }
        #endregion

        #region [Private Methods]
        private void OnPreprocessTexture()
        {
            if (assetPath != TextureAssetPath) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
        }
        #endregion
    }
}
