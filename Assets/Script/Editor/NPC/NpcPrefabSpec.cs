using CoreSystem.Entity;

namespace CoreSystem.Editor.NPC
{
    public class NpcPrefabSpec
    {
        #region [Variables/Fields]
        public readonly string Name;
        public readonly string PsbPath;
        public readonly string PrefabPath;
        public readonly string NpcId;
        public readonly int CharacterId;
        public readonly CharacterTag Tags;
        public readonly string MerchantWindow;
        public readonly string MerchantLabel;
        #endregion

        #region [Properties]
        public bool IsMerchant => !string.IsNullOrEmpty(MerchantWindow);
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        public NpcPrefabSpec(string name, string psbPath, string prefabPath, string npcId, int characterId, CharacterTag tags, string merchantWindow = null, string merchantLabel = null)
        {
            Name = name;
            PsbPath = psbPath;
            PrefabPath = prefabPath;
            NpcId = npcId;
            CharacterId = characterId;
            Tags = tags;
            MerchantWindow = merchantWindow;
            MerchantLabel = string.IsNullOrEmpty(merchantLabel) ? merchantWindow : merchantLabel;
        }

        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region [Private Methods]
        #endregion
    }
}
