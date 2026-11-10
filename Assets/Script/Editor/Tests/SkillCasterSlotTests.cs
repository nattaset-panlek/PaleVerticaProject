using System.Collections.Generic;
using CoreSystem.Skills;
using NUnit.Framework;

namespace CoreSystem.Tests
{
    public class SkillCasterSlotTests
    {
        #region [Public Methods]
        [Test]
        public void Initialize_TreatsSerializedEmptySlotsAsEmpty()
        {
            var caster = new SkillCaster { EquippedSkills = new List<SkillInstance> { new SkillInstance(), new SkillInstance(), new SkillInstance() } };
            caster.Initialize(null);
            Assert.AreEqual(10, caster.EquippedSkills.Count);
            foreach (SkillInstance slot in caster.EquippedSkills) Assert.IsNull(slot);
        }
        #endregion
    }
}
