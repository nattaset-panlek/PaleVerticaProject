# Character System: One Body, Separate Controllers

Status: **implemented 2026-10-16** (phases C1–C7 of `Character_Architecture_Plan_TH.md`).
Supersedes `99-Archive/Command_Director_Manual.md` and `99-Archive/NPC_and_AgentCore_Walkthrough.md`.

## 1. Overview

```
 PlayerCore ([Runtime] PlayerCore)          AgentCore ([Runtime] AgentCore)
  input, camera, save/load, death screen      one instance, one AgentHandle per AI body
            │ claims / commands                          │ claims / commands
            ▼                                            ▼
     ControlSystem (per body): lanes Dev 100 > Possession 50 > Scripted 30 > Owner 10
            │ only the active lease holder may command
            ▼
 Character (one class for every body) ── CharacterPresenter (animation, flip, death, world HP bar)
            ▲ register on enable / unregister on disable
     CharacterRegistry (RuntimeId → Character)
```

- Every character in the world is the same `Character` component. There are no Player/Enemy/NPC subclasses.
- A character never reads input and never runs AI. It only executes commands that pass its `ControlSystem`.
- `PlayerCore` and `AgentCore` are created at runtime under the `[Runtime]` root (see `Coding_Standards.md` section 2) and are not placed in scenes or prefabs.

## 2. Pieces

| Type | Path | Role |
|---|---|---|
| `Character` | `CoreSystem/Entity/Character.cs` | The body: stats, movement, skills, equipment, passives, basic attack, regen. Identity: `CharacterId` (data id), `RuntimeId` (this instance), `Faction`, `Tags`, `DefaultController`. Holds `Presenter`, `AgentProfile`, `Control` |
| `CharacterTypes` | `CoreSystem/Entity/CharacterTypes.cs` | `CharacterControllerType` (None/Player/Agent), `ControlLane`, `CharacterTag` flags (Villager, Monster, Boss, Merchant, TrainingDummy, ImmuneToControl) |
| `ControlSystem` / `ControlLease` | `CoreSystem/Entity/` | Core control logic. `Claim(holder, lane, duration, keepWhile, onEnded)`, `Release`, `TransferOwnership`, `CanCommand`, `Tick`. Commands: `CommandMove`, `CommandCastSkill`, `CommandBasicAttack`, `CommandInterruptCast`. Legacy `RequestOverride`/`ReleaseOverride` map to the Possession lane |
| `CharacterPresenter` | `CoreSystem/Entity/CharacterPresenter.cs` | Serializable module inside `Character`: animator params, flip, model rotation lock, death/attack triggers, world status bar. Auto-finds the `Animator` and the `Model` child when not assigned |
| `CharacterRegistry` | `CoreSystem/Entity/CharacterRegistry.cs` | Static `RuntimeId → Character` map with `OnRegistered`/`OnUnregistered` |
| `CharacterFactory` | `CoreSystem/Entity/CharacterFactory.cs` | `Spawn(characterId, position, parent, onSpawned)` (Addressables), `AssignController` (called from `Character.Start`), `ReleaseController` (called from `OnDestroy`) |
| `PlayerCore` | `CoreSystem/Player/PlayerCore.cs` | Player input, aiming, auto-attack, GameData binding, death screen. `Possess(character)`, `Release()`, events `OnPossessed`, `OnReleased`, `OnTick`, static `OnReady`. **The only way to get the player's character**: static `GetPlayer()`, `TryGetPlayer(out)`, `IsPlayer(character)`, static event `OnPlayerChanged(Character or null)` |
| `PlayerSaveStorage` | `CoreSystem/Player/PlayerSaveStorage.cs` | `Capture(character)` / `Restore(character, data)` for `PlayerDataModel` (format unchanged) |
| `PlayerDeathScreen` | `CoreSystem/Player/PlayerDeathScreen.cs` | Death modal used by `PlayerCore` |
| `AgentCore` | `CoreSystem/Agent/AgentCore.cs` | Single AI manager. `Register(character)`, `Unregister`, `GetHandle`, `CommandMoveTo`, `CommandFollow`, `ClearCommand`, `Revive`. Ticks every handle each frame |
| `AgentHandle` | `CoreSystem/Agent/AgentHandle.cs` | Per-body AI state and logic (state machine, perception, pathfinding, skill choice, damage reactions, group alerts). Pauses when it does not hold control |
| `AgentProfile` / `AgentSkillEntry` | `CoreSystem/Agent/` | AI configuration stored on each `Character` (behaviour, combat style, ranges, home zone, group, skill pool, pathfinding) |
| `AgentRegistry` | `CoreSystem/Agent/AgentRegistry.cs` | Group alert registry of `AgentHandle`s |
| `AgentGizmos` | `CoreSystem/Agent/AgentGizmos.cs` | Scene gizmos drawn when an agent-controlled character is selected |

## 3. Lifecycle

1. **Awake**: `Character` gets a `RuntimeId`, initialises its systems and `Presenter`, sets the control claim filter (Possession denied when `ImmuneToControl`).
2. **OnEnable**: registers in `CharacterRegistry`.
3. **Start**: `CharacterFactory.AssignController`:
   - `Player` → `PlayerCore.Ensure().Possess(character)` (takes the Owner lane, binds GameData, raises `OnPlayerChanged`; `CameraController` listens and follows the new player).
   - `Agent` → `AgentCore.Ensure().Register(character)` (takes the Owner lane, creates an `AgentHandle` from `AgentProfile`).
   - `None` → nobody controls it until something claims it.
4. **Runtime**: controllers send commands through `character.Control`; leases tick in `Character.Update`.
5. **Death (agent)**: the handle plays death, hides the world bar and destroys the GameObject after 5 s unless revived.
6. **OnDisable / OnDestroy**: leaves the registry; the controller releases its lease.

Spawners set `AgentProfile` overrides in the `onSpawned` callback, before `Start`, so the handle is built with them.

## 4. Getting the player

```csharp
Character player = PlayerCore.GetPlayer();
if (PlayerCore.TryGetPlayer(out var p)) p.skillCaster.GrantSkill(id);
if (PlayerCore.IsPlayer(someCharacter)) { }
PlayerCore.OnPlayerChanged += c => { };
```

- Returns `null` until a `Player` character has been possessed, and after it is released or destroyed.
- Use `.gameObject` / `.transform` on the result for the GameObject. Do not look the player up by tag, `Find`, or `GameData`.
- Code that must react when the player appears or changes (camera, HUD) subscribes to `OnPlayerChanged` instead of polling.

## 5. Taking control temporarily

```csharp
var lease = target.Control.Claim(PlayerCore.Instance, ControlLane.Possession, 5f, onEnded: () => PlayerCore.Instance.Possess(original));
if (lease == null) return;
PlayerCore.Instance.Possess(target);
```

- While a higher lane is active, lower holders (the AI) cannot command; their handles pause.
- When the lease ends (time, `keepWhile` returning false, or `Release`), control returns to the next lane automatically.
- Bosses with `ImmuneToControl` reject Possession claims. The Dev lane always succeeds.

## 6. Asset layout

- Entity prefabs (`Addressable/Prefab/Entity/...`) and the scene player all carry the same `Character` component. AI settings live under its **Agent Profile** section, animation references under **Presentation**.
- The scene player has `Default Controller = Player`; entities use `Agent`.
- The old `AgentCore`, `PlayerController`, `PlayerCharacter`, `EnemyCharacter` and `NonPlayerCharacter` components were migrated on 2026-10-16. Backups of the pre-migration scene and prefabs: `Backup/2026-10-16_character-refactor/assets/`.

## 7. Known follow-ups

- AI is still the original state machine ported into `AgentHandle`; the node-based AI and distance/chunk throttling are phases C8–C9.
- `CharacterPresenter.ShowWorldStatusBar` still uses `GameObject.Find("WorldSpace")`; it is covered by the Find-removal plan in `Coding_Standards.md`. The camera no longer looks the player up by tag (it listens to `PlayerCore.OnPlayerChanged`).
- `FloatingStatusBar` still decides player styling from `DefaultController` (configuration), not from who currently controls the body.
