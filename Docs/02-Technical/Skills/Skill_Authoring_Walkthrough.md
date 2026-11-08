# Skill Authoring Walkthrough

How to add a new Active skill, a new Support skill and a new Summon skill.
Status: all sections written (2026-11-08).

## How a cast flows
1. `SkillCaster.BuildPayload` clones the numbers from the skill into a `SkillPayload`, applies `OutputScaling`, then runs `ModifyPayload` on every linked Support.
2. Cast time runs out, then `ExecutePipeline`: pay cost, `IPayloadSplitter` (Spell Cascade), `ActiveSkill.Execute`, then `ICastRepeater` (Echo Spell).
3. `ActiveSkill.Execute`: `BaseTargeting.GetTargets`, then each `EffectLogic.Apply`, then `VisualManager.PlayVisual`.
4. `DamageEffect.Apply` calls `CombatCore.Execute`, then `SupportHitApplier.Apply` for on-hit Support effects.

## Add a new Support skill

### Where
- Code: `Assets/Script/CoreSystem/Skills/Modifiers/<Name>SupportModifier.cs`
- Data: `Assets/Resources/Data/SupportSkill.json` (edit through **Window > Pale Vertica > Pale Vertica GameDataWorkbench**)
- Test: `Assets/Script/Editor/Tests/SupportModifierPayloadTests.cs` and `RegistryTests.cs`

### Inputs
- A free id in the support range (now 1001-1022; see `Docs/03-Project/Planning_Guide.md`).
- The number(s) the Support changes, and which skills it may link to.

### Steps
1. Create the class: derive `SupportModifier`, add `[SupportModifierId(<id>)]`, set `Chroma` and `Mineral` in the constructor. Never set `ID`.
2. Put tunable numbers in `#region [Variables/Fields]` so the tooltip (`GetDescription`) and gameplay read the same value.
3. Write `ModifyPayload(ref SkillPayload payload)`. Pick the payload field:

| Goal | Field | Read by |
|---|---|---|
| More/less damage | `DamageMultiplier` (multiply it) | `DamageFormula` (keeps ATK/MATK) |
| Skill base damage only | `BaseDamage` | `DamageFormula` |
| Area, cost, cooldown, cast time | `AOERadius`, `ManaCost`, `Cooldown`, `VCT`/`FCT` | targeting, `SkillCaster` |
| Projectiles | `ProjectileCount`, `ProjectileSpeedMultiplier`, `PierceCount`, `ChainCount`/`ChainRange`, `IsHoming` | `SpawnProjectileEffect`, `ProjectileBehaviour` |
| On hit | `KnockbackDistance`, `LifeLeechPercent`, `ManaLeechPercent`, `BurnChance`, `FreezeChance`, `StatusChanceBonus` | `SupportHitApplier` |
| Durations | `DurationMultiplier` | buffs, stuns, continuous areas, burn/freeze |

4. If no field fits, add one to `SkillPayload`, copy it in `Clone()`, and read it where the effect happens. Put the math in `Combat/Formulas/SupportHitFormula.cs` (pure numbers, tested).
5. If it changes how the skill runs (not numbers), also implement `IPayloadSplitter` (one cast becomes several) or `ICastRepeater` (delayed repeat). `SkillCaster` finds them; never add id checks there.
6. Override `IsCompatible(ActiveSkill)` to limit which skills it links to (tag, element, targeting).
7. Add the id and type to `ExpectedSupportTypes` in `RegistryTests.cs`, and a test in `SupportModifierPayloadTests.cs`.
8. Add the JSON row (name, description with `{0}` placeholders matching `GetDescription`, icon).

### Expected result
- The Support shows in the Skill window Modifier page. Dropping it on a compatible skill changes the cast: the Basic page values change, and the effect shows in play.
- Tests pass: run the EditMode tests (Test Runner, or the batch command in `.claude/skills/pale-vertica-dev/SKILL.md`).

### Rules for the on-hit Supports
- They fire only when a `DamageEffect` hit lands (blocked or 0-damage hits do nothing). Proc damage (burn, bleed, thorn) does not trigger them again.
- Burn: 10% of the hit per 0.5 s for 3 s, as Status damage with no attacker stats (`ProcSkillIds.BurnTick`). The tick is credited to whoever set the burn (`CombatAction.CreditTo`), so hit/kill events and future kill EXP go to them. Freeze: 1.5 s stun with a "Frozen" buff icon. Both durations scale with Duration Up.
- Status Chance Up adds percentage points to Burn and Freeze chance (20% + 15 = 35%), and only when the skill already has that chance.
- Projectile order after a hit: chain to the nearest unhit enemy within `ChainRange` if chains are left, else pierce if pierces are left, else the projectile is destroyed.

### Common mistakes
- Scaling `BaseDamage` for "less/more damage": that skips ATK/MATK. Use `DamageMultiplier`.
- Adding a payload field but not copying it in `Clone()`: Echo Spell and Spell Cascade copies lose it.
- Freeze Chance links to every element; Burn Chance still links only to Fire skills.
- Writing HP directly in a Support: leech goes through `SupportHitApplier`; damage goes through `CombatCore`.

## Add a new Active skill
### Where
- Behaviour: an `ActiveSkill` asset in `Assets/Resources/Skill/ActiveSkill/` (scripted skills in `Unique/`). Targeting, effects and prefabs are read from the asset.
- Numbers: `Assets/Resources/Data/Skill.json` (Workbench, Skills tab). At runtime `Database.GetSynchronizedSkill` overwrites cost, cooldown, cast times, damage, tags and radius from JSON. **A JSON row with no asset does not exist in the game.**

### Steps
1. Create the asset (**Create > Skill System > Active Skill**), named `<Name>_<Id>`, with a free id.
2. Pick `BaseTargeting` and add `Effects` (`DamageEffect`, `HealEffect`, `ApplyTemporaryBuffEffect`, `MovementEffectLogic`, `SpawnProjectileEffect`).
3. For a lingering area, tick `IsContinuousArea` and set `AreaDuration` / `AreaTickRate` (see Chornos Field 1017).
4. Add the JSON row with the same `SkillID`, including `BaseTargeting.Radius` (it becomes the AOE radius; 0 means a radius search finds nothing) and `Tags` (Supports use them for compatibility).
5. If effects cannot express it, subclass `ActiveSkill` in `UniqueSkill/` and override `Execute`. Deal damage through `DamageEffect` on a `payload.Clone()`, and scale with `DamageMultiplier`, so repeats and on-hit Supports keep working (see Chain Lightning, Blink Strike).

### Expected result
Grant it with the dev tool (**K**), cast it: cost and cooldown apply, and targets take the effect.

### Common mistakes
- Changing the shared `payload` inside `Execute`: Echo Spell clones it afterwards and the change compounds. Clone first.
- `TeleportToTarget` goes to the clicked point when one is set; `Distance` is only the fallback.

## Add a new Summon skill

### Where
- Skill class: `CoreSystem/Skills/UniqueSkill/SummonSkill.cs` (menu **Create > Skill System > Unique > Summon Skill**)
- Runtime: `Skills/Summon/SummonFactory.cs` (the only spawn entry), `SummonedUnit.cs` (on each summon), `SummonRegistry.cs` (cap and lookup), `Entity/TeamRegistry.cs` (summon and owner are one team)
- Reference asset: `Assets/Resources/Skill/ActiveSkill/Unique/Summon_Valkyrie_1018.asset`
- Tests: `Assets/Script/Editor/Tests/SummonTests.cs`

### Inputs
- A body prefab with a `Character` component (and collider, animator). Today the placeholder is `Assets/Addressable/Prefab/Entity/Ally/Ally.prefab`.
- Lifetime, cap, leash radius.

### Steps
1. Create the asset with the menu above inside `Assets/Resources/Skill/ActiveSkill/Unique/`, named `<Name>_<Id>`.
2. Set `Id`, `BaseTargeting` = Self, cost, cooldown, cast time.
3. Summon fields:
   - `MinionPrefab`: the body. **This is the one field to change when the real art is ready.**
   - `IsPermanent` / `Lifetime` (seconds; Duration Up support scales it)
   - `MaxMinions`: cap per owner for this skill; casting at the cap removes the oldest one first
   - `SummonsPerCast`, `SpawnDistance`, `LeashRadius` (how far it strays from the owner)
4. Add the row to `Skill.json` with the same `SkillID` and `CustomClassName` `CoreSystem.Skills.UniqueSkill.SummonSkill` (Workbench).
5. Grant it (dev tool **K**, skill tree or item) and cast.

### What happens on cast
- `SummonFactory.Create` instantiates the prefab under `[Runtime]` as `[Runtime] Summon <prefab>`, gives it the owner's faction, sets its `AgentProfile` to Aggressive with the owner as `HomeAnchor`, and adds `SummonedUnit`, which registers itself.
- `AgentCore` drives it like any agent: it stays near the owner, chases hostiles in `DetectionRadius`, uses its basic attack and skill pool, and drops a target that pulls it past `LoseSightRadius` from the owner.
- `TeamRegistry` makes owner and summon allies even in faction 1 (Unaffiliated), where faction rules alone would make them enemies.
- Kills by a summon give EXP to the owner (`Kill_Exp_Walkthrough.md`). The summon disappears at the end of its lifetime, when replaced at the cap, or when the owner is killed.

### Expected result
Cast Summon Valkyrie: an Ally body appears 1.5 units in front, walks with you, attacks nearby monsters, and vanishes after 20 s. A third cast removes the oldest of two.

### Common mistakes
- A prefab without `Character` is refused (warning in the Console).
- Changing the prefab's own `AgentProfile` does not matter for behaviour; `SummonFactory` overrides Behavior, home zone and roaming.
- Never `Instantiate` a summon yourself; always go through `SummonFactory.Create` so the cap, team and EXP credit work.
- The body prefab's `CharacterId` decides what EXP it gives when it dies; keep it 0 for summons.
