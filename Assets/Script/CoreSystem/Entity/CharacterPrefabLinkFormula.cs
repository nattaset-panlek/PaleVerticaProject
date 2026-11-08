using System.Collections.Generic;

namespace CoreSystem.Entity
{
    public static class CharacterPrefabLinkFormula
    {
        #region [Public Methods]
        public static bool IsLinked(int prefabCharacterId, IEnumerable<int> idsUsingPrefab)
        {
            if (prefabCharacterId <= 0 || idsUsingPrefab == null) return false;

            foreach (int id in idsUsingPrefab)
            {
                if (id == prefabCharacterId) return true;
            }
            return false;
        }
        #endregion
    }
}
