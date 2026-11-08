using System.Text;
using CoreSystem.UI.GameInterface;
using CoreSystem.UI.Inventory.Utility;
using CoreSystem.UI.Refinement;
using CoreSystem.UI.Socket;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CoreSystem.Editor.UI.EditorUIFactory;

namespace CoreSystem.Editor.UI
{
    public static class ItemSlotAdoptionBuilder
    {
        #region [Variables/Fields]
        public const string EquipmentWindowPath = "Assets/Prefab/Game Page/Character Equipment Window.prefab";
        public const string RefinementWindowPath = "Assets/Prefab/Game Page/Refinement Window.prefab";
        public const string SocketWindowPath = "Assets/Prefab/Game Page/Socket Window.prefab";
        private const string ArtName = "ItemSlot Art";
        #endregion

        #region [Public Methods]
        [MenuItem("Pale Vertica/UI/Apply ItemSlot To Windows")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Apply ItemSlot To Windows", $"Puts the ItemSlot art into the item slots of\n{EquipmentWindowPath}\n{RefinementWindowPath}\n{SocketWindowPath}\nThe old icon objects are switched off, not deleted.", "Apply", "Cancel")) return;
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
            var log = new StringBuilder("[ItemSlotAdoptionBuilder]");
            log.Append($"\n- {EquipmentWindowPath}: {Edit(EquipmentWindowPath, AdoptEquipment)} slot(s)");
            log.Append($"\n- {RefinementWindowPath}: {Edit(RefinementWindowPath, AdoptRefinement)} slot(s)");
            log.Append($"\n- {SocketWindowPath}: {Edit(SocketWindowPath, AdoptSocket)} slot(s)");
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        public static void ApplyToRefinement(GameObject root) => AdoptRefinement(root);

        public static void ApplyToSocket(GameObject root) => AdoptSocket(root);
        #endregion

        #region [Private Methods]
        private static int Edit(string path, System.Func<GameObject, int> adopt)
        {
            if (Load<GameObject>(path) == null) return 0;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int count = adopt(root);
                if (count > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
                return count;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int AdoptEquipment(GameObject root)
        {
            int count = 0;
            foreach (EquipmentSlotUI slot in root.GetComponentsInChildren<EquipmentSlotUI>(true))
            {
                AdoptCharacterSlot(slot, slot.transform);
                count++;
            }
            foreach (CostumeSlotUI slot in root.GetComponentsInChildren<CostumeSlotUI>(true))
            {
                AdoptCharacterSlot(slot, slot.transform);
                count++;
            }
            return count;
        }

        private static void AdoptCharacterSlot(MonoBehaviour slot, Transform transform)
        {
            ItemSlotArtView art = ReplaceArt(transform, 0);
            Transform placeholder = transform.Find("Icon_slot");
            if (placeholder != null) placeholder.SetSiblingIndex(art.transform.GetSiblingIndex() + 1);
            Deactivate(transform, "Icon");
            Deactivate(transform, "rarity");
            SetObject(slot, "_art", art);
            SetObject(slot, "_emptyIcon", placeholder != null ? placeholder.gameObject : null);
            SetObject(slot, "Icon", art.Icon);
            SetObject(slot, "RarityFrame", art.Wedge);
        }

        private static int AdoptRefinement(GameObject root)
        {
            int count = 0;
            foreach (RefinementCenterSlot slot in root.GetComponentsInChildren<RefinementCenterSlot>(true))
            {
                ItemSlotArtView art = ReplaceArt(slot.transform, -1);
                Deactivate(slot.transform, "Item Icon");
                SetObject(slot, "_art", art);
                SetObject(slot, "_itemIcon", art.Icon);
                count++;
            }
            return count;
        }

        private static int AdoptSocket(GameObject root)
        {
            var window = root.GetComponentInChildren<SocketWindow>(true);
            Transform center = root.transform.Find("Center Item Slot");
            if (window == null || center == null) return 0;
            ItemSlotArtView art = ReplaceArt(center, -1);
            Deactivate(center, "Item Icon");
            SetObject(window, "_itemArt", art);
            SetObject(window, "_itemIcon", art.Icon);
            return 1;
        }

        private static ItemSlotArtView ReplaceArt(Transform parent, int siblingIndex)
        {
            Transform old = parent.Find(ArtName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            ItemSlotArtView art = ItemSlotPrefabBuilder.AddArt(parent, ArtName);
            if (siblingIndex >= 0) art.transform.SetSiblingIndex(siblingIndex);
            foreach (Graphic graphic in art.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            art.Clear();
            return art;
        }

        private static void Deactivate(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) child.gameObject.SetActive(false);
        }
        #endregion
    }
}
