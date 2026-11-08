using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.World
{
    [CreateAssetMenu(fileName = "GameCoreSettings", menuName = "Pale Vertica/World/Game Core Settings")]
    public class GameCoreSettings : ScriptableObject
    {
        #region [Variables/Fields]
        public const string ResourcePath = "GameCoreSettings";

        [SerializeField] private GameObject _corePrefab;
        [SerializeField] private List<string> _scenesWithoutCore = new List<string>();
        [SerializeField] private string _newGameScene = "Town";
        #endregion

        #region [Properties]
        public GameObject CorePrefab
        {
            get => _corePrefab;
            set => _corePrefab = value;
        }

        public List<string> ScenesWithoutCore => _scenesWithoutCore;
        public string NewGameScene => _newGameScene;
        #endregion
    }
}
