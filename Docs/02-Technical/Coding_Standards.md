# Coding Standards: Naming, Script Types, References

Status: **approved 2026-10-16**. Applies to all new code. Existing code is migrated in phases N1–N5 (see the end of this document).

Formatting rules (`#region` layout, no comments, event-driven UI) stay as defined in `AGENTS.md`.

## 1. Script types by suffix

A class name must tell you what kind of thing it is. One suffix, one meaning.

| Suffix | Meaning | Lifetime | Base | Examples |
|---|---|---|---|---|
| `Manager` | **Only** a single global instance per scene or session | Scene / session | MonoBehaviour | `UIManager`, `HotbarManager`, `ContextMenuManager`, `VisualManager`, `SkillTreeManager` |
| `System` | Subsystem owned by one character | Same as the character | Plain C# or component | `AttributeSystem`, `MovementSystem`, `ExperienceSystem`, `BuffSystem`, `UniquePassiveSystem` |
| `Player*` | Player state and data, no visuals | Session | Plain C# | `PlayerInventory`, `PlayerEquipment`, `PlayerCostumes`, `PlayerSkills`, `PlayerStats` |
| `Controller` | Drives one object from input or logic | Same as the object | MonoBehaviour | `PlayerController`, `CameraController` |
| `Director` | Coordinates several systems | Owner's | Either | `CombatJuiceDirector`, `CommandDirector` |
| `View` / `Window` / `Modal` / `Slot` | UI that only listens and renders | UI lifetime | MonoBehaviour | `AttributeListView`, `InventoryWindow`, `HotbarSlotView`, `TabGroup` |
| `Database` | Static lookup over loaded JSON data | App | Static | `ItemDatabase`, `FactionDatabase` |
| `Loader` | Static loading of assets or files | App | Static | `ItemIconLoader`, `SkillTreeLoader` |
| `Storage` | Static persistence (disk, prefs) | App | Static | `SaveStorage` |
| `Formula` | Pure math, numbers in and out | App | Static | Everything in `CoreSystem/Combat/Formulas/` |
| `Registry` | Live list of many instances | App | Static | `AgentRegistry`, `NpcRegistry` |
| `Events` | Cross-system event hub | App | Static | `CombatEvents`, `PlayerEvents` |
| `Factory` | Creates runtime objects (section 2) | App | Static | `SkillTreeManager.Create()` or `XFactory` |
| `Handler` / `Effect` / `Rider` / `Spec` / `Action` | Combat pipeline parts | Per action | Plain C# | `DamageHandler`, `DamageSpec`, `ActionRider` |
| `Settings` / `Table` / `SO` | Designer data | Asset | ScriptableObject | `EntityVisualSettings`, `ExperienceTable`, `UIWindowRegistrySO` |

Rules:
- Do not invent new suffixes. If nothing fits, discuss and add a row here first.
- `Manager` that is not a single global instance is a naming bug.
- `Helper`, `Utils`, `Misc` are not allowed. Put the code where it belongs or name what it does.

## 2. Runtime-created objects (not in a prefab or scene)

Anything code creates at runtime (for example `Database`, `SkillTreeManager` after player load, health bars, projectiles) follows all of these:

1. **One creation point.** Created only through `X.Create(...)` or `XFactory.Create(...)`. No scattered `AddComponent<X>()` or `new GameObject` for it elsewhere.
2. **Documented owner.** The class summary in `Docs/` states who creates it and when, e.g. "created by the player load flow".
3. **GameObject name** starts with `[Runtime]`, e.g. `[Runtime] SkillTreeManager`, and is parented under the single `[Runtime]` root, unless it is persistent.
4. **Self-registration.** Sets its static instance in `Awake`, clears it in `OnDestroy`, and exposes `IsReady` / `OnReady` for systems that may run before it exists.
5. **Not placeable by hand.** `[DisallowMultipleComponent]` and `[AddComponentMenu("")]`.
6. **Persistence.** `DontDestroyOnLoad` only for the persistent group. Today that is `Database` only.

## 3. How objects find each other

Runtime `Object.FindAnyObjectByType`, `FindObjectsByType`, `GameObject.Find` and `FindGameObjectWithTag` are not allowed in game code. They are fine in `Editor/` scripts and in `Reset()`/`OnValidate()`.

| Group | When | Use |
|---|---|---|
| A. Local | Same prefab or scene, known in advance | `[SerializeField]`, `GetComponentInParent` / `GetComponentInChildren` |
| B. Single instance | One per scene or session (`Manager`), including late-created ones like `SkillTreeManager` | Self-registering instance in `Awake` + `[DefaultExecutionOrder(-100)]` |
| C. Persistent | Lives across scenes, created by code before the first scene | Only `Database` |
| D. Many instances | Spawned and destroyed during play | `Registry` (register in `OnEnable`, unregister in `OnDisable`); spatial queries use `Physics2D` |
| E. Cross-system | Core must tell UI or presentation something | `Events`: core raises, UI listens. Core never calls UI directly |
| F. Stateless | Math, lookup, loading | Static class (`Formula`, `Database`, `Loader`) |
| G. Designer data | Tuned in the Inspector | ScriptableObject (`EntityVisualSettings` loads lazily from `Resources` on first use in the world) |
| H. Editor tools | `Editor/` folder | Anything, including Find |

## 4. Files, folders, namespaces

- **One public type per file, file name equals type name.** Exception: a file of small related enums or structs is named `<Area>Types.cs`.
- **Namespace equals folder path**: `CoreSystem.<Folder>[.<Sub>]` for `Assets/Script/CoreSystem`, and `CoreSystem.UI.<Folder>[.<Sub>]` for `Assets/Script/UI` (e.g. `UI/Hotbar` -> `CoreSystem.UI.Hotbar`). No global-namespace scripts.
- **Never name a folder after a common type** (`Attribute`, `Utility` types, etc.) if the namespace would shadow it: `UI/Attribute` became `UI/AttributePanel` because a `CoreSystem.UI.Attribute` namespace would hide the `Attribute` class.
- **Folder names**: PascalCase English, no double spaces, no typos. Utility code goes in `Utility/` (not `Utile/`).
- **Docs** live only in `Docs/`, never under `Assets/`.

## 5. Renaming safely

| What | Risk | Safe method |
|---|---|---|
| MonoBehaviour / ScriptableObject class | Prefabs and scenes reference the script GUID in `.meta` | Rename file and class together and keep (move) the `.meta` |
| Classes stored by `[SerializeReference]` (`EffectLogic`, `TargetingLogic`, `ActionRider` subclasses) | Assets store class name + namespace | Add `[UnityEngine.Scripting.APIUpdating.MovedFrom(...)]` with the old name/namespace |
| Names used as strings (Workbench `SkillConverter` cases like `"DamageEffect"`, JSON `EffectType`, save files) | String no longer matches | Do not rename, or update the converter in the same change |
| Folders under `Resources/` and Addressables addresses | String paths break | Search every path first; move in the Unity Editor |
| Asset folders | `.meta` must move with them | Rename in the Unity Editor, then run tests and open the main scenes |

Every rename phase compiles, runs the EditMode suite in Unity, and is committed on its own.

## 6. Migration plan

| Phase | Scope | Risk |
|---|---|---|
| N0 | This document, project skill, `AGENTS.md` | None. **Done** (`abc2e50`) |
| N1 | Typos and file/type mismatches in scripts: `Utile` -> `Utility` (3 folders), `PassiveSkilNode` -> `PassiveSkillNode`, split `SupportModifierSubclasses.cs` one class per file, `*Types.cs` for grouped enums | Low. **Done** (`75ca355`): also split every multi-type file except files with uncommitted user edits |
| N2 | Plain C# renames: `EquipmentManager` -> `PlayerEquipment`, `CostumeManager` -> `PlayerCostumes`, `SkillManager` -> `PlayerSkills`, `CharacterManager` -> `PlayerStats`, `UniquePassiveManager` -> `UniquePassiveSystem`, `SaveManager` -> `SaveStorage`, `AgentGroup` -> `AgentRegistry` | Low. **Done** (`e6d1fdc`); accessors are `GameData.Player.Equipment/.Costumes/.Skills/.Stats` |
| N3 | MonoBehaviour renames with `.meta`: `AttributeListManager` -> `AttributeListView`, `TabsManager` -> `TabGroup`, `PassiveSkillLoader` -> `PassiveTreeWindow`, `HotbarSlotUI` -> `HotbarSlotView`, `TemporaryBuffManager` -> `BuffSystem` (component on characters) | Medium. **Partly done** (`85455e0`): `TabGroup`, `HotbarSlotView`, `BuffSystem`. Pending with Unity closed: `AttributeListView`, `PassiveTreeWindow` (prefab UnityEvents store the type name) |
| N4 | Namespaces match folders (`AgentCore`, `AgentGroup`, `FaceDirectionModule` out of `Controllers`; `PassiveSkillLoader`, `CursorManager`, `UI/Attribute/*` into namespaces); `[MovedFrom]` where serialized | Medium. **Partly done** (`8864371`). Pending: `AttributeListManager`, `PassiveSkillLoader` (with N3) |
| N5 | Asset folders: `Materail` -> `Materials`, `Rescearch System` -> `Research System`, `SoundTack` -> `Soundtrack`, `Prefab/Game Page/Golbal` -> `Global`, `Character Eqipment Window` -> `Character Equipment Window`, `Character  PassiveTree Page` -> `Character PassiveTree Page` | High: do in the Unity Editor |
| Then | Remove runtime Find calls using groups A–H | Medium |


## Lessons from the migration
- Prefab UnityEvents store `m_TargetAssemblyTypeName` (type name + namespace). Renaming or re-namespacing a class used as a UnityEvent target requires updating those strings in the prefabs, with Unity closed.
- `m_EditorClassIdentifier` in scenes is cosmetic; Unity rewrites it on save.
- Mechanical renames must be string-literal aware, including nested quotes inside interpolated strings (`$"{(x ? "a" : "b")}"`). Verify any tool with an identity pass (no-op rename must leave every file unchanged) before writing.
