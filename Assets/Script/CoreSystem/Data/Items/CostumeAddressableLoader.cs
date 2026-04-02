using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data.Items
{
    /// <summary>
    /// Handles async loading and lifetime management of Addressable assets for Costume visuals.
    /// Each CostumeLoadSession tracks all handles for one costume slot — call Release() on Unequip.
    /// </summary>
    public class CostumeLoadSession
    {
        public List<AsyncOperationHandle> Handles = new List<AsyncOperationHandle>();

        public void Release()
        {
            foreach (var handle in Handles)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            Handles.Clear();
        }
    }

    public static class CostumeAddressableLoader
    {
        /// <summary>
        /// Load all Sprite layers for a costume asynchronously.
        /// Invokes onLoaded(spriteIndex, sprite) for each loaded sprite.
        /// Pass the session so handles can be released on Unequip.
        /// </summary>
        public static IEnumerator LoadSpritesAsync(
            Costume costume,
            CostumeLoadSession session,
            System.Action<int, Sprite> onLoaded)
        {
            if (costume.VisualSpriteKeys == null) yield break;

            for (int i = 0; i < costume.VisualSpriteKeys.Count; i++)
            {
                string key = costume.VisualSpriteKeys[i];
                if (string.IsNullOrEmpty(key)) continue;

                int capturedIndex = i;
                var handle = Addressables.LoadAssetAsync<Sprite>(key);
                session.Handles.Add(handle);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke(capturedIndex, handle.Result);
                }
                else
                {
                    Debug.LogWarning($"[CostumeLoader] Failed to load sprite: {key}");
                }
            }
        }

        /// <summary>
        /// Load all visual prefabs for a costume asynchronously.
        /// Invokes onLoaded(prefabData, gameObject) for each loaded prefab.
        /// </summary>
        public static IEnumerator LoadPrefabsAsync(
            Costume costume,
            CostumeLoadSession session,
            System.Action<CostumeVisualPrefabData, GameObject> onLoaded)
        {
            if (costume.VisualPrefabData == null) yield break;

            foreach (var pData in costume.VisualPrefabData)
            {
                if (string.IsNullOrEmpty(pData.PrefabKey)) continue;

                var capturedData = pData;
                var handle = Addressables.LoadAssetAsync<GameObject>(pData.PrefabKey);
                session.Handles.Add(handle);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke(capturedData, handle.Result);
                }
                else
                {
                    Debug.LogWarning($"[CostumeLoader] Failed to load prefab: {pData.PrefabKey}");
                }
            }
        }
    }
}
