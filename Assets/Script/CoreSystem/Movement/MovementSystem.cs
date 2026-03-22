using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Movement
{
    [System.Serializable]
    public class MovementSystem
    {
        private AttributeSystem attributeSystem;
        private Entity.Character owner;
        private float speedMultiplier = 1f;

        public void Initialize(Entity.Character owner)
        {
            this.owner = owner;
            this.attributeSystem = owner.attributeSystem;
        }

        public void Move(Vector3 direction)
        {
            float currentMoveSpeed = attributeSystem.GetAttribute(AttributeType.MoveSpeed) * speedMultiplier;
            owner.transform.position += direction.normalized * (currentMoveSpeed * Time.deltaTime);
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }
    }
}
