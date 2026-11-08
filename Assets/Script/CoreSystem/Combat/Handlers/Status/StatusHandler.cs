using CoreSystem.Attributes;
using CoreSystem.Entity;
using CoreSystem.Skills;
using CoreSystem.Skills.Logic;

namespace CoreSystem.Combat
{
    [EffectHandler(EffectKind.Status)]
    public sealed class StatusHandler : EffectHandler<StatusSpec, float>
    {
        #region [Variables/Fields]
        public const string BurnCode = "Status_Burn";
        public const string PoisonCode = "Status_Poison";
        public const string FreezeCode = "Status_Freeze";
        public const string SlowCode = "Status_Slow";
        #endregion

        #region [Properties]
        public override TargetRule TargetRule => TargetRule.Any;
        #endregion

        #region [Public Methods]
        public static ActionResult Apply(IEntity caster, IEntity target, StatusSpec spec, int procSkillId)
        {
            var action = new CombatAction(new SkillPayload { SkillID = procSkillId, Caster = caster as Character }).WithEffect(spec);
            return CombatCore.Execute(action, caster, target);
        }
        #endregion

        #region [Private Methods]
        protected override float Offense(StatusSpec spec, ResolveContext context)
        {
            return spec.Duration;
        }

        protected override Outcome Defense(StatusSpec spec, float outgoing, ResolveContext context)
        {
            Outcome outcome = context.CreateOutcome(spec);
            outcome.Amount = outgoing;
            return outcome;
        }

        protected override void Commit(StatusSpec spec, Outcome outcome, ActionResult result)
        {
            if (outcome.Amount <= 0f || outcome.Target == null) return;
            var character = outcome.Target as Character;

            switch (spec.Status)
            {
                case StatusKind.Stun:
                    ApplyStun(outcome.Target, character, outcome.Amount);
                    break;
                case StatusKind.Freeze:
                    ApplyStun(outcome.Target, character, outcome.Amount);
                    ApplyMarker(character, "Frozen", FreezeCode, outcome.Amount);
                    break;
                case StatusKind.Slow:
                    ApplySlow(character, spec, outcome.Amount);
                    break;
                case StatusKind.Burn:
                    ApplyTick(character, "Burn", BurnCode, ProcSkillIds.BurnTick, ElementType.Fire, spec, outcome.Amount);
                    break;
                case StatusKind.Poison:
                    ApplyTick(character, "Poison", PoisonCode, ProcSkillIds.PoisonTick, ElementType.Neutral, spec, outcome.Amount);
                    break;
            }
        }

        private static void ApplyStun(IEntity target, Character character, float duration)
        {
            float remaining = character != null ? character.StunDuration : 0f;
            target.ApplyStun(StatusFormula.ExtendedDuration(remaining, duration));
        }

        private static float Extended(Character character, string code, float duration)
        {
            var existing = character.Buffs.GetBuff(code, AttributeType.MoveSpeed);
            return StatusFormula.ExtendedDuration(existing != null ? existing.RemainingDuration : 0f, duration);
        }

        private static void ApplyMarker(Character character, string name, string code, float duration)
        {
            if (character == null || character.Buffs == null) return;
            character.Buffs.ApplyTemporaryBuff(name, code, AttributeType.MoveSpeed, new AttributeModifier(0f, ModifierType.Flat),
                Extended(character, code, duration), null, BuffCategory.Effect, 1);
        }

        private static void ApplySlow(Character character, StatusSpec spec, float duration)
        {
            if (character == null || character.Buffs == null || spec.SlowPercent <= 0f) return;
            character.Buffs.ApplyTemporaryBuff("Slow", SlowCode, AttributeType.MoveSpeed, new AttributeModifier(-spec.SlowPercent, ModifierType.PercentAdd),
                Extended(character, SlowCode, duration), null, BuffCategory.Effect, 1);
        }

        private static void ApplyTick(Character character, string name, string code, int procId, ElementType element, StatusSpec spec, float duration)
        {
            if (character == null || character.Buffs == null || spec.TickDamage <= 0f) return;
            character.Buffs.ApplyTemporaryBuffWithTick(name, code, AttributeType.MoveSpeed, new AttributeModifier(0f, ModifierType.Flat),
                Extended(character, code, duration), spec.TickInterval,
                () => Tick(spec.CreditTo, character, spec.TickDamage, procId, element), null, BuffCategory.Effect, 1);
        }

        private static void Tick(IEntity credit, Character target, float damage, int procId, ElementType element)
        {
            if (target == null || target.IsDead) return;
            var payload = new SkillPayload
            {
                SkillID = procId,
                SkillLevel = 1,
                SkillElement = element,
                DamageType = DamageType.Status,
                BaseDamage = damage
            };
            var action = CombatAction.Damage(payload);
            action.CreditTo = credit;
            CombatCore.Execute(action, null, target);
        }
        #endregion
    }
}
