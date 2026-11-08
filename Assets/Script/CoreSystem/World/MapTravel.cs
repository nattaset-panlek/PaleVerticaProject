using System.Collections;
using CoreSystem.Data;
using CoreSystem.Data.Save;
using CoreSystem.Entity;
using CoreSystem.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreSystem.World
{
    public sealed class MapTravel : MonoBehaviour
    {
        #region [Variables/Fields]
        private const int OverlaySortingOrder = 5000;
        private static MapTravel _instance;

        private PlayerDataModel _carried;
        private string _spawnId;
        private Canvas _canvas;
        #endregion

        #region [Properties]
        public static bool IsTravelling => _instance != null && _instance._canvas.enabled;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = OverlaySortingOrder;
            _canvas.enabled = false;

            var fill = new GameObject("Fill", typeof(RectTransform)).AddComponent<Image>();
            fill.transform.SetParent(transform, false);
            fill.color = Color.black;
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;
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
            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[MapTravel] Scene '{sceneName}' is not in Build Settings.");
                return;
            }

            MapTravel travel = _instance != null ? _instance : Create();
            travel.Begin(sceneName, spawnId);
        }
        #endregion

        #region [Private Methods]
        private void Begin(string sceneName, string spawnId)
        {
            _carried = PlayerCore.Ensure().SaveData();
            _spawnId = spawnId;
            _canvas.enabled = true;
            PlayerCore.OnPlayerChanged += HandlePlayerChanged;
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
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
            _canvas.enabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
        #endregion
    }
}
