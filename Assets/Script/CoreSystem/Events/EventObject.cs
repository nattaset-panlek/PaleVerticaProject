using CoreSystem.Data;
using UnityEngine;

namespace CoreSystem.Events
{
    public sealed class EventObject : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private string _objectId;
        [SerializeField] private GameObject _target;

        private PlayerEvents _events;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _events = GameData.Player.Events;
            _events.OnObjectStateChanged += HandleStateChanged;
            if (_events.TryGetObjectState(_objectId, out bool active)) Apply(active);
        }

        private void OnDestroy()
        {
            _events.OnObjectStateChanged -= HandleStateChanged;
        }
        #endregion

        #region [Private Methods]
        private void HandleStateChanged(string objectId, bool active)
        {
            if (objectId == _objectId) Apply(active);
        }

        private void Apply(bool active)
        {
            if (_target != null) _target.SetActive(active);
        }
        #endregion
    }
}
