using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public class CharacterSheetSession
    {
        #region [Variables/Fields]
        public const string PreviewLayerName = "_Ref_Preview";
        public const float MinScale = 0.25f;
        public const float MaxScale = 2.5f;
        private const int FirstLayerId = 1001;
        private const byte CoverAlpha = 128;
        private const string HeadPartName = "Head";
        private static readonly string[][] MirrorPairs = { new[] { "ArmL", "ArmR" }, new[] { "LegL", "LegR" } };

        private readonly CharacterAssetSettings _settings;
        private readonly Dictionary<string, byte[]> _parts = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, Vector2> _offsets = new Dictionary<string, Vector2>();
        private readonly Dictionary<string, float> _scales = new Dictionary<string, float>();
        private readonly Dictionary<string, Texture2D> _partTextures = new Dictionary<string, Texture2D>();
        private List<CharacterSheetCell> _cells = new List<CharacterSheetCell>();
        private Texture2D _sheetTexture;
        private Texture2D _compositeTexture;
        private bool _compositeDirty = true;
        #endregion

        #region [Properties]
        public string SourcePath { get; private set; }
        public byte[] SheetPixels { get; private set; }
        public int SheetWidth { get; private set; }
        public int SheetHeight { get; private set; }
        public CharacterSheetLayout Layout { get; private set; }
        public bool FoundGridLines { get; private set; }
        public int CanvasSize { get; }
        public bool IsLoaded => SheetPixels != null;
        public IReadOnlyList<CharacterSheetCell> Cells => _cells;
        public Texture2D SheetTexture => _sheetTexture;
        public float CenterX => _settings.FindPart(CharacterAssetSettings.BodyPartName)?.Joint.x ?? CanvasSize * 0.5f;
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public CharacterSheetSession(CharacterAssetSettings settings)
        {
            _settings = settings;
            CanvasSize = settings.CanvasSize.x;
        }

        public void Load(string path, CharacterSheetLayout layout)
        {
            Dispose();
            ResetAll();
            SourcePath = path;
            SheetPixels = CharacterSheetConverter.LoadImageFile(path, out int width, out int height);
            SheetWidth = width;
            SheetHeight = height;
            _sheetTexture = CharacterSheetConverter.CreateTexture(SheetPixels, width, height);
            Slice(layout);
        }

        public void Slice(CharacterSheetLayout layout)
        {
            if (!IsLoaded) return;
            ReleasePartTextures();
            _parts.Clear();
            Layout = CharacterSheetSlicer.ResolveLayout(layout, SheetWidth, SheetHeight);
            _cells = CharacterSheetSlicer.DetectCells(SheetPixels, SheetWidth, SheetHeight, Layout, out bool lines);
            FoundGridLines = lines;
            foreach (CharacterSheetCell cell in _cells)
                _parts[cell.Name] = CharacterSheetSlicer.ExtractPart(SheetPixels, SheetWidth, cell, CanvasSize);
            _compositeDirty = true;
        }

        public bool HasPart(string name)
        {
            return _parts.TryGetValue(name, out byte[] pixels) && !CharacterSheetSlicer.GetOpaqueBounds(pixels, CanvasSize, CanvasSize, 0).IsEmpty;
        }

        public Vector2 GetOffset(string name)
        {
            return _offsets.TryGetValue(name, out Vector2 offset) ? offset : Vector2.zero;
        }

        public void SetOffset(string name, Vector2 offset)
        {
            if (GetOffset(name) == offset) return;
            _offsets[name] = offset;
            _compositeDirty = true;
        }

        public float GetScale(string name)
        {
            return _scales.TryGetValue(name, out float scale) ? scale : 1f;
        }

        public void SetScale(string name, float scale)
        {
            scale = Mathf.Clamp(scale, MinScale, MaxScale);
            if (Mathf.Approximately(GetScale(name), scale)) return;
            _scales[name] = scale;
            _compositeDirty = true;
        }

        public void SetScaleForAllParts(float scale)
        {
            foreach (CharacterPartSpec part in _settings.Parts)
                SetScale(part.Name, scale);
        }

        public void ResetTransform(string name)
        {
            _offsets.Remove(name);
            _scales.Remove(name);
            _compositeDirty = true;
        }

        public void ResetAll()
        {
            _offsets.Clear();
            _scales.Clear();
            _compositeDirty = true;
        }

        public Vector2 PivotOf(string name)
        {
            CharacterPartSpec part = _settings.FindPart(name == CharacterSheetSlicer.PreviewCellName ? HeadPartName : name);
            return part != null ? JointOnCanvas(part) : new Vector2(CenterX, CanvasSize * 0.5f);
        }

        public Vector2 JointOnCanvas(CharacterPartSpec part)
        {
            return new Vector2(part.Joint.x, CanvasSize - part.Joint.y);
        }

        public Vector2 ToCanvas(string name, Vector2 partPoint)
        {
            Vector2 pivot = PivotOf(name);
            return pivot + (partPoint + GetOffset(name) - pivot) * GetScale(name);
        }

        public Vector2 ToPart(string name, Vector2 canvasPoint)
        {
            Vector2 pivot = PivotOf(name);
            return (canvasPoint - pivot) / GetScale(name) + pivot - GetOffset(name);
        }

        public Rect GetCanvasRect(string name)
        {
            Vector2 origin = ToCanvas(name, Vector2.zero);
            float size = CanvasSize * GetScale(name);
            return new Rect(origin, new Vector2(size, size));
        }

        public void MirrorPart(string name)
        {
            if (!_parts.TryGetValue(name, out byte[] pixels)) return;
            float axis = ToPart(name, PivotOf(name)).x;
            _parts[name] = MirrorCanvas(pixels, axis);
            ReleasePartTexture(name);
            _compositeDirty = true;
        }

        public void MirrorWholeCharacter()
        {
            foreach (string name in _parts.Keys.ToList())
            {
                _parts[name] = MirrorCanvas(_parts[name], CenterX);
                ReleasePartTexture(name);
            }
            foreach (string[] pair in MirrorPairs)
            {
                if (!_parts.ContainsKey(pair[0]) || !_parts.ContainsKey(pair[1])) continue;
                byte[] left = _parts[pair[0]];
                _parts[pair[0]] = _parts[pair[1]];
                _parts[pair[1]] = left;
            }
            ResetAll();
        }

        public void SnapToReferenceBox(CharacterPartSpec part)
        {
            if (!_parts.TryGetValue(part.Name, out byte[] pixels)) return;
            CharacterPixelBounds bounds = CharacterSheetSlicer.GetOpaqueBounds(pixels, CanvasSize, CanvasSize, 8);
            if (bounds.IsEmpty) return;
            RectInt box = part.ReferenceBox;
            Vector2 boxCenter = new Vector2(box.x + box.width * 0.5f, CanvasSize - (box.y + box.height * 0.5f));
            Vector2 partCenter = new Vector2((bounds.Left + bounds.Right) * 0.5f, (bounds.Top + bounds.Bottom) * 0.5f);
            Vector2 pivot = PivotOf(part.Name);
            SetOffset(part.Name, (boxCenter - pivot) / GetScale(part.Name) + pivot - partCenter);
        }

        public bool CoversPoint(string partName, Vector2 canvasPoint)
        {
            if (!_parts.TryGetValue(partName, out byte[] pixels)) return false;
            Vector2 local = ToPart(partName, canvasPoint);
            return CharacterSheetSlicer.MaxAlphaAround(pixels, CanvasSize, CanvasSize, Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y), 1) >= CoverAlpha;
        }

        public bool CoversJoint(string partName, CharacterPartSpec joint)
        {
            return CoversPoint(partName, JointOnCanvas(joint));
        }

        public List<CharacterCheckResult> Check()
        {
            List<CharacterCheckResult> results = new List<CharacterCheckResult>();
            if (!IsLoaded) return results;
            if (!FoundGridLines)
                results.Add(new CharacterCheckResult(CharacterCheckSeverity.Info, "No magenta grid lines found; the sheet was split into equal cells."));
            foreach (CharacterPartSpec part in _settings.Parts)
            {
                if (!HasPart(part.Name))
                {
                    results.Add(new CharacterCheckResult(CharacterCheckSeverity.Error, $"Cell '{part.Name}' is empty. Check the layout ({Layout}) and the cell order."));
                    continue;
                }
                if (!CoversJoint(part.Name, part))
                    results.Add(new CharacterCheckResult(CharacterCheckSeverity.Error, $"'{part.Name}' does not cover its joint. Drag it until the magenta cross sits on its rotation point."));
                if (part.BodyCoversJoint && HasPart(CharacterAssetSettings.BodyPartName) && !CoversJoint(CharacterAssetSettings.BodyPartName, part))
                    results.Add(new CharacterCheckResult(CharacterCheckSeverity.Warning, $"'Body' does not cover the {part.Name} joint; a gap can open when it rotates."));
            }
            if (results.All(r => r.Severity < CharacterCheckSeverity.Warning))
                results.Add(new CharacterCheckResult(CharacterCheckSeverity.Pass, "Every part is present and covers its joint."));
            return results;
        }

        public List<CharacterPsdLayer> BuildLayers(bool includePreview)
        {
            List<CharacterPsdLayer> layers = new List<CharacterPsdLayer>();
            if (includePreview && _parts.ContainsKey(CharacterSheetSlicer.PreviewCellName))
                AddLayer(layers, CharacterSheetSlicer.PreviewCellName, PreviewLayerName, false);
            foreach (CharacterPartSpec part in _settings.Parts.OrderBy(p => p.SortingOrder))
                AddLayer(layers, part.Name, part.Name, true);
            for (int i = 0; i < layers.Count; i++)
                layers[i].LayerId = FirstLayerId + i;
            return layers;
        }

        public Texture2D GetPartTexture(string name)
        {
            if (_partTextures.TryGetValue(name, out Texture2D texture) && texture != null) return texture;
            if (!_parts.TryGetValue(name, out byte[] pixels)) return null;
            texture = CharacterSheetConverter.CreateTexture(pixels, CanvasSize, CanvasSize);
            _partTextures[name] = texture;
            return texture;
        }

        public Texture2D GetCompositeTexture()
        {
            if (!_compositeDirty && _compositeTexture != null) return _compositeTexture;
            if (_compositeTexture != null) Object.DestroyImmediate(_compositeTexture);
            byte[] composite = CharacterPsdWriter.Composite(CanvasSize, CanvasSize, BuildLayers(false));
            _compositeTexture = CharacterSheetConverter.CreateTexture(composite, CanvasSize, CanvasSize);
            _compositeDirty = false;
            return _compositeTexture;
        }

        public void Dispose()
        {
            ReleasePartTextures();
            if (_sheetTexture != null) Object.DestroyImmediate(_sheetTexture);
            _sheetTexture = null;
            _parts.Clear();
            _cells.Clear();
            SheetPixels = null;
        }
        #endregion

        #region [Private Methods]
        private void AddLayer(List<CharacterPsdLayer> layers, string partName, string layerName, bool visible)
        {
            if (!_parts.TryGetValue(partName, out byte[] pixels)) return;
            CharacterPixelBounds bounds = CharacterSheetSlicer.GetOpaqueBounds(pixels, CanvasSize, CanvasSize, 0);
            if (bounds.IsEmpty) return;
            float scale = GetScale(partName);
            byte[] cropped = CharacterSheetSlicer.Crop(pixels, CanvasSize, bounds);
            int width = Mathf.Max(1, Mathf.RoundToInt(bounds.Width * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(bounds.Height * scale));
            byte[] scaled = width == bounds.Width && height == bounds.Height ? cropped : CharacterSheetSlicer.Resample(cropped, bounds.Width, bounds.Height, width, height);
            Vector2 topLeft = ToCanvas(partName, new Vector2(bounds.Left, bounds.Top));
            int left = Mathf.RoundToInt(topLeft.x);
            int top = Mathf.RoundToInt(topLeft.y);
            int clipLeft = Mathf.Max(0, -left);
            int clipTop = Mathf.Max(0, -top);
            int clipRight = Mathf.Min(width, CanvasSize - left);
            int clipBottom = Mathf.Min(height, CanvasSize - top);
            if (clipRight <= clipLeft || clipBottom <= clipTop) return;
            CharacterPixelBounds visibleBounds = new CharacterPixelBounds(clipLeft, clipTop, clipRight, clipBottom);
            layers.Add(new CharacterPsdLayer
            {
                Name = layerName,
                Rgba = CharacterSheetSlicer.Crop(scaled, width, visibleBounds),
                Left = left + clipLeft,
                Top = top + clipTop,
                Width = visibleBounds.Width,
                Height = visibleBounds.Height,
                Visible = visible,
            });
        }

        private byte[] MirrorCanvas(byte[] pixels, float axisX)
        {
            byte[] result = new byte[pixels.Length];
            for (int y = 0; y < CanvasSize; y++)
                for (int x = 0; x < CanvasSize; x++)
                {
                    int source = Mathf.RoundToInt(2f * axisX - x - 1f);
                    if (source < 0 || source >= CanvasSize) continue;
                    System.Buffer.BlockCopy(pixels, (y * CanvasSize + source) * 4, result, (y * CanvasSize + x) * 4, 4);
                }
            return result;
        }

        private void ReleasePartTexture(string name)
        {
            if (_partTextures.TryGetValue(name, out Texture2D texture) && texture != null) Object.DestroyImmediate(texture);
            _partTextures.Remove(name);
        }

        private void ReleasePartTextures()
        {
            foreach (Texture2D texture in _partTextures.Values)
                if (texture != null) Object.DestroyImmediate(texture);
            _partTextures.Clear();
            if (_compositeTexture != null) Object.DestroyImmediate(_compositeTexture);
            _compositeTexture = null;
            _compositeDirty = true;
        }
        #endregion
    }
}
