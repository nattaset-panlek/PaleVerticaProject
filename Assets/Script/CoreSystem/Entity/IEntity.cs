using CoreSystem.Attributes;
using UnityEngine;

namespace CoreSystem.Entity
{
    public interface IEntity
    {
        Transform transform { get; }
        AttributeSystem attributeSystem { get; }
        ElementType Element { get; }
        SizeType Size { get; }
        RaceType Race { get; }
        FactionType Faction { get; }

        void TriggerOnHit(IEntity target, float damage);
        void TriggerOnTakeDamage(IEntity attacker, float damage);
    }
}
