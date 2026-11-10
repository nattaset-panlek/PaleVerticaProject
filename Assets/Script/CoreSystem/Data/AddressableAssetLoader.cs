using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CoreSystem.Data
{
    public static class AddressableAssetLoader
    {
        #region [Public Methods]
        public static bool Exists<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(key, typeof(T));
            IList<IResourceLocation> locations = handle.WaitForCompletion();
            bool found = handle.Status == AsyncOperationStatus.Succeeded && locations != null && locations.Count > 0;
            Addressables.Release(handle);
            return found;
        }

        public static T TryLoad<T>(string key) where T : UnityEngine.Object
        {
            if (!Exists<T>(key)) return null;
            return Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
        }
        #endregion
    }
}
