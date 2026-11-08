using CoreSystem.Attributes;
using CoreSystem.Combat;
using CoreSystem.Entity;
using CoreSystem.Movement;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class SprintEffect : EffectLogic
    {
        #region [Variables/Fields]
        public const string DefaultBuffCode = "Sprint";

        public float SprintBonusPercent = 60f;
        public float Duration = 10f;
        [Range(0f, 1f)] public float JogRatio = MovementFormula.DefaultJogRatio;
        [Range(0f, 1f)] public float SprintShare = MovementFormula.DefaultSprintShare;
        [Range(0f, 1f)] public float RampShare = MovementFormula.DefaultRampShare;
        [Range(0f, 1f)] public float JogShare = MovementFormula.DefaultJogShare;
        public float TickInterval = 0.1f;
        public string BuffName = "Sprint";
        public string BuffCode = DefaultBuffCode;
        public Sprite Icon;
        public string BuffIconPath = "";
        #endregion

        #region [Properties]
        public override bool ApplyToCaster { get => true; set { } }
        public override bool ApplyOncePerCast { get => true; set { } }
        public string ResolvedBuffCode => string.IsNullOrEmpty(BuffCode) ? DefaultBuffCode : BuffCode;
        #endregion

        #region [Public Methods]
        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (!(target is Character character) || character.attributeSystem == null) return;
            Sprite icon = Icon != null ? Icon : (!string.IsNullOrEmpty(BuffIconPath) ? BuffIconLoader.Load(BuffIconPath) : null);
            Begin(character.Buffs, icon);
        }

        public void Begin(BuffSystem buffs, Sprite icon = null)
        {
            if (buffs == null) return;
            string code = ResolvedBuffCode;
            buffs.ApplyTemporaryBuffWithTick(BuffName, code, AttributeType.MoveSpeed, BonusModifier(0f), Duration, TickInterval,
                () => Refresh(buffs, code), icon, BuffCategory.Default, 1);
            ActiveBuff buff = buffs.GetBuff(code, AttributeType.MoveSpeed);
            if (buff != null) buffs.ReplaceModifier(buff, BonusModifier(0f));
        }

        public float BonusAt(float elapsed)
        {
            return MovementFormula.SprintBonus(elapsed, Duration, SprintBonusPercent / 100f, JogRatio, SprintShare, RampShare, JogShare);
        }

        public SprintPhase PhaseAt(float elapsed)
        {
            return MovementFormula.GetSprintPhase(elapsed, Duration, SprintShare, RampShare, JogShare);
        }
        #endregion

        #region [Private Methods]
        private void Refresh(BuffSystem buffs, string code)
        {
            ActiveBuff buff = buffs.GetBuff(code, AttributeType.MoveSpeed);
            if (buff == null) return;
            float elapsed = buff.TotalDuration - buff.RemainingDuration;
            buffs.ReplaceModifier(buff, BonusModifier(elapsed));
        }

        private AttributeModifier BonusModifier(float elapsed)
        {
            return new AttributeModifier(BonusAt(elapsed), ModifierType.PercentAdd);
        }
        #endregion
    }
}
