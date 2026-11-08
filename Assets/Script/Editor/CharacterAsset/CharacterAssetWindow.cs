using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.CharacterAsset
{
    public class CharacterAssetWindow : EditorWindow
    {
        #region [Variables/Fields]
        private const string MenuPath = "Pale Vertica/Character/Character Asset Tool";
        private const string ValidateMenuPath = "Assets/Pale Vertica/Validate Character PSB";

        [SerializeField] private Object _psb;
        [SerializeField] private GameObject _basePrefab;
        [SerializeField] private string _outputPrefabPath;
        [SerializeField] private Vector2 _scroll;

        private readonly List<CharacterCheckResult> _results = new List<CharacterCheckResult>();
        private readonly List<string> _log = new List<string>();
        private GUIStyle _wrapStyle;
        #endregion

        #region [Properties]
        private CharacterAssetSettings Settings => CharacterAssetSettings.Instance;
        private string PsbPath => _psb != null ? AssetDatabase.GetAssetPath(_psb) : null;
        private bool HasPsb => CharacterPsbLoader.GetImporter(PsbPath) != null;
        #endregion

        #region [Unity Methods]
        private void OnEnable()
        {
            titleContent = new GUIContent("Character Asset");
            if (_basePrefab == null) _basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Settings.BasePrefabPath);
            TakeSelection();
        }

        private void OnSelectionChange()
        {
            if (TakeSelection()) Repaint();
        }

        private void OnGUI()
        {
            if (_wrapStyle == null) _wrapStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = false };
            DrawSource();
            EditorGUILayout.Space();
            DrawActions();
            EditorGUILayout.Space();
            DrawResults();
        }
        #endregion

        #region [Public Methods]
        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<CharacterAssetWindow>().Show();
        }

        [MenuItem(ValidateMenuPath)]
        public static void ValidateSelection()
        {
            CharacterAssetWindow window = GetWindow<CharacterAssetWindow>();
            window.TakeSelection();
            window.RunValidation();
            window.Show();
        }

        [MenuItem(ValidateMenuPath, true)]
        public static bool CanValidateSelection()
        {
            return CharacterPsbLoader.IsPsdAsset(Selection.activeObject);
        }
        #endregion

        #region [Private Methods]
        private bool TakeSelection()
        {
            if (!CharacterPsbLoader.IsPsdAsset(Selection.activeObject)) return false;
            Object asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            if (asset == null || asset == _psb) return false;
            SetPsb(asset);
            return true;
        }

        private void SetPsb(Object asset)
        {
            _psb = asset;
            _results.Clear();
            _log.Clear();
            _outputPrefabPath = HasPsb && _basePrefab != null
                ? CharacterPrefabGenerator.GetDefaultOutputPath(PsbPath, AssetDatabase.GetAssetPath(_basePrefab))
                : string.Empty;
        }

        private void DrawSource()
        {
            EditorGUILayout.LabelField("Character PSB", EditorStyles.boldLabel);
            Object picked = EditorGUILayout.ObjectField("PSB file", _psb, typeof(Object), false);
            if (picked != _psb)
            {
                SetPsb(picked != null ? AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(picked)) : null);
                GUIUtility.ExitGUI();
            }
            if (_psb != null && !HasPsb)
                EditorGUILayout.HelpBox("This file is not imported with the PSD Importer. Use a .psb file.", MessageType.Error);
            GameObject pickedBase = (GameObject)EditorGUILayout.ObjectField("Base prefab", _basePrefab, typeof(GameObject), false);
            if (pickedBase != _basePrefab)
            {
                _basePrefab = pickedBase;
                if (HasPsb && _basePrefab != null)
                    _outputPrefabPath = CharacterPrefabGenerator.GetDefaultOutputPath(PsbPath, AssetDatabase.GetAssetPath(_basePrefab));
            }
            _outputPrefabPath = EditorGUILayout.TextField("New prefab path", _outputPrefabPath);
        }

        private void DrawActions()
        {
            using (new EditorGUI.DisabledScope(!HasPsb))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Validate"))
                    {
                        RunValidation();
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Apply Standard")) RunAction("Apply Standard", "Apply the standard importer settings, Automatic Reslice and rig anchor pivots to this PSB? Prefabs that use it will show its parts at their PSB positions.", () => CharacterPsbStandardizer.ApplyStandard(PsbPath, Settings, _log));
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Rebuild Atlas")) RunAction("Rebuild Atlas", "Recreate every sprite rect from its layer (Automatic Reslice) and raise the mosaic padding? Sprite IDs and prefab references are kept.", () => CharacterPsbStandardizer.RebuildAtlas(PsbPath, Settings, _log));
                    if (GUILayout.Button("Export Parts (PNG)")) RunAction("Export Parts", $"Write each part as its own PNG sprite into {CharacterPartExporter.GetExportFolder(PsbPath ?? string.Empty, Settings)}?", () => CharacterPartExporter.Export(PsbPath, Settings, _log));
                }
                using (new EditorGUI.DisabledScope(_basePrefab == null || string.IsNullOrEmpty(_outputPrefabPath)))
                {
                    if (GUILayout.Button("Create Prefab From Base")) RunAction("Create Prefab", $"Copy {(_basePrefab != null ? _basePrefab.name : "the base prefab")} to {_outputPrefabPath} and swap in the six part sprites? The base prefab is not changed.", CreatePrefab);
                }
            }
            if (GUILayout.Button("Open Template Folder"))
                EditorUtility.RevealInFinder(Path.GetFullPath(CharacterAssetSettings.TemplateFolder));
        }

        private void DrawResults()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_results.Count > 0)
            {
                EditorGUILayout.LabelField($"Result: {CharacterPsbValidator.WorstSeverity(_results)}", EditorStyles.boldLabel);
                foreach (CharacterCheckResult result in _results)
                    DrawResult(result);
            }
            if (_log.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
                foreach (string line in _log)
                    EditorGUILayout.LabelField(line, _wrapStyle);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawResult(CharacterCheckResult result)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(SeverityIcon(result.Severity), GUILayout.Width(20), GUILayout.Height(18));
                EditorGUILayout.LabelField(result.Message, _wrapStyle);
            }
        }

        private static GUIContent SeverityIcon(CharacterCheckSeverity severity)
        {
            switch (severity)
            {
                case CharacterCheckSeverity.Error: return EditorGUIUtility.IconContent("console.erroricon.sml");
                case CharacterCheckSeverity.Warning: return EditorGUIUtility.IconContent("console.warnicon.sml");
                case CharacterCheckSeverity.Info: return EditorGUIUtility.IconContent("console.infoicon.sml");
                default: return EditorGUIUtility.IconContent("TestPassed");
            }
        }

        private void RunValidation()
        {
            _results.Clear();
            if (!HasPsb) return;
            _results.AddRange(CharacterPsbValidator.Validate(PsbPath, Settings));
        }

        private void RunAction(string title, string message, System.Action action)
        {
            if (EditorUtility.DisplayDialog(title, message, "Run", "Cancel"))
            {
                _log.Clear();
                try
                {
                    action();
                }
                catch (System.Exception exception)
                {
                    _log.Add($"{title} failed: {exception.Message}");
                    Debug.LogException(exception);
                }
                AssetDatabase.SaveAssets();
                RunValidation();
            }
            GUIUtility.ExitGUI();
        }

        private void CreatePrefab()
        {
            List<CharacterCheckResult> checks = CharacterPsbValidator.Validate(PsbPath, Settings);
            if (CharacterPsbValidator.WorstSeverity(checks) == CharacterCheckSeverity.Error)
            {
                _log.Add("Fix the errors in the validation list before creating a prefab.");
                return;
            }
            string created = CharacterPrefabGenerator.Create(PsbPath, AssetDatabase.GetAssetPath(_basePrefab), _outputPrefabPath, Settings, _log);
            if (!string.IsNullOrEmpty(created))
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(created));
        }
        #endregion
    }
}
