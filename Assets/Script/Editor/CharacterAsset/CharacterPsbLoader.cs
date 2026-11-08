using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.PSD;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPsbLoader
    {
        #region [Variables/Fields]
        private const string LayersProperty = "m_PsdLayers";
        private const string SpritesProperty = "m_LayeredSpriteImportData";
        private const int HeaderSize = 26;
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static PSDImporter GetImporter(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? null : AssetImporter.GetAtPath(assetPath) as PSDImporter;
        }

        public static bool IsPsdAsset(Object asset)
        {
            return asset != null && GetImporter(AssetDatabase.GetAssetPath(asset)) != null;
        }

        public static CharacterPsbData Load(string assetPath)
        {
            PSDImporter importer = GetImporter(assetPath);
            if (importer == null) return null;
            CharacterPsbData data = new CharacterPsbData { AssetPath = assetPath };
            ReadHeader(assetPath, data);
            SerializedObject serialized = new SerializedObject(importer);
            ReadLayers(serialized.FindProperty(LayersProperty), data);
            ReadSprites(serialized.FindProperty(SpritesProperty), data);
            return data;
        }

        public static Sprite[] LoadSprites(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();
        }

        public static Texture2D LoadAtlas(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Texture2D>().FirstOrDefault();
        }

        public static Texture2D ReadAtlasPixels(Texture2D atlas)
        {
            if (atlas == null) return null;
            RenderTexture target = RenderTexture.GetTemporary(atlas.width, atlas.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(atlas, target);
            RenderTexture.active = target;
            Texture2D copy = new Texture2D(atlas.width, atlas.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, atlas.width, atlas.height), 0, 0);
            copy.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            return copy;
        }
        #endregion

        #region [Private Methods]
        private static void ReadHeader(string assetPath, CharacterPsbData data)
        {
            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath)) return;
            byte[] header = new byte[HeaderSize];
            using (FileStream stream = File.OpenRead(fullPath))
            {
                if (stream.Read(header, 0, HeaderSize) < HeaderSize) return;
            }
            if (header[0] != (byte)'8' || header[1] != (byte)'B' || header[2] != (byte)'P' || header[3] != (byte)'S') return;
            data.FileVersion = (header[4] << 8) | header[5];
            data.CanvasSize = new Vector2Int(ReadInt32BigEndian(header, 18), ReadInt32BigEndian(header, 14));
        }

        private static int ReadInt32BigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        private static void ReadLayers(SerializedProperty layers, CharacterPsbData data)
        {
            if (layers == null) return;
            for (int i = 0; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                data.Layers.Add(new CharacterLayerInfo
                {
                    Name = layer.FindPropertyRelative("m_Name").stringValue,
                    IsGroup = layer.FindPropertyRelative("m_IsGroup").boolValue,
                    IsVisible = layer.FindPropertyRelative("m_IsVisible").boolValue,
                    IsImported = layer.FindPropertyRelative("m_IsImported").boolValue,
                    ParentIndex = layer.FindPropertyRelative("m_ParentIndex").intValue,
                    LayerId = layer.FindPropertyRelative("m_LayerID").intValue,
                    SpriteId = layer.FindPropertyRelative("m_SpriteID").stringValue,
                });
            }
        }

        private static void ReadSprites(SerializedProperty sprites, CharacterPsbData data)
        {
            if (sprites == null) return;
            for (int i = 0; i < sprites.arraySize; i++)
            {
                SerializedProperty sprite = sprites.GetArrayElementAtIndex(i);
                data.Sprites.Add(new CharacterSpriteInfo
                {
                    Name = sprite.FindPropertyRelative("m_Name").stringValue,
                    SpriteId = sprite.FindPropertyRelative("m_SpriteID").stringValue,
                    Rect = sprite.FindPropertyRelative("m_Rect").rectValue,
                    Pivot = sprite.FindPropertyRelative("m_Pivot").vector2Value,
                    Alignment = (SpriteAlignment)sprite.FindPropertyRelative("m_Alignment").intValue,
                    CanvasPosition = sprite.FindPropertyRelative("spritePosition").vector2Value,
                });
            }
        }
        #endregion
    }
}
