using CoreSystem.Skills;
using CoreSystem.Combat;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class DamageFormulaTests
    {
        #region [Variables/Fields]
        private const float Tolerance = 0.0001f;
        #endregion

        #region [Public Methods]
        [Test]
        public void WithoutCaster_UsesBaseDamageElementAndDefenseOnly()
        {
            var input = new DamageInput { BaseDamage = 100f, ElementAdvantage = 1.5f, TargetDefense = 20f, CasterPower = 999f };

            var result = DamageFormula.Calculate(input, 0f);

            Assert.AreEqual(130f, result.Damage, Tolerance);
            Assert.IsFalse(result.IsCritical);
        }

        [Test]
        public void WithCaster_AppliesPowerModifiersAndCriticalInOrder()
        {
            var input = CasterInput();

            var result = DamageFormula.Calculate(input, 5f);

            Assert.IsTrue(result.IsCritical);
            Assert.AreEqual((20f + 50f + 30f) * 1.1f * 2f - 20f, result.Damage, Tolerance);
        }

        [Test]
        public void CriticalRoll_AtOrAboveChance_DoesNotCrit()
        {
            var input = CasterInput();

            var result = DamageFormula.Calculate(input, 10f);

            Assert.IsFalse(result.IsCritical);
            Assert.AreEqual((20f + 50f + 30f) * 1.1f - 20f, result.Damage, Tolerance);
        }

        [Test]
        public void CriticalResistance_ReducesChance()
        {
            var input = CasterInput();
            input.CritResist = 8f;

            Assert.IsFalse(DamageFormula.Calculate(input, 3f).IsCritical);
            Assert.IsTrue(DamageFormula.Calculate(input, 1f).IsCritical);
        }

        [Test]
        public void Damage_NeverDropsBelowMinimum()
        {
            var input = new DamageInput { BaseDamage = 10f, ElementAdvantage = 1f, TargetDefense = 500f };

            Assert.AreEqual(DamageFormula.MinimumDamage, DamageFormula.Calculate(input, 0f).Damage, Tolerance);
        }

        [Test]
        public void Resistance_AboveBonus_ClampsMultiplierAtZero()
        {
            var input = CasterInput();
            input.RaceResist = 3f;

            Assert.AreEqual(DamageFormula.MinimumDamage, DamageFormula.Calculate(input, 50f).Damage, Tolerance);
        }

        [Test]
        public void SizeAndRaceModifiers_Multiply()
        {
            var input = CasterInput();
            input.ElementBonus = 0f;
            input.ElementResist = 0f;
            input.SizeBonus = 0.5f;
            input.RaceBonus = 0.2f;
            input.RaceResist = 0.1f;
            input.TargetDefense = 0f;

            Assert.AreEqual(100f * 1.5f * 1.1f, DamageFormula.Calculate(input, 50f).Damage, Tolerance);
        }

        [Test]
        public void ProcSkillIds_AreRecognised()
        {
            Assert.IsTrue(ProcSkillIds.IsProc(ProcSkillIds.ThornReflect));
            Assert.IsTrue(ProcSkillIds.IsProc(ProcSkillIds.ChaosBurst));
            Assert.IsTrue(ProcSkillIds.IsProc(ProcSkillIds.BleedTick));
            Assert.IsFalse(ProcSkillIds.IsProc(1));
            Assert.IsFalse(ProcSkillIds.IsProc(0));
        }
        #endregion

        #region [Private Methods]
        private static DamageInput CasterInput()
        {
            return new DamageInput
            {
                BaseDamage = 20f,
                HasCasterStats = true,
                CasterPower = 50f,
                CasterWeaponPower = 30f,
                ElementAdvantage = 1f,
                ElementBonus = 0.2f,
                ElementResist = 0.1f,
                CritRate = 10f,
                CritDamage = 0.5f,
                TargetDefense = 20f
            };
        }
        #endregion
    }
}
