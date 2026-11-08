using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPartExporter
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static string GetExportFolder(string assetPath, CharacterAssetSettings settings)
        {
            return $"{settings.PartExportFolder}/{Path.GetFileNameWithoutExtension(assetPath)}";
        }

        public static List<string> Export(string assetPath, CharacterAssetSettings settings, List<string> log)
        {
            List<string> written = new List<string>();
            CharacterPsbData data = CharacterPsbLoader.Load(assetPath);
            Texture2D atlas = CharacterPsbLoader.LoadAtlas(assetPath);
            if (data == null || atlas == null)
            {
                log.Add("Nothing exported: the file is not an imported PSB.");
                return written;
            }
            string folder = GetExportFolder(assetPath, settings);
            Directory.CreateDirectory(folder);
            Dictionary<string, Vector2> pivots = new Dictionary<string, Vector2>();
            Texture2D pixels = CharacterPsbLoader.ReadAtlasPixels(atlas);
            try
            {
                foreach (CharacterPartSpec part in settings.Parts)
                {
                    CharacterSpriteInfo sprite = data.FindSprite(part.Name);
                    if (sprite == null)
                    {
                        log.Add($"Skipped '{part.Name}': no sprite with that name.");
                        continue;
                    }
                    string path = $"{folder}/{part.Name}.png";
                    File.WriteAllBytes(path, EncodePart(pixels, sprite.Rect));
                    pivots[path] = CharacterPivotFormula.AnchorPivot(part.Anchor, sprite.CanvasPosition, sprite.Rect.size);
                    written.Add(path);
                }
            }
            finally
            {
                Object.DestroyImmediate(pixels);
            }
            AssetDatabase.Refresh();
            foreach (KeyValuePair<string, Vector2> entry in pivots)
                ConfigureSprite(entry.Key, entry.Value, settings);
            log.Add($"Exported {written.Count} part(s) to {folder} as single sprites with anchor pivots.");
            return written;
        }
        #endregion

        #region [Private Methods]
        private static byte[] EncodePart(Texture2D pixels, Rect rect)
        {
            int x = Mathf.RoundToInt(rect.x);
            int y = Mathf.RoundToInt(rect.y);
            int width = Mathf.Min(Mathf.RoundToInt(rect.width), pixels.width - x);
            int height = Mathf.Min(Mathf.RoundToInt(rect.height), pixels.height - y);
            Texture2D part = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                part.SetPixels(pixels.GetPixels(x, y, width, height));
                part.Apply();
                return part.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(part);
            }
        }

        private static void ConfigureSprite(string path, Vector2 pivot, CharacterAssetSettings settings)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = settings.MipmapEnabled;
            importer.filterMode = settings.TextureFilterMode;
            importer.textureCompression = settings.Compression;
            TextureImporterSettings textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spritePixelsPerUnit = settings.PixelsPerUnit;
            textureSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            textureSettings.spritePivot = pivot;
            textureSettings.spriteMeshType = settings.MeshType;
            importer.SetTextureSettings(textureSettings);
            importer.SaveAndReimport();
        }
        #endregion
    }
}
