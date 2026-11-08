using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.PSD;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPsbStandardizer
    {
        #region [Variables/Fields]
        private const string DefaultPlatformName = "DefaultTexturePlatform";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static void ApplyStandard(string assetPath, CharacterAssetSettings settings, List<string> log)
        {
            ApplyImporterSettings(assetPath, settings, log);
            ApplyAnchorPivots(assetPath, settings, log);
        }

        public static void RebuildAtlas(string assetPath, CharacterAssetSettings settings, List<string> log)
        {
            PSDImporter importer = CharacterPsbLoader.GetImporter(assetPath);
            if (importer == null) return;
            bool keepAnchors = CharacterPsbValidator.UsesAnchorPivots(CharacterPsbLoader.Load(assetPath), settings);
            if (importer.mosiacPadding < settings.MosaicPadding) importer.mosiacPadding = (uint)settings.MosaicPadding;
            SerializedObject serialized = new SerializedObject(importer);
            serialized.FindProperty("m_ResliceFromLayer").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
            log.Add($"Atlas rebuilt from the layers (Automatic Reslice on, padding {importer.mosiacPadding} px).");
            if (keepAnchors) ApplyAnchorPivots(assetPath, settings, log);
        }

        public static void ApplyImporterSettings(string assetPath, CharacterAssetSettings settings, List<string> log)
        {
            PSDImporter importer = CharacterPsbLoader.GetImporter(assetPath);
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.useMosaicMode = true;
            importer.useCharacterMode = true;
            importer.spritePixelsPerUnit = settings.PixelsPerUnit;
            importer.mosiacPadding = (uint)settings.MosaicPadding;
            importer.filterMode = settings.TextureFilterMode;
            importer.mipmapEnabled = settings.MipmapEnabled;
            importer.spriteMeshType = settings.MeshType;
            SerializedObject serialized = new SerializedObject(importer);
            serialized.FindProperty("m_DocumentAlignment").intValue = (int)settings.DocumentAlignment;
            serialized.FindProperty("m_ImportHiddenLayers").boolValue = false;
            serialized.FindProperty("m_ResliceFromLayer").boolValue = settings.AutomaticReslice;
            WriteDefaultCompression(serialized, settings.Compression);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
            log.Add("Importer settings applied (PPU, mosaic, padding, filter, mipmaps, mesh, compression, document pivot, reslice).");
        }

        public static int ApplyAnchorPivots(string assetPath, CharacterAssetSettings settings, List<string> log)
        {
            PSDImporter importer = CharacterPsbLoader.GetImporter(assetPath);
            CharacterPsbData data = CharacterPsbLoader.Load(assetPath);
            if (importer == null || data == null) return 0;
            SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null)
            {
                log.Add("No sprite data provider for this importer; pivots were not changed.");
                return 0;
            }
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] rects = provider.GetSpriteRects();
            int changed = 0;
            foreach (SpriteRect rect in rects)
            {
                CharacterPartSpec part = settings.FindPart(rect.name);
                CharacterSpriteInfo sprite = data.Sprites.Find(s => s.SpriteId == rect.spriteID.ToString());
                if (part == null || sprite == null) continue;
                Vector2 pivot = CharacterPivotFormula.AnchorPivot(part.Anchor, sprite.CanvasPosition, rect.rect.size);
                if (rect.alignment == SpriteAlignment.Custom && (rect.pivot - pivot).sqrMagnitude < 1e-10f) continue;
                rect.alignment = SpriteAlignment.Custom;
                rect.pivot = pivot;
                changed++;
            }
            if (changed > 0)
            {
                provider.SetSpriteRects(rects);
                provider.Apply();
                importer.SaveAndReimport();
            }
            log.Add(changed > 0 ? $"Anchor pivots set on {changed} part(s)." : "Anchor pivots already correct.");
            return changed;
        }
        #endregion

        #region [Private Methods]
        private static void WriteDefaultCompression(SerializedObject serialized, TextureImporterCompression compression)
        {
            SerializedProperty platforms = serialized.FindProperty("m_PlatformSettings");
            if (platforms == null) return;
            for (int i = 0; i < platforms.arraySize; i++)
            {
                SerializedProperty platform = platforms.GetArrayElementAtIndex(i);
                if (platform.FindPropertyRelative("m_Name").stringValue != DefaultPlatformName) continue;
                platform.FindPropertyRelative("m_TextureCompression").intValue = (int)compression;
                return;
            }
        }
        #endregion
    }
}
