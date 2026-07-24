using CoreSystem.Attributes;
using CoreSystem.Entity;
using CoreSystem.UI;
using UnityEngine;

namespace CoreSystem.Skills.Logic
{
    [System.Serializable]
    public class DamageEffect : EffectLogic
    {
        public AttributeType TargetAttribute = AttributeType.HP;

        public override void Apply(SkillPayload payload, IEntity target)
        {
            if (payload.Caster != null && !payload.Caster.Faction.IsHostileTo(target.Faction))
            {
                return;
            }
            if (target.attributeSystem != null)
            {
                float calculatedDamage = payload.BaseDamage;
                if (payload.Caster != null && payload.Caster.attributeSystem != null)
                {
                    var stats = payload.Caster.attributeSystem;
                    if (payload.DamageType == DamageType.Physical)
                    {
                        calculatedDamage += stats.GetAttribute(AttributeType.ATK).CurrentValue;
                        calculatedDamage += stats.GetAttribute(AttributeType.WeaponATK).CurrentValue;
                    }
                    else
                    {
                        calculatedDamage += stats.GetAttribute(AttributeType.MATK).CurrentValue;
                        calculatedDamage += stats.GetAttribute(AttributeType.WeaponMATK).CurrentValue;
                    }
                }

                // Apply Elemental Advantage Table Multiplier
                float elementAdvantage = ElementAdvantageTable.GetMultiplier(payload.SkillElement, target.Element);
                calculatedDamage *= elementAdvantage;

                bool isCritical = false;
                if (payload.Caster != null && payload.Caster.attributeSystem != null)
                {
                    AttributeSystem casterAttr = payload.Caster.attributeSystem;
                    AttributeSystem targetAttr = target.attributeSystem;

                    // 1. Element DMG Bonus vs Element Resistance
                    float elementDmgBonus = GetElementDamageBonus(casterAttr, payload.SkillElement);
                    float elementResist = GetElementResistance(targetAttr, payload.SkillElement);
                    float elementMod = Mathf.Max(0f, 1f + (elementDmgBonus - elementResist));
                    calculatedDamage *= elementMod;

                    // 2. Size DMG Bonus vs Size Resistance
                    float sizeDmgBonus = GetSizeDamageBonus(casterAttr, target.Size);
                    float sizeResist = GetSizeResistance(targetAttr, payload.Caster.Size);
                    float sizeMod = Mathf.Max(0f, 1f + (sizeDmgBonus - sizeResist));
                    calculatedDamage *= sizeMod;

                    // 3. Race DMG Bonus vs Race Resistance
                    float raceDmgBonus = GetRaceDamageBonus(casterAttr, target.Race);
                    float raceResist = GetRaceResistance(targetAttr, payload.Caster.Race);
                    float raceMod = Mathf.Max(0f, 1f + (raceDmgBonus - raceResist));
                    calculatedDamage *= raceMod;

                    // 4. Critical Hit Roll
                    float critRate = casterAttr.GetAttribute(AttributeType.CriticalRate);
                    float critResist = targetAttr.GetAttribute(AttributeType.CritResistance);
                    float finalCritChance = Mathf.Max(0f, critRate - critResist);

                    if (Random.Range(0f, 100f) < finalCritChance)
                    {
                        isCritical = true;
                        float critDmgBonus = casterAttr.GetAttribute(AttributeType.CriticalDamage);
                        // Base Critical damage 150% + bonus
                        calculatedDamage *= (1.5f + critDmgBonus);
                    }
                }

                AttributeType defenseType = payload.DamageType == DamageType.Physical ? AttributeType.Defense : AttributeType.StatusDEF;
                float targetDefense = target.attributeSystem.GetAttribute(defenseType);
                float finalDamage = Mathf.Max(1f, calculatedDamage - targetDefense);

                var charCaster = payload.Caster as Character;
                var charTarget = target as Character;

                if (charCaster != null && charCaster.UniquePassives != null)
                {
                    charCaster.UniquePassives.TriggerOnHit(target, ref finalDamage, payload);
                }

                if (charTarget != null && charTarget.UniquePassives != null)
                {
                    charTarget.UniquePassives.TriggerOnTakeDamage(payload.Caster, ref finalDamage, payload);
                }

                var attrTarget = target.attributeSystem.GetAttribute(TargetAttribute);
                if (attrTarget != null)
                {
                    attrTarget.BaseValue = Mathf.Max(0f, attrTarget.CurrentValue - finalDamage);
                    attrTarget.Recalculate();
                }

                if (charCaster != null && charCaster.UniquePassives != null)
                {
                    charCaster.UniquePassives.TriggerOnPostHit(target, finalDamage, payload);
                }

                if (payload.Caster != null)
                {
                    payload.Caster.TriggerOnHit(target, finalDamage);
                }
                
                target.TriggerOnTakeDamage(payload.Caster, finalDamage);
                
                // Show floating damage text
                if (DamageTextManager.Instance != null)
                {
                    DamageTextManager.Instance.ShowDamage(target.transform.position, finalDamage, payload.DamageType, payload.SkillElement, isCritical);
                }
            }
        }

        private float GetElementDamageBonus(AttributeSystem caster, ElementType element)
        {
            switch (element)
            {
                case ElementType.Neutral: return caster.GetAttribute(AttributeType.NeutralDamage);
                case ElementType.Water: return caster.GetAttribute(AttributeType.WaterDamage);
                case ElementType.Earth: return caster.GetAttribute(AttributeType.EarthDamage);
                case ElementType.Fire: return caster.GetAttribute(AttributeType.FireDamage);
                case ElementType.Electric: return caster.GetAttribute(AttributeType.ElectricDamage);
                case ElementType.Ghost: return caster.GetAttribute(AttributeType.GhostDamage);
                case ElementType.Holy: return caster.GetAttribute(AttributeType.HolyDamage);
                default: return 0f;
            }
        }

        private float GetElementResistance(AttributeSystem target, ElementType element)
        {
            switch (element)
            {
                case ElementType.Neutral: return target.GetAttribute(AttributeType.NeutralResist);
                case ElementType.Water: return target.GetAttribute(AttributeType.WaterResist);
                case ElementType.Earth: return target.GetAttribute(AttributeType.EarthResist);
                case ElementType.Fire: return target.GetAttribute(AttributeType.FireResist);
                case ElementType.Electric: return target.GetAttribute(AttributeType.ElectricResist);
                case ElementType.Ghost: return target.GetAttribute(AttributeType.GhostResist);
                case ElementType.Holy: return target.GetAttribute(AttributeType.HolyResist);
                default: return 0f;
            }
        }

        private float GetSizeDamageBonus(AttributeSystem caster, SizeType targetSize)
        {
            switch (targetSize)
            {
                case SizeType.Small: return caster.GetAttribute(AttributeType.SmallDamage);
                case SizeType.Medium: return caster.GetAttribute(AttributeType.MediumDamage);
                case SizeType.Large: return caster.GetAttribute(AttributeType.LargeDamage);
                default: return 0f;
            }
        }

        private float GetSizeResistance(AttributeSystem target, SizeType attackerSize)
        {
            switch (attackerSize)
            {
                case SizeType.Small: return target.GetAttribute(AttributeType.SmallResist);
                case SizeType.Medium: return target.GetAttribute(AttributeType.MediumResist);
                case SizeType.Large: return target.GetAttribute(AttributeType.LargeResist);
                default: return 0f;
            }
        }

        private float GetRaceDamageBonus(AttributeSystem caster, RaceType targetRace)
        {
            switch (targetRace)
            {
                case RaceType.Formless: return caster.GetAttribute(AttributeType.FormlessDamage);
                case RaceType.Undead: return caster.GetAttribute(AttributeType.UndeadDamage);
                case RaceType.Brute: return caster.GetAttribute(AttributeType.BruteDamage);
                case RaceType.Plant: return caster.GetAttribute(AttributeType.PlantDamage);
                case RaceType.Insect: return caster.GetAttribute(AttributeType.InsectDamage);
                case RaceType.Fish: return caster.GetAttribute(AttributeType.FishDamage);
                case RaceType.Demon: return caster.GetAttribute(AttributeType.DemonDamage);
                case RaceType.DemiHuman: return caster.GetAttribute(AttributeType.DemiHumanDamage);
                case RaceType.Angel: return caster.GetAttribute(AttributeType.AngelDamage);
                case RaceType.Dragon: return caster.GetAttribute(AttributeType.DragonDamage);
                default: return 0f;
            }
        }

        private float GetRaceResistance(AttributeSystem target, RaceType attackerRace)
        {
            switch (attackerRace)
            {
                case RaceType.Formless: return target.GetAttribute(AttributeType.FormlessResist);
                case RaceType.Undead: return target.GetAttribute(AttributeType.UndeadResist);
                case RaceType.Brute: return target.GetAttribute(AttributeType.BruteResist);
                case RaceType.Plant: return target.GetAttribute(AttributeType.PlantResist);
                case RaceType.Insect: return target.GetAttribute(AttributeType.InsectResist);
                case RaceType.Fish: return target.GetAttribute(AttributeType.FishResist);
                case RaceType.Demon: return target.GetAttribute(AttributeType.DemonResist);
                case RaceType.DemiHuman: return target.GetAttribute(AttributeType.DemiHumanResist);
                case RaceType.Angel: return target.GetAttribute(AttributeType.AngelResist);
                case RaceType.Dragon: return target.GetAttribute(AttributeType.DragonResist);
                default: return 0f;
            }
        }
    }
}
