using System.Collections.Generic;
using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using CoreSystem.Skills.Summon;
using UnityEngine;

namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("RunicSkin")]
    public class RunicSkinUniquePassive : UniquePassive
    {
        #region [Variables/Fields]
        private bool _active;
        private AttributeType _resist;
        private float _time;
        #endregion

        #region [Public Methods]
        public override void TriggerOnSkillExecuted(ActiveSkill skill, SkillPayload payload)
        {
            if (!TryResistOf(payload.SkillElement, out AttributeType resist)) return;
            Clear();
            _resist = resist;
            _active = true;
            _time = 5f;
            SetBonus(_resist, 15f);
        }

        public override void Tick(float dt)
        {
            if (!_active) return;
            _time -= dt;
            if (_time <= 0f) Clear();
        }

        public override void OnUnequip()
        {
            Clear();
        }

        private void Clear()
        {
            if (_active) ClearBonus(_resist);
            _active = false;
        }

        private static bool TryResistOf(ElementType element, out AttributeType resist)
        {
            switch (element)
            {
                case ElementType.Water: resist = AttributeType.WaterResist; return true;
                case ElementType.Earth: resist = AttributeType.EarthResist; return true;
                case ElementType.Fire: resist = AttributeType.FireResist; return true;
                case ElementType.Electric: resist = AttributeType.ElectricResist; return true;
                case ElementType.Ghost: resist = AttributeType.GhostResist; return true;
                case ElementType.Holy: resist = AttributeType.HolyResist; return true;
                default: resist = AttributeType.NeutralResist; return true;
            }
        }
        #endregion
    }
}
