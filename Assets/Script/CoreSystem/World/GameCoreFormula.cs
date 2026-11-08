using System.Collections.Generic;

namespace CoreSystem.World
{
    public static class GameCoreFormula
    {
        #region [Public Methods]
        public static bool SceneUsesCore(string sceneName, IReadOnlyCollection<string> scenesWithoutCore)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            if (scenesWithoutCore == null) return true;

            foreach (string excluded in scenesWithoutCore)
            {
                if (excluded == sceneName) return false;
            }
            return true;
        }

        public static string ContinueScene(string savedScene, string fallbackScene)
        {
            return string.IsNullOrEmpty(savedScene) ? fallbackScene : savedScene;
        }
        #endregion
    }
}
