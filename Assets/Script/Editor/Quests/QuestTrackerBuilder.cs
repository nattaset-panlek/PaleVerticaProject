using System.Collections.Generic;
using System.Linq;
using CoreSystem.UI;
using CoreSystem.UI.Quests;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CoreSystem.Editor.Quests
{
    public static class QuestTrackerBuilder
    {
        #region [Variables/Fields]
        private const string WindowPrefabPath = "Assets/Prefab/Game Page/Quest Tracker Window.prefab";
        private const string RegistryPath = "Assets/Prefab/UIWindowRegistry.asset";
        private const string FontPath = "Assets/Art/Front/Kanit/Kanit-SemiBold SDF.asset";
        private static readonly Vector2 PanelSize = new Vector2(320f, 420f);
        private static readonly Vector2 PanelOffset = new Vector2(-24f, -120f);
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/Quests/Build Quest Tracker")]
        public static void Build()
        {
            var root = new GameObject("Quest Tracker Window", typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            var window = root.AddComponent<QuestTrackerWindow>();

            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(root.transform, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = PanelOffset;
            rect.sizeDelta = PanelSize;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.TopRight;
            text.richText = true;
            text.raycastTarget = false;

            var serialized = new SerializedObject(window);
            serialized.FindProperty("_text").objectReferenceValue = text;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, WindowPrefabPath);
            Object.DestroyImmediate(root);
            Register(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Quest tracker built. Press L in Play mode for the Quest Log.");
        }
        #endregion

        #region [Private Methods]
        private static void Register(GameObject prefab)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIWindowRegistrySO>(RegistryPath);
            UIWindowEntry entry = registry.Entries.FirstOrDefault(e => e.WindowName == QuestTrackerWindow.WindowName);
            if (entry == null)
            {
                entry = new UIWindowEntry { WindowName = QuestTrackerWindow.WindowName, Type = UIType.Window, Keys = new List<KeyCode>() };
                registry.Entries.Add(entry);
            }

            entry.Prefab = prefab;
            EditorUtility.SetDirty(registry);
        }
        #endregion
    }
}
