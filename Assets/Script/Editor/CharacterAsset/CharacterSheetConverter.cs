using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterSheetConverter
    {
        #region [Variables/Fields]
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static byte[] LoadImageFile(string path, out int width, out int height)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                    throw new IOException($"Could not read the image {path}. Use a PNG (or JPG) file.");
                width = texture.width;
                height = texture.height;
                Color32[] pixels = texture.GetPixels32();
                byte[] topDown = new byte[width * height * 4];
                for (int y = 0; y < height; y++)
                {
                    int sourceRow = (height - 1 - y) * width;
                    for (int x = 0; x < width; x++)
                    {
                        Color32 c = pixels[sourceRow + x];
                        int o = (y * width + x) * 4;
                        topDown[o] = c.r;
                        topDown[o + 1] = c.g;
                        topDown[o + 2] = c.b;
                        topDown[o + 3] = c.a;
                    }
                }
                return topDown;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        public static Texture2D CreateTexture(byte[] rgbaTopDown, int width, int height)
        {
            byte[] bottomUp = new byte[rgbaTopDown.Length];
            int row = width * 4;
            for (int y = 0; y < height; y++)
                System.Buffer.BlockCopy(rgbaTopDown, y * row, bottomUp, (height - 1 - y) * row, row);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.LoadRawTextureData(bottomUp);
            texture.Apply(false, false);
            return texture;
        }

        public static string GetPsbPath(CharacterAssetSettings settings, string characterName)
        {
            return $"{settings.PsbFolder}/{characterName}.psb";
        }

        public static string Build(CharacterSheetSession session, string characterName, CharacterAssetSettings settings, bool createPrefab, string basePrefabPath, List<string> log)
        {
            string psbPath = GetPsbPath(settings, characterName);
            if (File.Exists(Path.GetFullPath(psbPath)))
            {
                log.Add($"{psbPath} already exists. Pick another name; existing PSBs are never overwritten (their sprite IDs are used by prefabs).");
                return null;
            }
            List<CharacterPsdLayer> layers = session.BuildLayers(true);
            Directory.CreateDirectory(Path.GetFullPath(settings.PsbFolder));
            File.WriteAllBytes(Path.GetFullPath(psbPath), CharacterPsdWriter.Write(session.CanvasSize, session.CanvasSize, layers));
            AssetDatabase.ImportAsset(psbPath, ImportAssetOptions.ForceUpdate);
            log.Add($"Wrote {psbPath} ({layers.Count} layers, preview cell kept as hidden '{CharacterSheetSession.PreviewLayerName}').");
            CharacterPsbStandardizer.ApplyStandard(psbPath, settings, log);
            if (createPrefab && !string.IsNullOrEmpty(basePrefabPath))
            {
                string prefabPath = CharacterPrefabGenerator.GetDefaultOutputPath(psbPath, basePrefabPath);
                CharacterPrefabGenerator.Create(psbPath, basePrefabPath, prefabPath, settings, log);
            }
            return psbPath;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
