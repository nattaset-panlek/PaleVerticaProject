namespace CoreSystem.Player
{
    public readonly struct PlayerDeathInfo
    {
        #region [Variables/Fields]
        public static readonly PlayerDeathInfo Sample = new PlayerDeathInfo("Axnise", 24, "Volcanic Ridge", "Skeleton Warrior");

        public readonly string CharacterName;
        public readonly int Level;
        public readonly string Location;
        public readonly string KillerName;
        #endregion

        #region [Properties]
        public string Epitaph => Level > 0 ? $"{CharacterName}, Lv. {Level}" : CharacterName;
        #endregion

        #region [Public Methods]
        public PlayerDeathInfo(string characterName, int level, string location, string killerName)
        {
            CharacterName = string.IsNullOrEmpty(characterName) ? "Unknown" : characterName;
            Level = level;
            Location = string.IsNullOrEmpty(location) ? "Unknown" : location;
            KillerName = string.IsNullOrEmpty(killerName) ? "Unknown" : killerName;
        }
        #endregion
    }
}
