using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public class CharacterSheetWindow : EditorWindow
    {
        #region [Variables/Fields]
        private const string MenuPath = "Pale Vertica/Character/Character Sheet Importer";
        private const float SheetViewWidth = 340f;
        private const float PartViewSize = 400f;
        private const float ResultViewSize = 190f;
        private const float WheelScaleStep = 0.03f;
        private static readonly Color CellColor = new Color(0f, 0.8f, 1f, 0.9f);
        private static readonly Color PreviewCellColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        private static readonly Color SelectedCellColor = new Color(1f, 0.85f, 0f, 1f);
        private static readonly Color JointColor = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color AnchorColor = new Color(0f, 0.75f, 1f, 1f);
        private static readonly Color GroundColor = new Color(1f, 0.5f, 0f, 0.8f);
        private static readonly Color ViewBackground = new Color(0.18f, 0.18f, 0.2f, 1f);
        private static readonly string[] LayoutLabels = { "Auto (from aspect)", "3 x 3 (1:1)", "4 x 3 (4:3)" };
        private static readonly string[] GhostLabels = { "Ghost: AI preview", "Ghost: built parts" };

        [SerializeField] private string _sheetPath;
        [SerializeField] private CharacterSheetLayout _layout = CharacterSheetLayout.Auto;
        [SerializeField] private string _characterName;
        [SerializeField] private bool _createPrefab = true;
        [SerializeField] private GameObject _basePrefab;
        [SerializeField] private int _selectedTab = 1;
        [SerializeField] private int _ghostMode;
        [SerializeField] private Vector2 _scroll;

        private CharacterSheetSession _session;
        private readonly List<CharacterCheckResult> _checks = new List<CharacterCheckResult>();
        private readonly List<string> _log = new List<string>();
        private Vector2 _dragOffset;
        private bool _dragging;
        private bool _checksDirty;
        private GUIStyle _wrapStyle;
        private GUIStyle _cellLabelStyle;
        #endregion

        #region [Properties]
        private CharacterAssetSettings Settings => CharacterAssetSettings.Instance;
        private bool HasSheet => _session != null && _session.IsLoaded;
        private string[] TabNames => new[] { CharacterSheetSlicer.PreviewCellName }.Concat(Settings.Parts.Select(p => p.Name)).ToArray();
        private string SelectedName => TabNames[Mathf.Clamp(_selectedTab, 0, TabNames.Length - 1)];
        private bool PreviewSelected => SelectedName == CharacterSheetSlicer.PreviewCellName;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            titleContent = new GUIContent("Character Sheet");
            minSize = new Vector2(1000f, 800f);
            if (_basePrefab == null) _basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Settings.BasePrefabPath);
            if (!string.IsNullOrEmpty(_sheetPath) && File.Exists(_sheetPath)) LoadSheet(_sheetPath);
        }

        private void OnDisable()
        {
            _session?.Dispose();
            _session = null;
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (_checksDirty && Event.current.type == EventType.Layout) RefreshChecks();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSource();
            if (HasSheet)
            {
                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawSheetPanel();
                    GUILayout.Space(8f);
                    DrawPartPanel();
                    GUILayout.Space(8f);
                    DrawResultPanel();
                }
                EditorGUILayout.Space();
                DrawChecks();
                EditorGUILayout.Space();
                DrawBuild();
            }
            DrawLog();
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region [Public Methods]
        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<CharacterSheetWindow>().Show();
        }
        #endregion

        #region [Private Methods]
        private void EnsureStyles()
        {
            if (_wrapStyle == null) _wrapStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
            if (_cellLabelStyle == null)
            {
                _cellLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                _cellLabelStyle.normal.textColor = Color.white;
            }
        }

        private void DrawSource()
        {
            EditorGUILayout.LabelField("AI character sheet (one image, grid of parts)", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Sheet image", _sheetPath ?? string.Empty);
                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    string folder = string.IsNullOrEmpty(_sheetPath) ? string.Empty : Path.GetDirectoryName(_sheetPath);
                    string picked = EditorUtility.OpenFilePanel("Character sheet image", folder, "png,jpg,jpeg");
                    if (!string.IsNullOrEmpty(picked)) LoadSheet(picked);
                    GUIUtility.ExitGUI();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                CharacterSheetLayout layout = (CharacterSheetLayout)EditorGUILayout.Popup("Grid layout", (int)_layout, LayoutLabels);
                bool reslice;
                using (new EditorGUI.DisabledScope(!HasSheet))
                    reslice = GUILayout.Button("Re-slice", GUILayout.Width(80f));
                if (layout != _layout || reslice)
                {
                    _layout = layout;
                    if (HasSheet) Reslice();
                    GUIUtility.ExitGUI();
                }
            }
            string info = HasSheet
                ? $"{_session.SheetWidth} x {_session.SheetHeight} px, layout {_session.Layout}, {(_session.FoundGridLines ? "magenta grid lines found" : "no grid lines: split into equal cells")}. Cell order: {string.Join(", ", CharacterSheetSlicer.CellOrder)}."
                : "Pick the image the AI made with Prompt S.";
            EditorGUILayout.HelpBox(info, MessageType.None);
        }

        private void DrawSheetPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(SheetViewWidth)))
            {
                EditorGUILayout.LabelField("1. How it will be cut", EditorStyles.boldLabel);
                float height = SheetViewWidth * _session.SheetHeight / _session.SheetWidth;
                Rect view = GUILayoutUtility.GetRect(SheetViewWidth, height, GUILayout.Width(SheetViewWidth), GUILayout.Height(height));
                EditorGUI.DrawTextureTransparent(view, _session.SheetTexture, ScaleMode.StretchToFill);
                float scale = view.width / _session.SheetWidth;
                foreach (CharacterSheetCell cell in _session.Cells)
                {
                    Rect cellRect = new Rect(view.x + cell.X * scale, view.y + cell.Y * scale, cell.Width * scale, cell.Height * scale);
                    bool isPreview = cell.Name == CharacterSheetSlicer.PreviewCellName;
                    bool selected = cell.Name == SelectedName;
                    DrawOutline(cellRect, selected ? SelectedCellColor : isPreview ? PreviewCellColor : CellColor, selected ? 3f : 1f);
                    GUI.Label(new Rect(cellRect.x + 3f, cellRect.y + 2f, cellRect.width, 16f), cell.Name, _cellLabelStyle);
                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        Select(cell.Name);
                        Event.current.Use();
                    }
                }
                EditorGUILayout.LabelField("Click a cell to edit it.", EditorStyles.miniLabel);
            }
        }

        private void DrawPartPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(PartViewSize)))
            {
                EditorGUILayout.LabelField("2. Place: drag to move, mouse wheel to scale", EditorStyles.boldLabel);
                string[] tabs = TabNames;
                _selectedTab = GUILayout.Toolbar(Mathf.Clamp(_selectedTab, 0, tabs.Length - 1), tabs);
                _ghostMode = GUILayout.Toolbar(_ghostMode, GhostLabels);
                string name = SelectedName;
                Rect view = GUILayoutUtility.GetRect(PartViewSize, PartViewSize, GUILayout.Width(PartViewSize), GUILayout.Height(PartViewSize));
                DrawPartView(view, name);
                HandleMouse(view, name);
                Vector2 offset = _session.GetOffset(name);
                Vector2 editedOffset = EditorGUILayout.Vector2Field("Offset (px)", offset);
                if (editedOffset != offset) SetOffset(name, editedOffset);
                float scale = _session.GetScale(name);
                float editedScale = EditorGUILayout.Slider("Scale", scale, CharacterSheetSession.MinScale, CharacterSheetSession.MaxScale);
                if (!Mathf.Approximately(editedScale, scale)) SetScale(name, editedScale);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(PreviewSelected))
                    {
                        if (GUILayout.Button("Snap to box")) { _session.SnapToReferenceBox(Settings.FindPart(name)); MarkChanged(); }
                        if (GUILayout.Button("Scale to all parts")) { _session.SetScaleForAllParts(scale); MarkChanged(); }
                    }
                    if (GUILayout.Button("Mirror")) { _session.MirrorPart(name); MarkChanged(); }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset")) { _session.ResetTransform(name); MarkChanged(); }
                    if (GUILayout.Button("Reset all")) { _session.ResetAll(); MarkChanged(); }
                    if (GUILayout.Button("Mirror whole character")) { _session.MirrorWholeCharacter(); MarkChanged(); }
                }
                EditorGUILayout.HelpBox(PartHint(name), PartHintType(name));
            }
        }

        private void DrawPartView(Rect view, string name)
        {
            float viewScale = view.width / _session.CanvasSize;
            EditorGUI.DrawRect(view, ViewBackground);
            GUI.BeginClip(view);
            Color previous = GUI.color;
            if (!PreviewSelected)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.35f);
                if (_ghostMode == 0) DrawPart(CharacterSheetSlicer.PreviewCellName, viewScale);
                else GUI.DrawTexture(new Rect(0f, 0f, view.width, view.height), _session.GetCompositeTexture(), ScaleMode.StretchToFill);
                GUI.color = previous;
            }
            DrawPart(name, viewScale);
            EditorGUI.DrawRect(new Rect(0f, (_session.CanvasSize - Settings.GroundY) * viewScale, view.width, 1f), GroundColor);
            EditorGUI.DrawRect(new Rect(_session.CenterX * viewScale, 0f, 1f, view.height), new Color(GroundColor.r, GroundColor.g, GroundColor.b, 0.35f));
            if (PreviewSelected)
            {
                foreach (CharacterPartSpec part in Settings.Parts)
                    DrawCross(_session.JointOnCanvas(part) * viewScale, JointColor, 8f);
            }
            else
            {
                CharacterPartSpec part = Settings.FindPart(name);
                Vector2 anchor = new Vector2(part.Anchor.x, _session.CanvasSize - part.Anchor.y) * viewScale;
                EditorGUI.DrawRect(new Rect(anchor.x - 2f, anchor.y - 2f, 4f, 4f), AnchorColor);
                DrawCross(_session.JointOnCanvas(part) * viewScale, JointColor, 12f);
            }
            GUI.EndClip();
            DrawOutline(view, new Color(0.4f, 0.4f, 0.45f, 1f), 1f);
        }

        private void DrawPart(string name, float viewScale)
        {
            Texture2D texture = _session.GetPartTexture(name);
            if (texture == null) return;
            Rect rect = _session.GetCanvasRect(name);
            GUI.DrawTexture(new Rect(rect.x * viewScale, rect.y * viewScale, rect.width * viewScale, rect.height * viewScale), texture, ScaleMode.StretchToFill);
        }

        private void HandleMouse(Rect view, string name)
        {
            Event e = Event.current;
            float viewScale = view.width / _session.CanvasSize;
            if (e.type == EventType.MouseDown && e.button == 0 && view.Contains(e.mousePosition))
            {
                _dragging = true;
                _dragOffset = _session.GetOffset(name);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _dragging)
            {
                _dragOffset += e.delta / (viewScale * _session.GetScale(name));
                SetOffset(name, new Vector2(Mathf.Round(_dragOffset.x), Mathf.Round(_dragOffset.y)));
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _dragging)
            {
                _dragging = false;
                e.Use();
            }
            else if (e.type == EventType.ScrollWheel && view.Contains(e.mousePosition))
            {
                SetScale(name, _session.GetScale(name) * (1f - e.delta.y * WheelScaleStep));
                e.Use();
            }
        }

        private string PartHint(string name)
        {
            if (PreviewSelected)
                return "Preview: fit the AI's full character onto the rig. Move and scale it until the crosses sit on the neck, shoulders and hips and the feet touch the orange line. The parts are then matched against this ghost.";
            bool covered = _session.CoversJoint(name, Settings.FindPart(name));
            return covered
                ? $"{name} covers its rotation point. Match it to the ghost: same size, same place."
                : $"{name} does not cover its rotation point yet: move it onto the magenta cross (wheel to scale).";
        }

        private MessageType PartHintType(string name)
        {
            if (PreviewSelected) return MessageType.Info;
            return _session.CoversJoint(name, Settings.FindPart(name)) ? MessageType.Info : MessageType.Warning;
        }

        private void DrawResultPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(ResultViewSize)))
            {
                EditorGUILayout.LabelField("3. Result on the rig", EditorStyles.boldLabel);
                Rect built = GUILayoutUtility.GetRect(ResultViewSize, ResultViewSize, GUILayout.Width(ResultViewSize), GUILayout.Height(ResultViewSize));
                EditorGUI.DrawTextureTransparent(built, _session.GetCompositeTexture(), ScaleMode.StretchToFill);
                EditorGUILayout.LabelField("AI preview cell", EditorStyles.boldLabel);
                Rect preview = GUILayoutUtility.GetRect(ResultViewSize, ResultViewSize, GUILayout.Width(ResultViewSize), GUILayout.Height(ResultViewSize));
                EditorGUI.DrawRect(preview, ViewBackground);
                GUI.BeginClip(preview);
                DrawPart(CharacterSheetSlicer.PreviewCellName, preview.width / _session.CanvasSize);
                GUI.EndClip();
                EditorGUILayout.LabelField("The two should look the same.", EditorStyles.miniLabel);
            }
        }

        private void DrawChecks()
        {
            EditorGUILayout.LabelField("Checks", EditorStyles.boldLabel);
            foreach (CharacterCheckResult check in _checks)
            {
                MessageType type = check.Severity == CharacterCheckSeverity.Error ? MessageType.Error
                    : check.Severity == CharacterCheckSeverity.Warning ? MessageType.Warning : MessageType.Info;
                EditorGUILayout.HelpBox(check.Message, type);
            }
        }

        private void DrawBuild()
        {
            EditorGUILayout.LabelField("4. Build", EditorStyles.boldLabel);
            _characterName = EditorGUILayout.TextField("Character name", _characterName);
            EditorGUILayout.LabelField("PSB", CharacterSheetConverter.GetPsbPath(Settings, SafeName(_characterName)));
            _createPrefab = EditorGUILayout.Toggle("Create prefab", _createPrefab);
            using (new EditorGUI.DisabledScope(!_createPrefab))
                _basePrefab = (GameObject)EditorGUILayout.ObjectField("Base prefab", _basePrefab, typeof(GameObject), false);
            bool blocked = _checks.Any(c => c.Severity == CharacterCheckSeverity.Error) || string.IsNullOrEmpty(SafeName(_characterName));
            using (new EditorGUI.DisabledScope(blocked))
            {
                if (GUILayout.Button("Build PSB, Apply Standard" + (_createPrefab ? ", Create Prefab" : string.Empty), GUILayout.Height(28f)))
                {
                    Build();
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.LabelField(blocked ? "Fix the errors above and enter a name to build." : " ", EditorStyles.miniLabel);
        }

        private void DrawLog()
        {
            if (_log.Count == 0) return;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
            foreach (string line in _log)
                EditorGUILayout.LabelField(line, _wrapStyle);
        }

        private void LoadSheet(string path)
        {
            _log.Clear();
            try
            {
                if (_session == null) _session = new CharacterSheetSession(Settings);
                _session.Load(path, _layout);
                _sheetPath = path;
                if (string.IsNullOrEmpty(_characterName)) _characterName = SafeName(Path.GetFileNameWithoutExtension(path));
            }
            catch (System.Exception exception)
            {
                _log.Add($"Could not load the sheet: {exception.Message}");
                _session?.Dispose();
            }
            RefreshChecks();
        }

        private void Reslice()
        {
            _session.Slice(_layout);
            RefreshChecks();
        }

        private void Select(string name)
        {
            int index = System.Array.IndexOf(TabNames, name);
            if (index >= 0) _selectedTab = index;
            Repaint();
        }

        private void SetOffset(string name, Vector2 offset)
        {
            _session.SetOffset(name, offset);
            MarkChanged();
        }

        private void SetScale(string name, float scale)
        {
            _session.SetScale(name, scale);
            MarkChanged();
        }

        private void MarkChanged()
        {
            _checksDirty = true;
            Repaint();
        }

        private void RefreshChecks()
        {
            _checksDirty = false;
            _checks.Clear();
            if (HasSheet) _checks.AddRange(_session.Check());
        }

        private void Build()
        {
            string name = SafeName(_characterName);
            string psbPath = CharacterSheetConverter.GetPsbPath(Settings, name);
            if (!EditorUtility.DisplayDialog("Build character", $"Write {psbPath}, apply the standard import settings and anchor pivots{(_createPrefab ? ", and create a prefab from " + (_basePrefab != null ? _basePrefab.name : "the base prefab") : string.Empty)}?", "Build", "Cancel")) return;
            _log.Clear();
            try
            {
                string basePath = _basePrefab != null ? AssetDatabase.GetAssetPath(_basePrefab) : null;
                string built = CharacterSheetConverter.Build(_session, name, Settings, _createPrefab, basePath, _log);
                if (string.IsNullOrEmpty(built)) return;
                List<CharacterCheckResult> validation = CharacterPsbValidator.Validate(built, Settings);
                _log.Add($"Validation: {CharacterPsbValidator.WorstSeverity(validation)}.");
                foreach (CharacterCheckResult result in validation.Where(r => r.Severity >= CharacterCheckSeverity.Warning))
                    _log.Add($"{result.Severity}: {result.Message}");
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(built));
            }
            catch (System.Exception exception)
            {
                _log.Add($"Build failed: {exception.Message}");
                Debug.LogException(exception);
            }
        }

        private static string SafeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        private static void DrawOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawCross(Vector2 center, Color color, float size)
        {
            EditorGUI.DrawRect(new Rect(center.x - size, center.y - 1f, size * 2f, 2f), color);
            EditorGUI.DrawRect(new Rect(center.x - 1f, center.y - size, 2f, size * 2f), color);
            DrawOutline(new Rect(center.x - size * 0.6f, center.y - size * 0.6f, size * 1.2f, size * 1.2f), color, 2f);
        }
        #endregion
    }
}
