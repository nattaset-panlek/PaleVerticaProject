# Automated Tests (EditMode)

Core math is covered by NUnit EditMode tests so balance and refactor work can be checked without entering Play mode.

## Location
`Assets/Script/Editor/Tests/` compiles into `Assembly-CSharp-Editor` (no asmdef needed) under namespace `CoreSystem.Tests`.

| File | Covers |
|---|---|
| `AttributeMathTests.cs` | `Attribute.Recalculate` order (Base -> Flat -> PercentAdd -> PercentMult -> AdditionalChain), `AdditionalChain` ordering, derived stats from STR/AGI/VIT/INT/DEX/LUK |
| `SkillTimingTests.cs` | Variable Cast Time curve and reaching 0, Fixed Cast Time unaffected by DEX/INT, cooldown, AOE |
| `FormulaTests.cs` | Every file in `Combat/Formulas/`: stat value, derived stats, cast time split, cooldown/area/scaling, resource cost and regen, basic attack, refine/reforge/EXP |
| `DamageFormulaTests.cs` | Damage formula order, crit, resist floors, minimum damage, proc ids |
| `CombatCoreTests.cs` | `CombatCore` pipeline: Resolve is pure, Execute applies, Gate rules, dispatch, riders re-entering Resolve, depth guard, kill prediction |
| `FakeCombatant.cs` | Test `IEntity` + `FixedCombatRandom` for scene-free pipeline tests |
| `RegistryTests.cs` | Every support modifier id and unique passive id maps to the expected class |
| `SupportExecutionHookTests.cs` | SpellCascade split geometry/damage, EchoSpell repeat payload |
| `TestStats.cs` | Helper that builds an initialized `AttributeSystem` with chosen core stats |

## Running
- In the editor: **Window > General > Test Runner > EditMode > Run All**.
- Headless (Unity must be closed):
  ```
  "D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -nographics -projectPath "F:/GameProject/Pale Vertica" -runTests -testPlatform EditMode -testResults results.xml -logFile unity.log
  ```

## Rules
- A formula change (requires approval per `AGENTS.md`) must update the matching test in the same change.
- New support modifiers / unique passives must be added to `RegistryTests`.
- Keep tests scene-free: build data with `TestStats`, `FakeCombatant` and plain payloads; pass a `FixedCombatRandom` to `CombatCore.Resolve` (or an explicit crit roll to `DamageFormula.Calculate`) instead of relying on `Random`.
