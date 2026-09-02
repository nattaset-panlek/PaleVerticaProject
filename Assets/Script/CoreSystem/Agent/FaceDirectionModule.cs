using UnityEngine;

namespace CoreSystem.Controllers
{
    public enum FacingDirection2D
    {
        Right,
        Left,
        Up,
        Down
    }

    /// <summary>
    /// A lightweight module to track 2D facing direction without rotating the Transform.
    /// </summary>
    [System.Serializable]
    public class FaceDirectionModule
    {
        #region [Variables/Fields]
        [SerializeField] private Vector2 currentFacingVector = Vector2.right;
        [SerializeField] private FacingDirection2D currentDirection = FacingDirection2D.Right;
        #endregion

        #region [Properties]
        public Vector2 CurrentFacingVector => currentFacingVector;
        public FacingDirection2D CurrentDirection => currentDirection;
        #endregion

        #region [Public Methods]
        public void UpdateDirection(Vector2 movementDirection)
        {
            if (movementDirection.sqrMagnitude < 0.001f) return;

            currentFacingVector = movementDirection.normalized;

            if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
            {
                currentDirection = movementDirection.x > 0 ? FacingDirection2D.Right : FacingDirection2D.Left;
            }
            else
            {
                currentDirection = movementDirection.y > 0 ? FacingDirection2D.Up : FacingDirection2D.Down;
            }
        }

        public void SetDirection(Vector2 newDirection)
        {
            if (newDirection.sqrMagnitude < 0.001f) return;
            UpdateDirection(newDirection);
        }
        #endregion
    }
}
