using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("FeralBond")]
    public class FeralBondUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private readonly HashSet<SummonedUnit> _bonded = new HashSet<SummonedUnit>();
        private float _timer;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            _timer -= dt;
            if (_timer > 0f) return;
            _timer = 0.5f;
            foreach (SummonedUnit unit in OwnedSummons())
            {
                if (!_bonded.Add(unit)) continue;
                unit.Body.attributeSystem.AddAdditional(AttributeType.ATK, 2, AdditionalOp.Add, Stat(AttributeType.DEX), Id);
                unit.Body.attributeSystem.AddAdditional(AttributeType.MaxHP, 2, AdditionalOp.Add, Stat(AttributeType.VIT) * 5f, Id);
            }
        }
        #endregion
    }
}
