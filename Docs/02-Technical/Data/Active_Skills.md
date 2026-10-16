# Active Skills Catalog

Verified against `Assets/Resources/Data/Skill.json` and `Assets/Resources/Skill/ActiveSkill/**/*.asset`: **2026-10-17**.

Numbers are the **runtime** values: `Skill.json` merged onto the asset (rules in [README](./README.md#how-an-active-skill-is-assembled-at-runtime)). The radius column is `BaseTargeting.Radius` from the JSON, which is what the game uses. Damage numbers are the skill's own `BaseDamage`; the caster's ATK (physical) or MATK (magic) is always added on top ([Base_Formulas](./Base_Formulas.md#4-damage)). All skills start at level 1, cooldowns and cast times are before stat reductions.

Status legend: **Working** = does what its data says. **Partial** = runs, but part of the data has no effect. **Prototype** = unique skill whose code only logs or lacks data. **Data only** = cannot be cast.

## Summary

| ID | Name | Element | Type | Base dmg | MP | VCT | FCT | Cast delay | CD | Radius | Targeting | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1001 | Tackle | Neutral | Physical | 1 | 0 | 0 | 0.35 | 0.5 | 1 | (box) | Box 2 x 2.5 | Working |
| 1002 | Healing | Holy | Magic | 0 | 25 | 0 | 0 | 0 | 0 | 0 | Self | Partial |
| 1003 | Hellfire Blast | Fire | Physical | 180 | 10 | 1.5 | 0.5 | 0.2 | 5 | 3.5 | Cone 245 | Working |
| 1004 | Divine Blessing | Holy | Magic | 0 | 20 | 1 | 0.15 | 0.15 | 1 | 5 | Sphere (allies + self) | Working (toggle) |
| 1005 | Earthshatter | Earth | Physical | 50 | 10 | 0.35 | 0 | 0.25 | 2 | 5 | Cone 60 | Working |
| 1006 | Lightning Strike | Electric | Physical | 220 | 15 | 1.5 | 0 | 0.1 | 2.5 | 10 | Line (width 0.5) | Working |
| 1007 | Shadow Dash | Ghost | Physical | 0 | 8 | 0.15 | 0 | 0.1 | 0.5 | 0 | Self | Working |
| 1008 | Aura of Flames | Fire | Magic | 5 | 10 | 5 | 0.5 | 1 | 1 | 3 | Sphere (enemies) | Working (toggle) |
| 1009 | Lightning Blessing | Electric | Magic | 0 | 10 | 2 | 0.15 | 0.1 | 0 | 2 | Self | Partial |
| 1010 | Fire Pila | Fire | Magic | 20 | 20 | 1 | 0.15 | 0 | 0.15 | 2 | Ground target, range 5 | Working |
| 1011 | Perfect Parry | Neutral | Physical | 0 | 15 | 0 | 0 | 0 | 5 | 0 | Single | Prototype |
| 1012 | Chain Lightning | Electric | Magic | 100 | 40 | 0 | 0 | 0 | 8 | 0 | Single | Prototype |
| 1013 | Blink Strike | Neutral | Physical | 150 | 30 | 0 | 0 | 0 | 6 | 1 | Ground target, range 8 | Prototype |
| 1014 | Time Dilation | Neutral | Magic | 1 | 20 | 1 | 0 | 0.1 | 1 | 3 | Ground target, range 10 | Working |
| 1015 | Force Nova | Neutral | Magic | 0 | 10 | 0.25 | 0 | 0.15 | 0 | 3 | Ground target, range 5 | Working |
| 1016 | Blink | Electric | Magic | 0 | 10 | 0 | 0.15 | 0 | 0.25 | 1 | Ground target, range 20 | Working (see notes) |
| 1017 | Chornos Field | Neutral | Physical | 0 | 0 | 0 | 0 | 0 | 0 | 2 | Sphere | Prototype (no effects) |
| 1019 | Mind Control | Neutral | Magic | 0 | 30 | 1.5 | 0.5 | 0.5 | 12 | 5 | (JSON: SingleUnit) | Data only |
| 1020 | Fire Bolt | Fire | Magic | 40 | 8 | 0.4 | 0.1 | 0 | 0.5 | 0 | Self + projectile | Working |
| 3001 | Wind Slash | Neutral | Physical | 10 | 5 | 0 | 0 | 0.5 | 1 | 2 | Cone 60 | Working (combo start) |
| 3002 | Crescent Sweep | Neutral | Physical | 15 | 5 | 0 | 0 | 0.5 | 1 | 4 | Cone 102 | Working (combo 2) |
| 3003 | Cherry Blossom Execution | Neutral | Physical | 30 | 5 | 0 | 0 | 0.5 | 15 | 2 | Sphere | Working (combo 3) |
| 0 | Summon Valkyrie | Neutral | Physical | 0 | 50 | 2 | 0.5 | 1 | 30 | 0 | Single | Prototype (null prefab) |

Every skill accepts these 10 supports: Echo (1002), Damage Up (1006), Cooldown Reduction (1007), Cost Reduction (1008), Duration Up (1009), Status Chance Up (1010), Knockback (1013), Critical Power (1014), Mana Leech (1015), Echo Spell (1021). The "Extra supports" line below lists the conditional ones. Full matrix: [Support_Skills.md](./Support_Skills.md#compatibility-matrix). No skill has default linked supports (`SupportSkillIDs` is empty everywhere).

---

## Standard skills

### 1001 Tackle
- **Tags**: Melee, Physical, Instant. Visual: InstantHit.
- **Targeting**: `BoxTargeting`, enemies, box 2 wide x 2.5 long, centered 1.5 units in front of the caster along the aim.
- **Effects**: damage to every target; stun each target 0.15 s.
- **Scaling**: BaseDamage += STR x 15 x 0.15 = **STR x 2.25** (STR 30 -> base 68.5, then + ATK).
- **Extra supports**: Life Leech.

### 1002 Healing
- **Tags**: Self, Buff, Magic, Heal. Visual: InstantHit.
- **Targeting**: `SelfTargeting`.
- **Effects**: heal the caster **30 HP** (asset `BaseHealAmount`; JSON `Heal` is 0, so it is not overridden). Capped at MaxHP.
- **Scalings in data**: Heal += HP x 20, Heal += HealingItems x 1, Heal += HealingItemsPercent x 100. These have **no effect**: `HealEffect` ignores the payload's Heal value.
- **Extra supports**: Empowered Cast.

### 1003 Hellfire Blast
- **Tags**: AreaOfEffect, Melee, Elemental, Status. Visual: GroundEruption.
- **Targeting**: `ConeTargeting`, enemies, 245 degree cone, radius 3.5, from the caster toward the aim.
- **Effects**: damage.
- **Scalings**: BaseDamage += 50 x level (the stat id in the data is 0, which does not exist and reads 0) -> **230** at level 1. AOE radius += CooldownReducPercent x 0.035.
- **Note**: projectile count 1 in data, unused (no projectile effect).
- **Extra supports**: Area Increase, Burn Chance, Life Leech.

### 1004 Divine Blessing (toggle)
- **Tags**: Buff, AreaOfEffect, Spirit, Magic. Visual: AuraBuff.
- **Targeting**: `SphereAOETargeting`, **allies, includes self**, radius 5 around the caster.
- **Effects per target, every pulse**: heal **200 HP** (JSON `Heal` 200); AGI +10, DEX +10, INT +10 (flat, 5 s each, max 1 stack, refreshed by the next pulse).
- **Toggle**: pulses every 1 s (its cooldown) and pays 20 MP per pulse. VCT/FCT/cast delay are not used by toggles.
- **Scaling**: AOE radius += attribute 221 x 1 (id does not exist, reads 0).
- **Extra supports**: Area Increase, Empowered Cast.

### 1005 Earthshatter
- **Tags**: Melee, Physical, Elemental, Movement. Visual: GroundEruption.
- **Targeting**: `ConeTargeting`, enemies, 60 degrees, radius **5** (the asset says 0; the JSON targeting radius wins).
- **Effects**: damage; push each target 5 units away from the caster over 0.35 s.
- **Extra supports**: Life Leech. (No AreaOfEffect tag, so no Area Increase.)

### 1006 Lightning Strike
- **Tags**: Ranged, Ray, Magic, Elemental, AreaOfEffect. Visual: RayBeam.
- **Targeting**: `LinePierceTargeting`, enemies, a 0.5-radius circle cast along the aim for **10 units** (the radius value is the line length). Hits everything on the line.
- **Effects**: damage.
- **Scalings**: BaseDamage += 20 + 60 x level (stat id 0 reads 0) -> **300** at level 1. AOE (line length) += CooldownReducPercent x 0.2.
- **Note**: tagged Magic but its damage type is **Physical** (uses ATK, reduced by Defense).
- **Extra supports**: Area Increase, Life Leech.

### 1007 Shadow Dash
- **Tags**: Movement, Self, Instant. Visual: InstantHit.
- **Targeting**: `SelfTargeting`.
- **Effects**: dash 3 units toward the aim over 0.15 s (once per cast).
- **Extra supports**: Life Leech.

### 1008 Aura of Flames (toggle)
- **Tags**: Aura, Self, AreaOfEffect, Elemental, Magic. Visual: AuraBuff.
- **Targeting**: `SphereAOETargeting`, enemies, radius 3 around the caster.
- **Effects**: damage every pulse (1 s).
- **Scalings**: BaseDamage += MaxHP x 0.1 + 10 + 20 x level; ManaCost += MaxMP x 0.1; radius += INT x 0.005 and += AOE x 0.06.
- **Example** (MaxHP 120, MaxMP 75, INT 50, level 1): base damage 5 + 12 + 10 + 20 = **47** (+ MATK), cost 10 + 7.5 = **17.5 MP per pulse**, radius 3.25.
- **Toggle**: VCT 5 / FCT 0.5 / cast delay 1 in the data are not used by toggles.
- **Extra supports**: Area Increase, Burn Chance, Empowered Cast.

### 1009 Lightning Blessing
- **Tags**: Elemental, Buff, AreaOfEffect, Self, Magic. Visual: InstantHit.
- **Targeting**: `SelfTargeting`.
- **Effects**: "Lightning Agility" AGI +30 flat for 1 s; "Lightning Speed" MoveSpeedPercent +70 flat for 10 s.
- **Note**: the MoveSpeedPercent buff does **not** change movement speed (buffs do not convert `...Percent` attributes and movement reads only MoveSpeed). Both buffs have MaxStacks 0, so recasting only refreshes the duration.
- **Extra supports**: Area Increase (no effect: self-targeted), Empowered Cast.

### 1010 Fire Pila
- **Tags**: AreaOfEffect, Elemental, Magic. Visual: GroundEruption.
- **Targeting**: `TargetLocationTargeting`, enemies, radius 2 at the clicked point, cast range 5 (the point is clamped to the range).
- **Effects**: damage.
- **Scaling**: radius += CooldownReducPercent x 0.02.
- **Extra supports**: Area Increase, Burn Chance, Empowered Cast, **Spell Cascade**.

### 1014 Time Dilation (unique: `TimeDilationSkill`)
- **Targeting**: `TargetLocationTargeting`, enemies, radius 3, cast range 10.
- **Behaviour**: records each target's position and HP every 2 s for 10 s, then rewinds them to the recorded states over 1 s. Can resurrect targets that died during the window, shows a ghost trail, does not affect the caster. A target already being tracked is skipped.
- **Tags**: none. **Extra supports**: Empowered Cast.

### 1015 Force Nova
- **Tags**: AreaOfEffect, Movement, Origin, Magic. Visual: GroundEruption.
- **Targeting**: `TargetLocationTargeting`, enemies, radius 3, cast range 5.
- **Effects**: pull each target up to 5 units **toward the caster** (not the circle center) over 1 s.
- **Note**: no damage effect, so it triggers the Depression Chaos passive.
- **Extra supports**: Area Increase, Empowered Cast, Spell Cascade.

### 1016 Blink
- **Tags**: Movement, Self, Instant. Visual: GroundEruption.
- **Targeting**: `TargetLocationTargeting`, enemies, radius 1, cast range 20.
- **Effects**: teleport, once per cast, no lerp.
- **Behaviour**: if an enemy is within 1 unit of the (range-clamped) point, the caster teleports **onto that enemy**. Otherwise the caster moves **50 units** along the aim direction (the effect's Distance), ignoring the 20 range.
- **Extra supports**: Empowered Cast.

### 1020 Fire Bolt
- **Tags**: Ranged, Projectile, Elemental, Magic. Visual: Projectile.
- **Targeting**: `SelfTargeting` (only to run the effect once).
- **Effects**: spawn `ProjectileCount` (1) `MagicProjectile.prefab` toward the aim: speed 8, lifetime 5 s (40 units), destroyed on the first hostile hit, which takes the damage.
- **Example**: 122 damage per bolt from MATK 84 on a core-1 target ([Base_Formulas 4g](./Base_Formulas.md#4g-worked-example-fire-bolt)); cast time 0.318 s with DEX 30 / INT 50.
- **Extra supports**: Multi-Shot, Pierce, Chain, Projectile Speed, Split Projectile, Homing (only Multi-Shot and Split Projectile do something today), Burn Chance, Empowered Cast.

---

## Combo chain: Wind Slash -> Crescent Sweep -> Cherry Blossom Execution

Each step dashes the caster forward and leaves a 3 s stance buff. While the buff is active, the hotbar slot and the cast command swap the skill for the next one (`ChainOverrides`) and the buff is consumed.

| Step | ID | Damage | Shape | Caster dash | Buff left | Next |
|---|---|---|---|---|---|---|
| 1 | 3001 Wind Slash | 10 | cone 60, radius 2 | 2 units / 0.1 s | Iai_Stance_1, 3 s | 3002 |
| 2 | 3002 Crescent Sweep | 15 | cone 102, radius 4 | 3 units / 0.15 s | Iai_Stance_2, 3 s | 3003 |
| 3 | 3003 Cherry Blossom Execution | 30 | sphere, radius 2 | 6 units / 0.2 s | - | - |

All three: Physical, Neutral, 5 MP, cast delay 0.5 s; cooldown 1 / 1 / 15 s; tags Melee, Physical. Damage is applied before the dash. The JSON effect `Value` fields (100 / 120 / 250) are not used at runtime.

**Warning**: the assets store the combo key as `RequiredBuffName`, but the code field is `RequiredBuffCode`, so at runtime the key is empty and the swap probably never triggers. Verify in Play mode ([Known_Data_Issues](./Known_Data_Issues.md)).

---

## Prototype and data-only skills

| ID | Name | Class | What actually happens |
|---|---|---|---|
| 1011 | Perfect Parry | `ParrySkill` | Pays 15 MP, starts a 5 s cooldown, logs "Parry Window". No parry state, no counter. `ParryWindow` and `CounterDamageMult` are 0 in the asset. |
| 1012 | Chain Lightning | `ChainLightningSkill` | Single targeting with radius 0 finds no target. Even with a target the code only logs; it deals no damage. `MaxJumps`, `JumpRadius`, `DamageDecayPerJump` are 0 in the asset (class defaults 5 / 5 / 0.8 are overwritten). |
| 1013 | Blink Strike | `BlinkStrikeSkill` | Needs a ground target; teleports the caster to 1 unit before the point, then runs an empty effect list: **no damage** despite base 150. `StrikeDamageMult` 0 and unused. |
| 1017 | Chornos Field | `ActiveSkill` | No effects, no tags. Asset radius 20, runtime radius 2. Visual only. |
| 0 | Summon Valkyrie | `SummonSkill` | `MinionPrefab` is empty, so the cast throws an error after paying 50 MP. `Lifetime` and `MaxMinions` 0. Its id 0 is also the basic attack's SkillID. |
| 1019 | Mind Control | `MindControlSkill` (missing) | In `Skill.json` only (cost 30, VCT 1.5, FCT 0.5, cast delay 0.5, CD 12, SingleUnit range 8, SummonObject effect). No asset and no C# class: `GetSynchronizedSkill(1019)` returns null. Its tag `Control` is not a `SkillTag`. |

## Items that grant or cast skills

| Skill | Granted by (while equipped) | Auto-cast by |
|---|---|---|
| 1001 Tackle | - | Time Hourglass 3046 (10% when taking damage) |
| 1002 Healing | Ice Crown 3027 | - |
| 1004 Divine Blessing | Ice Orb 3028 | - |
| 1008 Aura of Flames | Fire Amulet 3026 | - |
| 1014 Time Dilation | Chrono Pendant 3034 | - |
| 1015 Force Nova | Time Staff 3042 | - |

Skill runes 6001-6017 carry skills 1001-1017. Details: [Items.md](./Items.md).
