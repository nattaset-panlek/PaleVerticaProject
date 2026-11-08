using System.Collections.Generic;
using CoreSystem.UI.Effects;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.UI
{
    public static class FogAuraMaterialBuilder
    {
        #region [Variables/Fields]
        public const string Folder = "Assets/Resources/UI/FogAura";
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Create Fog Aura Materials")]
        public static void CreateFromMenu()
        {
            var created = CreateMissing();
            Debug.Log(created.Count > 0 ? $"[FogAura] Created: {string.Join(", ", created)}" : "[FogAura] All Fog Aura materials already exist; nothing changed.");
        }

        public static void CreateMissingBatch()
        {
            var created = CreateMissing();
            Debug.Log($"[FogAura] Batch created {created.Count} material(s): {string.Join(", ", created)}");
        }

        public static List<string> CreateMissing()
        {
            var created = new List<string>();
            var shader = Shader.Find(FogAuraView.ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[FogAura] Shader \"{FogAuraView.ShaderName}\" was not found.");
                return created;
            }

            EnsureFolder(Folder);
            foreach (var palette in new[] { FogAuraPalette.Ink, FogAuraPalette.Crimson, FogAuraPalette.Pale })
            {
                string path = $"{Folder}/FogAura_{palette.Name}.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) continue;
                var material = new Material(shader) { name = $"FogAura_{palette.Name}" };
                palette.ApplyTo(material);
                AssetDatabase.CreateAsset(material, path);
                created.Add(path);
            }
            AssetDatabase.SaveAssets();
            return created;
        }
        #endregion

        #region [Private Methods]
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
        #endregion
    }
}
