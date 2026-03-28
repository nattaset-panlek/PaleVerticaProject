using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Movement
{
    [System.Serializable]
    public class MovementSystem
    {
        private AttributeSystem attributeSystem;
        private Entity.Character owner;
        private float speedMultiplier = 3f;

        public void Initialize(Entity.Character owner)
        {
            this.owner = owner;
            this.attributeSystem = owner.attributeSystem;
        }

        public void Move(Vector3 direction)
        {
            if (owner != null && owner.IsStunned) return;

            // Get MoveSpeed attribute and handle null cases (safety)
            var moveSpeedAttr = attributeSystem.GetAttribute(AttributeType.MoveSpeed);
            if (moveSpeedAttr == null) return;

            float currentMoveSpeed = (moveSpeedAttr.CurrentValue / 100f) * speedMultiplier;

            if (currentMoveSpeed > 0)
            {
                owner.transform.position += direction.normalized * (currentMoveSpeed * Time.deltaTime);
            }
            else
            {
                // Warning if speed is 0 - might be from inspector override
                if (Time.frameCount % 60 == 0) // Log once a second
                    Debug.LogWarning($"[MovementSystem] {owner.name} move speed is 0! (Attr: {moveSpeedAttr.CurrentValue}%)");
            }
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }
    }
}
