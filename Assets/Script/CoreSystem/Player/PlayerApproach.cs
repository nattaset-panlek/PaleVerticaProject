using System;
using System.Collections.Generic;
using CoreSystem.Entity;
using CoreSystem.Pathfinding;
using UnityEngine;

namespace CoreSystem.Player
{
    public sealed class PlayerApproach
    {
        #region [Variables/Fields]
        private const float WaypointReach = 0.15f;

        private readonly List<Vector2> _path = new List<Vector2>();
        private Func<Vector2?> _destination;
        private Action _onArrive;
        private float _range;
        private int _pathIndex;
        private bool _hasPath;
        #endregion

        #region [Properties]
        public bool IsActive => _destination != null;
        #endregion

        #region [Public Methods]
        public bool Begin(Character player, Vector2 start, Func<Vector2?> destination, float range, Action onArrive)
        {
            Cancel();
            _path.Clear();
            _pathIndex = 0;
            _hasPath = false;

            var grid = WalkableGridManager.Instance;
            if (grid != null)
            {
                PathOutcome outcome = grid.FindPath(player.transform.position, start, _path);
                if (outcome == PathOutcome.NoRoute) return false;
                _hasPath = GridPathFormula.Found(outcome);
            }

            _destination = destination;
            _range = range;
            _onArrive = onArrive;
            return true;
        }

        public void Cancel()
        {
            _destination = null;
            _onArrive = null;
        }

        public void Tick(Character player, object sender)
        {
            if (_destination == null) return;

            Vector2? target = _destination();
            if (!target.HasValue)
            {
                player.Presenter.SetMoving(false);
                Cancel();
                return;
            }

            Vector2 position = player.transform.position;
            if (Vector2.Distance(position, target.Value) <= _range)
            {
                player.Presenter.SetMoving(false);
                Action arrive = _onArrive;
                Cancel();
                arrive?.Invoke();
                return;
            }

            Vector2 next = NextPoint(position, target.Value);
            if (next == position)
            {
                player.Presenter.SetMoving(false);
                Cancel();
                return;
            }

            Vector3 direction = (next - position).normalized;
            player.Control.CommandMove(direction, sender);
            player.Presenter.Face(direction);
            player.Presenter.SetMoving(true);
        }
        #endregion

        #region [Private Methods]
        private Vector2 NextPoint(Vector2 position, Vector2 target)
        {
            if (!_hasPath) return target;

            while (_pathIndex < _path.Count && Vector2.Distance(position, _path[_pathIndex]) <= WaypointReach)
            {
                _pathIndex++;
            }
            return _pathIndex < _path.Count ? _path[_pathIndex] : position;
        }
        #endregion
    }
}
