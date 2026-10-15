using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Tests
{
    public sealed class FakeCombatant : IEntity, System.IDisposable
    {
        #region [Variables/Fields]
        private readonly GameObject _gameObject;
        #endregion

        #region [Properties]
        public Transform transform => _gameObject.transform;
        public AttributeSystem attributeSystem { get; }
        public ElementType Element { get; set; }
        public SizeType Size { get; set; }
        public RaceType Race { get; set; }
        public int Faction { get; set; }
        public Vector3 FacingDirection => Vector3.up;
        public bool IsStunned => false;
        public int HitEvents { get; private set; }
        public int DamageTakenEvents { get; private set; }
        public float Hp => attributeSystem.GetAttribute(AttributeType.HP).CurrentValue;
        #endregion

        #region [Public Methods]
        public FakeCombatant(int faction, AttributeSystem stats = null)
        {
            _gameObject = new GameObject("FakeCombatant");
            attributeSystem = stats ?? TestStats.Create();
            Faction = faction;
        }

        public void ApplyImpulse(Vector3 direction, float distance, float duration) {}
        public void ApplyStun(float duration) {}
        public void TriggerOnHit(IEntity target, float damage) => HitEvents++;
        public void TriggerOnTakeDamage(IEntity attacker, float damage) => DamageTakenEvents++;

        public void SetHp(float value)
        {
            TestStats.SetBase(attributeSystem, AttributeType.HP, value);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_gameObject);
        }
        #endregion
    }
}
