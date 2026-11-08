using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.World
{
    public sealed class MapSpawnPoint : MonoBehaviour
    {
        #region [Variables/Fields]
        private static readonly Dictionary<string, MapSpawnPoint> _points = new Dictionary<string, MapSpawnPoint>();

        [SerializeField] private string _id;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            if (!string.IsNullOrEmpty(_id)) _points[_id] = this;
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(_id) && _points.TryGetValue(_id, out MapSpawnPoint point) && point == this)
            {
                _points.Remove(_id);
            }
        }
        #endregion

        #region [Public Methods]
        public static MapSpawnPoint Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _points.TryGetValue(id, out MapSpawnPoint point);
            return point;
        }
        #endregion

        #region [Private Methods]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _points.Clear();
        }
        #endregion
    }
}
