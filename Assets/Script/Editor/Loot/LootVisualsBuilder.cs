using System.IO;
using System.Linq;
using CoreSystem.Loot;
using CoreSystem.UI;
using CoreSystem.UI.Loot;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Editor.Loot
{
    public static class LootVisualsBuilder
    {
        #region [Variables/Fields]
        private const string SettingsPath = "Assets/Resources/Loot/LootVisualSettings.asset";
        private const string DropPrefabPath = "Assets/Prefab/Loot/Ground Item Default.prefab";
        private const string WindowPrefabPath = "Assets/Prefab/Game Page/Loot Label Window.prefab";
        private const string RegistryPath = "Assets/Prefab/UIWindowRegistry.asset";
        private const string FontPath = "Assets/Art/Front/Kanit/Kanit-SemiBold SDF.asset";
        private const float LabelHeight = 22f;
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/Loot/Build Loot Visuals")]
        public static void Build()
        {
            GameObject dropPrefab = EnsureDropPrefab();
            EnsureSettings(dropPrefab);
            GameObject window = BuildWindowPrefab();
            RegisterWindow(window);
            AssetDatabase.SaveAssets();
            Debug.Log("Loot visuals built. Press Z in Play mode to toggle the labels.");
        }
        #endregion

        #region [Private Methods]
        private static GameObject EnsureDropPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(DropPrefabPath);
            if (existing != null) return existing;

            EnsureFolder(DropPrefabPath);
            var go = new GameObject("Ground Item Default");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            go.transform.localScale = Vector3.one * 2f;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, DropPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void EnsureSettings(GameObject dropPrefab)
        {
            var settings = AssetDatabase.LoadAssetAtPath<LootVisualSettings>(SettingsPath);
            if (settings == null)
            {
                EnsureFolder(SettingsPath);
                settings = ScriptableObject.CreateInstance<LootVisualSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            if (settings.DefaultPrefab == null)
            {
                settings.DefaultPrefab = dropPrefab;
                EditorUtility.SetDirty(settings);
            }
        }

        private static GameObject BuildWindowPrefab()
        {
            EnsureFolder(WindowPrefabPath);

            var root = new GameObject("Loot Label Window", typeof(RectTransform));
            Stretch(root.GetComponent<RectTransform>());
            var window = root.AddComponent<LootLabelWindow>();

            var container = new GameObject("Labels", typeof(RectTransform));
            container.transform.SetParent(root.transform, false);
            var containerRect = container.GetComponent<RectTransform>();
            Stretch(containerRect);

            LootLabelView template = BuildLabelTemplate(root.transform);

            var serialized = new SerializedObject(window);
            serialized.FindProperty("_container").objectReferenceValue = containerRect;
            serialized.FindProperty("_labelTemplate").objectReferenceValue = template;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, WindowPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static LootLabelView BuildLabelTemplate(Transform parent)
        {
            var label = new GameObject("Label Template", typeof(RectTransform));
            label.transform.SetParent(parent, false);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(120f, LabelHeight);

            var background = label.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.78f);
            var button = label.AddComponent<Button>();
            button.targetGraphic = background;

            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(label.transform, false);
            Stretch(textObject.GetComponent<RectTransform>());
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            text.fontSize = 14f;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;

            var view = label.AddComponent<LootLabelView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_rect").objectReferenceValue = rect;
            serialized.FindProperty("_text").objectReferenceValue = text;
            serialized.FindProperty("_button").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            label.SetActive(false);
            return view;
        }

        private static void RegisterWindow(GameObject prefab)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIWindowRegistrySO>(RegistryPath);
            UIWindowEntry entry = registry.Entries.FirstOrDefault(e => e.WindowName == LootLabelWindow.WindowName);
            if (entry == null)
            {
                entry = new UIWindowEntry { WindowName = LootLabelWindow.WindowName, Type = UIType.Window };
                registry.Entries.Add(entry);
            }

            entry.Prefab = prefab;
            entry.Keys = new System.Collections.Generic.List<KeyCode> { KeyCode.Z };
            EditorUtility.SetDirty(registry);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string assetPath)
        {
            string fullFolder = Path.GetDirectoryName(Path.GetFullPath(assetPath));
            if (Directory.Exists(fullFolder)) return;
            Directory.CreateDirectory(fullFolder);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
