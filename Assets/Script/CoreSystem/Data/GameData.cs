namespace CoreSystem.Data
{
    public static class GameData
    {
        public static PlayerData Player { get; private set; } = new PlayerData();

        public static void ResetPlayer()
        {
            Player = new PlayerData();
        }
    }
}
