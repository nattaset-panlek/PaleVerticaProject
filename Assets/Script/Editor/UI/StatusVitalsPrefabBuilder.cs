using System;
using CoreSystem.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CoreSystem.Editor.UI
{
    public static class StatusVitalsPrefabBuilder
    {
        #region [Variables/Fields]
        public const string StyleName = "Status Vitals";
        public const string PrefabPath = "Assets/Prefab/Game Page/Global/UIElement/Status Vitals.prefab";
        public const string SourcePrefabPath = "Assets/Prefab/Game Page/StatusModal Window.prefab";
        private const string RegistryPath = "Assets/Prefab/UIElementRegistry.asset";
        private const string SourceContentPath = "Content";

        private static readonly (string Key, string Path)[] Mappings =
        {
            ("HP Fill", "Status/HealPoint/Frame/Bar/Value"),
            ("HP Value", "Status/HealPoint/ValueText"),
            ("MP Fill", "Status/ManaPoint/Frame/Bar/Value"),
            ("MP Value", "Status/ManaPoint/ValueText"),
            ("EXP Fill", "Status/Exp/Frame/Bar/Value"),
            ("EXP Value", "Status/Exp/ValueText"),
            ("Base", "Level/Base/Value"),
            ("Job", "Level/Job/Value")
        };
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build Status Vitals Element")]
        public static void BuildFromMenu()
        {
            bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
            if (exists && !EditorUtility.DisplayDialog("Build Status Vitals Element", $"{PrefabPath} will be overwritten from {SourcePrefabPath} and the '{StyleName}' registry entry will be updated.", "Build", "Cancel")) return;
            Debug.Log($"[StatusVitalsPrefabBuilder] Built {AssetDatabase.GetAssetPath(Build())}");
        }

        public static void BuildBatch()
        {
            int exitCode = 0;
            try
            {
                Build();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static GameObject Build()
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIElementRegistrySO>(RegistryPath);
            if (registry == null) throw new InvalidOperationException($"UIElementRegistry not found at {RegistryPath}.");

            GameObject source = PrefabUtility.LoadPrefabContents(SourcePrefabPath);
            try
            {
                var content = source.transform.Find(SourceContentPath) as RectTransform;
                if (content == null) throw new InvalidOperationException($"'{SourceContentPath}' not found in {SourcePrefabPath}.");

                Vector2 size = content.rect.size;
                GameObject root = Object.Instantiate(content.gameObject, source.transform, false);
                root.transform.SetParent(null, false);
                root.name = StyleName;
                Shape((RectTransform)root.transform, size);
                FitTextHeights(root);
                Map(root);

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Object.DestroyImmediate(root);
                Register(registry, prefab);
                return prefab;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(source);
            }
        }
        #endregion

        #region [Private Methods]
        private static void Shape(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            var layout = rect.GetComponent<LayoutElement>();
            if (layout == null) layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = size.x;
            layout.preferredWidth = size.x;
            layout.minHeight = size.y;
            layout.preferredHeight = size.y;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;
        }

        private static void FitTextHeights(GameObject root)
        {
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                RectTransform rect = text.rectTransform;
                float needed = Mathf.Ceil(text.GetPreferredValues(text.text, Mathf.Max(1f, rect.rect.width), 0f).y);
                float grow = needed - rect.rect.height;
                if (grow <= 0f) continue;

                float keep = text.verticalAlignment == VerticalAlignmentOptions.Top ? 1f : text.verticalAlignment == VerticalAlignmentOptions.Bottom ? 0f : 0.5f;
                rect.sizeDelta += new Vector2(0f, grow);
                rect.anchoredPosition += new Vector2(0f, grow * (rect.pivot.y - keep));
            }
        }

        private static void Map(GameObject root)
        {
            var custom = root.GetComponent<UICustomElement>();
            if (custom == null) custom = root.AddComponent<UICustomElement>();

            var serialized = new SerializedObject(custom);
            SerializedProperty list = serialized.FindProperty("_elements");
            list.ClearArray();
            foreach (var (key, path) in Mappings)
            {
                Transform target = root.transform.Find(path);
                if (target == null) throw new InvalidOperationException($"'{path}' not found under {SourceContentPath} in {SourcePrefabPath}.");

                list.InsertArrayElementAtIndex(list.arraySize);
                SerializedProperty item = list.GetArrayElementAtIndex(list.arraySize - 1);
                item.FindPropertyRelative("Key").stringValue = key;
                item.FindPropertyRelative("Target").objectReferenceValue = target.gameObject;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Register(UIElementRegistrySO registry, GameObject prefab)
        {
            var entry = registry.GetEntry(UIElementType.Custom, StyleName);
            if (entry == null)
            {
                entry = new UIElementEntry { StyleName = StyleName, Type = UIElementType.Custom };
                registry.Entries.Add(entry);
            }
            entry.Prefab = prefab;
            entry.TargetComponentPath = string.Empty;
            entry.MinSize = Vector2.zero;
            entry.FixedSize = Vector2.zero;
            entry.CornerRadius = 0f;
            entry.KeepPrefabLook = true;
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssetIfDirty(registry);
        }
        #endregion
    }
}
