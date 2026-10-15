using UnityEngine;

namespace CoreSystem.Utility
{
    public static class RuntimeRoot
    {
        #region [Variables/Fields]
        public const string RootName = "[Runtime]";
        private static Transform _root;
        #endregion

        #region [Public Methods]
        public static Transform Get()
        {
            if (_root == null) _root = new GameObject(RootName).transform;
            return _root;
        }

        public static GameObject CreateChild(string name)
        {
            var go = new GameObject("[Runtime] " + name);
            go.transform.SetParent(Get(), false);
            return go;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _root = null;
        }
        #endregion
    }
}
