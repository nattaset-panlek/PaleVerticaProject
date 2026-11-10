using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("AlphasBurden")]
    public class AlphasBurdenUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private readonly HashSet<SummonedUnit> _empowered = new HashSet<SummonedUnit>();
        private float _timer;
        #endregion

        #region [Public Methods]
        public override void Tick(float dt)
        {
            _timer -= dt;
            if (_timer > 0f) return;
            _timer = 0.25f;
            List<SummonedUnit> units = OwnedSummons();
            units.Sort((a, b) => b.SpawnTime.CompareTo(a.SpawnTime));
            for (int i = 1; i < units.Count; i++) units[i].Dismiss();
            if (units.Count == 0 || !_empowered.Add(units[0])) return;
            Character body = units[0].Body;
            body.transform.localScale *= 1.3f;
            body.attributeSystem.AddAdditional(AttributeType.MaxHPPercent, 2, AdditionalOp.Add, 100f, Id);
            body.attributeSystem.AddAdditional(AttributeType.ATKPercent, 2, AdditionalOp.Add, 100f, Id);
            body.attributeSystem.AddAdditional(AttributeType.MATKPercent, 2, AdditionalOp.Add, 100f, Id);
        }
        #endregion
    }
}
