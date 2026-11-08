using CoreSystem.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class UIArtStyleTests
    {
        #region [Variables/Fields]
        private static readonly Color ArtDark = new Color(0.12f, 0.12f, 0.12f, 1f);
        private static readonly Color ArtLabel = new Color(0.79f, 0.79f, 0.79f, 1f);
        private static readonly Color ChipColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        private GameObject _root;
        private GameObject _secondaryPrefab;
        private GameObject _primaryPrefab;
        private GameObject _chipPrefab;
        private GameObject _keyValuePrefab;
        private UIElementRegistrySO _registry;
        private UIDocument _document;
        #endregion

        #region [Public Methods]
        [SetUp]
        public void SetUp()
        {
            _secondaryPrefab = CreateButtonPrefab("Secondary", ArtDark, ArtLabel, 18f);
            _primaryPrefab = CreateButtonPrefab("Primary", Color.white, ArtDark, 18f);
            _chipPrefab = CreateButtonPrefab("Chip", ChipColor, ArtDark, 12f);
            _keyValuePrefab = CreateKeyValuePrefab();

            _registry = ScriptableObject.CreateInstance<UIElementRegistrySO>();
            _registry.Entries.Add(new UIElementEntry { StyleName = UIBuilderContext.DefaultButtonStyle, Type = UIElementType.Button, Prefab = _secondaryPrefab, KeepPrefabLook = true });
            _registry.Entries.Add(new UIElementEntry { StyleName = UIBuilderContext.PrimaryButtonStyle, Type = UIElementType.Button, Prefab = _primaryPrefab, KeepPrefabLook = true });
            _registry.Entries.Add(new UIElementEntry { StyleName = "Nav Chip", Type = UIElementType.Button, Prefab = _chipPrefab, KeepPrefabLook = true });
            _registry.Entries.Add(new UIElementEntry { StyleName = UIBuilderContext.KeyValueStyle, Type = UIElementType.Custom, Prefab = _keyValuePrefab });

            _root = new GameObject("Root", typeof(RectTransform));
            var group = _root.AddComponent<VerticalLayoutGroup>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            _document = new UIDocument((RectTransform)_root.transform, _registry);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_secondaryPrefab);
            Object.DestroyImmediate(_primaryPrefab);
            Object.DestroyImmediate(_chipPrefab);
            Object.DestroyImmediate(_keyValuePrefab);
            Object.DestroyImmediate(_registry);
        }

        [Test]
        public void ArtButton_Danger_RecoloursOnlyTheLabel()
        {
            _document.Build(ui => ui.Button("Delete", null, UIVariant.Danger).Id("button"));

            var button = _document.Find("button");
            Assert.AreEqual(ArtDark, button.GameObject.GetComponent<Image>().color);
            Assert.AreEqual(_registry.Theme.Danger, button.TextComponent.color);
        }

        [Test]
        public void ArtButton_Selected_UsesPrimaryArt()
        {
            _document.Build(ui => ui.Button("Pick", null, UIVariant.Selected).Id("button"));

            var button = _document.Find("button");
            Assert.AreEqual(Color.white, button.GameObject.GetComponent<Image>().color);
            Assert.AreEqual(ArtDark, button.TextComponent.color);
        }

        [Test]
        public void ArtButton_KeepsPrefabFontSizeAndPadding()
        {
            _document.Build(ui => ui.Button("Save", null).Id("button"));

            var button = _document.Find("button");
            Assert.AreEqual(18f, button.TextComponent.fontSize, 0.01f);
            Assert.AreEqual(20, button.GameObject.GetComponent<HorizontalLayoutGroup>().padding.left);
        }

        [Test]
        public void CustomNavStyle_KeepsPrefabLookForPagesNotSelected()
        {
            _document.Build(ui => ui.Pages("tool", pages =>
            {
                pages.NavButtonStyle = "Nav Chip";
                pages.Add("A", _ => { }).Add("B", _ => { });
            }, UIPageNav.SideMenu));

            var buttons = _document.Find(UIDocument.NavId("tool")).GameObject.GetComponentsInChildren<Button>();
            Assert.AreEqual(2, buttons.Length);
            Assert.AreEqual(_registry.Theme.Selected, buttons[0].GetComponent<Image>().color);
            Assert.AreEqual(ChipColor, buttons[1].GetComponent<Image>().color);
        }

        [Test]
        public void KeyValue_UsesRegisteredStyle()
        {
            _document.Build(ui => ui.KeyValue("Rarity", "Unique").Id("pair"));

            var custom = _document.Find("pair").GameObject.GetComponent<UICustomElement>();
            Assert.IsNotNull(custom);
            Assert.AreEqual("Rarity", custom.Get<TMP_Text>("Key").text);
            Assert.AreEqual("Unique", custom.Get<TMP_Text>("Value").text);
        }

        [Test]
        public void SectionHeader_WithoutStyle_FallsBackToHeading()
        {
            _document.Build(ui => ui.SectionHeader("DETAILS").Id("header"));

            var text = _document.Find("header").TextComponent;
            Assert.AreEqual("DETAILS", text.text);
            Assert.AreEqual(_registry.Theme.HeadingSize, text.fontSize, 0.01f);
        }

        [Test]
        public void RoundedBadge_DoesNotForceItsSize()
        {
            _document.Build(ui => ui.Badge("TAG", Color.red).Id("badge").CornerRadius(10f));

            var rect = _document.Find("badge").RectTransform;
            Assert.Less(LayoutUtility.GetPreferredHeight(rect), 30f);
        }

        [Test]
        public void Badge_UsesDarkTextOnLightColours()
        {
            _document.Build(ui =>
            {
                ui.Badge("LIGHT", new Color(1f, 0.73f, 0f, 1f)).Id("light");
                ui.Badge("DARK", new Color(0.2f, 0.2f, 0.2f, 1f)).Id("dark");
            });

            Assert.AreEqual(_registry.Theme.Text, _document.Find("light").TextComponent.color);
            Assert.AreEqual(_registry.Theme.TextOnAccent, _document.Find("dark").TextComponent.color);
        }
        #endregion

        #region [Private Methods]
        private static GameObject CreateButtonPrefab(string name, Color art, Color ink, float fontSize)
        {
            var prefab = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup));
            prefab.GetComponent<Image>().color = art;
            prefab.GetComponent<Button>().targetGraphic = prefab.GetComponent<Image>();
            prefab.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(20, 20, 6, 6);

            var label = new GameObject("Text", typeof(RectTransform));
            label.transform.SetParent(prefab.transform, false);
            var text = label.AddComponent<TextMeshProUGUI>();
            text.color = ink;
            text.fontSize = fontSize;
            return prefab;
        }

        private static GameObject CreateKeyValuePrefab()
        {
            var prefab = new GameObject("KeyValue", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var key = new GameObject("Key", typeof(RectTransform));
            key.transform.SetParent(prefab.transform, false);
            key.AddComponent<TextMeshProUGUI>();
            var value = new GameObject("Value", typeof(RectTransform));
            value.transform.SetParent(prefab.transform, false);
            value.AddComponent<TextMeshProUGUI>();

            var custom = prefab.AddComponent<UICustomElement>();
            var so = new SerializedObject(custom);
            var list = so.FindProperty("_elements");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).FindPropertyRelative("Key").stringValue = "Key";
            list.GetArrayElementAtIndex(0).FindPropertyRelative("Target").objectReferenceValue = key;
            list.GetArrayElementAtIndex(1).FindPropertyRelative("Key").stringValue = "Value";
            list.GetArrayElementAtIndex(1).FindPropertyRelative("Target").objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return prefab;
        }
        #endregion
    }
}
