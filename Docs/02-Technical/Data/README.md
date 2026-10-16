# Game Data Reference

Verified against code and data: **2026-10-17**. Catalog of what is in the game right now: every skill, support, unique passive and item with its numbers, plus the base formulas that turn those numbers into gameplay. Built from the real files, not from design docs. When code and docs disagree, the code wins; disagreements are listed in [Known_Data_Issues.md](./Known_Data_Issues.md).

## Pages

| Page | Contents | Built from |
|---|---|---|
| [Base_Formulas.md](./Base_Formulas.md) | Standalone explanation of every base formula: attribute pipeline, derived stats, damage, element table, basic attack and ASPD, cast time, cooldown, area, scaling, costs, regen, repeats, refine, EXP | `CoreSystem/Combat/Formulas/`, `AttributeSystem`, `SkillCaster`, `Character` |
| [Active_Skills.md](./Active_Skills.md) | All 23 active skills: numbers, targeting, effects, scalings, tags, combos, compatible supports, status | `Skill.json`, `Resources/Skill/ActiveSkill/**/*.asset`, `Skills/UniqueSkill/` |
| [Support_Skills.md](./Support_Skills.md) | All 22 support skills: chroma, link rule, numbers, what the code does, compatibility matrix | `SupportSkill.json`, `Skills/Modifiers/` |
| [Unique_Passives.md](./Unique_Passives.md) | All 15 unique passives (+ the internal Blood Bleeding DOT): hook, exact numbers, notes | `UniquePassive.json`, `Entity/UniquePassives/` |
| [Items.md](./Items.md) | 199 items: equipment stats and effects, sets, fragments, runes, costumes, other types | `Item.json`, `ItemSet.json`, `Data/Items/` |
| [Known_Data_Issues.md](./Known_Data_Issues.md) | Every mismatch found between code, data and docs | all of the above |

Related system docs: [Combat Core Pipeline](../Combat/Combat_Core_Pipeline.md), [All Attribute Formulas](../Attributes/All_Attribute_Formulas.md) (full 154-attribute list), [Effect Templates](../Items/EffectTemplates.md), [Unique Skills Guide](../Skills/UNIQUE_SKILLS_GUIDE.md).

## Where the numbers come from

All data files are in `Assets/Resources/Data/` and are edited through **Window > Realm of Hero > Game Data Workbench**.

| File | Holds |
|---|---|
| `Attribute.json` | 154 attribute ids, names, `IsPercent` flag (ids match the `AttributeType` enum) |
| `Skill.json` | numbers for every active skill (`ActiveSkills`; `SupportSkills` is empty) |
| `SupportSkill.json` | name and description template per support id (numbers are in the C# class) |
| `UniquePassive.json` | name and description per unique passive id (numbers are in the C# class) |
| `Item.json`, `ItemSet.json` | items, their effects, set bonuses |

### How an active skill is assembled at runtime
`Database.GetSynchronizedSkill(id)` clones the `ActiveSkill` asset found under `Assets/Resources/Skill/ActiveSkill/` (subfolders included) and overwrites it with `Skill.json`:

| Comes from `Skill.json` | Comes from the asset only |
|---|---|
| `SkillName`, `SkillElement`, `DamageType` | targeting class and its settings (cast range, cone angle, box size, line width, alignment) |
| `BaseDamage`, `BaseManaCost`, `BaseVCT`, `BaseFCT`, `BaseCastDelay`, `BaseCooldown`, `BaseProjectileCount`, `IsToggle` | the effect list and every effect setting (damage, heal amount unless overridden, buffs, movement, projectile prefab) |
| **AOE radius = `BaseTargeting.Radius`** (the top-level `BaseAOERadius` is ignored) | `ChainOverrides` (skill combos) |
| `Scalings` (only when the JSON list is not empty) | unique-skill fields (`MaxJumps`, `ParryWindow`, ...) |
| `Tags` (the asset's tags are replaced) | visual prefabs, `IsContinuousArea` |
| `Heal` > 0 -> written into every `HealEffect.BaseHealAmount` | the C# class (`CustomClassName` is only used by the Workbench converter) |
| `IconPath`, `MaxSupportSlots` | |

The `Effects` and `BaseTargeting` blocks inside `Skill.json` (including effect `Value` fields such as 250 for Cherry Blossom Execution) are **not read at runtime**; they exist for the Workbench. A skill in `Skill.json` without an asset (1019 Mind Control) cannot be cast.

Supports are created from their C# class (`[SupportModifierId(id)]`); `SupportSkill.json` only provides the name, icon and the description template filled with the class's numbers. Unique passives work the same way with `[UniquePassiveId("Name")]`.

## Units used in these pages

| Value | Unit |
|---|---|
| Time (VCT, FCT, cast delay, cooldown, durations) | seconds |
| Distance (radius, cast range, dash distance, offsets) | world units |
| CriticalRate, CritResistance | percentage points (3 = 3% chance) |
| AOE, CooldownReducPercent, `...Percent` cast-time attributes | percent (10 = 10%) |
| Element / size / race damage and resist | fraction in the formula (0.2 = +20%), see Known_Data_Issues |
| ASPD, MoveSpeed | percent of base speed (100 = normal) |
| Skill level | `SkillInstance.CurrentLevel`, 1 by default |
