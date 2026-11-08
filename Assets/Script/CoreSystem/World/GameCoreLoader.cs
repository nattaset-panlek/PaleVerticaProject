using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.UI.Loading;
using CoreSystem.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreSystem.World
{
    public static class GameCoreLoader
    {
        #region [Variables/Fields]
        public const string StartSpawnId = "start";
        public const string TitleSceneName = "Title";

        private static GameCoreSettings _settings;
        private static GameObject _core;
        private static string _nextSpawnId;
        private static PlayerDataModel _pendingSave;
        #endregion

        #region [Properties]
        public static GameObject Core => _core;
        #endregion

        #region [Public Methods]
        public static void SetNextSpawn(string spawnId)
        {
            _nextSpawnId = spawnId;
        }

        public static void StartNewGame()
        {
            if (_settings == null) return;
            GameData.ResetPlayer();
            LoadingManager.LoadScene(_settings.NewGameScene);
        }

        public static void ContinueGame(PlayerDataModel save)
        {
            if (_settings == null || save == null) return;
            _pendingSave = save;
            string saved = Application.CanStreamedLevelBeLoaded(save.Meta.SceneName) ? save.Meta.SceneName : null;
            string scene = GameCoreFormula.ContinueScene(saved, _settings.NewGameScene);
            if (!LoadingManager.LoadScene(scene)) _pendingSave = null;
        }

        public static void ReturnToTitle()
        {
            LoadingManager.LoadScene(TitleSceneName);
        }

        public static void PrepareFor(string sceneName)
        {
            if (_settings == null || _settings.CorePrefab == null) return;

            bool usesCore = GameCoreFormula.SceneUsesCore(sceneName, _settings.ScenesWithoutCore);
            if (usesCore && _core == null) SpawnCore();
            else if (!usesCore && _core != null) DestroyCore();
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            _settings = Resources.Load<GameCoreSettings>(GameCoreSettings.ResourcePath);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            PrepareFor(SceneManager.GetActiveScene().name);
        }

        private static void SpawnCore()
        {
            _core = Object.Instantiate(_settings.CorePrefab);
            _core.name = _settings.CorePrefab.name;
            Object.DontDestroyOnLoad(_core);
        }

        private static void DestroyCore()
        {
            Object.Destroy(_core);
            _core = null;
            RuntimeRoot.Clear();
            GameData.ResetPlayer();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_core == null) return;

            ApplyPendingSave();
            string spawnId = string.IsNullOrEmpty(_nextSpawnId) ? StartSpawnId : _nextSpawnId;
            _nextSpawnId = null;

            MapSpawnPoint point = MapSpawnPoint.Find(spawnId);
            if (point == null)
            {
                Debug.LogWarning($"[GameCoreLoader] Scene '{scene.name}' has no spawn point '{spawnId}'.");
                return;
            }

            Character player = _core.GetComponentInChildren<Character>();
            if (player != null) player.transform.position = point.transform.position;
        }

        private static void ApplyPendingSave()
        {
            if (_pendingSave == null) return;
            if (PlayerCore.GetPlayer() == null)
            {
                PlayerCore.OnPlayerChanged -= HandlePlayerReady;
                PlayerCore.OnPlayerChanged += HandlePlayerReady;
                return;
            }

            PlayerCore.Ensure().LoadData(_pendingSave);
            _pendingSave = null;
        }

        private static void HandlePlayerReady(Character character)
        {
            if (character == null) return;
            PlayerCore.OnPlayerChanged -= HandlePlayerReady;
            ApplyPendingSave();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _settings = null;
            _core = null;
            _nextSpawnId = null;
            _pendingSave = null;
        }
        #endregion
    }
}
