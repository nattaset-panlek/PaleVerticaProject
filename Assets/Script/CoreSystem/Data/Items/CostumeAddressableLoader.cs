using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CoreSystem.Data.Items.Model;

namespace CoreSystem.Data.Items
{
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
            System.Action<int, Sprite, bool> onLoaded)
        {
            if (costume.VisualSpriteKeys == null) yield break;

            for (int i = 0; i < costume.VisualSpriteKeys.Count; i++)
            {
                string key = costume.VisualSpriteKeys[i];
                if (string.IsNullOrEmpty(key)) continue;

                int capturedIndex = i;

                string baseKey = key;
                string subSpriteName = null;

                if (key.Contains(":"))
                {
                    var parts = key.Split(':');
                    baseKey = parts[0];
                    if (parts.Length > 1) subSpriteName = parts[1];
                }
                else if (key.Contains("["))
                {
                    int bracketStart = key.IndexOf('[');
                    baseKey = key.Substring(0, bracketStart);
                    subSpriteName = key.Substring(bracketStart + 1).TrimEnd(']');
                }

                // กรณีที่ระบุชื่อ Sprite ย่อยมาด้วย (เช่น :Head) หรือเป็นไฟล์ .psb ตัวเต็ม
                if (!string.IsNullOrEmpty(subSpriteName) || baseKey.ToLower().EndsWith(".psb"))
                {
                    AsyncOperationHandle<IList<Sprite>> multiHandle = default;
                    bool handleStarted = false;

                    // รายการ Key ที่เป็นไปได้ เพื่อความยืดหยุ่น (เผื่อพิมพ์มาแค่แบบย่อ)
                    string[] possibleKeys = new string[] 
                    {
                        baseKey,
                        baseKey + ".psb",
                        "Assets/Addressable/" + baseKey + ".psb",
                        "Assets/Addressable/" + baseKey
                    };

                    string validKey = null;

                    // พยายามโหลดดูทีละ Key ถ้าติด Exception ข้ามไปลองอันถัดไป
                    foreach (var pKey in possibleKeys)
                    {
                        try
                        {
                            var checkHandle = Addressables.LoadResourceLocationsAsync(pKey, typeof(Sprite));
                            checkHandle.WaitForCompletion(); // เช็คแบบไวๆ เบื้องหลัง
                            if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
                            {
                                validKey = pKey;
                                break;
                            }
                        }
                        catch (UnityEngine.AddressableAssets.InvalidKeyException) { /* ข้าม */ }
                    }

                    if (validKey != null)
                    {
                        multiHandle = Addressables.LoadAssetAsync<IList<Sprite>>(validKey);
                        session.Handles.Add(multiHandle);
                        yield return multiHandle;
                        handleStarted = true;
                    }

                    if (handleStarted && multiHandle.Status == AsyncOperationStatus.Succeeded && multiHandle.Result != null)
                    {
                        if (!string.IsNullOrEmpty(subSpriteName))
                        {
                            // หาตัวที่ตรงกับชื่อที่ระบุ
                            bool found = false;
                            foreach (var sprite in multiHandle.Result)
                            {
                                if (sprite != null && sprite.name == subSpriteName)
                                {
                                    onLoaded?.Invoke(capturedIndex, sprite, false);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) Debug.LogWarning($"[CostumeLoader] Sub-sprite '{subSpriteName}' not found in '{baseKey}'");
                        }
                        else
                        {
                            // โหลดหมดทุกชิ้น (Auto-map)
                            foreach (var sprite in multiHandle.Result)
                            {
                                onLoaded?.Invoke(capturedIndex, sprite, true);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[CostumeLoader] Failed to load multiple sprites from base key: {baseKey}");
                    }
                }
                else
                {
                    var handle = Addressables.LoadAssetAsync<Sprite>(key);
                    session.Handles.Add(handle);
                    yield return handle;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        onLoaded?.Invoke(capturedIndex, handle.Result, false);
                    }
                    else
                    {
                        Debug.LogWarning($"[CostumeLoader] Failed to load sprite: {key}");
                    }
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
