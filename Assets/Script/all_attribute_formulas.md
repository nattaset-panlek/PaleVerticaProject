# Realm of Hero: All Attribute Formulas (Total 154)

This document provides the derivation and calculation logic for every attribute defined in the `AttributeType` enum.

---

## 0. General Calculation Pipeline
For most attributes, the final value is calculated as follows:
**CurrentValue** = `(BaseValue + StatFlatBonus + PassiveFlatBonus + Modifier.Flat) * (1 + PercentAdd) * PercentMult`
*Followed by the **AdditionalChain** (Order 0-5) for dynamic buffs/debuffs.*

---

## 1. Derived Attribute Formulas
These attributes have complex derivations from Core Stats (STR, AGI, VIT, INT, DEX, LUK).

| ID | Attribute Name | Derivation Formula (StatFlatBonus) |
| :--- | :--- | :--- |
| 21 | **ATK** | `STR + (DEX / 5) + (LUK / 3)` |
| 23 | **MATK** | `(INT * 1.5) + (DEX / 5) + (LUK / 3)` |
| 25 | **Status DMG** | `(LUK * 0.5) + (INT / 4)` |
| 33 | **Status DEF** | `(VIT * 0.7) + (AGI * 0.2) + (LUK * 0.2) + (DEX * 0.2) + INT` |
| 37 | **Hit** | `DEX + (LUK / 3)` |
| 39 | **Flee** | `AGI + (LUK / 5)` |
| 41 | **ASPD** | `(AGI * 0.5) + (DEX * 0.15)` |
| 43 | **Critical Rate** | `LUK * 0.3` |
| 45 | **Perfect Dodge** | `LUK / 10` |
| 49 | **Crit Resistance** | `LUK / 5` |
| 133 | **Weight Limit** | `STR * 30` |
| 135 | **HP Recovery** | `VIT / 5` |
| 137 | **MP Recovery** | `INT / 6` *(If INT >= 120, add (INT - 120) / 2)* |
| 139 | **Healing Items** | `+2% Effectiveness per 1 VIT` (StatPercentBonus) |
| 3 | **MaxHP** | `+1% per 1 VIT` (StatPercentBonus) |
| 7 | **MaxMP** | `+1% per 1 INT` (StatPercentBonus) |
| 141-148 | **Cast Time (VCT/FCT)** | **VCT**: `(Base - Flats) * (1 - sqrt((DEX*2+INT)/530)) * (1 - Gear%) * (1 - Skill%)` <br> **FCT**: `(Base - Flat) * (1 - Percent%)` |
| 151-152 | **Cooldown Reduc** | `(Base - Flat) * (1 - Percent%)` |
| 150 | **AOE Radius** | `Base * (1 + Bonus%)` |

---

## 2. Complete Attribute List (1 - 154)
Attributes marked as **"Normal Pipeline"** use the standard calculation without secondary derivations.

| ID | Attribute Name | Type | Formula / Source |
| :--- | :--- | :--- | :--- |
| 1 | HP | Resource | Current Hit Points |
| 2 | HPPercent | Modifier | HP Percentage Bonus |
| 3 | MaxHP | Resource | Base + (VIT * 1%) |
| 4 | MaxHPPercent | Modifier | Max HP Percentage Bonus |
| 5 | MP | Resource | Current Mana Points |
| 6 | MPPercent | Modifier | MP Percentage Bonus |
| 7 | MaxMP | Resource | Base + (INT * 1%) |
| 8 | MaxMPPercent | Modifier | Max MP Percentage Bonus |
| 9 | STR | Core | Primary Attribute |
| 10 | STRPercent | Modifier | STR Percentage Bonus |
| 11 | AGI | Core | Primary Attribute |
| 12 | AGIPercent | Modifier | AGI Percentage Bonus |
| 13 | VIT | Core | Primary Attribute |
| 14 | VITPercent | Modifier | VIT Percentage Bonus |
| 15 | INT | Core | Primary Attribute |
| 16 | INTPercent | Modifier | INT Percentage Bonus |
| 17 | DEX | Core | Primary Attribute |
| 18 | DEXPercent | Modifier | DEX Percentage Bonus |
| 19 | LUK | Core | Primary Attribute |
| 20 | LUKPercent | Modifier | LUK Percentage Bonus |
| 21 | ATK | Offensive | See Derivation Table |
| 22 | ATKPercent | Modifier | ATK Percentage Bonus |
| 23 | MATK | Offensive | See Derivation Table |
| 24 | MATKPercent | Modifier | MATK Percentage Bonus |
| 25 | StatusDMG | Offensive | See Derivation Table |
| 26 | StatusDMGPercent | Modifier | Status Damage Bonus |
| 27 | WeaponATK | Base | Normal Pipeline |
| 28 | WeaponATKPercent | Modifier | Weapon ATK Bonus |
| 29 | WeaponMATK | Base | Normal Pipeline |
| 30 | WeaponMATKPercent | Modifier | Weapon MATK Bonus |
| 31 | Defense | Defensive | Normal Pipeline (Physical) |
| 32 | DefensePercent | Modifier | Defense Bonus |
| 33 | StatusDEF | Defensive | See Derivation Table |
| 34 | StatusDEFPercent | Modifier | Status DEF Bonus |
| 35 | MDEF | Defensive | Normal Pipeline (Magic) |
| 36 | MDEFPercent | Modifier | Magic DEF Bonus |
| 37 | Hit | Offensive | See Derivation Table |
| 38 | HitPercent | Modifier | Hit Percentage Bonus |
| 39 | Flee | Defensive | See Derivation Table |
| 40 | FleePercent | Modifier | Flee Percentage Bonus |
| 41 | ASPD | Combat | See Derivation Table |
| 42 | ASPDPercent | Modifier | ASPD Percentage Bonus |
| 43 | CriticalRate | Combat | See Derivation Table |
| 44 | CriticalRatePercent | Modifier | Critical Rate Bonus |
| 45 | PerfectDodge | Defensive | See Derivation Table |
| 46 | PerfectDodgePercent | Modifier | Perfect Dodge Bonus |
| 47 | CriticalDamage | Combat | Normal Pipeline (Base 100%) |
| 48 | CriticalDamagePercent| Modifier | Critical Damage Bonus |
| 49 | CritResistance | Defensive | See Derivation Table |
| 50 | CritResistancePercent| Modifier | Crit Resist Bonus |
| 51 | NeutralDamage | Element | Normal Pipeline |
| 52 | NeutralDamagePercent | Modifier | none |
| 53 | NeutralResist | Element | Normal Pipeline |
| 54 | NeutralResistPercent | Modifier | none |
| 55 | WaterDamage | Element | Normal Pipeline |
| 56 | WaterDamagePercent | Modifier | none |
| 57 | WaterResist | Element | Normal Pipeline |
| 58 | WaterResistPercent | Modifier | none |
| 59 | EarthDamage | Element | Normal Pipeline |
| 60 | EarthDamagePercent | Modifier | none |
| 61 | EarthResist | Element | Normal Pipeline |
| 62 | EarthResistPercent | Modifier | none |
| 63 | FireDamage | Element | Normal Pipeline |
| 64 | FireDamagePercent | Modifier | none |
| 65 | FireResist | Element | Normal Pipeline |
| 66 | FireResistPercent | Modifier | none |
| 67 | ElectricDamage | Element | Normal Pipeline |
| 68 | ElectricDamagePercent | Modifier | none |
| 69 | ElectricResist | Element | Normal Pipeline |
| 70 | ElectricResistPercent | Modifier | none |
| 71 | GhostDamage | Element | Normal Pipeline |
| 72 | GhostDamagePercent | Modifier | none |
| 73 | GhostResist | Element | Normal Pipeline |
| 74 | GhostResistPercent | Modifier | none |
| 75 | HolyDamage | Element | Normal Pipeline |
| 76 | HolyDamagePercent | Modifier | none |
| 77 | HolyResist | Element | Normal Pipeline |
| 78 | HolyResistPercent | Modifier | none |
| 79 | SmallDamage | Size | Normal Pipeline |
| 80 | SmallDamagePercent | Modifier | none |
| 81 | SmallResist | Size | Normal Pipeline |
| 82 | SmallResistPercent | Modifier | none |
| 83 | MediumDamage | Size | Normal Pipeline |
| 84 | MediumDamagePercent | Modifier | none |
| 85 | MediumResist | Size | Normal Pipeline |
| 86 | MediumResistPercent | Modifier | none |
| 87 | LargeDamage | Size | Normal Pipeline |
| 88 | LargeDamagePercent | Modifier | none |
| 89 | LargeResist | Size | Normal Pipeline |
| 90 | LargeResistPercent | Modifier | none |
| 91 | FormlessDamage | Race | Normal Pipeline |
| 92 | FormlessDamagePercent| Modifier | none |
| 93 | FormlessResist | Race | Normal Pipeline |
| 94 | FormlessResistPercent| Modifier | none |
| 95 | UndeadDamage | Race | Normal Pipeline |
| 96 | UndeadDamagePercent | Modifier | none |
| 97 | UndeadResist | Race | Normal Pipeline |
| 98 | UndeadResistPercent | Modifier | none |
| 99 | BruteDamage | Race | Normal Pipeline |
| 100| BruteDamagePercent | Modifier | none |
| 101| BruteResist | Race | Normal Pipeline |
| 102| BruteResistPercent | Modifier | none |
| 103| PlantDamage | Race | Normal Pipeline |
| 104| PlantDamagePercent | Modifier | none |
| 105| PlantResist | Race | Normal Pipeline |
| 106| PlantResistPercent | Modifier | none |
| 107| InsectDamage | Race | Normal Pipeline |
| 108| InsectDamagePercent | Modifier | none |
| 109| InsectResist | Race | Normal Pipeline |
| 110| InsectResistPercent | Modifier | none |
| 111| FishDamage | Race | Normal Pipeline |
| 112| FishDamagePercent | Modifier | none |
| 113| FishResist | Race | Normal Pipeline |
| 114| FishResistPercent | Modifier | none |
| 115| DemonDamage | Race | Normal Pipeline |
| 116| DemonDamagePercent | Modifier | none |
| 117| DemonResist | Race | Normal Pipeline |
| 118| DemonResistPercent | Modifier | none |
| 119| DemiHumanDamage | Race | Normal Pipeline |
| 120| DemiHumanDamagePercent| Modifier | none |
| 121| DemiHumanResist | Race | Normal Pipeline |
| 122| DemiHumanResistPercent| Modifier | none |
| 123| AngelDamage | Race | Normal Pipeline |
| 124| AngelDamagePercent | Modifier | none |
| 125| AngelResist | Race | Normal Pipeline |
| 126| AngelResistPercent | Modifier | none |
| 127| DragonDamage | Race | Normal Pipeline |
| 128| DragonDamagePercent | Modifier | none |
| 129| DragonResist | Race | Normal Pipeline |
| 130| DragonResistPercent | Modifier | none |
| 131| MoveSpeed | Utility | Base Movement Velocity |
| 132| MoveSpeedPercent | Modifier | none |
| 133| WeightLimit | Utility | See Derivation Table |
| 134| WeightLimitPercent | Modifier | none |
| 135| HPRecovery | Utility | See Derivation Table |
| 136| HPRecoveryPercent | Modifier | none |
| 137| MPRecovery | Utility | See Derivation Table |
| 138| MPRecoveryPercent | Modifier | none |
| 139| HealingItems | Utility | See Derivation Table |
| 140| HealingItemsPercent | Modifier | none |
| 141| FixedCastTimeReduc | Utility | Normal Pipeline |
| 142| FixedCastTimeReducPercent| Modifier | none |
| 143| GearVariableCastTime| Utility | Normal Pipeline |
| 144| GearVariableCastTimePercent| Modifier | none |
| 145| SkillVariableCastTime| Utility | Normal Pipeline |
| 146| SkillVariableCastTimePercent| Modifier | none |
| 147| VariableCastTimeReduc| Utility | Normal Pipeline |
| 148| VariableCastTimeReducPercent| Modifier | none |
| 149| TrueDamage | Offensive | Ignores Defense Pipeline |
| 150| AOE | Utility | Skill Radius Modifier |
| 151| CooldownReduc | Utility | Normal Pipeline |
| 152| CooldownReducPercent| Modifier | none |
| 153| none | none | none |
| 154| none | none | none |
| 210| BaseLevel | Progression| Base Character Level |
| 211| JobLevel | Progression| Job/Class Level |
