using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PaleVertica.Editor
{
    public class SpriteSheetVFXWorkbench : EditorWindow
    {
        #region [Variables/Fields]
        private const string DefaultSavePath = "Assets/Image/VFX/VFX_Pure_Slash_Sheet.png";
        private const string DefaultMaterialPath = "Assets/Materials/Slash_VFX.mat";

        private DefaultAsset _folderAsset;
        private string _folderPath = "";
        private List<string> _frameFilePaths = new List<string>();
        private List<Texture2D> _loadedFrames = new List<Texture2D>();

        private int _columns = 4;
        private int _rows = 4;
        private bool _autoRows = true;
        private int _outputResolution = 1024;

        private float _rotation = 0f;
        private Vector2 _offset = Vector2.zero;
        private float _scale = 1f;
        private bool _flipX = false;
        private bool _flipY = false;

        private int _previewTab = 0;
        private int _selectedFrame = 0;
        private bool _isPlaying = true;
        private float _fps = 30f;
        private double _lastFrameTime = 0;
        private float _playbackFrame = 0f;

        private bool _showCrosshair = true;
        private string _targetSavePath = DefaultSavePath;
        private bool _autoAssignToMaterial = true;

        private Vector2 _scrollPosition;
        private Vector2 _filmstripScroll;
        private Texture2D _checkeredBackground;
        #endregion

        #region [Properties]
        public int FrameCount => _loadedFrames.Count;
        #endregion

        #region [Unity Methods]
        [MenuItem("Tools/VFX/Sprite Sheet Workbench")]
        public static void Open()
        {
            var window = GetWindow<SpriteSheetVFXWorkbench>("Sprite Sheet Workbench");
            window.minSize = new Vector2(780, 720);
            window.Show();
        }

        private void OnEnable()
        {
            CreateCheckeredBackground();
            EditorApplication.update += OnEditorUpdate;
            if (!string.IsNullOrEmpty(_folderPath) && Directory.Exists(_folderPath))
            {
                LoadFramesFromFolder(_folderPath);
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (_checkeredBackground != null)
            {
                DestroyImmediate(_checkeredBackground);
            }
        }

        private void OnEditorUpdate()
        {
            if (!_isPlaying || FrameCount == 0 || _previewTab != 0) return;

            double currentTime = EditorApplication.timeSinceStartup;
            double delta = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;

            _playbackFrame += (float)(delta * _fps);
            if (_playbackFrame >= FrameCount)
            {
                _playbackFrame = 0f;
            }

            _selectedFrame = Mathf.FloorToInt(_playbackFrame) % FrameCount;
            Repaint();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(360));
                DrawFolderSelectionSection();
                EditorGUILayout.Space(8);
                DrawGridLayoutSection();
                EditorGUILayout.Space(8);
                DrawGlobalTransformSection();
                EditorGUILayout.Space(8);
                DrawSaveSection();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(14);

                EditorGUILayout.BeginVertical();
                DrawPreviewSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region [Public Methods]
        public void SetFolder(string path)
        {
            _folderPath = path;
            LoadFramesFromFolder(path);
        }
        #endregion

        #region [Private Methods]
        private void CreateCheckeredBackground()
        {
            if (_checkeredBackground != null) return;
            _checkeredBackground = new Texture2D(32, 32);
            _checkeredBackground.filterMode = FilterMode.Point;
            Color c1 = new Color(0.12f, 0.12f, 0.15f, 1f);
            Color c2 = new Color(0.18f, 0.18f, 0.22f, 1f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool isEven = ((x / 16) + (y / 16)) % 2 == 0;
                    _checkeredBackground.SetPixel(x, y, isEven ? c1 : c2);
                }
            }
            _checkeredBackground.Apply();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Sprite Sheet VFX Workbench", EditorStyles.boldLabel);
            GUILayout.Label("Select a folder of sprite frames -> Set Grid Columns/Rows -> Rotate & Offset all frames -> Preview & Save.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawFolderSelectionSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Sprite Frames Folder", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder Asset", _folderAsset, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck() && _folderAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(_folderAsset);
                if (Directory.Exists(assetPath))
                {
                    _folderPath = assetPath;
                    LoadFramesFromFolder(_folderPath);
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Folder Path", _folderPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(70)))
            {
                string chosen = EditorUtility.OpenFolderPanel("Select Sprite Frames Folder", "Assets", "");
                if (!string.IsNullOrEmpty(chosen))
                {
                    if (chosen.StartsWith(Application.dataPath))
                    {
                        _folderPath = "Assets" + chosen.Substring(Application.dataPath.Length);
                        _folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_folderPath);
                    }
                    else
                    {
                        _folderPath = chosen;
                        _folderAsset = null;
                    }
                    LoadFramesFromFolder(_folderPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (FrameCount > 0)
            {
                EditorGUILayout.HelpBox($"Loaded {FrameCount} frames in natural order.", MessageType.Info);
                DrawFilmstrip();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a folder containing PNG/JPG sequence images.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawFilmstrip()
        {
            EditorGUILayout.LabelField("Frame Sequence Filmstrip", EditorStyles.miniBoldLabel);
            _filmstripScroll = EditorGUILayout.BeginScrollView(_filmstripScroll, GUILayout.Height(64));
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < _loadedFrames.Count; i++)
            {
                Texture2D tex = _loadedFrames[i];
                if (tex == null) continue;

                Rect r = GUILayoutUtility.GetRect(48, 48);
                bool isSelected = (i == _selectedFrame);

                if (isSelected)
                {
                    Handles.DrawSolidRectangleWithOutline(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), new Color(0.2f, 0.6f, 1f, 0.3f), Color.cyan);
                }

                GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(r.x, r.y + 32, r.width, 16), $"{i}", EditorStyles.miniBoldLabel);

                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                {
                    _selectedFrame = i;
                    _playbackFrame = i;
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        private void DrawGridLayoutSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("2. Grid Layout", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _columns = Mathf.Max(1, EditorGUILayout.IntSlider("Columns", _columns, 1, 16));
            _autoRows = EditorGUILayout.Toggle("Auto Calculate Rows", _autoRows);

            if (_autoRows)
            {
                _rows = Mathf.Max(1, Mathf.CeilToInt((float)Mathf.Max(1, FrameCount) / _columns));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Rows (Auto)", _rows);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                _rows = Mathf.Max(1, EditorGUILayout.IntSlider("Rows", _rows, 1, 16));
            }

            _outputResolution = EditorGUILayout.IntPopup("Export Resolution", _outputResolution,
                new[] { "512 x 512", "1024 x 1024", "2048 x 2048", "4096 x 4096" },
                new[] { 512, 1024, 2048, 4096 });

            int totalSlots = _columns * _rows;
            EditorGUILayout.LabelField("Grid Capacity", $"{FrameCount} / {totalSlots} frames");
            if (FrameCount > totalSlots)
            {
                EditorGUILayout.HelpBox($"Warning: {FrameCount - totalSlots} frames will be clipped! Increase columns or rows.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawGlobalTransformSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("3. Global Transform (All Frames)", EditorStyles.boldLabel);

            _rotation = EditorGUILayout.Slider("Rotation (Deg)", _rotation, -180f, 180f);
            _offset = EditorGUILayout.Vector2Field("Offset (X, Y)", _offset);
            _scale = EditorGUILayout.Slider("Scale", _scale, 0.1f, 3f);

            EditorGUILayout.BeginHorizontal();
            _flipX = EditorGUILayout.ToggleLeft("Flip X", _flipX, GUILayout.Width(80));
            _flipY = EditorGUILayout.ToggleLeft("Flip Y", _flipY, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Transform", GUILayout.Height(22)))
            {
                _rotation = 0f;
                _offset = Vector2.zero;
                _scale = 1f;
                _flipX = false;
                _flipY = false;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSaveSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("4. Save & Apply", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _targetSavePath = EditorGUILayout.TextField("Save Path", _targetSavePath);
            if (GUILayout.Button("Browse...", GUILayout.Width(70)))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save As Sprite Sheet", "VFX_Pure_Slash_Sheet.png", "png", "Save sprite sheet");
                if (!string.IsNullOrEmpty(path))
                {
                    _targetSavePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            _autoAssignToMaterial = EditorGUILayout.Toggle("Assign to Slash_VFX.mat", _autoAssignToMaterial);

            EditorGUI.BeginDisabledGroup(FrameCount == 0);
            if (GUILayout.Button("Bake & Save Sprite Sheet", GUILayout.Height(36)))
            {
                BakeSpriteSheet(_targetSavePath);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _previewTab = GUILayout.Toolbar(_previewTab, new[] { "Live Animation Player", "Full Sheet Grid Preview" });
            EditorGUILayout.Space(4);

            if (_previewTab == 0)
            {
                DrawAnimationPlayerPreview();
            }
            else
            {
                DrawSheetGridPreview();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationPlayerPreview()
        {
            GUILayout.Label($"Live Preview - Frame {_selectedFrame + 1} / {Mathf.Max(1, FrameCount)}", EditorStyles.boldLabel);

            Rect previewRect = GUILayoutUtility.GetRect(380, 380, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint && _checkeredBackground != null)
            {
                GUI.DrawTextureWithTexCoords(previewRect, _checkeredBackground, new Rect(0, 0, previewRect.width / 32f, previewRect.height / 32f));
            }
            Handles.DrawSolidRectangleWithOutline(previewRect, Color.clear, new Color(0.35f, 0.35f, 0.45f, 0.8f));

            if (FrameCount > 0 && _selectedFrame < _loadedFrames.Count && _loadedFrames[_selectedFrame] != null)
            {
                DrawTransformedFrame(previewRect, _loadedFrames[_selectedFrame]);
            }

            if (_showCrosshair)
            {
                Vector2 center = previewRect.center;
                Handles.color = new Color(1f, 0.25f, 0.25f, 0.8f);
                Handles.DrawLine(new Vector3(center.x - 45, center.y, 0), new Vector3(center.x + 45, center.y, 0));
                Handles.DrawLine(new Vector3(center.x, center.y - 45, 0), new Vector3(center.x, center.y + 45, 0));
                Handles.DrawWireDisc(new Vector3(center.x, center.y, 0), Vector3.forward, 22f);

                Handles.color = new Color(0.2f, 0.95f, 0.35f, 0.9f);
                Handles.DrawLine(new Vector3(center.x, center.y, 0), new Vector3(center.x + 70, center.y, 0));
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(_isPlaying ? "Pause" : "Play", GUILayout.Width(75), GUILayout.Height(24)))
            {
                _isPlaying = !_isPlaying;
                _lastFrameTime = EditorApplication.timeSinceStartup;
            }

            EditorGUI.BeginDisabledGroup(_isPlaying);
            if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(24)))
            {
                _selectedFrame = (_selectedFrame - 1 + FrameCount) % Mathf.Max(1, FrameCount);
            }
            if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(24)))
            {
                _selectedFrame = (_selectedFrame + 1) % Mathf.Max(1, FrameCount);
            }
            EditorGUI.EndDisabledGroup();

            _fps = EditorGUILayout.Slider("FPS", _fps, 1f, 60f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _selectedFrame = EditorGUILayout.IntSlider("Frame", _selectedFrame, 0, Mathf.Max(0, FrameCount - 1));
            _showCrosshair = EditorGUILayout.ToggleLeft("Crosshair Pivot", _showCrosshair, GUILayout.Width(130));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSheetGridPreview()
        {
            GUILayout.Label($"Grid Sheet Preview ({_columns} x {_rows})", EditorStyles.boldLabel);

            Rect previewRect = GUILayoutUtility.GetRect(380, 380, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint && _checkeredBackground != null)
            {
                GUI.DrawTextureWithTexCoords(previewRect, _checkeredBackground, new Rect(0, 0, previewRect.width / 32f, previewRect.height / 32f));
            }
            Handles.DrawSolidRectangleWithOutline(previewRect, Color.clear, new Color(0.35f, 0.35f, 0.45f, 0.8f));

            float cellWidth = previewRect.width / _columns;
            float cellHeight = previewRect.height / _rows;

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    int index = r * _columns + c;
                    Rect cellRect = new Rect(previewRect.x + c * cellWidth, previewRect.y + r * cellHeight, cellWidth, cellHeight);
                    Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, new Color(0.4f, 0.4f, 0.5f, 0.25f));

                    if (index < _loadedFrames.Count && _loadedFrames[index] != null)
                    {
                        DrawTransformedFrame(cellRect, _loadedFrames[index]);
                    }
                }
            }
        }

        private void DrawTransformedFrame(Rect targetRect, Texture2D tex)
        {
            Vector2 center = targetRect.center + new Vector2(_offset.x, -_offset.y);
            float size = Mathf.Min(targetRect.width, targetRect.height) * 0.9f * _scale;
            Rect drawRect = new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);

            Matrix4x4 backupMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(_rotation, center);

            Rect uv = new Rect(
                _flipX ? 1 : 0,
                _flipY ? 1 : 0,
                _flipX ? -1 : 1,
                _flipY ? -1 : 1
            );

            GUI.DrawTextureWithTexCoords(drawRect, tex, uv, true);
            GUI.matrix = backupMatrix;
        }

        private void LoadFramesFromFolder(string folder)
        {
            _frameFilePaths.Clear();
            _loadedFrames.Clear();

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".tga" };
            var files = new List<string>();

            foreach (string file in Directory.GetFiles(folder))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                foreach (string allowed in allowedExtensions)
                {
                    if (ext == allowed)
                    {
                        files.Add(file);
                        break;
                    }
                }
            }

            files.Sort(EditorUtility.NaturalCompare);

            foreach (string file in files)
            {
                string relativePath = file;
                if (file.StartsWith(Application.dataPath))
                {
                    relativePath = "Assets" + file.Substring(Application.dataPath.Length);
                }

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                if (tex == null && File.Exists(file))
                {
                    byte[] data = File.ReadAllBytes(file);
                    tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                }

                if (tex != null)
                {
                    _frameFilePaths.Add(relativePath);
                    _loadedFrames.Add(tex);
                }
            }

            if (_autoRows && _columns > 0)
            {
                _rows = Mathf.Max(1, Mathf.CeilToInt((float)Mathf.Max(1, FrameCount) / _columns));
            }

            _selectedFrame = 0;
            _playbackFrame = 0f;
            Repaint();
        }

        private void BakeSpriteSheet(string targetPath)
        {
            if (FrameCount == 0) return;

            int cols = _columns;
            int rows = _rows;
            int cellW = _outputResolution / cols;
            int cellH = _outputResolution / rows;

            Texture2D output = new Texture2D(_outputResolution, _outputResolution, TextureFormat.RGBA32, false);
            Color32[] clearPixels = new Color32[_outputResolution * _outputResolution];
            output.SetPixels32(clearPixels);

            float rotRad = _rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rotRad);
            float sin = Mathf.Sin(rotRad);
            float scaleInv = 1f / Mathf.Max(0.01f, _scale);

            for (int index = 0; index < FrameCount; index++)
            {
                int col = index % cols;
                int row = index / cols;
                if (row >= rows) break;

                int flippedRow = (rows - 1) - row;
                int cellStartX = col * cellW;
                int cellStartY = flippedRow * cellH;

                float cellCenterX = cellStartX + cellW * 0.5f;
                float cellCenterY = cellStartY + cellH * 0.5f;

                Texture2D frameTex = GetReadableTexture(_loadedFrames[index]);
                if (frameTex == null) continue;

                float srcW = frameTex.width;
                float srcH = frameTex.height;
                float srcCenterX = srcW * 0.5f;
                float srcCenterY = srcH * 0.5f;

                for (int y = 0; y < cellH; y++)
                {
                    for (int x = 0; x < cellW; x++)
                    {
                        float px = (cellStartX + x) - cellCenterX - _offset.x;
                        float py = (cellStartY + y) - cellCenterY - _offset.y;

                        float unrotX = (px * cos + py * sin) * scaleInv;
                        float unrotY = (-px * sin + py * cos) * scaleInv;

                        if (_flipX) unrotX = -unrotX;
                        if (_flipY) unrotY = -unrotY;

                        float srcX = srcCenterX + unrotX;
                        float srcY = srcCenterY + unrotY;

                        if (srcX >= 0 && srcX < srcW && srcY >= 0 && srcY < srcH)
                        {
                            Color c = frameTex.GetPixelBilinear(srcX / srcW, srcY / srcH);
                            output.SetPixel(cellStartX + x, cellStartY + y, c);
                        }
                    }
                }

                if (frameTex != _loadedFrames[index])
                {
                    DestroyImmediate(frameTex);
                }
            }

            output.Apply();

            byte[] pngBytes = output.EncodeToPNG();
            DestroyImmediate(output);

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), targetPath);
            string parentDir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            File.WriteAllBytes(fullPath, pngBytes);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

            ConfigureTextureImporter(targetPath);

            if (_autoAssignToMaterial)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialPath);
                Texture2D bakedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
                if (mat != null && bakedTex != null)
                {
                    mat.mainTexture = bakedTex;
                    EditorUtility.SetDirty(mat);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sprite Sheet Workbench", $"Sprite Sheet successfully baked to:\n{targetPath}\n\nFrames: {FrameCount}\nGrid: {cols}x{rows}\nResolution: {_outputResolution}x{_outputResolution}", "OK");
        }

        private Texture2D GetReadableTexture(Texture2D source)
        {
            if (source == null) return null;

            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            return readable;
        }

        private void ConfigureTextureImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.isReadable = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
        #endregion
    }
}
