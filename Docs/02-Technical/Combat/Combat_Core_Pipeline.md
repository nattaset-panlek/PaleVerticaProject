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
| `CombatAction.cs` | `CombatAction` (payload + effects + riders + `IsProc`), `EffectSpec` base |
| `ActionResult.cs` | `ActionResult` (outcomes, sub-results, `Total`, `HasHit`, `HasCritical`, `PredictsKill`), `Outcome` |
| `ResolveContext.cs` | Data passed through the layers; `Spawn` for riders |
| `CombatEnums.cs` | `EffectKind`, `TargetRule`, `BlockReason`, `RiderTrigger` |
| `CombatRandom.cs` | `ICombatRandom`, `UnityCombatRandom` (inject a fixed random in tests) |
| `Gate/Gate.cs` | Layer 0 |
| `Handlers/EffectHandler.cs` | `IEffectHandler`, `EffectHandler<TSpec, TOutgoing>` template (Offense, Defense, Commit), `[EffectHandler(kind)]` |
| `Handlers/EffectHandlers.cs` | Registry, one cached handler per kind |
| `Handlers/Damage/` | `DamageSpec`, `DamageHandler` |
| `Formulas/` | Pure math: `DamageFormula` (`BuildInput`, `Offense`, `Mitigate`, `Calculate`), `CombatStatLookup` |
| `Riders/` | `ActionRider`, `RiderStage` |
| `Apply/OutcomeApplier.cs` | Layer 5 |

## Where to change what

| I want to change... | Edit |
|---|---|
| A damage number or order of operations | `Formulas/DamageFormula.cs` (+ `DamageFormulaTests`) |
| Who can be hit (factions, immunity) | `Gate/Gate.cs` or the handler's `TargetRule` |
| What happens when damage lands (HP, events, damage text) | `Handlers/Damage/DamageHandler.cs` `Commit` |
| An on-hit special effect (lifesteal, charm, convert on kill) | New `ActionRider` in `Riders/` |
| A new kind of effect | New spec + handler in `Handlers/<Kind>/` (see the project skill recipe) |

## Phase status

| Phase | Scope | Changes numbers | Status |
|---|---|---|---|
| 1 | Foundation: core types, Gate, Dispatch, `DamageHandler`, Riders stage, `OutcomeApplier`; all damage routed through `CombatCore` | No | Done |
| 2 | Move remaining formulas into `Formulas/` (stats, derived stats, cast time, cooldown, AOE, scaling, cost, regen, ASPD, EXP, refine) | No | Planned |
| 3 | Handlers for Heal, Status, Displace, Summon wrapping existing effects | No | Planned |
| 4 | Offense/Defense modifiers + concrete riders; migrate unique passives off `ref` hooks and `ProcSkillIds` | No | Planned |
| 5 | Wire the 11 support modifiers that currently do nothing | Yes | Needs approval |
| 6 | New mechanics: Hit/Flee/Dodge, heal scaling, Control (charm), convert-on-kill, FCT rule | Yes | Needs approval |
| 7 | Damage breakdown in the K dev tool | No | Planned |

## Phase 1 notes
- Unique passive `ref` hooks (`TriggerOnHit`, `TriggerOnTakeDamage`) still run in `DamageHandler.Commit`, after mitigation, exactly where they ran before. Moving them into the Offense/Defense layers is Phase 4.
- Matchup multipliers (element/size/race bonus vs resist, crit vs crit resist) stay in Offense because the current formula combines both sides in one factor. Splitting them would change numbers.
- Behaviour fixes that came with the Gate: a missing target or a null payload is now a blocked result instead of a `NullReferenceException`.
- Riders read the outcome amount before passive `ref` hooks adjust it. This only matters once riders and those passives coexist (Phase 4 resolves it).
