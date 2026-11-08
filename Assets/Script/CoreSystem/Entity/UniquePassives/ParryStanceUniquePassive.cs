using CoreSystem.Combat;
using CoreSystem.Skills;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    public class ParryStanceUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        public const string StanceId = "ParryStance";
        private const float VfxLifetime = 1f;

        private readonly SkillPayload _source;
        private readonly float _counterMultiplier;
        private readonly GameObject _counterVfx;
        #endregion

        #region [Public Methods]
        public ParryStanceUniquePassive(SkillPayload source, float counterMultiplier, GameObject counterVfx)
        {
            _source = source;
            _counterMultiplier = counterMultiplier;
            _counterVfx = counterVfx;
        }

        public override void TriggerOnTakeDamage(IEntity attacker, ref float damage, SkillPayload payload)
        {
            if (attacker == null || ReferenceEquals(attacker, Owner) || damage <= 0f) return;
            if (payload != null && ProcSkillIds.IsProc(payload.SkillID)) return;

            damage = 0f;
            Owner.UniquePassives.RemovePassive(Id);
            Counter(attacker);
        }
        #endregion

        #region [Private Methods]
        private void Counter(IEntity attacker)
        {
            var counter = new SkillPayload
            {
                Caster = Owner,
                SkillID = ProcSkillIds.ParryCounter,
                SkillLevel = _source.SkillLevel,
                SkillElement = _source.SkillElement,
                DamageType = _source.DamageType,
                BaseDamage = _source.BaseDamage,
                DamageMultiplier = _source.DamageMultiplier * _counterMultiplier
            };
            CombatCore.Execute(CombatAction.Damage(counter), Owner, attacker);

            if (_counterVfx != null)
            {
                Object.Destroy(Object.Instantiate(_counterVfx, attacker.transform.position, Quaternion.identity), VfxLifetime);
            }
        }
        #endregion
    }
}
