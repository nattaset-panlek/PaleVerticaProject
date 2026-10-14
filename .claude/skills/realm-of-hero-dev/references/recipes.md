# Recipes

Paths are relative to `Assets/Script/`. Every new script uses the five `#region` blocks and no comments.

## New skill effect
1. Create `CoreSystem/Skills/Logic/Effects/<Name>Effect.cs`, namespace `CoreSystem.Skills.Logic`, `[System.Serializable] public class <Name>Effect : EffectLogic`.
2. Implement `public override void Apply(SkillPayload payload, IEntity target)`. Override `ApplyToCaster` / `ApplyOncePerCast` if needed.
3. It appears automatically in the `ActiveSkill.Effects` list dropdown (`[SerializeReference, SubclassSelector]`).
4. Damage/heal math belongs in `DamageEffect` / `HealEffect`; do not duplicate it. Route hit hooks through the target's `UniquePassiveManager` like `DamageEffect` does.

## New targeting shape
1. `CoreSystem/Skills/Logic/Targeting/<Name>Targeting.cs : TargetingLogic`.
2. Implement `List<IEntity> GetTargets(SkillPayload payload)`; filter with the base `TargetAlignment` helpers (faction checks via `FactionDatabase`).
3. Override `RequiresSelection` and `DrawGizmos` as needed. If it needs an aim preview, add an indicator in `Skills/Visuals/Indicators/` and route it through `SkillIndicatorManager` from `PlayerController`.

## New support modifier
1. Add a class to `CoreSystem/Skills/Modifiers/SupportModifierSubclasses.cs` deriving `SupportModifier`; implement `ModifyPayload(ref SkillPayload payload)`, optionally `IsCompatible(ActiveSkill)`.
2. Register its id in the switch in `CoreSystem/Skills/SupportModifier.cs` `Create(int id)` (current range 1001-1022; check `Docs/03-Project/Planning_Guide.md` for id ranges).
3. Add the data row in `SupportSkill.json` via Game Data Workbench. Chroma/Mineral semantics: `Docs/01-Design/SupportChromaSystem.md`.

## New unique passive
1. Add a subclass of `UniquePassive` (`CoreSystem/Entity/UniquePassives/UniquePassive.cs`). Override only the hooks needed: `TriggerOnHit`, `TriggerOnTakeDamage` (by `ref damage`), `TriggerOnPostHit`, `TriggerOnBasicAttack`, `TriggerOnSkillExecuted`, `ModifyCoreAttributes`, `ModifyManaCost`, `Tick`.
2. Add a `case "<Id>"` in `UniquePassiveManager.CreatePassive(string)`.
3. Add metadata to `UniquePassive.json` (Workbench "UniquePassives" tab). Grant through an item with `UniquePassiveEffect`.
4. Ideas backlog: `Docs/01-Design/UniquePassiveIdeas.md`.

## New unique (scripted) skill
Subclass `ActiveSkill` in `CoreSystem/Skills/UniqueSkill/`, override `Execute`, add `[CreateAssetMenu(menuName = "Skill System/Unique/<Name>")]`. Start from `CoreSystem/Skills/UniqueSkillTemplate.cs`. Guide: `Docs/02-Technical/Skills/UNIQUE_SKILLS_GUIDE.md`.

## New equipment effect
1. `CoreSystem/Data/Items/Effects/<Name>Effect.cs : EquipmentEffect`; implement `Apply(Character, ItemInstance)` and `Remove(...)` symmetrically (every modifier added in Apply must be removed in Remove, usually by tag/source).
2. Add a `case` in `ItemDatabase.PopulateEffects` mapping `EffectEntry.EffectType` string to the class; add any new JSON field to `EffectEntry` (`CoreSystem/Data/SkillDatabaseModels.cs`).
3. Document the JSON shape in `Docs/02-Technical/Items/EffectTemplates.md`.

## New attribute / stat
1. Add it in Game Data Workbench (Attributes tab). It regenerates `CoreSystem/Enums/AttributeType.cs` (X and XPercent pairs, XPercent id = X + 1). Never hand-edit the enum.
2. If it is derived from core stats, that is a formula change: get user approval, then edit `AttributeSystem.InnerUpdateDerivedAttributes` and `Docs/02-Technical/Attributes/All_Attribute_Formulas.md`.
3. Temporary/conditional changes: use `AddModifier` (Flat / PercentAdd / PercentMult) or `AddAdditional(type, order, op, value, tag)` + `RemoveAdditionalByTag`. Order: Base -> Flat -> Percent -> AdditionalChain.

## New prefab UI window or modal
1. Prefab root gets a subclass of `UIWindow` or `UIModal` (`CoreSystem/UI/Manager/`). Template via **GameObject > UI > Realm UI > Create UI Window/Modal Template**.
2. Override `OnLoad(object data)` (once) and `OnOpen(object data)` (each open), `OnClose()`.
3. Subscribe to data events in `OnEnable`, unsubscribe in `OnDisable`. Example: `GameData.Player.Inventory.OnInventoryChanged`.
4. Add a `UIWindowEntry` (WindowName, Type, Prefab, DestroyOnClose, hotkey Keys) to the `UIWindowRegistrySO` asset.
5. Open with `UIManager.Instance.Open("<WindowName>", data)` / `Toggle` / `Close`.

## Code-built modal (UIBuilder)
```csharp
_modal = UIBuilder.BuildGlobalModal("My Panel", BuildUI, new Vector2(600, 400));
_modal.OnClosed += () => _modal = null;

private void BuildUI(UIBuilderContext ctx)
{
    ctx.Vertical("Div", col =>
    {
        col.Text("Text", "Title").SetFontSize(32);
        col.Button("Primary Button", "Apply", OnApply).SetPreferredSize(200, -1);
    }).SetFitParent(stretchWidth: true, stretchHeight: false);
}
```
- First arg is a style name looked up in `Assets/Prefab/UIElementRegistry.asset`; unknown names fall back to code defaults.
- State change -> call `_modal.Build(BuildUI)` again.
- Full API: `Docs/02-Technical/UI/UIBuilder_Guide.md`. Largest real example: `CoreSystem/UI/DevTools/GameDevToolUI.cs`.

## Hotbar action / drop target
- New action kind: extend `HotbarActionType` and `HotbarSlotUI.ExecuteAction()`; hotkeys stay in `HotbarManager`.
- New drop target: implement `IDropHandler`, read `GlobalDragManager.Payload`, type-check it, then act. Start drags with `GlobalDragManager.Instance.StartDrag(...)`.
- Docs: `Docs/02-Technical/UI/Hotbar_Interaction.md`, `Docs/02-Technical/UI/GlobalDrag.md`.

## New persistent player data
Add fields to `CoreSystem/Data/Save/PlayerDataModel.cs`, write them in `PlayerCharacter.SaveData()`, read them in `LoadData()`. Keep defaults safe for old save files (missing JSON fields).

## External control of a character
Use `character.commandDirector` (override/lock move and cast) instead of disabling `PlayerController` or `AgentCore`. Manual: `Docs/02-Technical/Character/Command_Director_Manual.md`.

## Verifying a change
No automated tests. Let Unity compile (Hot Reload is installed), open `Assets/Scenes/SandBox.unity`, enter Play, press **K** for the dev tool (spawn items, edit attributes, grant skills). `CoreSystem/Data/Tests/DataSystemTest.cs` has a "TestSystem" context-menu smoke test.
