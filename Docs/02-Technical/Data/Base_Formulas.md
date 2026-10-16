# Pale Vertica: Base Formulas (standalone reference)

Verified against code: **2026-10-17**. This page explains every base formula in the game by itself, so it can be read or presented without opening the other docs. The code is the source of truth: when this page and the code disagree, the code wins (list of known disagreements: [Known_Data_Issues.md](./Known_Data_Issues.md)).

All formulas live in `Assets/Script/CoreSystem/Combat/Formulas/` (`StatFormula`, `DerivedStatFormula`, `DamageFormula`, `CombatStatLookup`, `BasicAttackFormula`, `CastTimeFormula`, `SkillValueFormula`, `ResourceFormula`, `ProgressionFormula`). Callers (`Attribute`, `AttributeSystem`, `SkillCaster`, `SkillInstance`, `Character`, `Equipment`) only collect inputs and apply results. Changing any formula needs user approval (AGENTS.md section 6).

Running example used on this page: a character with **STR 30, AGI 40, VIT 20, INT 50, DEX 30, LUK 10**, no gear, no buffs.

---

## 1. Attributes and their starting values

Every `Character` owns one `AttributeSystem` holding one `Attribute` per `AttributeType` (154 types, ids from `Attribute.json`). All start at 0 except:

| Attribute | Starting `BaseValue` |
|---|---|
| STR, AGI, VIT, INT, DEX, LUK | 1 |
| MaxHP | 100 |
| MaxMP | 50 |
| HP, MP | start full (= MaxHP / MaxMP after derived stats) |
| HPRecovery, MPRecovery | 1 (per second) |
| MoveSpeed | 100 (percent of the movement speed multiplier) |
| ASPD | 100 (percent attack speed, `BasicAttackFormula.BaseAspd`) |
| WeightLimit | 2000 |
| ATK | 10 (MATK starts at 0) |
| StatusDMG | 10 |
| BaseLevel, JobLevel | 1 |

Per-character overrides set in the Inspector (`initialAttributes`) go into `InitialValue`, not `BaseValue` (see section 2). The training dummy gets `InitialValue` 9,999,999 MaxHP.

---

## 2. How one attribute value is calculated

`Attribute.Recalculate()` calls `StatFormula.Compute`:

```
value = Base * (1 + StatPercentBonus) + StatFlatBonus     <- derived stats (section 3)
      + InitialValue                                      <- Inspector override
      + SUM(Flat modifiers)                               <- gear, buffs
value = value * (1 + SUM(PercentAdd modifiers))           <- additive percents
value = value * PRODUCT(PercentMult modifiers)            <- multiplicative percents
value = AdditionalChain.Apply(value)                      <- ordered extra steps
```

This is the project's "Additional Value" order: **Base -> Flat -> Percent**.

- `StatPercentBonus` (from VIT/INT, section 3) multiplies **only `BaseValue`**. Flat gear HP is not scaled by VIT.
- `AdditionalChain` entries are sorted by `Order` (equal orders keep insertion order). Each entry is either `+ value` or `x value`. Convention: 0 base override, 1 skill bonus, 2 skill tree passives (tag `"Passive"`, rebuilt by `ApplyPassiveStats`), 3 buffs, 4 debuffs, 5 caps.
- Skill tree node stats go through the chain at Order 2: a `...Percent` stat becomes `x (1 + value/100)` on its base attribute, anything else becomes `+ value`.
- Current HP and MP are stored the same way: the game writes `BaseValue` and recalculates.

### 2a. Percent attributes on equipment
Most stats have a twin `...Percent` attribute with id + 1 (e.g. ATK 21 / ATKPercent 22). When an equipment effect (`AttributeBonusEffect`) gives an attribute whose `IsPercent` is true in `Attribute.json`:
1. the value is added as a **Flat** modifier to that attribute itself (so it shows in the UI), and
2. `value / 100` is added as a **PercentAdd** modifier to the attribute with **id - 1**.

Example: ATKPercent +10 gives ATK `x (1 + 0.10)` in the PercentAdd step.
Buffs from skills (`ApplyTemporaryBuffEffect`) and scaling effects do **not** do step 2; they only touch the attribute they name.

### 2b. Refine bonus on equipment stat effects
An `AttributeBonusEffect` on refined gear adds, per refine level:
- flat attribute: **+1**
- percent attribute (`IsPercent`): **+1% of its own value** (`value * 0.01`)

Examples: ASPD +10 at refine +5 = +15. MoveSpeed +15 (flagged `IsPercent`) at refine +10 = 15 + 10 * 0.15 = 16.5.

---

## 3. Derived stats from the six core stats

`DerivedStatFormula.Compute` runs after every change (`AttributeSystem.UpdateDerivedAttributes`). It reads the six core stats' **current** values, lets unique passives rewrite them (`ModifyCoreAttributes`, e.g. Curse of Kanix swaps, Eldritch Faith halves INT), then writes the results into `StatFlatBonus` or `StatPercentBonus`:

| Result | Formula | Goes into |
|---|---|---|
| ATK | `STR + DEX/5 + LUK/3` | flat (on top of base 10) |
| MATK | `INT*1.5 + DEX/5 + LUK/3` | flat |
| Hit | `DEX + LUK/3` | flat |
| Flee | `AGI + LUK/5` | flat |
| ASPD | `AGI + DEX*0.3` | flat (on top of base 100) |
| CriticalRate | `LUK*0.3` (percentage points) | flat |
| CritResistance | `LUK/5` | flat |
| PerfectDodge | `LUK/10` | flat |
| StatusDMG | `LUK*0.5 + INT/4` | flat (on top of base 10) |
| StatusDEF | `VIT/2 + AGI/5 + LUK/5 + VIT/5 + DEX/5 + INT` = `VIT*0.7 + AGI*0.2 + LUK*0.2 + DEX*0.2 + INT` | flat |
| WeightLimit | `STR*30` | flat (on top of base 2000) |
| HPRecovery | `VIT/5` | flat (on top of base 1) |
| MPRecovery | `INT/6`, plus `(INT - 120)/2` when INT >= 120 | flat (on top of base 1) |
| MaxHP | `VIT * 1%` | percent of `BaseValue` |
| MaxMP | `INT * 1%` | percent of `BaseValue` |
| HealingItems | `VIT * 2%` | percent of `BaseValue` (base is 0, so this is 0 today) |

Notes:
- `ModifyCoreAttributes` only changes the numbers fed to this table. The STR/AGI/VIT/INT/DEX/LUK attributes themselves keep their values, so cast time, basic attack bonus damage, skill scalings and supports still read the unmodified stats.
- MPRecovery examples: INT 30 -> +5, INT 99 -> +16.5, INT 120 -> +20, INT 150 -> +40 (25 + 15).

### Worked example (STR 30, AGI 40, VIT 20, INT 50, DEX 30, LUK 10)

| Stat | Calculation | Value |
|---|---|---|
| ATK | 10 + 30 + 6 + 3.33 | **49.33** |
| MATK | 0 + 75 + 6 + 3.33 | **84.33** |
| MaxHP | 100 * (1 + 0.20) | **120** |
| MaxMP | 50 * (1 + 0.50) | **75** |
| HPRecovery | 1 + 4 | **5 / s** |
| MPRecovery | 1 + 8.33 | **9.33 / s** |
| Hit | 30 + 3.33 | 33.33 |
| Flee | 40 + 2 | 42 |
| ASPD | 100 + 40 + 9 | **149** (1.49 attacks/s, 0.671 s delay) |
| CriticalRate | 10 * 0.3 | 3 |
| CritResistance | 10 / 5 | 2 |
| PerfectDodge | 10 / 10 | 1 |
| StatusDMG | 10 + 5 + 12.5 | 27.5 |
| StatusDEF | 14 + 8 + 2 + 6 + 50 | **80** |
| WeightLimit | 2000 + 900 | 2900 |

---

## 4. Damage

Every hit (skill, basic attack, projectile, proc, damage over time) goes through `CombatCore.Execute` -> `DamageHandler` -> `DamageFormula`.

### 4a. Who can be hit (Gate)
- The target must exist and have stats.
- Damage requires the target's faction to be hostile to the user's (`FactionDatabase.IsHostile`). Self-inflicted procs (bleeding) skip this check.

### 4b. Offense (attacker side, `DamageFormula.Offense`)

```
damage  = payload.BaseDamage
damage += ATK + WeaponATK            if DamageType is Physical
          MATK + WeaponMATK          otherwise (Magic, Status, Elemental)
damage *= ElementTable[skill element][target element]          (section 4d)
damage *= max(0, DamageMultiplier)                              (whole-hit multiplier, default 1)
damage *= max(0, 1 + ElementDamage[skill element] - target ElementResist[skill element])
damage *= max(0, 1 + SizeDamage[target size]      - target SizeResist[attacker size])
damage *= max(0, 1 + RaceDamage[target race]      - target RaceResist[attacker race])
crit chance = max(0, CriticalRate - target CritResistance)      (percentage points, roll 0-100)
on crit: damage *= 1.5 + CriticalDamage
```

- The caster power, element/size/race factors and the crit roll only apply when the attacker has stats. The element table always applies.
- The element/size/race bonus and resist values are used as **fractions** (0.2 = +20%). See Known_Data_Issues for the item data that uses whole numbers.
- `CriticalDamage` starts at 0, so a crit is x1.5.
- `DamageMultiplier` is the only whole-hit scaler. Repeats use it: Echo Spell x0.6, Double Spell x0.4 (they multiply). Supports that change `BaseDamage` instead (Damage Up, Critical Power, Spell Cascade) are added **before** ATK/MATK, so they do not scale the caster's power.

### 4c. Defense (target side, `DamageFormula.Mitigate`)

```
final = max(1, damage - defense)
defense = target Defense      for Physical
          target StatusDEF    for Magic, Status, Elemental
```

Every hit deals at least **1**. `MDEF` is not used.

### 4d. Element table (attacker row x defender column)

| Attack \ Defend | Neutral | Water | Earth | Fire | Electric | Ghost | Holy |
|---|---|---|---|---|---|---|---|
| **Neutral** | 1 | 1 | 1 | 1 | 1 | **0** | 1 |
| **Water** | 1 | 0.25 | 0.5 | **1.5** | 1 | 1 | 0.75 |
| **Earth** | 1 | 1 | 0.25 | 0.5 | **1.5** | 0.75 | 0.75 |
| **Fire** | 1 | 0.5 | **1.5** | 0.25 | 1 | 1 | 0.75 |
| **Electric** | 1 | **1.5** | 0.5 | 1 | 0.25 | 1 | 0.75 |
| **Ghost** | **0** | 1 | 1 | 1 | 1 | **1.5** | 0.75 |
| **Holy** | 0.75 | 1 | 1 | 1 | 1 | **1.5** | **0** |

Cycle: Water > Fire > Earth > Electric > Water. Neutral cannot hurt Ghost and Ghost cannot hurt Neutral (both deal the minimum 1).

### 4e. After the number is final (`DamageHandler.Commit`)
1. Attacker's unique passives `OnHit` may change the damage (Blood Mage x1.3, Eldritch Faith + INT).
2. Target's unique passives `OnTakeDamage` may change it (Blessing of Magearna absorbs 40% into MP; Thorn reflects).
3. HP is reduced (never below 0).
4. Attacker's `OnPostHit` passives run (Vampire, Mana Leech, Blood Bleeding, Execution).
5. `OnHitTarget` / `OnTakeDamage` events fire (item auto-casts), damage text shows.

These passive changes happen **after** defense, so they are not reduced by it.

### 4f. Proc damage
Passive damage (Thorn reflect -999, Depression Chaos burst -998, bleed ticks -997) is a new hit with `BaseDamage = amount`, Neutral element, level 1, sent through the same formula with the passive's owner as the attacker. The owner's ATK/MATK and weapon power are therefore **added** to the stated amount, crit can roll, and the target's defense is subtracted. Procs can chain at most `CombatCore.MaxDepth = 2` deep.

### 4g. Worked example: Fire Bolt
Fire Bolt (base 40, Magic, Fire) from the example character (MATK 84.33, no weapon) on a target with all core stats 1 and Neutral element (StatusDEF 2.3, CritResistance 0.2):

| Case | Calculation | Damage |
|---|---|---|
| Normal hit | (40 + 84.33) x 1.0 - 2.3 | **122.03** |
| Critical (2.8% chance) | 124.33 x 1.5 - 2.3 | 184.2 |
| Echo Spell repeat | 124.33 x 0.6 - 2.3 | 72.3 |
| Double Spell repeat | 124.33 x 0.4 - 2.3 | 47.43 |
| Echo of the Double Spell repeat | 124.33 x 0.24 - 2.3 | 27.54 |

---

## 5. Basic attack

`Character.BasicAttack` builds a payload with SkillID 0, level 1, Neutral element and the character's auto-attack damage type (default Physical).

| Item | Formula |
|---|---|
| Bonus base damage | Magic auto-attack: `INT`; else Melee: `STR`; Range: `DEX` (`BasicAttackFormula.BonusDamage`) |
| Then | the normal damage formula adds ATK + WeaponATK (or MATK + WeaponMATK) |
| Attacks per second | `1.0 * clamp(ASPD, 50, 300) / 100` |
| Delay between attacks | `1 / attacks per second` = 0.33 s (cap 300) to 2 s (floor 50) |

- Melee with STR 30 and ATK 49.33, no weapon, vs Defense 0: 30 + 49.33 = **79.33**.
- ASPD is linear: +10 ASPD is always +0.1 attacks per second, and a +10% ASPD buff on 149 gives 163.9 (1.64/s).
- Ranged auto-attacks spawn a projectile; melee hits instantly.
- Double Attack (unique passive) strikes again after **0.12 s at x1.0**, if both sides are still alive. The repeat also counts as a basic attack for "on basic attack" passives (Time Collector).

---

## 6. Skills

### 6a. Payload build order (`SkillCaster.BuildPayload`)
1. Copy the skill's numbers: BaseDamage, VCT, FCT, CastDelay, ManaCost, ProjectileCount.
2. Area: `AOERadius = BaseAOERadius * (1 + AOE/100)`. For ground-target skills the cast range is scaled the same way: `CastRange * (1 + AOE/100)`.
3. Cooldown: `max(0, (BaseCooldown - CooldownReduc) * (1 - CooldownReducPercent/100))`.
4. Scalings (`OutputScaling`), each adds to its target field (BaseDamage, AOERadius, ProjectileCount, Heal, VCT, FCT, CastDelay, Cooldown, ManaCost):
   `(stat * AttributeMultiplier + FlatBonus + LevelMultiplier * skillLevel) * GlobalMultiplier`
   The stat is read from the caster's current value. An attribute id that does not exist reads as 0. Scaling runs after steps 2-3, so scaled area or cooldown is not affected by AOE% or CDR.
5. Linked supports, in slot order (`SupportModifier.ModifyPayload`), up to `MaxSupportSlots` (5 for every skill today).
6. Total cast time from section 6b.

### 6b. Cast time (`CastTimeFormula`)

```
statReduction = sqrt( clamp01( (DEX*2 + INT) / 530 ) )
VCT = (baseVCT - VariableCastTimeReduc - GearVariableCastTime - SkillVariableCastTime)
      * (1 - statReduction)
      * (1 - GearVariableCastTimePercent/100)
      * (1 - SkillVariableCastTimePercent/100)
      * (1 - VariableCastTimeReducPercent/100)
FCT = (baseFCT - FixedCastTimeReduc) * (1 - FixedCastTimeReducPercent/100)
Total = max(0, VCT) + max(0, FCT)
```

- VCT reaches 0 when `DEX*2 + INT >= 530` (for example DEX 150 + INT 230).
- FCT can be reduced by the two FixedCastTime attributes. AGENTS.md says FCT is immutable; the code does not enforce that (open question for the user).
- DEX and INT are read directly from the attributes (not through `ModifyCoreAttributes`).

| DEX / INT | Stat reduction of VCT |
|---|---|
| 1 / 1 | 7.5% |
| 30 / 50 | 45.6% |
| 50 / 50 | 53.2% |
| 99 / 99 | 74.9% |
| 150 / 230 | 100% |

Example: Fire Bolt VCT 0.4 with DEX 30 / INT 50 -> 0.4 x (1 - 0.456) = 0.218 s, plus FCT 0.1 = **0.318 s** total.

### 6c. Casting sequence
1. Cost check (section 7). If the caster cannot pay, nothing happens.
2. Total cast time 0 -> execute now, otherwise cast for that long (interruptible).
3. On execute: cooldown starts, the cast delay (`CastDelay`) locks other casts for that many seconds, the cost is paid, then targeting -> effects -> visuals -> "on skill executed" passives.

### 6d. Toggle skills (`IsToggle`)
Pressing turns the skill on (cost checked) and fires immediately if off cooldown. While on, it fires again **every time its cooldown ends**, paying the cost each pulse, and turns itself off when the cost cannot be paid. Toggles never use cast time or cast delay.

### 6e. Repeats and splits

| Source | Trigger | Delay | Changes on the copy | Cost / cooldown |
|---|---|---|---|---|
| Echo Spell (support 1021) | every cast of the linked skill | 0.15 s | `DamageMultiplier x0.6`, `AOERadius x1.3` | free |
| Double Spell (unique passive) | skill instance at level 1 | 0.2 s | `DamageMultiplier x0.4`, cast from the caster's current position | free |
| Double Attack (unique passive) | every basic attack | 0.12 s | damage x1.0 | n/a |
| Spell Cascade (support 1022) | ground-target skill | none | 3 copies: main `BaseDamage x0.75`, left/right `BaseDamage x0.5`, sides 5 units to each side, perpendicular to the aim | one cost |

- A Double Spell copy is a full cast, so a linked Echo Spell echoes it too: **4 hits at 100% / 60% / 40% / 24%** of the whole hit.
- Echo Spell repeats go through Spell Cascade again (3 circles per echo).
- Spell Cascade scales `BaseDamage` only, so the caster's ATK/MATK is added in full. Fire Pila (base 20) with MATK 84.33: normal 104.33, main circle 15 + 84.33 = 99.33 (95%), side circle 10 + 84.33 = 94.33 (90%).

### 6f. Projectiles
`SpawnProjectileEffect` fires `ProjectileCount` projectiles from the caster toward the aim. With more than one, they spread 45 degrees apart up to a 180 degree arc. A projectile flies at its prefab `Speed` (Fire Bolt's `MagicProjectile.prefab`: 8 units/s), lives 5 s, hits the first hostile it touches and is destroyed.

---

## 7. Resources

| Rule | Formula |
|---|---|
| Mana cost | `ManaCost` after scalings and supports (Cost Reduction x0.5) |
| Blood Mage | the mana cost is paid in HP instead (`hpCost = manaCost`, mana 0) |
| Can cast | `MP >= cost` (or `HP >= cost` for HP costs) |
| After paying | `MP = max(0, MP - cost)`; `HP = max(1, HP - cost)` (an HP cost never kills) |
| Regeneration | every **1 s**, while HP > 0: `HP += HPRecovery`, `MP += MPRecovery`, capped at max |

Example: the example character regenerates 5 HP and 9.33 MP per second.

---

## 8. Equipment and progression (`ProgressionFormula`)

### 8a. Weapon and armor baseline
```
baseline = max(1, base) * (1 + RefineMultiplier(refine)) + reforge * 5
weapon (one-hand left/right, two-hand): BasePhysical -> WeaponATK, BaseMagic -> WeaponMATK
anything else:                          BasePhysical -> Defense,   BaseMagic -> StatusDEF
```
Every equipped piece gives at least 1 of each (the `max(1, base)` floor). Example: Iron Sword (BasePhysical 35) at refine +7, reforge 2 = 35 x 1.50 + 10 = **62.5 WeaponATK**.

### 8b. Refine table

| Refine | +0 | +1 | +2 | +3 | +4 | +5 | +6 | +7 | +8 | +9 | +10 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Bonus | 0% | 5% | 10% | 15% | 20% | 30% | 40% | 50% | 65% | 80% | 100% |

| Refine | +11 | +12 | +13 | +14 | +15 | +16 | +17 | +18 | +19 | +20 |
|---|---|---|---|---|---|---|---|---|---|---|
| Bonus | 120% | 145% | 170% | 200% | 230% | 265% | 300% | 350% | 400% | 500% |

Levels above +20 use +20. Reforge adds a flat **+5 per level** to the baseline.

### 8c. Other equipment effects (from `Item.json`)

| Effect | Rule |
|---|---|
| Stat bonus (`AttributeBonusEffect`) | flat on the attribute, percent rule from section 2a, refine bonus from 2b |
| Conditional bonus (`BonusCondition`) | stat bonus on while `attribute <op> threshold` holds, re-checked on every stat change |
| Scaling bonus | `floor(sum of source stats / threshold) * value`, flat |
| Refine bonus | stat bonus active when the item's refine >= threshold |
| Auto-cast | `chance %` on hit or on taking damage: cast the skill id as a normal cast (cost, cast time and "caster is idle" all apply) |
| Skill unlock | grants the skill while equipped |
| Unique passive | adds the named unique passive while equipped |
| Set bonus (`ItemSet.json`) | stat bonuses at N equipped pieces |

### 8d. Experience
The game uses the `ExperienceTable` asset. Only when no table is assigned it falls back to: next base level `100 * L^1.5`, next job level `50 * L^1.6` (rounded down).

---

## 9. Attributes that exist but do nothing yet

These are calculated and shown in the UI but nothing in combat reads them (verified by search, 2026-10-17):

| Attribute | Why |
|---|---|
| Hit, Flee, PerfectDodge | there is no hit/miss/dodge roll (planned "Phase 6", needs approval) |
| MDEF | magic damage is reduced by StatusDEF |
| TrueDamage | not read by the damage formula |
| WeightLimit | no inventory weight check |
| HealingItems, StatusDMG | no reader |
| `...Percent` element/size/race twins | only matter through the id - 1 rule on equipment (section 2a) |

---

## 10. Quick formula sheet

| Name | Formula |
|---|---|
| Attribute | `(Base*(1+StatPct) + StatFlat + Initial + Flat) * (1+PctAdd) * PctMult -> AdditionalChain` |
| ATK / MATK | `10 + STR + DEX/5 + LUK/3` / `INT*1.5 + DEX/5 + LUK/3` |
| MaxHP / MaxMP | `100*(1+VIT%)` / `50*(1+INT%)` + flats |
| StatusDEF | `VIT*0.7 + AGI*0.2 + LUK*0.2 + DEX*0.2 + INT` |
| ASPD | `100 + AGI + DEX*0.3`; attacks/s `clamp(ASPD,50,300)/100` |
| Crit | chance `CritRate - CritRes` (LUK*0.3 vs LUK/5), x(1.5 + CritDamage) |
| Damage | `(Base + Power) * Element * Mult * (1+Elem) * (1+Size) * (1+Race) * Crit - Defense`, min 1 |
| VCT | `(VCT - flats) * (1 - sqrt((DEX*2+INT)/530)) * three % factors` |
| FCT | `(FCT - flat) * (1 - %)` |
| Cooldown | `(CD - flat) * (1 - %)` |
| Area / range | `base * (1 + AOE%)` |
| Skill scaling | `(stat*AttrMult + Flat + LevelMult*lvl) * Global` |
| Regen | `+HPRecovery`, `+MPRecovery` every 1 s |
| Gear baseline | `max(1,base) * (1 + refine%) + reforge*5` |
