# Production Strip List

Dev-only tools, batch entry points, render hooks and test helpers. Remove them, or check that they stay out of the player build, before shipping. Add a row whenever you add a dev-only hook. Started 2026-10-21.

Anything under an `Editor/` folder is left out of player builds by Unity automatically. Those rows are about cleaning the project. Rows marked **Runtime** compile into the game and must be removed or guarded by hand.

## 1. Batch entry points (call `EditorApplication.Exit`, used by `Tools/FigmaToUnity/unity_batch.sh`)

| Method | File | Added for |
|---|---|---|
| `SkillRepairAssetsTool.Apply` (menu Pale Vertica > Skills > Apply Skill Repairs; no Exit call) | `Assets/Script/Editor/SkillRepairAssetsTool.cs` | one-time setup of the repaired skill assets (B1); delete after it has been run |
| `FogAuraMaterialBuilder.CreateMissingBatch` | `Assets/Script/Editor/UI/FogAuraMaterialBuilder.cs` | creating the Fog Aura material files with Unity closed (2026-10-24) |
| `MiniHeaderUIKitGenerator.BuildKitBatch` | `Assets/Script/Editor/UI/MiniHeaderUIKitGenerator.cs` | building the MH kit and registry styles with Unity closed |
| `LightUIKitGenerator.ResizeArtButtonsBatch` | `Assets/Script/Editor/UI/LightUIKitGenerator.cs` | resizing the art buttons to 181 x 55 |
| `UISnapshotRenderer.RenderAllBatch` | `Assets/Script/Editor/UI/UISnapshotRenderer.cs` | rendering every snapshot |
| `RefinementAndSocketPrefabBuilder.BuildSocketBatch`, `BuildAndSnapshotBatch`, `BuildSocketAndSnapshotBatch`, `TakeSnapshotsBatch` | `Assets/Script/Editor/UI/RefinementAndSocketPrefabBuilder.cs` | socket / refinement rebuilds |
| `LightUIKitGenerator.BuildBatch`, `SkillManagePrefabBuilder.BuildBatch`, `ActionProgressPrefabBuilder.BuildBatch`, `SkillConverter.ConvertSkillIdsBatch` | `Assets/Script/Editor/...` | older builders |
| `MapLab.Editor.MapLabIconImporter.ApplyBatch` | `Assets/ThirdParty/MapLab/Editor/` | setting up the Figma-exported MapLab icons and assigning them to scripts (2026-10-23) |
| `MapLab.Editor.MapLabPerformanceProbe.ReportBatch` | `Assets/ThirdParty/MapLab/Editor/` | measuring MapLab rebuild, bake, NavMesh and render cost into `Logs/MapLab/MapLab_performance.txt` (2026-10-23) |
| `MapLab.Editor.MapLabBakeProbe.ReportBatch`, `MapLab.Editor.MapLabStaticBaker.BakeDemoBatch` | `Assets/ThirdParty/MapLab/Editor/` | static ground bake prototype: live vs baked image diff and timing into `Logs/MapLab/MapLab_bake.txt`; baking the demo with Unity closed (2026-10-23) |
| `MapLab.Editor.MapLabWorldProbe.ReportBatch` | `Assets/ThirdParty/MapLab/Editor/MapLabWorldProbe.cs` | mega chunk / floor / preset check on a throw-away copy of the demo (`Demo/WorldProbe/`, deleted at the end): mega folders, single mega rebake, PSD per mega chunk export and folder import, presets, Decorate line (step 8 in `MapLabDrawLayerProbe.cs`: a Decorate layer takes the Cliffs set and a tree prop, shadow layer split, Cast shadows off and on, a type sent to Keep live and back, bake order floor < decor < shadows < actors); writes `Logs/MapLab/MapLab_world.txt`, `MapLab_world_shot.png`, `MapLab_world_map.png`, `MapLab_drawlayers_before/ground/baked.png`, PSDs in `Logs/MapLab/WorldProbe/` (2026-10-24) |
| `MapLab.Samples.Elevation.Editor.ElevationDemoBuilder.BuildBatch`, `BuildAndRenderBatch` | `Assets/ThirdParty/MapLab/Samples/Elevation/Editor/ElevationDemoBuilder.cs` | building the elevation demo scene (`Samples/Elevation/Scenes/MapLab_Elevation_Demo.unity`, art in `Samples/Elevation/Art/`, bake folder next to the scene) with Unity closed; `BuildAndRenderBatch` also walks the hero to the top tier and writes `Logs/MapLab/MapLab_elevation_walk.txt` / `.png` with a sort check per frame (2026-10-24) |
| `MapLab.Editor.MapLabElevationProbe.ReportBatch` | `Assets/ThirdParty/MapLab/Editor/Elevation/MapLabElevationProbe.cs` | P0 of the elevation proposal: throw-away in-memory scene (never saved) with a walk-on-top Plateau, a tier, stand-in actors and trees; compares the sort variants `None` / `SnapToSlice` / `LikeATier` with `MapLabElevationSortCheck` (with a south ramp and west stairs and actors on them since P2); then opens the demo scene in memory (never saved), checks `Walkable.json` is unchanged, turns on Walk On Top for its plateaus, adds a south ramp to each and bakes the elevation data; writes `Logs/MapLab/MapLab_elevation.txt`, `MapLab_elevation_<variant>.png` / `_misses.png`, `MapLab_elevation_demo.png` / `_demo_raised.png` (2026-10-24) |
| `MapLab.Editor.MapLabLayerProbe.ReportBatch` (uses `MapLabSortedProbe`) | `Assets/ThirdParty/MapLab/Editor/MapLabLayerProbe.cs`, `MapLabSortedProbe.cs` | bake layers check on the demo: bake, sorted layer vs live with stand-in actors, Flatten on temporary paint, sample paint from a PSD (ground and wall paint), rebake keeps paint, changed-under-paint flag, export/import round trip, draw order, shot; writes `Logs/MapLab/MapLab_layers.txt`, `MapLab_sorted_*` images and PSDs in `Logs/MapLab/` (2026-10-23) |
| `MapLab.Editor.MapLabSceneBuilder.BuildBatch`, `BuildAndRenderBatch`, `MapLab.Editor.MapLabPreviewRenderer.RenderBatch` | `Assets/ThirdParty/MapLab/Editor/` | building and rendering the MapLab demo scene with Unity closed (2026-10-23; moved into the standalone MapLab module the same day) |
| `CoreSystem.Editor.CharacterAsset.CharacterSortingSceneInstaller.InstallSandBoxBatch` (+ `Install(scenePath, rootName)`) | `Assets/Script/Editor/CharacterAsset/CharacterSortingSceneInstaller.cs` | adding the `Model` (order 6) and `Model/Shadow` (order 1, Sort At Root) sorting groups to the unpacked `Player` in `SandBox.unity` with Unity closed (2026-10-24) |
| `CoreSystem.Editor.World.WalkableGridSceneInstaller.InstallSandBoxBatch` (+ `Install(scenePath, jsonPaths)`) | `Assets/Script/Editor/World/WalkableGridSceneInstaller.cs` | adding the `Walkable Grid` object (WalkableGridManager with `SandBox_MapLab/Environment/Walkable.json`) to `SandBox.unity` with Unity closed (2026-10-24) |
| `CoreSystem.Editor.NPC.AlchemiteReturnNpcBuilder.BuildBatch`, `StandardizeBatch`, `PreviewBatch` | `Assets/Script/Editor/NPC/AlchemiteReturnNpcBuilder.cs` | building the four NPC prefabs ported from AlchemiteReturn (Alchemist, Shopkeeper, OldMan, Blacksmith) and placing the Alchemist and the Blacksmith in `SandBox.unity`; rendering their Idle / walk sheets next to `Ally` into `Logs/CharacterPreview/` (2026-11-07) |
| `StatusVitalsPrefabBuilder.BuildBatch` | `Assets/Script/Editor/UI/StatusVitalsPrefabBuilder.cs` | copying the StatusModal `Content` into the `Status Vitals` element (Inspect VITALS) with Unity closed (2026-11-07) |
| `ItemSlotPrefabBuilder.BuildBatch`, `ItemCardPrefabBuilder.BuildBatch`, `MerchantStallPrefabBuilder.BuildBatch`, `ItemSlotAdoptionBuilder.BuildBatch` | `Assets/Script/Editor/UI/` | building the new ItemSlot / ItemCard (Classic Strip) prefabs, the Merchant's Stall window and moving Equipment / Costume / Refinement / Socket slots onto the ItemSlot art with Unity closed (2026-11-08) |

## 2. Builder menus

| Menu | File |
|---|---|
| Pale Vertica > UI > Build MiniHeader Window Style | `MiniHeaderUIKitGenerator.cs` |
| Pale Vertica > UI > Build MiniHeader Kit Only | `MiniHeaderUIKitGenerator.cs` |
| Pale Vertica > UI > Build Light UI Kit | `LightUIKitGenerator.cs` |
| Pale Vertica > UI > Build Refinement and Socket Windows / Take Snapshots of Refinement and Socket | `RefinementAndSocketPrefabBuilder.cs` |
| Pale Vertica > UI > Build Skill Manage Pages, Build Action Progress Window | their builders |
| Pale Vertica > UI > Create Fog Aura Materials | `FogAuraMaterialBuilder.cs` (content tool: creates missing material files only; keep or move to a dev-only folder) |
| Pale Vertica > UI > Render UI Snapshots | `UISnapshotRenderer.cs` |
| Pale Vertica > UI > Build ItemSlot, Build ItemCard, Build Merchant Stall, Apply ItemSlot To Windows | `ItemSlotPrefabBuilder.cs`, `ItemCardPrefabBuilder.cs`, `MerchantStallPrefabBuilder.cs`, `ItemSlotAdoptionBuilder.cs` (content tools: they rebuild prefabs, keep or move to a dev-only folder; `EditorUIFactory.cs` is their shared helper) |
| Pale Vertica > UI > Build Status Vitals Element | `StatusVitalsPrefabBuilder.cs` (content tool: rebuilds `Status Vitals.prefab` from `StatusModal Window.prefab`; StatusModal itself is never saved) |
| Pale Vertica > World > Set Up SandBox Test Scene | `Editor/World/SandBoxSceneSetup.cs` (one-off migration: delete after SandBox is moved) |
| Pale Vertica > World > Preview Camera | `Editor/World/ScenePreviewCamera.cs` (editor-only temporary camera for map editing) |
| Pale Vertica > World > Import Alchemite Monsters And Environment | `Editor/World/AlchemiteContentBuilder.cs` (one-off content import: builds monsters and dresses Town and Field) |
| Pale Vertica > World > Rebuild Town and Field | `Editor/World/WorldMapsBuilder.cs` (content tool: builds Town and Field from project art; archives the old scenes) |
| Pale Vertica > Quests > Build Quest Tracker | `Editor/Quests/QuestTrackerBuilder.cs` (content tool: builds the tracker window and its registry entry) |
| Pale Vertica > World > Convert Scenes To Game Core | `Editor/World/GameCoreSceneConverter.cs` (one-off migration: delete after the scenes are converted) |
| Pale Vertica > World > Build Title Scene | `Editor/World/TitleSceneBuilder.cs` (content tool: creates the placeholder Title scene) |
| Pale Vertica > Loot > Build Loot Visuals | `Editor/Loot/LootVisualsBuilder.cs` (content tool: builds loot settings, placeholder drop prefab, label window and its registry entry) |
| Pale Vertica > Entity > AlchemiteReturn NPCs > Build Prefabs / Place Alchemist In Scene / Place Blacksmith In Scene / Apply Standard To PSBs / Render Preview | `Assets/Script/Editor/NPC/AlchemiteReturnNpcBuilder.cs` (Build and Place are content tools; Render Preview is dev-only) |
| Tools > MapLab > MapLab Window, GameObject > MapLab > Map / Set, Open MapLab Window button on MapLabMap | `ThirdParty/MapLab/Editor/Window/`, `MapLabSets.cs`, `ExtrudedShapeMenu.cs` (product feature, not dev-only) |
| Tools > MapLab > Developer > Build Demo Scene | `ThirdParty/MapLab/Editor/MapLabSceneBuilder.cs` |
| Tools > MapLab > Developer > Render Demo Preview | `ThirdParty/MapLab/Editor/MapLabPreviewRenderer.cs` |
| GameObject > MapLab > Plateau / Void / Ring / Box Shape, Path; Edit Points tools and inspector | `ThirdParty/MapLab/Editor/ExtrudedShapeMenu.cs`, `PointShapeEditor.cs`, `ExtrudedShapeTool.cs`, `PathShapeTool.cs` (product feature, not dev-only) |
| Tools > MapLab > Developer > Apply Icons | `ThirdParty/MapLab/Editor/MapLabIconImporter.cs` (product tooling; icons exported from Figma live in `Editor/Icons/`) |
| Tools > MapLab > Developer > Performance Report | `ThirdParty/MapLab/Editor/MapLabPerformanceProbe.cs` (dev tool; move to a dev-only folder or drop before selling) |
| Tools > MapLab > Bake Walkable Area (Open Scenes), Bake button on MapLabMap, Walkable area page (four steps) and its Scene view frame | `ThirdParty/MapLab/Editor/MapLabWalkableBaker.cs`, `MapLabMapEditor.cs`, `Editor/Walkable/`, `Editor/Window/MapLabBakeView.cs` (product feature, not dev-only; listed so it is not mistaken for one) |
| Tools > MapLab > Developer > Static Bake Report | `ThirdParty/MapLab/Editor/MapLabBakeProbe.cs` (dev tool; uses the now public `MapLabPerformanceProbe.RenderMs`) |
| Tools > MapLab > Bake All Layers / Unbake All Layers (Open Scenes), Open Layers / Bake All Layers / Unbake buttons on MapLabMap, Layers page (Export / Import PSD and PNG) | `ThirdParty/MapLab/Editor/MapLabStaticBaker.cs`, `MapLabMapEditor.cs`, `Editor/Layers/`, `Editor/Window/MapLabLayersView.cs` (product feature, not dev-only) |
| Tools > MapLab > Developer > World Report | `ThirdParty/MapLab/Editor/MapLabWorldProbe.cs` (dev tool; drop before selling) |
| Tools > MapLab > Developer > Bake Layers Report | `ThirdParty/MapLab/Editor/MapLabLayerProbe.cs`, `MapLabSortedProbe.cs` (dev tools; drop before selling) |
| Tools > MapLab > Samples > Build Elevation Demo, Render Elevation Walk | `ThirdParty/MapLab/Samples/Elevation/Editor/ElevationDemoBuilder.cs` (Build is part of the sellable sample; Render Elevation Walk and `RenderWalk` are dev-only, drop before selling) |
| Tools > MapLab > Developer > Elevation Sort Report | `ThirdParty/MapLab/Editor/Elevation/` (`MapLabElevationProbe`, `MapLabElevationSortCheck`, `ElevationSortTypes`; dev tools for the elevation P0, 2026-10-24). The variants `None` and `SnapToSlice` and the public `ActorSprite` / `CreateActor` / `SetSortOffset` / `SortOffset` (both overloads) and `MapLabElevationSortCheck.AddRamp` / `TopPieceSortY` / `Describe` exist only for the probe, the walk render and the tests; drop them before selling. `ExtrudedShape.WalkOnTop`, `ActorFloor`, `SurfaceSortBias`, `BaseSortBias`, `SharedMaterial`, `RampShape`, `RampFormula`, the ground UV channel in `ShapeMeshData`, the Add Ramp tool, `MapLab.Elevation` (`ElevationMap`, `ElevationPathFormula`, `ElevationBody`) and the `Samples/Elevation` sample are product features, not dev-only |
| Context menu **Find Baked Walkable Data** and `Reset()` auto-fill on Walkable Grid Manager | `Assets/Script/CoreSystem/Pathfinding/WalkableGridManager.cs` inside `#if UNITY_EDITOR` (content tool, not dev-only; listed so it is not mistaken for one; compiled out of player builds) |

## 3. Render (snapshot) hooks

| Hook | Where | Notes |
|---|---|---|
| Snapshots `miniheader_settings`, `system_menu`, `settings_<n>_<Title>` (one per `SettingsRegistry` page, 2026-10-24), `system_confirm_exit` | `UISnapshotRenderer.Run` | added 2026-10-20 / 21 |
| Snapshot `status_modal` and `SetupStatusModal` | `UISnapshotRenderer.Run` | renders `StatusModal Window.prefab` bound to the preview player as the reference for the Inspect VITALS; render only, not part of `InspectAll` (2026-11-07) |
| Snapshots `itemslot_sheet`, `itemcard_sheet`, `merchant_stall_a/b` | `UISnapshotRenderer.Run`, `Editor/UI/ItemSlotSnapshotSheet.cs`, `ItemCardSnapshotSheet.cs`, `MerchantStallSnapshot.cs` | sample sheets for comparing ItemSlot / ItemCard with the Figma `Shop Components`, and the Stall window in states A/B with a sample shop (2026-11-08). `ItemCardView.ForceState`, `CartSlotView.ForceHover` / `ForceDropTarget` are **Runtime** and public only for these renders |
| `SettingsWindow.Setup(modal, tab)` | `Assets/Script/UI/SystemMenu/SettingsWindow.cs` | **Runtime**: overload used only by the renderer to open a given page in a prepared modal; `Open(tab)` covers the game (2026-10-24) |
| `MiniHeaderUIKitGenerator.BuildKeyButton`, registry styles `MH Foldout Header` / `MH Key Button` | `Editor/UI/MiniHeaderUIKitGenerator.cs` | part of the MH kit build; the styles themselves are runtime data and stay (2026-10-24) |
| `MiniHeaderUIKitGenerator.BuildSampleSettings` | `Editor/UI/MiniHeaderUIKitGenerator.cs` | sample Settings layout used only for the render |
| `PlayerDeathScreen.Setup(modal)` / `Prime(...)`, `PlayerDeathInfo.Sample` | `Assets/Script/CoreSystem/Player/` | **Runtime**: public so the renderer can build the death screen with sample data (`deathscreen` snapshot; `dark_deathscreen` removed 2026-10-24). `Show` uses `Prime` too, so make it private when the hook goes |
| `SystemConfirmDialog.Prime(...)` | `Assets/Script/UI/SystemMenu/SystemConfirmDialog.cs` | **Runtime**: public only so the renderer can fill the dialog without opening it; `Show` uses it too, so make it private when the renderer hook is removed |
| `SettingsWindow.Setup(modal, tab)` | `Assets/Script/UI/SystemMenu/SettingsWindow.cs` | **Runtime**: overload used by the renderer to show a given tab |
| `CharacterPreviewRenderer.RenderSheet` | `Assets/Script/Editor/CharacterAsset/CharacterPreviewRenderer.cs` | renders character prefabs in a preview scene, sampling an Animator clip, into one PNG sheet; used by the AlchemiteReturn NPC preview (2026-11-07) |
| MapLab shots `MapLab_overview`, `MapLab_gameplay`, `MapLab_walkable` (bake overlay) | `MapLab.Editor.MapLabPreviewRenderer.RenderAll` | writes to `Logs/MapLab/`; reopens the scene without saving |
| `MapLabSceneBuilder.OnEnvironmentBuilt` event + `DemoNavigationInstaller` | `ThirdParty/MapLab/Editor/`, `ThirdParty/MapLab/Samples/Navigation/Editor/` | lets the Navigation sample add itself to the demo scene without the core knowing it |
| Game play-test scene | `Assets/Scenes/MapLab/MapLab.unity`, `Assets/Scenes/MapLab/Legacy/` (old SpriteShape profiles, cliff / curb / pillar art it still uses) | SandBox copy that holds a snapshot of the MapLab environment; the MapLab tools no longer rebuild it; delete both before shipping |
| MapLab module | `Assets/ThirdParty/MapLab/` (`Runtime/`, `Editor/`, `Samples/`, `Tests/`, `Demo/`) | **Runtime**: the `MapLab` and `MapLab.Samples` assemblies compile into the player build although the game does not use them yet; remove the folder from the game project or keep it only once it is wired in. The module is meant to be sold on its own (see `Docs/02-Technical/World/Map_Lab_Walkthrough_TH.md`) |
| Dev Tool page **World** (`BuildWorldPage`, `SpawnRandomMerchant`), `MerchantFactory.SpawnRandom` / `CreateRandomShop` | `CoreSystem/UI/DevTools/GameDevToolUI.cs`, `CoreSystem/Shop/MerchantFactory.cs` | **Runtime**: random merchant and shop for testing (2026-11-08). `OpenShop`, `Spawn` and `AttachShop` are normal API and stay |
| `SystemMenuWindow.Setup(modal)` / `SettingsWindow.Setup(modal)` being public | `Assets/Script/UI/SystemMenu/` | **Runtime**: public for the renderer; `Open()` is the game entry point |

## 4. Test-only code

| Item | Where |
|---|---|
| `UIStyleSizeLockTests`, `SystemMenuTests`, `ItemInsertRegistryTests` | `Assets/Script/Editor/Tests/` |
| `UILayoutRegressionTests` (fails on render layout errors) | `Assets/Script/Editor/Tests/` |
| `AlchemiteReturnNpcTests` (reads the four NPC PSBs and prefabs) | `Assets/Script/Editor/Tests/` |
| `WalkableGridTests`, `GridPathFormulaTests` | `Assets/Script/Editor/Tests/` (read the MapLab demo bake `Assets/ThirdParty/MapLab/Demo/MapLab_Demo_MapLab/Environment/`; ignored when it is missing) |
| `WalkableAreaBakeTests`, `ExtrudeFormulaTests`, `ExtrudedShapeTests`, `StaticBakeTests`, `BakeLayerTests`, `SortedLayerTests`, `IncrementalBakeTests`, `MapLabSetsTests`, `NavMesh2DFactoryTests`, `NavigationManagerTests`, `MegaChunkTests`, `WorldFloorTests`, `PresetTests`, `DrawLayerTests`, `WalkablePageTests`, `ElevationSortTests`, `ElevationBakeTests` (writes to `Assets/MapLabElevationTests`, deleted after each test), `RampShapeTests`, `ElevationApiTests`, `MapLabWindowStateTests` | `Assets/ThirdParty/MapLab/Tests/Editor/` (assembly `MapLab.Tests.Editor`) |

## 5. Outside the repo

| Item | Where |
|---|---|
| Talk to Figma plugin commands `list_pages`, `find_assets` (and the earlier `set_node_props` etc.) | `D:\Figma FlowUX Prototype\cursor-talk-to-figma-mcp\src\cursor_mcp_plugin\code.js` |
| Figma automation scripts (`fig.ts`, `kit.ts`, `mockups.ts`, `swapbtn.ts`, `figma.ts`, `icon_sets.py`) | session scratchpads only, not in the project |
| Figma section **MapLab / Icon Sets (6 styles)** on the Preview page | design reference for the MapLab icons (Set 5 / Tile is in use) |
| Figma section **MapLab / Walkable page (6 options)** | design options for the Walkable area page; Option 1 (Four steps) is the one built |
| Figma sections **MapLab / Window (6 layouts)**, **Window v2: sets + bake outputs (6 layouts)**, **Window final (Split View, cool tones)** | design options and the chosen spec for the MapLab window |
| Figma sections **MapLab / UI components**, **Window built from components**, **Guide: start from zero** | component library the window is built from, and the 12-card beginner guide; keep with the MapLab product, not the game |

## 6. Before shipping

1. Make a player build and check it compiles without anything from `Editor/` (Unity does this, but runtime code must not reference editor types).
2. Go through the **Runtime** rows in section 3 and make the renderer-only members private or remove them.
3. Decide whether the builder menus stay for content work or move to a dev-only package.
4. Remove the entries in this list as they are cleaned up.
