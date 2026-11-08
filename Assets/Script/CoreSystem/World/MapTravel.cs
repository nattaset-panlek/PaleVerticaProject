using System.Collections;
using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Player;
using CoreSystem.UI.Loading;
using UnityEngine;

namespace CoreSystem.World
{
    public sealed class MapTravel : MonoBehaviour
    {
        #region [Variables/Fields]
        private static MapTravel _instance;

        private PlayerDataModel _carried;
        private string _spawnId;
        private bool _travelling;
        #endregion

        #region [Properties]
        public static bool IsTravelling => _instance != null && _instance._travelling;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
            if (_instance == this) _instance = null;
        }
        #endregion

        #region [Public Methods]
        public static MapTravel Create()
        {
            return new GameObject("[Runtime] MapTravel").AddComponent<MapTravel>();
        }

        public static void Travel(string sceneName, string spawnId)
        {
            if (IsTravelling) return;
            MapTravel travel = _instance != null ? _instance : Create();
            travel.Begin(sceneName, spawnId);
        }
        #endregion

        #region [Private Methods]
        private void Begin(string sceneName, string spawnId)
        {
            _carried = PlayerCore.Ensure().SaveData();
            _spawnId = spawnId;
            _travelling = true;
            PlayerCore.OnPlayerChanged += HandlePlayerChanged;

            if (LoadingManager.LoadScene(sceneName, null, () => !_travelling)) return;

            PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
            _carried = null;
            _travelling = false;
        }

        private void HandlePlayerChanged(Character character)
        {
            if (character == null) return;
            PlayerCore.OnPlayerChanged -= HandlePlayerChanged;
            StartCoroutine(FinishArrival(character));
        }

        private IEnumerator FinishArrival(Character character)
        {
            yield return null;

            if (character != null)
            {
                PlayerCore.Ensure().LoadData(_carried);
                MapSpawnPoint point = MapSpawnPoint.Find(_spawnId);
                if (point != null) character.transform.position = point.transform.position;
                else Debug.LogWarning($"[MapTravel] Spawn point '{_spawnId}' not found.");
            }

            _carried = null;
            _travelling = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
