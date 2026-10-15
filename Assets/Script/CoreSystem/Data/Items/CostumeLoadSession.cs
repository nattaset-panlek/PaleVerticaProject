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
}
