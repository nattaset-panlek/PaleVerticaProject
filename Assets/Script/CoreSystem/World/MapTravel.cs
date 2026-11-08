using CoreSystem.UI.Loading;

namespace CoreSystem.World
{
    public static class MapTravel
    {
        #region [Public Methods]
        public static void Travel(string sceneName, string spawnId)
        {
            GameCoreLoader.SetNextSpawn(spawnId);
            if (!LoadingManager.LoadScene(sceneName)) GameCoreLoader.SetNextSpawn(null);
        }
        #endregion
    }
}
