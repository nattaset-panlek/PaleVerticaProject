using UnityEditor;
using UnityEngine;
using CoreSystem.Data;
using System.IO;

namespace CoreSystem.Editor
{
    public class ExperienceEditorWindow : EditorWindow
    {
        // ─── State ───────────────────────────────────────────────────────────
        private ExperienceTable _table;
        private string _savePath = "Assets/Resources/Data/ExperienceTable.asset";
        private Vector2 _scroll;
        private bool _showPreview;
        private int _previewPage;
        private const int PreviewPageSize = 10;
        private int _selectedTab;
        private string[] _tabs = { "Curves", "Preview Table", "Import / Export" };

        // ─── Styles ──────────────────────────────────────────────────────────
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _sectionBox;
        private GUIStyle _statLabel;
        private GUIStyle _labelCentered;
        private bool _stylesInitialized;

        private static readonly Color AccentGreen  = new Color(0.15f, 0.80f, 0.45f);
        private static readonly Color AccentCyan   = new Color(0.20f, 0.75f, 0.95f);
        private static readonly Color PanelBg      = new Color(0.13f, 0.14f, 0.16f);
        private static readonly Color HeaderBg     = new Color(0.10f, 0.11f, 0.13f);
        private static readonly Color SectionBg    = new Color(0.17f, 0.18f, 0.21f);

        [MenuItem("Tools/⚗️ Experience Editor")]
        public static void ShowWindow()
        {
            var w = GetWindow<ExperienceEditorWindow>(false, "Exp Designer", true);
            w.minSize = new Vector2(480, 560);
        }

        private void OnEnable() => LoadTable();

        // ─── Style Init ───────────────────────────────────────────────────────
        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            _subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.85f, 0.90f, 1.0f) }
            };

            _sectionBox = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin  = new RectOffset(4, 4, 4, 4),
                normal  = { background = MakeTex(SectionBg) }
            };

            _statLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal   = { textColor = new Color(0.75f, 0.78f, 0.85f) }
            };

            _labelCentered = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 11,
                normal    = { textColor = new Color(0.75f, 0.78f, 0.85f) }
            };
        }

        // ─── Main Draw ───────────────────────────────────────────────────────
        private void OnGUI()
        {
            InitStyles();

            // Background fill
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), PanelBg);

            DrawHeader();

            if (_table == null)
            {
                DrawNoTableScreen();
                return;
            }

            DrawTabBar();
            GUILayout.Space(6);
            _scroll = GUILayout.BeginScrollView(_scroll);

            switch (_selectedTab)
            {
                case 0: DrawCurvesTab(); break;
                case 1: DrawPreviewTab(); break;
                case 2: DrawImportExportTab(); break;
            }

            GUILayout.EndScrollView();
            DrawFooter();
        }

        // ─── Header ──────────────────────────────────────────────────────────
        private void DrawHeader()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, 52), HeaderBg);

            GUILayout.BeginVertical();
            GUILayout.Space(8);
            GUILayout.Label("⚗️  Experience Designer", _headerStyle);
            GUILayout.Space(6);
            GUILayout.EndVertical();

            Handles.color = AccentGreen;
            Handles.DrawLine(new Vector3(0, 52), new Vector3(position.width, 52));
        }

        // ─── No Table ────────────────────────────────────────────────────────
        private void DrawNoTableScreen()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Space(60);

            EditorGUILayout.HelpBox("No Experience Table found.\nCreate one to get started.", MessageType.Info);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (DrawAccentButton("✚  Create Experience Table", AccentGreen, 240, 36))
                CreateNewTable();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        // ─── Tab Bar ─────────────────────────────────────────────────────────
        private void DrawTabBar()
        {
            GUILayout.Space(56);
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            for (int i = 0; i < _tabs.Length; i++)
            {
                bool active = i == _selectedTab;
                var style = new GUIStyle(EditorStyles.miniButton)
                {
                    fontSize = 11,
                    fixedHeight = 26,
                    normal    = { textColor = active ? Color.white : new Color(0.65f, 0.65f, 0.65f) },
                    fontStyle = active ? FontStyle.Bold : FontStyle.Normal,
                };
                GUI.backgroundColor = active ? new Color(0.22f, 0.25f, 0.30f) : new Color(0.18f, 0.19f, 0.22f);
                if (GUILayout.Button(_tabs[i], style, GUILayout.Height(26)))
                    _selectedTab = i;
                GUI.backgroundColor = Color.white;
            }
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        // ─── Tab: Curves ─────────────────────────────────────────────────────
        private void DrawCurvesTab()
        {
            // Settings card
            BeginSection("⚙️  Global Settings");
            EditorGUI.BeginChangeCheck();

            DrawFieldRow("Max Level", () =>
            {
                _table.MaxLevel = EditorGUILayout.IntSlider(_table.MaxLevel, 1, 200);
            });

            if (EditorGUI.EndChangeCheck()) SetDirty();
            EndSection();

            GUILayout.Space(6);

            // Base EXP card
            BeginSection("🟢  Base Level Curve");
            DrawStatRow("Max EXP needed", $"{_table.BaseExpAtMax:N0}", AccentGreen);
            EditorGUI.BeginChangeCheck();
            _table.BaseExpAtMax = EditorGUILayout.LongField("Exp at Max Level", _table.BaseExpAtMax);
            GUILayout.Space(4);
            _table.BaseExpCurve = EditorGUILayout.CurveField("Curve", _table.BaseExpCurve, AccentGreen, new Rect(0, 0, 1, 1), GUILayout.Height(70));
            if (EditorGUI.EndChangeCheck()) SetDirty();
            EndSection();

            GUILayout.Space(6);

            // Job EXP card
            BeginSection("🔵  Job Level Curve");
            DrawStatRow("Max EXP needed", $"{_table.JobExpAtMax:N0}", AccentCyan);
            EditorGUI.BeginChangeCheck();
            _table.JobExpAtMax = EditorGUILayout.LongField("Exp at Max Level", _table.JobExpAtMax);
            GUILayout.Space(4);
            _table.JobExpCurve = EditorGUILayout.CurveField("Curve", _table.JobExpCurve, AccentCyan, new Rect(0, 0, 1, 1), GUILayout.Height(70));
            if (EditorGUI.EndChangeCheck()) SetDirty();
            EndSection();

            GUILayout.Space(8);

            // Quick presets
            BeginSection("✨  Quick Presets");
            GUILayout.BeginHorizontal();
            if (DrawAccentButton("Linear",      Color.gray, 90, 28)) ApplyPreset(PresetType.Linear);
            if (DrawAccentButton("Exponential", AccentGreen, 100, 28)) ApplyPreset(PresetType.Exponential);
            if (DrawAccentButton("Logarithmic", AccentCyan,  100, 28)) ApplyPreset(PresetType.Logarithmic);
            if (DrawAccentButton("S-Curve",     new Color(1f, 0.7f, 0.2f), 80, 28)) ApplyPreset(PresetType.SCurve);
            GUILayout.EndHorizontal();
            EndSection();
        }

        // ─── Tab: Preview ────────────────────────────────────────────────────
        private void DrawPreviewTab()
        {
            GUILayout.Space(4);
            BeginSection("📊  EXP Per Level Preview");

            int totalLevels = Mathf.Min(_table.MaxLevel, 500);
            int totalPages  = Mathf.CeilToInt(totalLevels / (float)PreviewPageSize);
            _previewPage    = Mathf.Clamp(_previewPage, 0, totalPages - 1);

            // Column header
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true)), new Color(0.12f, 0.13f, 0.15f));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level",    _labelCentered, GUILayout.Width(60));
            GUILayout.Label("Base EXP", _labelCentered, GUILayout.ExpandWidth(true));
            GUILayout.Label("Job EXP",  _labelCentered, GUILayout.ExpandWidth(true));
            GUILayout.Label("Cumul. Base", _labelCentered, GUILayout.Width(110));
            GUILayout.EndHorizontal();

            Handles.color = new Color(0.30f, 0.32f, 0.38f);
            Handles.DrawLine(new Vector3(4, GUILayoutUtility.GetLastRect().yMax), new Vector3(position.width - 4, GUILayoutUtility.GetLastRect().yMax));

            long cumBase = 0;
            int start = _previewPage * PreviewPageSize;
            int end   = Mathf.Min(start + PreviewPageSize, totalLevels);

            // Accumulate up to start
            for (int i = 1; i < start + 1; i++) cumBase += _table.GetBaseExp(i);

            for (int lvl = start + 1; lvl <= end; lvl++)
            {
                long bExp = _table.GetBaseExp(lvl);
                long jExp = _table.GetJobExp(lvl);
                cumBase += bExp;

                Color rowColor = (lvl % 2 == 0)
                    ? new Color(0.17f, 0.18f, 0.21f)
                    : new Color(0.14f, 0.15f, 0.17f);

                Rect rowRect = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rowRect, rowColor);

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{lvl}",          _labelCentered, GUILayout.Width(60));
                GUILayout.Label($"{bExp:N0}",       _labelCentered, GUILayout.ExpandWidth(true));
                GUILayout.Label($"{jExp:N0}",       _labelCentered, GUILayout.ExpandWidth(true));
                GUILayout.Label($"{cumBase:N0}",    _labelCentered, GUILayout.Width(110));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6);

            // Pagination
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = _previewPage > 0;
            if (GUILayout.Button("◀  Prev", GUILayout.Width(70))) _previewPage--;
            GUI.enabled = true;
            GUILayout.Label($"Page {_previewPage + 1} / {totalPages}", _labelCentered, GUILayout.Width(90));
            GUI.enabled = _previewPage < totalPages - 1;
            if (GUILayout.Button("Next  ▶", GUILayout.Width(70))) _previewPage++;
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EndSection();
        }

        // ─── Tab: Import / Export ────────────────────────────────────────────
        private void DrawImportExportTab()
        {
            GUILayout.Space(4);
            BeginSection("📁  File Reference");
            EditorGUILayout.LabelField("Save Path:", _subHeaderStyle);
            EditorGUILayout.SelectableLabel(_savePath, EditorStyles.textField, GUILayout.Height(18));

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (DrawAccentButton("Ping Asset", Color.gray, 100, 28) && _table != null)
                EditorGUIUtility.PingObject(_table);

            if (DrawAccentButton("Duplicate Table", new Color(0.6f, 0.4f, 0.9f), 130, 28))
                DuplicateTable();
            GUILayout.EndHorizontal();
            EndSection();

            GUILayout.Space(6);
            BeginSection("🔄  Reset Options");
            EditorGUILayout.HelpBox("Resetting will overwrite the current curves with preset values.", MessageType.Warning);
            GUILayout.Space(4);
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("⚠️  Reset to Default Curves", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirm Reset", "This will reset the curves. Continue?", "Reset", "Cancel"))
                    ResetToDefaults();
            }
            GUI.backgroundColor = Color.white;
            EndSection();
        }

        // ─── Footer ──────────────────────────────────────────────────────────
        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();
            Handles.color = new Color(0.25f, 0.28f, 0.32f);
            float y = position.height - 42;
            Handles.DrawLine(new Vector3(0, y), new Vector3(position.width, y));

            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUILayout.Label($"Table: {(_table != null ? _table.name : "None")} | Max Lvl: {(_table != null ? _table.MaxLevel.ToString() : "-")}", _statLabel);
            GUILayout.FlexibleSpace();
            if (DrawAccentButton("💾  Save", AccentGreen, 90, 28))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[ExpEditor] Saved to {_savePath}");
            }
            GUILayout.Space(6);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private void BeginSection(string title)
        {
            GUILayout.BeginVertical(_sectionBox);
            GUILayout.Label(title, _subHeaderStyle);
            GUILayout.Space(4);
        }

        private void EndSection()
        {
            GUILayout.EndVertical();
        }

        private void DrawFieldRow(string label, System.Action field)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _statLabel, GUILayout.Width(110));
            field();
            GUILayout.EndHorizontal();
        }

        private void DrawStatRow(string label, string value, Color color)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _statLabel, GUILayout.Width(120));
            var oldColor = GUI.color;
            GUI.color = color;
            GUILayout.Label(value, EditorStyles.boldLabel);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();
        }

        private bool DrawAccentButton(string text, Color color, float width, float height)
        {
            GUI.backgroundColor = color;
            bool result = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(height));
            GUI.backgroundColor = Color.white;
            return result;
        }

        // ─── Presets ────────────────────────────────────────────────────────
        private enum PresetType { Linear, Exponential, Logarithmic, SCurve }

        private void ApplyPreset(PresetType type)
        {
            switch (type)
            {
                case PresetType.Linear:
                    _table.BaseExpCurve = AnimationCurve.Linear(0, 0, 1, 1);
                    _table.JobExpCurve  = AnimationCurve.Linear(0, 0, 1, 1);
                    break;
                case PresetType.Exponential:
                    _table.BaseExpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.1f), new Keyframe(1, 1));
                    _table.JobExpCurve  = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.08f), new Keyframe(1, 1));
                    break;
                case PresetType.Logarithmic:
                    _table.BaseExpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 0.7f), new Keyframe(1, 1));
                    _table.JobExpCurve  = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 0.65f), new Keyframe(1, 1));
                    break;
                case PresetType.SCurve:
                    _table.BaseExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    _table.JobExpCurve  = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    break;
            }
            SetDirty();
        }

        private void ResetToDefaults()
        {
            _table.BaseExpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            _table.JobExpCurve  = AnimationCurve.EaseInOut(0, 0, 1, 1);
            _table.MaxLevel     = 99;
            _table.BaseExpAtMax = 1000000;
            _table.JobExpAtMax  = 500000;
            SetDirty();
        }

        private void DuplicateTable()
        {
            string dir  = Path.GetDirectoryName(_savePath);
            string copy = Path.Combine(dir, "ExperienceTable_Copy.asset");
            AssetDatabase.CopyAsset(_savePath, copy);
            AssetDatabase.Refresh();
            Debug.Log($"[ExpEditor] Duplicated to {copy}");
        }

        private void SetDirty()
        {
            if (_table != null) EditorUtility.SetDirty(_table);
            Repaint();
        }

        // ─── Asset Management ────────────────────────────────────────────────
        private void LoadTable()
        {
            _table = AssetDatabase.LoadAssetAtPath<ExperienceTable>(_savePath);
            if (_table == null)
                _table = Resources.Load<ExperienceTable>("Data/ExperienceTable");
        }

        private void CreateNewTable()
        {
            string dir = Path.GetDirectoryName(_savePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _table = CreateInstance<ExperienceTable>();
            AssetDatabase.CreateAsset(_table, _savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Repaint();
        }

        // ─── Texture Helper ──────────────────────────────────────────────────
        private static Texture2D MakeTex(Color col)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }
    }
}
