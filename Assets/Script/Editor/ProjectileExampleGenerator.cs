using UnityEditor;
using UnityEngine;
using System.IO;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Editor
{
    public static class ProjectileExampleGenerator
    {
        #region [Variables/Fields]
        private static readonly string FolderPath = "Assets/Addressable/Prefab/Effect";
        private static readonly string PhysicalPath = "Assets/Addressable/Prefab/Effect/PhysicalProjectile.prefab";
        private static readonly string MagicPath = "Assets/Addressable/Prefab/Effect/MagicProjectile.prefab";
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [MenuItem("Tools/Projectiles/Generate Sample Projectiles")]
        public static void GenerateSampleProjectiles()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            CreateProjectilePrefab(PhysicalPath, Color.red, 12f);
            CreateProjectilePrefab(MagicPath, Color.cyan, 8f);

            AssetDatabase.Refresh();
        }
        #endregion

        #region [Private Methods]
        private static void CreateProjectilePrefab(string path, Color color, float speed)
        {
            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(path));

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            ProjectileBehaviour pb = go.AddComponent<ProjectileBehaviour>();
            pb.Speed = speed;
            pb.DestroyOnHit = true;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            RegisterAddressable(path, path);
        }

        private static void RegisterAddressable(string assetPath, string address)
        {
#if UNITY_EDITOR
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var group = settings.DefaultGroup;
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                var entry = settings.CreateOrMoveEntry(guid, group);
                if (entry != null)
                {
                    entry.address = address;
                    settings.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
                }
            }
#endif
        }
        #endregion
    }
}
