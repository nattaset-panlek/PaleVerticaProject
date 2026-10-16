# Unique Passives Catalog

Verified against `Assets/Resources/Data/UniquePassive.json` and `Assets/Script/CoreSystem/Entity/UniquePassives/`: **2026-10-17**.

A unique passive is a C# class registered with `[UniquePassiveId("Id")]`. `UniquePassive.json` only holds the display name and description; every number below comes from the class. A character can hold each passive once (`UniquePassiveSystem`). Passives are added by equipment (`UniquePassiveEffect`; today only item 3048 Vampiric Soul-Reaver -> Vampire), by the **K** dev tool, and restored from save data.

Design ideas and status for future passives: [UniquePassiveIdeas.md](../../01-Design/UniquePassiveIdeas.md).

## When each hook runs

| Hook | Moment | Passives |
|---|---|---|
| `ModifyCoreAttributes` | while derived stats are calculated (only the derived stats see the change) | Curse of Kanix, Eldritch Faith |
| `ModifyManaCost` | when a skill's cost is resolved | Blood Mage |
| `TriggerOnHit` (attacker) | after defense, before HP changes; may change the damage | Blood Mage, Eldritch Faith |
| `TriggerOnTakeDamage` (target) | right after the attacker's OnHit; may change the damage | Blessing of Magearna, Thorn |
| `TriggerOnPostHit` (attacker) | after HP is reduced | Vampire, Mana Leech, Execution, Blood Bleeding |
| `TriggerOnBasicAttack` | on every basic attack strike (also the Double Attack strike) | Time Collector |
| `TriggerOnSkillExecuted` | after a skill's effects ran (also repeats) | Depression Chaos |
| `Tick` | every frame; damage once per second | Bleeding, Blood Bleeding DOT |
| repeater interfaces | after a basic attack / a skill cast | Double Attack, Double Spell |
| flag check | `Character.ApplyImpulse` | Rock |

Several passives on the same hook run in the order they were added. Proc damage (Thorn, Depression Chaos, bleeds) is a new hit through the full damage formula, so the **proc owner's ATK or MATK and weapon power are added** to the amount below and the target's defense is subtracted ([Base_Formulas 4f](./Base_Formulas.md#4f-proc-damage)).

## Catalog

| Id | Name | Exact behaviour (code) | JSON description | Notes |
|---|---|---|---|---|
| `Vampire` | Vampire | After the owner deals **Physical** damage > 0, heal **30%** of the final damage (capped at MaxHP). | Leeches 30% physical damage as HP. | Includes physical basic attacks and physical procs (Thorn). Granted by item 3048. |
| `TimeCollector` | Time Collector | On each basic attack strike, every equipped and granted skill that is cooling down loses **0.1 s**. | Reduces cooldowns by 0.1s on basic attack. | Fires when the strike starts, so a ranged shot that misses still counts. Double Attack makes it 0.2 s per attack. |
| `DepressionChaos` | Depression Chaos | When the owner executes a skill tagged **AreaOfEffect** that has **no damage effect** and found targets, each target takes a Magic proc of **INT x 2** (id -998). | Deals 200% INT magic damage on casting a non-damaging AOE skill. | Plus the owner's MATK + WeaponMATK. Today: Force Nova (enemies), and Divine Blessing / Lightning Blessing, whose target list contains the caster, so the **caster hits themselves** (self procs pass the hostility check). |
| `Thorn` | Thorn | When the owner takes damage > 0 from another attacker (not from a proc), reflect a Physical proc of **damage x 0.1 x STR x 0.01** (id -999). | Reflects 10% of damage taken scaling with STR back to the attacker. | 10% at STR 100, 3% at STR 30. Plus the owner's ATK + WeaponATK, so even tiny hits reflect about ATK. Reduced by the attacker's Defense. |
| `ManaLeech` | Mana Leech | After each hit by the owner, **30%** chance to restore **5 x (1 + MPRecoveryPercent / 100)** MP (capped). | 30% chance to leech mana on hit. | Flat amount, not based on damage. Procs and basic attacks count. |
| `Execution` | Execution | After a hit by the owner on another target, if the target's HP is above 0 and **<= 5% of MaxHP**, set its HP to 0 (shows "999999"). | Instantly execute targets whose HP <= 5%. | Writes HP directly, outside the combat pipeline. |
| `BloodBleeding` | Blood Bleeding | After a hit by the owner on another character, **20%** chance to put **Blood Bleeding DOT** on the target for **5 s** (re-applying refreshes the 5 s). | 20% chance to apply a Bleeding DOT dealing 1% Max HP + 1% (INT + DEX) damage. | See the DOT row. |
| `BloodBleedingDOT` | (internal) | Every 1 s for 5 s: Status proc of **1% of the target's MaxHP + 1% of (owner INT + owner DEX)** (INT/DEX captured when applied). | - | Not registered (cannot be added by id). The DOT's own "owner" is the bleeding target, so the target's MATK + WeaponMATK are added and its StatusDEF subtracted. 5 ticks per application. |
| `Bleeding` | Bleeding | The **owner** takes a Status proc of **1% of their own MaxHP** every 1 s, forever. | Ticking DOT dealing 1% Max HP per second. | A curse on whoever has it. Plus the owner's own MATK + WeaponMATK, minus own StatusDEF. |
| `CurseOfKanix` | Curse of Kanix | Derived stats are calculated as if **INT <-> STR, DEX <-> VIT, LUK <-> AGI** were swapped. | Swaps core stats: INT <-> STR, DEX <-> VIT, LUK <-> AGI. | The STR..LUK attributes keep their values, so cast time, basic attack bonus damage, skill scalings and supports still use the real stats. |
| `BlessingOfMagearna` | Blessing of Magearna | When the owner takes damage > 0, **40%** of it is taken from MP instead (up to current MP); HP takes the rest. | 40% damage absorbed by Mana. | Runs after the attacker's OnHit changes. |
| `BloodMage` | Blood Mage | Skill costs are paid in **HP** (HP cost = mana cost, can cast while HP >= cost, HP never drops below 1). Hits from skills (SkillID > 0) deal **x1.3** after defense. | Use HP instead of Mana, skills deal 30% more final damage. | Basic attacks (id 0) and procs (negative ids) do not get x1.3. Toggles pay HP every pulse. |
| `EldritchFaith` | Eldritch Faith | Derived stats use **INT x 0.5**. Every hit by the owner gets **+ INT** (the full current INT) after defense. | INT -50%, adds flat damage equal to lost INT. | The bonus is 2x the "lost INT", applies to basic attacks and procs too, and the INT attribute itself is not halved (cast time uses full INT). |
| `Rock` | Rock | The owner ignores `ApplyImpulse`. | Immune to pushes and impulses. | Also blocks the owner's own lerped dashes (Shadow Dash, Wind Slash, Crescent Sweep, Cherry Blossom Execution). Instant moves (Blink) and stuns still work. |
| `DoubleAttack` | Double Attack | Every basic attack strikes again after **0.12 s** at **x1.0**, if attacker and target are still alive. | Every basic attack strikes twice. Basic attacks only. | The second strike counts as a basic attack (Time Collector, item auto-casts on hit) but does not reset the attack timer. |
| `DoubleSpell` | Double Spell | A skill whose instance is at **level 1** is cast again after **0.2 s**, free (no cost, no cooldown), whole hit **x0.4**, from the caster's position at that moment. | Level 1 skills cast a second time for free; the second cast deals 60% less damage. Stacks with Echo Spell. | The copy is a full cast: a linked Echo Spell echoes it (100 / 60 / 40 / 24%) and Spell Cascade splits it. Every skill is level 1 today, and non-damage skills repeat too (Blink teleports twice, Healing heals twice, toggles pulse twice). |

## Code-vs-description differences (code wins)

1. **Eldritch Faith** adds the full INT, not the lost half, and only derived stats see the halving.
2. **Thorn, Depression Chaos, Bleeding, Blood Bleeding** add the proc owner's ATK/MATK and weapon power on top of the stated amount.
3. **Depression Chaos** damages the caster when the skill's targets include the caster (Divine Blessing, Lightning Blessing).
4. **Rock** also stops the owner's own dashes.
5. **Mana Leech** restores a flat 5 MP (scaled by MPRecoveryPercent), not a share of the damage.
6. **Curse of Kanix** swaps only the inputs of the derived-stat table.

All of these are also listed in [Known_Data_Issues.md](./Known_Data_Issues.md).
