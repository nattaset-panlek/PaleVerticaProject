using UnityEngine;

namespace CoreSystem.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class MapPortal : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private string _targetScene;
        [SerializeField] private string _targetSpawnId;
        [SerializeField, Min(0f)] private float _range = 1.5f;
        #endregion

        #region [Properties]
        public float Range => _range;
        #endregion

        #region [Public Methods]
        public static MapPortal AtPoint(Vector2 worldPoint)
        {
            foreach (Collider2D collider in Physics2D.OverlapPointAll(worldPoint))
            {
                var portal = collider.GetComponent<MapPortal>();
                if (portal != null) return portal;
            }
            return null;
        }

        public void Enter()
        {
            MapTravel.Travel(_targetScene, _targetSpawnId);
        }
        #endregion
    }
}
