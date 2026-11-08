using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.PSD;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public static class CharacterPsbValidator
    {
        #region [Variables/Fields]
        private const string DefaultPlatformName = "DefaultTexturePlatform";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public static List<CharacterCheckResult> Validate(string assetPath, CharacterAssetSettings settings)
        {
            List<CharacterCheckResult> results = new List<CharacterCheckResult>();
            PSDImporter importer = CharacterPsbLoader.GetImporter(assetPath);
            if (importer == null)
            {
                results.Add(Error($"{assetPath} is not imported with the PSD Importer. Use a .psb file."));
                return results;
            }
            CharacterPsbData data = CharacterPsbLoader.Load(assetPath);
            CheckFile(data, settings, results);
            CheckLayers(data, settings, results);
            CheckImporter(importer, settings, results);
            CheckPivots(data, settings, results);
            CheckJoints(data, settings, results);
            CheckGround(data, settings, results);
            return results;
        }

        public static CharacterCheckSeverity WorstSeverity(IEnumerable<CharacterCheckResult> results)
        {
            CharacterCheckSeverity worst = CharacterCheckSeverity.Pass;
            foreach (CharacterCheckResult result in results)
                if (result.Severity > worst) worst = result.Severity;
            return worst;
        }

        public static bool UsesAnchorPivots(CharacterPsbData data, CharacterAssetSettings settings)
        {
            if (data == null) return false;
            foreach (CharacterPartSpec part in settings.Parts)
            {
                CharacterSpriteInfo sprite = data.FindSprite(part.Name);
                if (sprite != null && sprite.Alignment == SpriteAlignment.Custom) return true;
            }
            return false;
        }
        #endregion

        #region [Private Methods]
        private static void CheckFile(CharacterPsbData data, CharacterAssetSettings settings, List<CharacterCheckResult> results)
        {
            if (!data.AssetPath.EndsWith(".psb"))
                results.Add(Warning("File extension is not .psb. Save or rename the file as .psb so the PSD Importer picks it up by default."));
            if (data.CanvasSize == Vector2Int.zero)
                results.Add(Error("Could not read the PSB header."));
            else if (data.CanvasSize.x != settings.CanvasSize.x || data.CanvasSize.y < settings.CanvasSize.y || data.CanvasSize.y > settings.MaxCanvasHeight)
                results.Add(Error($"Canvas is {data.CanvasSize.x} x {data.CanvasSize.y} px; the standard is {settings.CanvasSize.x} x {settings.CanvasSize.y} (up to {settings.MaxCanvasHeight} tall, extra room added above only). Resize the canvas (not the image) and keep the parts on the template positions."));
            else if (data.CanvasSize.y > settings.CanvasSize.y)
                results.Add(Pass($"Canvas {data.CanvasSize.x} x {data.CanvasSize.y} px: {data.CanvasSize.y - settings.CanvasSize.y} px of headroom above the standard {settings.CanvasSize.y} (file version {data.FileVersion})."));
            else
                results.Add(Pass($"Canvas {data.CanvasSize.x} x {data.CanvasSize.y} px (file version {data.FileVersion})."));
        }

        private static void CheckLayers(CharacterPsbData data, CharacterAssetSettings settings, List<CharacterCheckResult> results)
        {
            List<CharacterLayerInfo> layers = data.Layers;
            foreach (CharacterPartSpec part in settings.Parts)
            {
                List<CharacterLayerInfo> exact = layers.Where(l => !l.IsGroup && l.Name == part.Name).ToList();
                if (exact.Count == 0)
                {
                    CharacterLayerInfo wrongCase = layers.FirstOrDefault(l => !l.IsGroup && string.Equals(l.Name, part.Name, System.StringComparison.OrdinalIgnoreCase));
                    results.Add(Error(wrongCase != null
                        ? $"Layer '{wrongCase.Name}' must be named exactly '{part.Name}'."
                        : $"Missing layer '{part.Name}'."));
                    continue;
                }
                if (exact.Count > 1)
                    results.Add(Error($"Layer name '{part.Name}' is used {exact.Count} times. Part names must be unique in the whole file."));
                CharacterLayerInfo layer = exact[0];
                if (layer.ParentIndex != -1)
                    results.Add(Error($"Layer '{part.Name}' is inside group '{layers[layer.ParentIndex].Name}'. Move every part to the root of the file."));
                if (!layer.IsVisible || !layer.IsImported)
                    results.Add(Error($"Layer '{part.Name}' is hidden or not imported. Every part must be visible."));
            }

            foreach (CharacterLayerInfo layer in layers)
            {
                if (settings.FindPart(layer.Name) != null && !layer.IsGroup) continue;
                if (layer.IsImported)
                    results.Add(Error($"Extra layer '{layer.Name}' is imported as a sprite. Hide it and prefix its name with '{settings.IgnoredLayerPrefix}'."));
                else if (layer.ParentIndex == -1 && !settings.IsIgnoredName(layer.Name))
                    results.Add(Info($"Helper layer '{layer.Name}' is not imported. Prefix it with '{settings.IgnoredLayerPrefix}' so its purpose is clear."));
            }

            List<string> rootPartOrder = layers.Where(l => !l.IsGroup && l.ParentIndex == -1 && settings.FindPart(l.Name) != null).Select(l => l.Name).ToList();
            List<string> expectedOrder = settings.Parts.OrderByDescending(p => p.SortingOrder).Select(p => p.Name).Where(rootPartOrder.Contains).ToList();
            if (rootPartOrder.SequenceEqual(expectedOrder))
                results.Add(Pass($"Layers and order: {string.Join(", ", rootPartOrder)}."));
            else
                results.Add(Warning($"Layer order is {string.Join(", ", rootPartOrder)}; the in-game order (top to bottom) is {string.Join(", ", expectedOrder)}. Reorder so the PSB preview matches the game."));
        }

        private static void CheckImporter(PSDImporter importer, CharacterAssetSettings settings, List<CharacterCheckResult> results)
        {
            int before = results.Count;
            SerializedObject serialized = new SerializedObject(importer);
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, settings.PixelsPerUnit))
                results.Add(Error($"Pixels Per Unit is {importer.spritePixelsPerUnit}; the standard is {settings.PixelsPerUnit}."));
            if (importer.spriteImportMode != SpriteImportMode.Multiple || !importer.useMosaicMode)
                results.Add(Error("Sprite Mode must be Multiple with Mosaic on, so every layer becomes its own sprite."));
            if (importer.mosiacPadding < settings.MosaicPadding)
                results.Add(Warning($"Mosaic padding is {importer.mosiacPadding} px; use {settings.MosaicPadding} px so mipmaps do not bleed neighbouring parts into each other."));
            if (!serialized.FindProperty("m_ResliceFromLayer").boolValue)
                results.Add(Warning("Automatic Reslice is off: sprite rects keep their old size when a layer is redrawn, which crops or shifts parts (the atlas distortion). Use Rebuild Atlas."));
            if (importer.filterMode != settings.TextureFilterMode)
                results.Add(Warning($"Filter Mode is {importer.filterMode}; the standard is {settings.TextureFilterMode}."));
            if (importer.mipmapEnabled != settings.MipmapEnabled)
                results.Add(Warning($"Generate Mip Maps is {importer.mipmapEnabled}; the standard is {settings.MipmapEnabled}."));
            if (importer.spriteMeshType != settings.MeshType)
                results.Add(Warning($"Mesh Type is {importer.spriteMeshType}; the standard is {settings.MeshType}."));
            if (serialized.FindProperty("m_DocumentAlignment").intValue != (int)settings.DocumentAlignment)
                results.Add(Warning($"Document pivot must be {settings.DocumentAlignment}."));
            if (serialized.FindProperty("m_ImportHiddenLayers").boolValue)
                results.Add(Error("Include Hidden Layers is on: guide and reference layers would become sprites."));
            int compression = ReadDefaultCompression(serialized);
            if (compression >= 0 && compression != (int)settings.Compression)
                results.Add(Warning($"Default platform compression is {(TextureImporterCompression)compression}; the standard is {settings.Compression}."));
            if (results.Count == before)
                results.Add(Pass("Importer settings match the standard."));
        }

        private static void CheckPivots(CharacterPsbData data, CharacterAssetSettings settings, List<CharacterCheckResult> results)
        {
            List<string> off = new List<string>();
            foreach (CharacterPartSpec part in settings.Parts)
            {
                CharacterSpriteInfo sprite = data.FindSprite(part.Name);
                if (sprite == null) continue;
                float distance = Vector2.Distance(CharacterPivotFormula.PivotOnCanvas(sprite), part.Anchor);
                if (distance > settings.AnchorTolerancePx) off.Add($"{part.Name} {distance:0.#} px");
            }
            if (off.Count == 0)
                results.Add(Pass("Every part pivot sits on its rig anchor."));
            else
                results.Add(Warning($"Pivots are off their rig anchors ({string.Join(", ", off)}). In the shared prefab rig these parts will be shifted by that much. Use Apply Standard."));
        }

        private static void CheckJoints(CharacterPsbData data, CharacterAssetSettings settings, List<CharacterCheckResult> results)
        {
            Texture2D atlas = CharacterPsbLoader.LoadAtlas(data.AssetPath);
            if (atlas == null)
            {
                results.Add(Info("Atlas texture not found; joint coverage was not checked. Reimport the file."));
                return;
            }
            Texture2D pixels = CharacterPsbLoader.ReadAtlasPixels(atlas);
            try
            {
                int before = results.Count;
                CharacterSpriteInfo body = data.FindSprite(CharacterAssetSettings.BodyPartName);
                foreach (CharacterPartSpec part in settings.Parts)
                {
                    CharacterSpriteInfo sprite = data.FindSprite(part.Name);
                    if (sprite == null) continue;
                    if (SampleAlpha(pixels, sprite, part.Joint) < settings.JointAlphaThreshold)
                        results.Add(Error($"'{part.Name}' has no opaque pixels on its joint ({part.Joint.x:0}, {part.Joint.y:0}). The part will detach when it rotates. Extend it over the joint (see the template)."));
                    if (part.BodyCoversJoint && body != null && SampleAlpha(pixels, body, part.Joint) < settings.JointAlphaThreshold)
                        results.Add(Warning($"'Body' does not cover the {part.Name} joint ({part.Joint.x:0}, {part.Joint.y:0}). A gap can open when {part.Name} rotates; let the body overlap it."));
                }
                if (results.Count == before)
                    results.Add(Pass("Every joint is covered by its part and by the body."));
            }
            finally
            {
                Object.DestroyImmediate(pixels);
            }
        }

        private static void CheckGround(CharacterPsbData data, CharacterAssetSettings settings, List<CharacterCheckResult> results)
        {
            List<CharacterSpriteInfo> legs = data.Sprites.Where(s => s.Name.StartsWith("Leg")).ToList();
            if (legs.Count == 0) return;
            float feet = legs.Min(s => s.CanvasPosition.y);
            if (Mathf.Abs(feet - settings.GroundY) > settings.GroundTolerance)
                results.Add(Warning($"Feet end at y = {feet:0}; the ground line is y = {settings.GroundY:0}. The shadow will float or sink."));
            else
                results.Add(Pass($"Feet at y = {feet:0} (ground {settings.GroundY:0})."));
        }

        private static float SampleAlpha(Texture2D pixels, CharacterSpriteInfo sprite, Vector2 canvasPoint)
        {
            if (!CharacterPivotFormula.TryCanvasToAtlas(canvasPoint, sprite, out Vector2Int pixel)) return 0f;
            float best = 0f;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = Mathf.Clamp(pixel.x + dx, (int)sprite.Rect.xMin, (int)sprite.Rect.xMax - 1);
                    int y = Mathf.Clamp(pixel.y + dy, (int)sprite.Rect.yMin, (int)sprite.Rect.yMax - 1);
                    best = Mathf.Max(best, pixels.GetPixel(x, y).a);
                }
            return best;
        }

        private static int ReadDefaultCompression(SerializedObject serialized)
        {
            SerializedProperty platforms = serialized.FindProperty("m_PlatformSettings");
            if (platforms == null) return -1;
            for (int i = 0; i < platforms.arraySize; i++)
            {
                SerializedProperty platform = platforms.GetArrayElementAtIndex(i);
                if (platform.FindPropertyRelative("m_Name").stringValue == DefaultPlatformName)
                    return platform.FindPropertyRelative("m_TextureCompression").intValue;
            }
            return -1;
        }

        private static CharacterCheckResult Pass(string message) => new CharacterCheckResult(CharacterCheckSeverity.Pass, message);
        private static CharacterCheckResult Info(string message) => new CharacterCheckResult(CharacterCheckSeverity.Info, message);
        private static CharacterCheckResult Warning(string message) => new CharacterCheckResult(CharacterCheckSeverity.Warning, message);
        private static CharacterCheckResult Error(string message) => new CharacterCheckResult(CharacterCheckSeverity.Error, message);
        #endregion
    }
}
