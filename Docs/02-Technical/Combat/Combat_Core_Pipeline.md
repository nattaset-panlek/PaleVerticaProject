# Combat Core Pipeline

Every combat action (skill, basic attack, item, passive proc, DoT) is resolved by one entry point:

```csharp
ActionResult result = CombatCore.Resolve(action, user, target);
OutcomeApplier.Apply(result);
CombatCore.Execute(action, user, target);
```

`Resolve` only calculates. `OutcomeApplier` is the only stage that changes HP, MP, buffs or position. `Execute` does both.

Code: `Assets/Script/CoreSystem/Combat/` (namespace `CoreSystem.Combat`).

## Layers

```
Resolve(action, user, target)
  0. Gate       CheckAction: target exists and has stats
                CheckEffect: handler.TargetRule (Hostile / Ally / Any); self-inflicted procs pass
  1. Dispatch   each EffectSpec -> EffectHandlers.For(spec.Kind)
  2. Offense    handler.Offense: action formula + user attributes          (user side)
  3. Defense    handler.Defense: mitigation, resist, absorb -> Outcome     (target side)
  4. Riders     OnHit / OnCrit / OnKill riders spawn sub-actions that re-enter Resolve
                (marked IsProc, limited to CombatCore.MaxDepth = 2)
  -> ActionResult (Outcomes + SubResults)
Apply
  5. OutcomeApplier -> handler.Commit per outcome, then sub-results
```

## Files

| Path | Role |
|---|---|
| `CombatCore.cs` | `Resolve`, `Execute`, `MaxDepth` |
| `CombatAction.cs` | `CombatAction` (payload + effects + riders + `IsProc`) |
| `EffectSpec.cs` | Base class of every effect spec |
| `ActionResult.cs` | `ActionResult` (outcomes, sub-results, `Total`, `HasHit`, `HasCritical`, `PredictsKill`) |
| `Outcome.cs` | One resolved effect result |
| `ResolveContext.cs` | Data passed through the layers; `Spawn` for riders |
| `CombatTypes.cs` | `EffectKind`, `TargetRule`, `BlockReason`, `RiderTrigger` |
| `ICombatRandom.cs`, `UnityCombatRandom.cs` | Random source (inject a fixed random in tests) |
| `Gate/Gate.cs` | Layer 0 |
| `Handlers/IEffectHandler.cs`, `EffectHandler.cs`, `EffectHandlerAttribute.cs` | Handler contract, `EffectHandler<TSpec, TOutgoing>` template (Offense, Defense, Commit), `[EffectHandler(kind)]` |
| `Handlers/EffectHandlers.cs` | Registry, one cached handler per kind |
| `Handlers/Damage/` | `DamageSpec`, `DamageHandler` |
| `Formulas/` | Pure math, numbers in and out (table below); input/output structs in `FormulaTypes.cs` |
| `Riders/` | `ActionRider`, `RiderStage` |
| `Apply/OutcomeApplier.cs` | Layer 5 |

## Formulas (`Combat/Formulas/`)

Every number the game derives lives here. Callers only gather inputs and apply results.

| File | Formulas | Called from |
|---|---|---|
| `StatFormula.cs` | Attribute value: Base x (1+StatPercent) + StatFlat + Initial + Flat -> x(1+PercentAdd) -> x PercentMult -> AdditionalChain | `Attribute.Recalculate` |
| `DerivedStatFormula.cs` | STR/AGI/VIT/INT/DEX/LUK -> ATK, MATK, Flee, Hit, StatusDEF, ASPD, MaxHP%, MaxMP%, regen, crit, etc. | `AttributeSystem.InnerUpdateDerivedAttributes` |
| `DamageFormula.cs` | Damage Offense (power, element, size, race, crit) and Mitigate (defense, min 1) | `DamageHandler` |
| `CombatStatLookup.cs` | Which attribute holds the element/size/race bonus or resist | `DamageFormula` |
| `CastTimeFormula.cs` | Variable cast time (DEX/INT curve, gear/skill/buff %) and Fixed cast time | `SkillInstance.CalculateCastTimeModified` |
| `SkillValueFormula.cs` | Cooldown, area/range scaling, skill output scaling | `SkillInstance`, `OutputScaling.ApplyScaling` |
| `ResourceFormula.cs` | Can-afford check, HP/MP after cost, HP/MP regen tick | `SkillCaster` (`ResolveCost`/`CanAfford`/`PayCost`), `Character.HandleRecovery` |
| `BasicAttackFormula.cs` | Basic attack bonus damage (STR/DEX/INT), attack delay from ASPD | `Character.BasicAttack` |
| `ProgressionFormula.cs` | Refine table and multiplier, reforge bonus, equipment baseline and tooltip bonus, fallback EXP curves | `Equipment`, `EquipmentDatabase` (facade), `ItemInformationUI`, `ExperienceSystem` |

## Where to change what

| I want to change... | Edit |
|---|---|
| A damage number or order of operations | `Formulas/DamageFormula.cs` (+ `DamageFormulaTests`) |
| Any other stat, timing, cost, regen, refine or EXP number | The matching file in `Formulas/` (+ `FormulaTests`) |
| Who can be hit (factions, immunity) | `Gate/Gate.cs` or the handler's `TargetRule` |
| What happens when damage lands (HP, events, damage text) | `Handlers/Damage/DamageHandler.cs` `Commit` |
| An on-hit special effect (lifesteal, charm, convert on kill) | New `ActionRider` in `Riders/` |
| A new kind of effect | New spec + handler in `Handlers/<Kind>/` (see the project skill recipe) |

## Phase status

| Phase | Scope | Changes numbers | Status |
|---|---|---|---|
| 1 | Foundation: core types, Gate, Dispatch, `DamageHandler`, Riders stage, `OutcomeApplier`; all damage routed through `CombatCore` | No | Done |
| 2 | Move remaining formulas into `Formulas/` (stats, derived stats, cast time, cooldown, AOE, scaling, cost, regen, ASPD, EXP, refine) | No | Done |
| 3 | Handlers for Heal, Status, Displace, Summon wrapping existing effects | No | Planned |
| 4 | Offense/Defense modifiers + concrete riders; migrate unique passives off `ref` hooks and `ProcSkillIds` | No | Planned |
| 5 | Wire the 11 support modifiers that currently do nothing | Yes | Needs approval |
| 6 | New mechanics: Hit/Flee/Dodge, heal scaling, Control (charm), convert-on-kill, FCT rule | Yes | Needs approval |
| 7 | Damage breakdown in the K dev tool | No | Planned |

## Phase 2 notes
- Equivalence was verified with a temporary test that ran frozen copies of the old inline code against the new formulas on 5,000 seeded random inputs per formula, requiring exact float equality. All matched.
- `SkillCaster` had four copies of the mana/HP cost logic; they are now `ResolveCost`, `CanAfford` and `PayCost` built on `ResourceFormula`.
- `EquipmentDatabase` remains as a facade (`RefineTable`, `ReforgeValuePerLevel`, `GetRefineMultiplier`) so existing callers keep working.

## Phase 1 notes
- Unique passive `ref` hooks (`TriggerOnHit`, `TriggerOnTakeDamage`) still run in `DamageHandler.Commit`, after mitigation, exactly where they ran before. Moving them into the Offense/Defense layers is Phase 4.
- Matchup multipliers (element/size/race bonus vs resist, crit vs crit resist) stay in Offense because the current formula combines both sides in one factor. Splitting them would change numbers.
- Behaviour fixes that came with the Gate: a missing target or a null payload is now a blocked result instead of a `NullReferenceException`.
- Riders read the outcome amount before passive `ref` hooks adjust it. This only matters once riders and those passives coexist (Phase 4 resolves it).
