# Support Skills Catalog

Verified against `Assets/Resources/Data/SupportSkill.json` and `Assets/Script/CoreSystem/Skills/Modifiers/`: **2026-10-17**.

A support is linked into a skill's support slots (up to 5). Its numbers live in the C# class (`[SupportModifierId(id)]`); `SupportSkill.json` only gives the name and a description template that the class fills in. Supports modify the skill payload in slot order while it is built ([Base_Formulas 6a](./Base_Formulas.md#6a-payload-build-order-skillcasterbuildpayload)). Support runes 7001-7022 are the inventory items for supports 1001-1022.

**11 of the 22 supports currently do nothing** (empty `ModifyPayload`, no other code reads them). This is tracked as "Phase 5, needs approval" in the [Combat Core Pipeline](../Combat/Combat_Core_Pipeline.md#phase-status).

## Catalog

| ID | Name | Chroma / Mineral | Can link to | Numbers (code) | What it does | Status |
|---|---|---|---|---|---|---|
| 1001 | Multi-Shot | Physical / Hematite | Projectile tag | `extraProjectiles` 1 | ProjectileCount +1 | Working |
| 1002 | Echo | Magic / Fluorite | any | `damageMultiplier` 0.8, `cooldownMultiplier` 1.25 | BaseDamage = BaseDamage x 0.8 + Cooldown x 1.25 | Working |
| 1003 | Pierce | Physical / Obsidian | Projectile tag | - | nothing | No-op |
| 1004 | Chain | Physical / Obsidian | Projectile tag | - | nothing | No-op |
| 1005 | Area Increase | Cosmic / Pallasite | AreaOfEffect tag | `aoePercentBonus` 4, `vctPercentBonus` 2 | AOE radius x1.04, VCT x1.02 | Working |
| 1006 | Damage Up | Origin / Zircon | any | `damageMultiplier` 1.2 | BaseDamage x1.2 | Working |
| 1007 | Cooldown Reduction | Dimensional / Alexandrite | any | `cdrPercent` 10 | Cooldown x0.9 | Working |
| 1008 | Cost Reduction | Dimensional / Ulexite | any | `costMultiplier` 0.5 | ManaCost x0.5 (shown as "-50%") | Working |
| 1009 | Duration Up | Cosmic / Tektite | any | `durationPercent` 20 | nothing | No-op |
| 1010 | Status Chance Up | Soul / Celestite | any | `statusChancePercent` 15 | nothing | No-op |
| 1011 | Burn Chance | Origin / Amber | Fire element | `burnChancePercent` 20 | nothing | No-op |
| 1012 | Freeze Chance | Origin / Amber | Water element | `freezeChancePercent` 20 | nothing | No-op |
| 1013 | Knockback | Physical / Hematite | any | `knockbackDistance` 2 | nothing | No-op |
| 1014 | Critical Power | Soul / Moldavite | any | `critDamageMult` 1.5, `lukScalingMult` 0.05 | BaseDamage x (1.5 + LUK x 0.05), on every hit | Working |
| 1015 | Mana Leech | Soul / Celestite | any | `leechPercent` 10 | nothing | No-op |
| 1016 | Life Leech | Soul / Moldavite | Physical damage type | `leechPercent` 10 | nothing | No-op |
| 1017 | Projectile Speed | Cosmic / Tektite | Projectile tag | `speedPercent` 30 | nothing | No-op |
| 1018 | Split Projectile | Cosmic / Pallasite | Projectile tag | `extraProjectiles` 2 | ProjectileCount +2 | Working |
| 1019 | Homing | Dimensional / Alexandrite | Projectile tag | - | nothing | No-op |
| 1020 | Empowered Cast | Magic / Labradorite | Magic damage type | `baseEmpowerDamage` 50, `intScalingMult` 1 | BaseDamage + 50 + INT | Working |
| 1021 | Echo Spell | Magic / Fluorite | any | `repeatDelay` 0.15, `aoeMultiplier` 1.3, `damageMultiplier` 0.6 | recasts 0.15 s later: AOE x1.3, whole hit x0.6, free | Working |
| 1022 | Spell Cascade | Cosmic / Pallasite | ground-target skill with AreaOfEffect tag | `mainDamageMultiplier` 0.75, `sideDamageMultiplier` 0.5, `offsetMargin` 5 | 3 circles: main BaseDamage x0.75, left/right x0.5, 5 units to the sides | Working (quirk) |

## How the working supports interact

- **Slot order matters.** Supports run top to bottom. Empowered Cast (+50 + INT) before Damage Up (x1.2) gives (base + 50 + INT) x 1.2; the other way round gives base x 1.2 + 50 + INT.
- **BaseDamage supports do not scale ATK/MATK.** Damage Up, Critical Power, Echo and Spell Cascade change `BaseDamage`, which is added to the caster's power later. On a low-base skill they barely matter; on Lightning Strike (300 at level 1) they matter a lot.
- **Critical Power** is not tied to critical hits: it always multiplies. At LUK 10 it is x2.0, at LUK 50 x4.0.
- **Echo (1002)** uses the payload cooldown at its slot (after stat CDR and any earlier Cooldown Reduction). On 0-cooldown skills it is a flat 20% loss.
- **Area Increase** raises the variable cast time before the DEX/INT reduction, so the +2% becomes smaller in practice.
- **Echo Spell** is a whole-hit x0.6 (through `DamageMultiplier`), so it scales ATK/MATK too. The echo goes through Spell Cascade again, does not pay a cost and does not touch the cooldown. With the Double Spell unique passive: 4 hits at 100 / 60 / 40 / 24%.
- **Spell Cascade** only splits skills that use `TargetLocationTargeting`. Each circle gets its own cast indicator. Because it scales `BaseDamage` only, Fire Pila with MATK 84 deals 95% (main) and 90% (sides) of a normal hit, not 75% / 50%. Whether to change this is still open.
- Only the **first** linked repeater (Echo Spell) and the first linked splitter (Spell Cascade) are used.

## Compatibility matrix

The 10 supports with no condition (1002, 1006, 1007, 1008, 1009, 1010, 1013, 1014, 1015, 1021) link to every skill. The conditional ones, checked against the runtime tags, element, damage type and targeting:

| Skill | Projectile group (1001, 1003, 1004, 1017, 1018, 1019) | 1005 Area Increase | 1011 Burn | 1012 Freeze | 1016 Life Leech | 1020 Empowered Cast | 1022 Spell Cascade |
|---|---|---|---|---|---|---|---|
| 1001 Tackle | | | | | Y | | |
| 1002 Healing | | | | | | Y | |
| 1003 Hellfire Blast | | Y | Y | | Y | | |
| 1004 Divine Blessing | | Y | | | | Y | |
| 1005 Earthshatter | | | | | Y | | |
| 1006 Lightning Strike | | Y | | | Y | | |
| 1007 Shadow Dash | | | | | Y | | |
| 1008 Aura of Flames | | Y | Y | | | Y | |
| 1009 Lightning Blessing | | Y | | | | Y | |
| 1010 Fire Pila | | Y | Y | | | Y | Y |
| 1011 Perfect Parry | | | | | Y | | |
| 1012 Chain Lightning | | | | | | Y | |
| 1013 Blink Strike | | | | | Y | | |
| 1014 Time Dilation | | | | | | Y | |
| 1015 Force Nova | | Y | | | | Y | Y |
| 1016 Blink | | | | | | Y | |
| 1017 Chornos Field | | | | | Y | | |
| 1020 Fire Bolt | Y | | Y | | | Y | |
| 3001 Wind Slash | | | | | Y | | |
| 3002 Crescent Sweep | | | | | Y | | |
| 3003 Cherry Blossom Execution | | | | | Y | | |
| 0 Summon Valkyrie | | | | | Y | | |

Notes:
- No skill has the Water element, so Freeze Chance cannot be linked anywhere.
- Fire Bolt is the only Projectile skill; of the projectile group only Multi-Shot and Split Projectile do something (Fire Bolt with both fires 4 bolts spread 45 degrees apart).
- Lightning Strike is tagged Magic but its damage type is Physical, so it takes Life Leech, not Empowered Cast.
- Time Dilation and Blink Strike are ground-target skills without the AreaOfEffect tag, so Spell Cascade cannot link to them.

## Description templates (from `SupportSkill.json`)

`{0}` and `{1}` are filled by the class's `GetDescription` with the numbers above.

| ID | Template | Filled with |
|---|---|---|
| 1001 | Adds {0} projectile(s). | 1 |
| 1002 | Converts damage to {0}% and adds {1}x cooldown as extra damage. | 80, 1.25 |
| 1005 | Increases area of effect radius by {0}% but increases cast time by {1}%. | 4, 2 |
| 1006 | Increases base damage by {0}%. | 20 |
| 1007 | Reduces cooldown by {0}%. | 10 |
| 1008 | Reduces mana cost by {0}%. | 50 |
| 1014 | Multiplies base damage by {0}% plus {1}% of character LUK attribute value. | 50, 5 |
| 1018 | Fires {0} additional projectiles. | 2 |
| 1020 | Adds {0} flat damage, plus {1}% of character INT attribute value. | 50, 100 |
| 1021 | Repeats the skill 0.15s after casting. Repeated cast has {0}% increased area but {1}% less damage. | 30, 40 |
| 1022 | Creates 3 cascading AOE circles. Left/right circles deal {0}% damage, main circle deals {1}% damage. | 50, 75 |

The no-op supports keep descriptions that promise an effect (for example Pierce: "Allows projectiles to pierce targets."). Critical Power's text reads as "+50%", but the code multiplies by 1.5 plus 5% per LUK point.
