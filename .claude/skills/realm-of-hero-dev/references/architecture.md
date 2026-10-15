# Architecture Map (verified against code 2026-10-15)

Paths relative to `Assets/Script/`.

## Namespaces
`CoreSystem` (enums), `CoreSystem.Attributes`, `.Data`, `.Data.Items`, `.Data.Items.Effects`, `.Data.Items.Model`, `.Data.Save`, `.Data.Skills`, `.Entity`, `.Entity.UniquePassives`, `.Movement`, `.Pathfinding`, `.Settings`, `.SkillTree(.Models)`, `.Skills`, `.Skills.Logic` (effects, targeting, buffs, projectile), `.Skills.Modifiers`, `.Skills.UniqueSkill`, `.Skills.Visuals`, `.Utile`, `.Visuals`, `CoreSystem.UI.*`, `UI.Components`.
Exceptions: `Agent/AgentCore.cs`, `AgentRegistry.cs`, `FaceDirectionModule.cs` use `CoreSystem.Controllers`; `SkillTree/PassiveSkillLoader.cs`, `UI/CursorManager.cs`, `UI/AttributePanel/*` have no namespace.

## Singletons and static access
- `.Instance`: `PlayerController`, `CameraController`, `GlobalGameTime`, `PathfinderGrid`, `SkillIndicatorManager`, `VisualManager`, `CombatJuiceDirector`, `SkillTreeManager`, `Database`, `EntityVisualSettings`, `UIManager`, `HotbarManager`, `GlobalDragManager`, `CursorManager`.
- Static: `GameData`, `SaveStorage`, `ItemDatabase`, `AttributeDatabase`, `FactionDatabase`, `SkillTreeDatabase`, `AgentRegistry`, `GameSetting`.

## Stat pipeline
- Each `Character` owns an `AttributeSystem` with one `Attribute` per `AttributeType`. Defaults: core stats 1, MaxHP 100, MaxMP 50, ASPD 150, MoveSpeed 100, ATK 10, StatusDMG 10.
- `Attribute.Recalculate()` -> `Combat/Formulas/StatFormula.Compute`:
  `((Base * (1 + StatPercent) + StatFlat) + InitialValue + SUM Flat) * (1 + SUM PercentAdd) * PROD PercentMult`, then `AdditionalChain.Apply()` (entries sorted by Order, op Add or Multiply, tagged).
- Derived formulas only in `Combat/Formulas/DerivedStatFormula.Compute` (called by `AttributeSystem.InnerUpdateDerivedAttributes()` after `UniquePassives.ModifyCoreAttributes`):

| Derived | Formula |
|---|---|
| ATK | STR + DEX/5 + LUK/3 |
| MATK | INT*1.5 + DEX/5 + LUK/3 |
| Flee | AGI + LUK/5 |
| Hit | DEX + LUK/3 |
| ASPD | + AGI*0.5 + DEX*0.15 |
| MaxHP / MaxMP | + VIT*1% / + INT*1% |
| HPRecovery | VIT/5 |
| MPRecovery | INT/6 (+ (INT-120)/2 at INT >= 120) |
| CriticalRate | LUK*0.3 |
| CritResistance | LUK/5 |
| PerfectDodge | LUK/10 |
| StatusDMG | LUK*0.5 + INT/4 |
| WeightLimit | + STR*30 |

- Current HP/MP = write `BaseValue` then `Recalculate()`.
- Every change raises `AttributeSystem.OnAttributesChanged` (guarded by `_isUpdating`).

## Combat formulas (all in `CoreSystem/Combat/Formulas/`; see `Docs/02-Technical/Combat/Combat_Core_Pipeline.md` for the file map)
- **VCT** = (base - flat) * (1 - sqrt(clamp01((DEX*2 + INT)/530))) * (1 - gear%) * (1 - skill%) * (1 - buff%). Can reach 0.
- **FCT** = (base - FixedCastTimeReduc) * (1 - FCT%). Conflicts with the AGENTS.md "immutable" rule.
- **Cooldown** = (base - flat) * (1 - %). **AOE** = base * (1 + AOE%).
- **Basic attack delay** = clamp((200 - ASPD)/50, 0.01, 1).
- **Damage order** (`Combat/Formulas/DamageFormula`: `Offense` then `Mitigate`, pure): base + ATK + WeaponATK (phys) / MATK + WeaponMATK (magic) -> element table -> (1 + element/size/race bonus - resist, each floored at 0) -> crit (CritRate - CritRes)% for x(1.5 + CritDamage) -> minus Defense / StatusDEF (min 1).
- **Combat pipeline** (`CombatCore.Execute(action, user, target)`): `Gate.CheckAction` (target exists, has stats) -> per effect: `EffectHandlers.For(kind)` + `Gate.CheckEffect(handler.TargetRule)` (Damage = Hostile; self-inflicted procs pass) -> handler `Offense` (`DamageFormula.BuildInput` + crit roll from `ICombatRandom`, only when the user has stats) -> `Defense` (`Mitigate`, min 1) -> `RiderStage` (OnHit/OnCrit/OnKill riders spawn proc sub-actions through `Resolve`) -> `OutcomeApplier` -> `DamageHandler.Commit`: passive OnHit/OnTakeDamage (ref) -> apply HP -> OnPostHit -> entity events -> `DamageTextManager`. Sub-results apply after the parent.

## Entities
- `Character` (abstract, `IEntity`) owns `AttributeSystem`, `MovementSystem`, `SkillCaster`, `CommandDirector`, `ExperienceSystem`, `UniquePassiveSystem`, `FaceDirectionModule`, 8 equipment slots, 4 costume slots. Events `OnHitTarget`, `OnTakeDamage`, `OnCostumeChanged`. Resolver `Func` hooks for auto-attack type/range/projectile address.
- `PlayerCharacter`: binds `GameData.Player` managers, `SaveData()/LoadData()`, creates `GameDevToolUI`.
- `EnemyCharacter`: minimal; training dummy (id 10 or name contains "Dummy") gets 9,999,999 HP.
- `NonPlayerCharacter`: requires `AgentCore`; `NPCInventory`, `NPCRoutine` (driven by `GlobalGameTime`).
- `AgentCore`: AI states Idle/Roam/Chase/Attack/Flee/Dead/CommandMove/CommandFollow; `SkillPool` of `SkillEquipEntry`; pathfinding via `PathfinderGrid` + `Pathfinder2D`.

## Skill flow
1. `PlayerController.HandleSkillInput` (Input System `InputSystem_Actions`) or `AgentCore` -> aim via `SkillIndicatorManager` -> `commandDirector.CommandCastSkill`.
2. `SkillCaster.BuildPayload`: base values -> AOE/Cooldown modifiers -> `OutputScaling.ApplyScaling` per scaling entry `(stat*AttrMult + Flat + LevelMult*lvl)*GlobalMult` into a `PayloadTarget` -> each linked `SupportModifier.ModifyPayload` -> `CalculateCastTimeModified`.
3. `InitiateCastSequence`: resource check (MP, or HP through passive), target clamp, cast indicator + VFX; 0 cast time executes immediately.
4. `Tick` -> `ExecuteSkill` (cooldown, cast delay) -> `ExecutePipeline` (pay cost, then `ExecuteWithSplitters`: the first linked `IPayloadSplitter` that `CanSplit` the skill turns one payload into several, otherwise `ActiveSkill.Execute` once; the first linked `ICastRepeater` schedules a delayed re-run through the same path).
5. `ActiveSkill.Execute`: `BaseTargeting.GetTargets` -> effects (to caster / once per cast / per target) -> `VisualManager.PlayVisual` -> `UniquePassives.TriggerOnSkillExecuted`. `ChainOverrides` mutate the skill when a buff code is present.
- Caster events: `OnCastInitiated`, `OnCastProgress`, `OnCastExecuted`, `OnCastInterrupted`, `OnGrantedSkillsChanged`.
- Buffs: `BuffSystem` (MonoBehaviour on character) with `ActiveBuff` (modifiers, stacks, tick, category), `HasBuff(code)`, `OnBuffsChanged`.
- Supports: max 5 per `SkillInstance`, auto-linked from `Database.GetLinkedSupportIDs`. Created by `SupportModifier.Create(id)` through `AttributedTypeRegistry` (`[SupportModifierId]`); unique passives likewise via `[UniquePassiveId]`.
- Visuals: `VisualManager`, `VisualPayload` SO, `ISkillVisual` (InstantHit, Projectile, RayBeam, AuraBuff, GroundEruption).
- Skill tree: `SkillTreeLoader`/`SkillTreeDatabase` (static, `SkillTree.json`), `SkillTreeManager` (points, static events `OnNodeUnlocked/OnNodeReset/OnAllReset`), UI `PassiveSkillLoader`.

## Data and items
- `Database` (auto-created before scene load from `Resources/Prefab/Database`, DontDestroyOnLoad). Init order: Items -> Factions -> Attributes -> Skills -> SkillTree -> Characters -> UniquePassives.
- `Item` subclasses: Equipment, Consumable, Costume, Currency, Fragment, Resource, SkillRune, SupportRune, Additional. Runtime: `ItemInstance`, `EquipmentInstance` (Refine, Reforge, sockets), `CostumeInstance`.
- `Equipment.OnEquip`: weapon/armor baseline x (1 + refine) + reforge, then `Effects`, then socketed fragment effects. Set bonuses in `EquipmentSetData`.
- `GameData.Player` (`PlayerData`): `Inventory`, `PlayerStats`, `PlayerEquipment`, `PlayerCostumes`, `PlayerSkills`, `Data` (PlayerDataModel).

## UI
- `UIManager`: registry-driven windows/modals, modal stack + overlay, Esc closes top modal, registry hotkeys, `GetOrCreateGlobalModal`.
- `UIBaseView` lifecycle: `OnLoad` once, `OnOpen` every open, `OnClose`; built-in drag with canvas clamp.
- `UIGlobalModal`: name-keyed reusable modal from `_globalModalTemplate`; chainable `SetTitle`, `SetWindowSize`, `SetWindowPosition`, `SetWindowAnchors`, `AutoFitWindow`, `SetResizable`, `SetMinSize/SetMaxSize`, `SetCloseButtonVisible`, `OverrideCloseAction`; events `OnClosed`, `OnResized`.
- `UIBuilderContext` elements: `Div`, `Vertical`, `Horizontal`, `Grid`, `ScrollView`, `HorizontalScrollView`, `ScrollRect`, `Text`, `Button`, `Image`, `RawImage`, `Toggle`, `InputField`, `ProgressBar`, `Divider`, `Custom` (-> `UICustomElement.Get<T>(key)`); others return chainable `UIElementRef`.
- Older `WindowToggleController` (KeyCode -> GameObject) still exists alongside registry hotkeys.

## Editor menus
- Window > Realm of Hero > Game Data Workbench (all JSON data, skill simulator, validation, regenerates `AttributeType`).
- Window > Realm of Hero > Scene Switcher; Window > Agent Monitor.
- Realm of Hero > UI > Generate Base UIElement Prefabs; Realm of Hero > UI > Create Global Modal Template.
- GameObject > UI > Realm UI > Create UI Window/Modal Template; Tools > Realm of Hero > UI > Create UI Manager Hierarchy Structure.
- GameObject > UI > Custom Components (Switch Toggle, Animated Button, Draggable Zoom Panel, UI Line Renderer).
- Tools > Experience Editor; Tools > GameObject Structure Debugger; Tools > Projectiles > Generate Sample Projectiles.
