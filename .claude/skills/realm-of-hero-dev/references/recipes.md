# Recipes

Paths are relative to `Assets/Script/`. Every new script uses the five `#region` blocks and no comments.

## New skill effect
1. Create `CoreSystem/Skills/Logic/Effects/<Name>Effect.cs`, namespace `CoreSystem.Skills.Logic`, `[System.Serializable] public class <Name>Effect : EffectLogic`.
2. Implement `public override void Apply(SkillPayload payload, IEntity target)`. Override `ApplyToCaster` / `ApplyOncePerCast` if needed.
3. It appears automatically in the `ActiveSkill.Effects` list dropdown (`[SerializeReference, SubclassSelector]`).
4. Never rename or move an existing effect class: `[SerializeReference]` stores the class name + namespace in skill assets.
5. Effects are adapters: build a `CombatAction` and call `CombatCore.Execute(action, user, target)` (see `DamageEffect`). Never compute damage/heal inside an effect. Add a case to `Assets/Script/Editor/Tests/CombatCoreTests.cs` or `DamageFormulaTests.cs`.

## New effect kind (Heal, Status, Control, Summon ...)
1. Add a spec: `Combat/Handlers/<Kind>/<Kind>Spec.cs : EffectSpec` with its parameters and `Kind => EffectKind.<Kind>` (add the enum value in `Combat/CombatTypes.cs` if new).
2. Add a handler: `[EffectHandler(EffectKind.<Kind>)] public sealed class <Kind>Handler : EffectHandler<<Kind>Spec, TOutgoing>`. Implement `TargetRule` (Any/Hostile/Ally), `Offense` (user-side value), `Defense` (target-side mitigation, returns an `Outcome`), `Commit` (the only code that mutates state for this kind). Offense/Defense must not change game state.
3. Put the math in `Combat/Formulas/<Kind>Formula.cs` (pure, numbers in/out) and test it.
4. Expose it to skills through an `EffectLogic` adapter so it shows up in `ActiveSkill.Effects`.

## New rider (special on-hit behaviour)
1. `public class <Name>Rider : ActionRider` in `Combat/Riders/`, mark `[System.Serializable]`. Set `Trigger` (OnHit/OnCrit/OnKill) and `ChancePercent`.
2. In `Execute(ResolveContext context)` read `context.Result` (outcomes, `Total(kind)`) and call `context.Spawn(subAction, user, target)`; the sub-action goes through Gate/Defense like any action, is marked `IsProc`, and stops at `CombatCore.MaxDepth`.
3. Attach with `action.WithRider(...)`. Never mutate HP directly in a rider.

## New targeting shape
1. `CoreSystem/Skills/Logic/Targeting/<Name>Targeting.cs : TargetingLogic`.
2. Implement `List<IEntity> GetTargets(SkillPayload payload)`; filter with the base `TargetAlignment` helpers (faction checks via `FactionDatabase`).
3. Override `RequiresSelection` and `DrawGizmos` as needed. If it needs an aim preview, add an indicator in `Skills/Visuals/Indicators/` and route it through `SkillIndicatorManager` from `PlayerCore`.

## New support modifier
1. Create `CoreSystem/Skills/Modifiers/<Name>SupportModifier.cs` (one class per file) deriving `SupportModifier`, tagged `[SupportModifierId(<id>)]` (current range 1001-1022; check `Docs/03-Project/Planning_Guide.md`). Set `Chroma`/`Mineral` in the constructor; do not set `ID` (the factory does).
2. Implement `ModifyPayload(ref SkillPayload payload)`, optionally `IsCompatible(ActiveSkill)` and `GetDescription(template)`. Keep tunable numbers as fields so tooltip and gameplay share them.
3. If it changes how the skill executes rather than the payload numbers, also implement `IPayloadSplitter` (one cast becomes several payloads) or `ICastRepeater` (delayed re-cast). `SkillCaster` picks them up automatically.
4. Add the id + type to `ExpectedSupportTypes` in `Assets/Script/Editor/Tests/RegistryTests.cs`.
5. Add the data row in `SupportSkill.json` via Game Data Workbench. Chroma/Mineral semantics: `Docs/01-Design/SupportChromaSystem.md`.

## New unique passive
1. Create `CoreSystem/Entity/UniquePassives/<Name>UniquePassive.cs` (one class per file) deriving `UniquePassive`, tagged `[UniquePassiveId("<Name>")]`. That attribute is the registration; there is no switch.
2. Override only the hooks needed: `TriggerOnHit`, `TriggerOnTakeDamage` (by `ref damage`), `TriggerOnPostHit`, `TriggerOnBasicAttack`, `TriggerOnSkillExecuted`, `ModifyCoreAttributes`, `ModifyManaCost`, `Tick`.
3. Use the base helpers: `DealProcDamage(target, amount, damageType, ProcSkillIds.X)` for passive-triggered damage and `RestoreResource(AttributeType.HP, AttributeType.MaxHP, amount)` for heals/leech. Add a new constant to `ProcSkillIds` if the proc must be distinguishable (e.g. to avoid Thorn reflecting it).
4. Internal-only passives spawned by another passive (like `BloodBleedingDOT`) get no attribute.
5. Add the id + type to `ExpectedPassiveTypes` in `RegistryTests.cs`, metadata to `UniquePassive.json` (Workbench "UniquePassives" tab), and grant it through an item with `UniquePassiveEffect`.
6. Ideas backlog: `Docs/01-Design/UniquePassiveIdeas.md`.

## New unique (scripted) skill
Subclass `ActiveSkill` in `CoreSystem/Skills/UniqueSkill/`, override `Execute`, add `[CreateAssetMenu(menuName = "Skill System/Unique/<Name>")]`. Start from `CoreSystem/Skills/UniqueSkillTemplate.cs`. Guide: `Docs/02-Technical/Skills/UNIQUE_SKILLS_GUIDE.md`.

## New equipment effect
1. `CoreSystem/Data/Items/Effects/<Name>Effect.cs : EquipmentEffect`; implement `Apply(Character, ItemInstance)` and `Remove(...)` symmetrically (every modifier added in Apply must be removed in Remove, usually by tag/source).
2. Add a `case` in `ItemDatabase.PopulateEffects` mapping `EffectEntry.EffectType` string to the class; add any new JSON field to `EffectEntry` (`CoreSystem/Data/SkillDatabaseModels.cs`).
3. Document the JSON shape in `Docs/02-Technical/Items/EffectTemplates.md`.

## New attribute / stat
1. Add it in Game Data Workbench (Attributes tab). It regenerates `CoreSystem/Enums/AttributeType.cs` (X and XPercent pairs, XPercent id = X + 1). Never hand-edit the enum.
2. If it is derived from core stats, that is a formula change: get user approval, add the field to `DerivedStats` and the math to `Combat/Formulas/DerivedStatFormula.Compute`, assign it in `AttributeSystem.InnerUpdateDerivedAttributes`, and update `FormulaTests` and `Docs/02-Technical/Attributes/All_Attribute_Formulas.md`.
3. Temporary/conditional changes: use `AddModifier` (Flat / PercentAdd / PercentMult) or `AddAdditional(type, order, op, value, tag)` + `RemoveAdditionalByTag`. Order: Base -> Flat -> Percent -> AdditionalChain.

## New prefab UI window or modal
1. Prefab root gets a subclass of `UIWindow` or `UIModal` (`CoreSystem/UI/Manager/`). Template via **GameObject > UI > Realm UI > Create UI Window/Modal Template**.
2. Override `OnLoad(object data)` (once) and `OnOpen(object data)` (each open), `OnClose()`.
3. Subscribe to data events in `OnEnable`, unsubscribe in `OnDisable`. Example: `GameData.Player.Inventory.OnInventoryChanged`.
4. Add a `UIWindowEntry` (WindowName, Type, Prefab, DestroyOnClose, hotkey Keys) to the `UIWindowRegistrySO` asset.
5. Open with `UIManager.Instance.Open("<WindowName>", data)` / `Toggle` / `Close`.

## Code-built page, menu or dialog (UIBuilder)
```csharp
_modal = UIBuilder.BuildGlobalModal("My Panel", BuildUI, new Vector2(700, 480));
_modal.OnClosed += () => _modal = null;

private void BuildUI(UIBuilderContext ui)
{
    ui.Pages("panel", pages => pages
        .WithHeader(h => h.Row(r => r.Input(_query, q => { _query = q; _modal.RefreshPage("panel"); }).Fill()))
        .Add("Items", BuildItems)
        .Add("Stats", BuildStats), UIPageNav.SideMenu);
}

private void BuildStats(UIBuilderContext ui)
{
    ui.Panel(p =>
    {
        p.KeyValue("Level", _level.ToString());
        p.Row(r => { r.Spacer(); r.Button("Apply", OnApply, UIVariant.Primary); });
    });
}
```
- Layout: `Column`/`Row`/`Grid`/`Panel`; size with `Width`/`Fill`; colours with `UIVariant` (theme on the `UIElementRegistry` asset). Do not hard-code colours or add `ContentSizeFitter`s.
- After an action refresh the smallest part: `_modal.RefreshPage(id)`, `_modal.Refresh(regionId)`, or update one element via `.Id("x")` + `_modal.Find("x").Text(...)`.
- Blocking dialog: `.SetDismissable(false).FitContent(true, maxSize).Center()` (see `PlayerDeathScreen`).
- Full API and layout rules: `Docs/02-Technical/UI/UIBuilder_Guide.md`. Largest real example: `CoreSystem/UI/DevTools/GameDevToolUI.cs`.

## Hotbar action / drop target
- New action kind: extend `HotbarActionType` and `HotbarSlotView.ExecuteAction()`; hotkeys stay in `HotbarManager`.
- New drop target: implement `IDropHandler`, read `GlobalDragManager.Payload`, type-check it, then act. Start drags with `GlobalDragManager.Instance.StartDrag(...)`.
- Docs: `Docs/02-Technical/UI/Hotbar_Interaction.md`, `Docs/02-Technical/UI/GlobalDrag.md`.

## New persistent player data
Add fields to `CoreSystem/Data/Save/PlayerDataModel.cs`, write them in `PlayerSaveStorage.Capture`, read them in `PlayerSaveStorage.Restore`. Keep defaults safe for old save files (missing JSON fields).

## External control of a character
Use `character.Control`. Permanent change of owner: `PlayerCore.Instance.Possess(character)` or `AgentCore.Ensure().Register(character)`. Temporary control: `var lease = character.Control.Claim(holder, ControlLane.Possession, seconds, keepWhile, onEnded)`; the previous owner gets control back automatically when the lease ends. Bosses tagged `ImmuneToControl` refuse Possession; `ControlLane.Dev` always wins. Never disable `PlayerCore`/`AgentCore` or call `movementSystem`/`skillCaster`/`BasicAttack` directly. Guide: `Docs/02-Technical/Character/Character_System.md`.

## New character (monster, villager, NPC)
1. Duplicate an entity prefab (or use the shared body prefab once it exists); keep only the `Character` component (plus visual components; buffs live inside `Character.Buffs`). Never add controller components.
2. Set identity: `CharacterId`, `Faction`, `Tags` (Monster/Villager/Boss/Merchant/ImmuneToControl), `DefaultController = Agent`.
3. Configure behaviour in the **Agent Profile** section (behaviour, combat style, ranges, home zone, group, skill pool).
4. Spawn with `CharacterFactory.Spawn(characterId, position, parent, onSpawned)` or an `EntitySpawner`; set profile overrides inside `onSpawned` (before `Start`).

## Verifying a change
1. Run the EditMode tests (`Assets/Script/Editor/Tests/`): Test Runner window, or headless with the editor closed (command in `SKILL.md`). All must pass.
2. Then open `Assets/Scenes/SandBox.unity`, enter Play, press **K** for the dev tool (spawn items, edit attributes, grant skills) to check feel and UI.
3. `CoreSystem/Data/Tests/DataSystemTest.cs` has a "TestSystem" context-menu smoke test for the data layer.
