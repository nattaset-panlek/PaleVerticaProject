using System.Collections.Generic;
using System.Text;
using CoreSystem.UI.Inventory.Utility;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CoreSystem.Editor.UI.EditorUIFactory;

namespace CoreSystem.Editor.UI
{
    public static class ItemSlotPrefabBuilder
    {
        #region [Variables/Fields]
        public const string ArtPrefabPath = "Assets/Prefab/UI/ItemSlot Art.prefab";
        public const string SlotPrefabPath = "Assets/Prefab/UI/ItemSlot.prefab";
        private const string ArtFolder = "Assets/Art/UI/New/ItemSlot/";
        private const string FramePath = ArtFolder + "UI_ItemSlot_Frame.png";
        private const string WedgePath = ArtFolder + "UI_ItemSlot_RarityWedge.png";
        private const string BorderPath = ArtFolder + "UI_ItemSlot_Border.png";
        private const string SocketFilledPath = ArtFolder + "UI_ItemSlot_SocketFilled.png";
        private const string SocketEmptyPath = ArtFolder + "UI_ItemSlot_SocketEmpty.png";
        private const string StarPath = "Assets/Art/UI/New/Icon/UI_StarGylp_Icon.png";
        private const float SocketPitch = 16f;
        private const float StarPitch = 15.6f;
        private static readonly Color StarOff = new Color(0.2156863f, 0.2156863f, 0.2156863f, 1f);
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Build ItemSlot")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Build ItemSlot", $"Rebuilds\n{ArtPrefabPath}\nand the children of\n{SlotPrefabPath}\n(the root, its GUID and its ItemSlotSquare stay).", "Build", "Cancel")) return;
            Debug.Log(Build());
        }

        public static void BuildBatch()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(Build());
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static string Build()
        {
            var log = new StringBuilder("[ItemSlotPrefabBuilder]");
            PrepareSprites();
            BuildArtPrefab();
            log.Append($"\n- {ArtPrefabPath}");
            BuildSlotPrefab();
            log.Append($"\n- {SlotPrefabPath}");
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        public static ItemSlotArtView AddArt(Transform parent, string name = "ItemSlot Art")
        {
            var prefab = Load<GameObject>(ArtPrefabPath);
            if (prefab == null) throw new System.InvalidOperationException($"Missing {ArtPrefabPath}. Run Pale Vertica > UI > Build ItemSlot first.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            var rect = (RectTransform)instance.transform;
            StretchFull(rect);
            var view = instance.GetComponent<ItemSlotArtView>();
            view.Fit();
            return view;
        }
        #endregion

        #region [Private Methods]
        private static void PrepareSprites()
        {
            foreach (string path in new[] { FramePath, WedgePath, BorderPath, SocketFilledPath, SocketEmptyPath }) PrepareSprite(path, Vector4.zero);
        }

        private static void BuildArtPrefab()
        {
            GameObject root;
            bool exists = Load<GameObject>(ArtPrefabPath) != null;
            root = exists ? PrefabUtility.LoadPrefabContents(ArtPrefabPath) : new GameObject("ItemSlot Art", typeof(RectTransform));
            try
            {
                root.layer = 5;
                var rootRect = (RectTransform)root.transform;
                rootRect.sizeDelta = new Vector2(ItemSlotArtView.ArtSize, ItemSlotArtView.ArtSize);
                ClearChildren(root.transform);
                var view = root.GetComponent<ItemSlotArtView>() ?? root.AddComponent<ItemSlotArtView>();
                FillArt(view, root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, ArtPrefabPath);
            }
            finally
            {
                if (exists) PrefabUtility.UnloadPrefabContents(root);
                else Object.DestroyImmediate(root);
            }
        }

        private static void FillArt(ItemSlotArtView view, Transform root)
        {
            TMP_FontAsset font = Load<TMP_FontAsset>(NovecentoPath);

            RectTransform art = NewChild(root, "Art");
            art.anchorMin = art.anchorMax = art.pivot = new Vector2(0.5f, 0.5f);
            art.anchoredPosition = Vector2.zero;
            art.sizeDelta = new Vector2(ItemSlotArtView.ArtSize, ItemSlotArtView.ArtSize);

            Image frame = AddImage(art, "Frame", LoadSprite(FramePath), Color.white, 0f, 0f, 128f, 128f);
            Image wedge = AddImage(art, "Rarity Wedge", LoadSprite(WedgePath), Color.white, 0f, 0f, 128f, 128f);
            Image border = AddImage(art, "Equipped Border", LoadSprite(BorderPath), Color.white, 0f, 0f, 128f, 128f);
            Image icon = AddImage(art, "Icon", null, Color.white, 32f, 32f, 64f, 64f);
            icon.preserveAspect = true;

            RectTransform socketRow = NewChild(art, "Sockets");
            SetTopLeft(socketRow, 12f, 8f, 64f, 16f);
            Sprite socketFilled = LoadSprite(SocketFilledPath);
            Sprite socketEmpty = LoadSprite(SocketEmptyPath);
            var sockets = new List<Image>();
            for (int i = 0; i < ItemSlotDisplay.MaxSockets; i++)
                sockets.Add(AddImage(socketRow, $"Socket {i + 1}", socketEmpty, Color.white, i * SocketPitch, 0f, 16f, 16f));

            RectTransform starColumn = NewChild(art, "Refine");
            SetTopLeft(starColumn, 104f, 11f, 13f, 75.4f);
            Sprite star = LoadSprite(StarPath);
            var stars = new List<Image>();
            for (int i = 0; i < ItemSlotDisplay.MaxStars; i++)
                stars.Add(AddImage(starColumn, $"Star {i + 1}", star, StarOff, 0f, i * StarPitch, 13f, 13f));

            RectTransform advancement = NewChild(art, "Advancement");
            SetTopLeft(advancement, 0f, 0f, 128f, 128f);
            AddText(advancement, "Prefix", "+", font, 24f, Color.white, TextAlignmentOptions.TopLeft, 14f, 89f, 17f, 29f);
            TextMeshProUGUI advancementValue = AddText(advancement, "Value", "999", font, 24f, Color.white, TextAlignmentOptions.TopLeft, 30f, 89f, 80f, 29f);

            RectTransform quantity = NewChild(art, "Quantity");
            SetTopLeft(quantity, 0f, 0f, 128f, 128f);
            AddText(quantity, "Prefix", "x", font, 16f, Color.white, TextAlignmentOptions.TopLeft, 14f, 97f, 10f, 19f);
            TextMeshProUGUI quantityValue = AddText(quantity, "Value", "999", font, 24f, Color.white, TextAlignmentOptions.TopLeft, 25f, 89f, 80f, 29f);

            SetObject(view, "_art", art);
            SetObject(view, "_frame", frame);
            SetObject(view, "_wedge", wedge);
            SetObject(view, "_border", border);
            SetObject(view, "_icon", icon);
            SetObject(view, "_socketRow", socketRow.gameObject);
            SetObjects(view, "_sockets", sockets.ToArray());
            SetObject(view, "_socketFilled", socketFilled);
            SetObject(view, "_socketEmpty", socketEmpty);
            SetObject(view, "_starColumn", starColumn.gameObject);
            SetObjects(view, "_stars", stars.ToArray());
            SetObject(view, "_advancement", advancement.gameObject);
            SetObject(view, "_advancementValue", advancementValue);
            SetObject(view, "_quantity", quantity.gameObject);
            SetObject(view, "_quantityValue", quantityValue);

            view.Clear();
        }

        private static void BuildSlotPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SlotPrefabPath);
            try
            {
                ClearChildren(root.transform);
                var image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
                image.sprite = null;
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = true;

                ItemSlotArtView art = AddArt(root.transform);
                var slot = root.GetComponent<ItemSlotSquare>() ?? root.AddComponent<ItemSlotSquare>();
                SetObject(slot, "_art", art);
                SetObject(slot, "ItemIcon", art.Icon);
                SetObject(slot, "RarityFrame", art.Wedge);
                PrefabUtility.SaveAsPrefabAsset(root, SlotPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        #endregion
    }
}
