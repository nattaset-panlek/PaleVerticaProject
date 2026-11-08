using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPrefabGenerator
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static string GetDefaultOutputPath(string psbPath, string basePrefabPath)
        {
            string folder = Path.GetDirectoryName(basePrefabPath)?.Replace('\\', '/') ?? "Assets";
            return $"{folder}/{Path.GetFileNameWithoutExtension(psbPath)}.prefab";
        }

        public static string Create(string psbPath, string basePrefabPath, string outputPath, CharacterAssetSettings settings, List<string> log)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath) == null)
            {
                log.Add($"Base prefab not found: {basePrefabPath}");
                return null;
            }
            if (AssetDatabase.LoadAssetAtPath<Object>(outputPath) != null)
            {
                log.Add($"{outputPath} already exists. Pick another name; existing prefabs are never overwritten.");
                return null;
            }
            Sprite[] sprites = CharacterPsbLoader.LoadSprites(psbPath);
            if (sprites.Length == 0)
            {
                log.Add($"No sprites found in {psbPath}.");
                return null;
            }
            if (!AssetDatabase.CopyAsset(basePrefabPath, outputPath))
            {
                log.Add($"Could not copy {basePrefabPath} to {outputPath}.");
                return null;
            }
            GameObject root = PrefabUtility.LoadPrefabContents(outputPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(outputPath);
                foreach (CharacterPartSpec part in settings.Parts)
                    AssignPart(root.transform, part, sprites, log);
                ApplySortingGroups(root.transform, settings, log);
                PrefabUtility.SaveAsPrefabAsset(root, outputPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            log.Add($"Created {outputPath} from {Path.GetFileName(basePrefabPath)}. Set its stats and add it to Character.json in the Pale Vertica GameDataWorkbench.");
            return outputPath;
        }

        public static void ApplySortingGroups(Transform root, CharacterAssetSettings settings, List<string> log)
        {
            SetSortingGroup(root, settings.ModelPath, settings.ModelSortingOrder, false, log);
            SetSortingGroup(root, settings.ShadowPath, settings.ShadowSortingOrder, true, log);
        }
        #endregion

        #region [Private Methods]
        private static void AssignPart(Transform root, CharacterPartSpec part, Sprite[] sprites, List<string> log)
        {
            Transform node = root.Find(part.RendererPath);
            SpriteRenderer renderer = node != null ? node.GetComponent<SpriteRenderer>() : null;
            if (renderer == null)
            {
                log.Add($"Base prefab has no SpriteRenderer at '{part.RendererPath}'.");
                return;
            }
            Sprite sprite = sprites.FirstOrDefault(s => s.name == part.Name);
            if (sprite == null)
            {
                log.Add($"PSB has no sprite named '{part.Name}'.");
                return;
            }
            renderer.sprite = sprite;
            renderer.sortingOrder = part.SortingOrder;
        }

        private static void SetSortingGroup(Transform root, string path, int sortingOrder, bool sortAtRoot, List<string> log)
        {
            Transform node = root.Find(path);
            if (node == null)
            {
                log.Add($"Base prefab has no '{path}' for its SortingGroup.");
                return;
            }
            SortingGroup group = node.GetComponent<SortingGroup>();
            if (group == null) group = node.gameObject.AddComponent<SortingGroup>();
            group.sortingOrder = sortingOrder;
            group.sortAtRoot = sortAtRoot;
        }
        #endregion
    }
}
