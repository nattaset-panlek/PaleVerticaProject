# Known Data Issues

Verified: **2026-10-17**. Every place where the code, the data files and the docs disagree, found while building this catalog. **The code is treated as the truth**; nothing here has been fixed (no code, JSON or asset was changed). Items that touch formulas need user approval before anyone changes them (AGENTS.md section 6).

Severity: **High** = changes combat numbers a lot or breaks a feature. **Medium** = a feature silently does nothing. **Low** = cosmetic, misleading text or harmless side effect.

## 1. Code vs data

| # | Severity | Issue | Where | Effect today |
|---|---|---|---|---|
| 1 | High | **Element / size / race units.** `DamageFormula` uses these as fractions (`x (1 + bonus - resist)`, tests use 0.2 = 20%), but item data uses whole numbers. | `CombatStatLookup`, `DamageFormula.Offense`; items 3008, 3013-3016, 3019, 3020, 3025-3028, fragments 4008, 4009, 4015, 4017, 4018, 4020 | WaterResist +25 -> water hits x0 (minimum 1 damage, full immunity). FireDamage +15 -> fire hits **x16**. DragonDamage +25 -> x26 vs Dragon race. DemonDamage +20 -> x21 vs Demon race. GhostDamage +20 -> x21 for Ghost-element skills (only Shadow Dash, which deals no damage). Fix either the data (0.25) or the formula (/100). |
| 2 | High | **`HPPercent <= 30` conditions are always true.** `HPPercent` (id 2) is a bonus stat, normally 0, not the current HP ratio. | 19 entries: 3015, 3016, 3019, 3020, 3027, 3028, 3036-3046, fragments 4015, 4018 | "Defense +100 when HP is low" is **permanently on**. |
| 3 | High | **Combo chain key not loaded.** The assets serialize `RequiredBuffName`, the code field is `RequiredBuffCode` (renamed without `[FormerlySerializedAs]`). `ChainOverrides` are not merged from JSON. | `Wind_Slash_3001.asset`, `Crescent_Sweep_3002.asset`, `SkillChainOverride.cs` | The key is empty, so Wind Slash -> Crescent Sweep -> Cherry Blossom Execution probably never swaps. **Verify in Play mode.** |
| 4 | Medium | Auto-cast trigger `OnAttack` does not exist (`AutoCastTrigger` has only `OnHit`, `OnTakeDamage`). | 3015, 3016, 4015, 4020 | `ItemDatabase` silently drops these effects. |
| 5 | Medium | Missing skill ids referenced by items. | 101 (3025, 4008), 102 (3008, 4009), 108 (3015, 3016, 4015, 4020), 110 (3019, 3020, 4018), 202 (3013, 3014, 3043, 3044, 3045, 4017) | Skill unlocks grant nothing, auto-casts cast nothing. |
| 6 | Medium | `HealEffect` ignores the payload's `Heal`; it heals `BaseHealAmount` only. Scalings into `Heal` have no consumer. | `HealEffect.cs`; skill 1002 | Healing heals a flat **30 HP**; its three Heal scalings (HP x20, HealingItems, HealingItemsPercent x100) do nothing. |
| 7 | Medium | Buffs do not apply the percent-attribute rule, and movement reads only `MoveSpeed`. | skill 1009 "Lightning Speed" (MoveSpeedPercent +70) | No movement speed change. |
| 8 | Medium | Scaling attribute ids that do not exist read as 0. | 1003 Hellfire Blast and 1006 Lightning Strike (id 0), 1004 Divine Blessing (id 221) | Only the flat and per-level parts count (Hellfire +50/level, Lightning +20 +60/level); Divine Blessing's radius scaling is 0. |
| 9 | Low | AOE radius scales with `CooldownReducPercent` (id 152), probably meant `AOE` (150). | 1010 Fire Pila (x0.02), 1003 Hellfire Blast (x0.035), 1006 Lightning Strike (x0.2) | Area grows with cooldown reduction. |
| 10 | Low | Runtime AOE radius comes from `BaseTargeting.Radius`; the top-level `BaseAOERadius` and the asset value are ignored. | 1017 Chornos Field (asset/JSON 20 -> runtime 2), 1005 Earthshatter (0 -> 5), 3001 (0 -> 2), 3002 (0 -> 4), 3003 (0 -> 2) | Two JSON radius fields that can disagree. |
| 11 | Medium | Unique skills are prototypes, and their asset fields are 0 (the converter wrote the JSON "0" values over the class defaults). | 1011 Perfect Parry (log only), 1012 Chain Lightning (log only, `MaxJumps` 0, radius 0), 1013 Blink Strike (no damage effect), 0 Summon Valkyrie (`MinionPrefab` empty: the cast throws after paying 50 MP) | Cost and cooldown are paid for no effect. |
| 12 | Medium | 11 supports are empty (`ModifyPayload` does nothing, no other reader) but their descriptions promise effects. | 1003, 1004, 1009, 1010, 1011, 1012, 1013, 1015, 1016, 1017, 1019 | No effect. Tracked as Combat Core Phase 5. |
| 13 | Low | Summon Valkyrie has SkillID **0**, the same id as basic attacks. | `Summon_Valkyrie_0.asset`, `Skill.json` | Code that checks `SkillID > 0` (Blood Mage) treats it as a basic attack; id-based lookups collide. |
| 14 | Low | 1019 Mind Control exists only in `Skill.json`: no asset, no `MindControlSkill` class, tag `Control` is not a `SkillTag`. | `Skill.json` | Cannot be cast. |
| 15 | Medium | Depression Chaos fires on AOE-tagged skills without a damage effect, for every target, and self procs pass the hostility check. | `DepressionChaosUniquePassive`; skills 1004, 1009 | With Divine Blessing (includes self) and Lightning Blessing (self target) the **caster damages themselves** for INT x 2 + MATK. |
| 16 | Medium | Proc damage runs the full formula with the passive owner as attacker, so ATK/MATK and weapon power are added. | Thorn, Depression Chaos, Bleeding, Blood Bleeding DOT | Damage is far above the JSON text (for example Thorn reflects about ATK even from a 1-damage hit). |
| 17 | Medium | Eldritch Faith adds the **full** current INT to every hit (after defense), and only derived stats see the halved INT. | `EldritchFaithUniquePassive` | Twice the described "lost INT"; cast time still uses full INT. |
| 18 | Low | Curse of Kanix (and Eldritch Faith) change only the inputs of the derived-stat table. | `AttributeSystem.InnerUpdateDerivedAttributes` | Cast time, basic attack bonus, scalings and supports use the real stats. |
| 19 | Low | Rock blocks every `ApplyImpulse`, including the owner's own lerped dashes. | `Character.ApplyImpulse` | Shadow Dash and the Wind Slash combo dashes do not move a Rock owner. |
| 20 | Low | Blink with no enemy near the point moves the caster 50 units along the aim (its effect Distance), ignoring the 20 cast range. | `Blink_1016.asset`, `MovementEffectLogic` | Much longer blink than the indicator shows. |
| 21 | Low | Lightning Strike is tagged Magic but its damage type is Physical. | `Skill.json` 1006 | Uses ATK and Defense; links Life Leech, not Empowered Cast. |
| 22 | Low | Spell Cascade (and Damage Up, Critical Power, Echo) scale `BaseDamage` only, so ATK/MATK is not scaled. | `SpellCascadeSupportModifier` | Cascade circles deal about 95% / 90% of a normal Fire Pila hit instead of 75% / 50%. User has not decided. |
| 23 | Low | `IsPercent` is true on `MoveSpeed` (131) and `AOE` (150), so the equipment rule also adds a PercentAdd to ids 130 (DragonResistPercent) and 149 (TrueDamage). | `Attribute.json` | Harmless today (both bases are 0). |
| 24 | Low | `CooldownReducPercent` set bonuses also add the same percent to flat `CooldownReduc` (id - 1 rule). | `ItemSet.json` | Flat cooldown reduction is amplified by 10-40%. |
| 25 | Low | `UndeadResistPercent` +2% has no flat base to scale. | costume 8001 | No effect. |
| 26 | Low | Toggle skills ignore cast time and cast delay. | 1004, 1008 (Aura of Flames VCT 5, FCT 0.5, delay 1) | Those numbers are unused. |
| 27 | Low | `Consumable.OnUse` is empty and consumables have no effects. | 2001-2020 | Potions and food do nothing. |
| 28 | Low | VIT's +2% `HealingItems` and VIT/INT's MaxHP/MaxMP % only multiply `BaseValue`. | `StatFormula` | HealingItems stays 0; flat gear HP is not scaled by VIT. |
| 29 | Low | Hit, Flee, PerfectDodge, MDEF, TrueDamage, WeightLimit, HealingItems, StatusDMG are calculated or shown but never used. | combat code | No effect until hit/dodge and the other systems exist. |
| 30 | Low | Fixed cast time is reducible (`FixedCastTimeReduc`, `FixedCastTimeReducPercent`). | `CastTimeFormula` | Conflicts with AGENTS.md "Fixed Cast Time remains immutable". Needs a decision. |
| 31 | Low | Execution writes HP directly instead of going through `OutcomeApplier`. | `ExecutionUniquePassive` | Breaks the "OutcomeApplier is the only place HP changes" rule of the combat pipeline. |
| 32 | Low | `Skill.json` effect `Value` fields and the JSON `BaseTargeting` settings other than `Radius` are not read at runtime. | `Skill.json` (e.g. Cherry Blossom Value 250, Wind Slash 100, Crescent Sweep 120) | Editing them changes nothing in game. |

## 2. Docs vs code

| # | Doc | Says | Code |
|---|---|---|---|
| D1 | [All_Attribute_Formulas.md](../Attributes/All_Attribute_Formulas.md) section 0 | `(Base + StatFlat + PassiveFlat + Flat) * (1+PctAdd) * PctMult` | `Base * (1 + StatPercent) + StatFlat + Initial + Flat`; skill-tree passives are in the AdditionalChain (Order 2), not flat |
| D2 | All_Attribute_Formulas.md, VCT row | `(1 - Gear%) * (1 - Skill%)` | a third factor `(1 - VariableCastTimeReducPercent%)`, the stat term is clamped to 1, and three flat reductions |
| D3 | All_Attribute_Formulas.md, id 47 | CriticalDamage "Normal Pipeline (Base 100%)" | base 0; a crit is x(1.5 + CriticalDamage) |
| D4 | All_Attribute_Formulas.md, `...Percent` rows | "none" | on equipment each `IsPercent` attribute also adds PercentAdd to id - 1 |
| D5 | All_Attribute_Formulas.md, ids 35, 139, 149 | MDEF "Magic", HealingItems "+2% per VIT", TrueDamage "Ignores Defense" | none of them is used; magic is reduced by StatusDEF |
| D6 | [Attribute_Mechanics.md](../Attributes/Attribute_Mechanics.md) | Elemental 51-82, Size 83-90; Hit = accuracy, Flee = evasion, Perfect Dodge avoids physical hits; Status DEF "soft defense" | Elemental 51-78, Size 79-90; no hit/dodge roll exists; StatusDEF is the defense subtracted from every non-physical hit |
| D7 | [AdditionalValue_Standard.md](../Attributes/AdditionalValue_Standard.md) pipeline | `+ PassiveFlatBonus (from Skill Tree)` in the normal pipeline | passives are chain entries at Order 2 (the doc's own Order table says so) |
| D8 | [UNIQUE_SKILLS_GUIDE.md](../Skills/UNIQUE_SKILLS_GUIDE.md) | Summon and Parry as working examples (Parry enters `IsParrying`) | both are stubs; links point to the old `Realm of Hero/My project` path |
| D9 | [Planning_Guide.md](../../03-Project/Planning_Guide.md) | Active skills 100-999; Equipment 3000-5999; Materials 7000-8999; a "Mobility Assistant" tool | skills use 0, 1001-1020, 3001-3003; 4001-4020 are fragments and 5001-5020 resources; 7001-7022 support runes, 8001-8012 costumes, 9001-9020 additional; `Editor/ProjectMobilityAssistant/` is empty |
| D10 | [UniquePassiveIdeas.md](../../01-Design/UniquePassiveIdeas.md) rows 41-42 | `$0.12<TAB>ext{ s}$` | the `\t` of `\text` became a tab character; the LaTeX is broken |
| D11 | `SupportSkill.json` | Pierce, Chain, Homing, Knockback, Burn, Freeze, leeches, Duration, Status Chance, Projectile Speed describe effects | no effect (issue 12) |
| D12 | `UniquePassive.json` | Eldritch Faith "lost INT", Thorn "10%", Depression Chaos "200% INT", Bleeding "1% Max HP" | see issues 15-17 |
| D13 | AGENTS.md section 5 | "Fixed Cast Time remains immutable" | reducible (issue 30) |

[Combat_Core_Pipeline.md](../Combat/Combat_Core_Pipeline.md) and the ASPD section of All_Attribute_Formulas.md match the code.
