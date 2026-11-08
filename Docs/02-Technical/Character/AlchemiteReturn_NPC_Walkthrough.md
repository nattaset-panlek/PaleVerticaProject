# AlchemiteReturn NPC Walkthrough

Status: **built 2026-11-07**. Four village NPCs from the AlchemiteReturn project (`Assets/Raw Source/Player/NPC/alchemist.psb`) ported into Pale Vertica as standard character PSBs and NPC prefabs. The Alchemist is a Socket merchant and the Blacksmith a Refine merchant (since 2026-11-07, replacing the old `RefinementMerchant` placeholder), both placed in `SandBox.unity`; Shopkeeper and OldMan are standing NPCs with no role yet.

How the port step works in general: [Character Asset Walkthrough, Flow E](./Character_Asset_Walkthrough.md#5b-flow-e-import-a-psb-from-another-project-port). Merchant component and the Refine flow: [Blacksmith NPC Walkthrough](./Blacksmith_NPC_Walkthrough.md).

## 1. What the source file holds

`alchemist.psb` (512 x 512, no layer IDs, generic names) is a sheet of several characters drawn on the same canvas. In AlchemiteReturn each village NPC wears a head and a body from it over arms and legs taken from the player file `main_m.psb`. The hidden group `Folder 5` is a pixel-identical copy of `main_m.psb`, so everything needed is inside `alchemist.psb`.

| Group | Look | AlchemiteReturn NPC (`Main.unity`, `VillagePlate/...`) | Pale Vertica name |
|---|---|---|---|
| `Folder 7` | witch hat, long coat | `Alchemite` | **Alchemist** |
| `Folder 8` | wizard hat, round glasses, beard | `Shop` | **Shopkeeper** |
| `Folder 9` | white hair, white coat | `Dept` | **OldMan** |
| `Folder 6` | dark purple hair, apron | `BlackSmite` | **Blacksmith** |
| `Folder 5` (hidden) | teal-haired player (= `main_m.psb`) | arms and legs of every NPC | not imported on its own |

Parts used ("as in AlchemiteReturn"):

| Pale Vertica | Head | Body | ArmL / ArmR | LegL / LegR |
|---|---|---|---|---|
| Alchemist | `Folder 7/Layer 12` | `Folder 7/Layer 11` | `Folder 5/Folder 3 Copy` / `Folder 3` | `Folder 5/Folder 4 Copy` / `Folder 4` |
| Shopkeeper | `Folder 8/Layer 13` | `Folder 8/Layer 5` | `Folder 6/Layer 3` / `Layer 8` | `Folder 5/Folder 4 Copy` / `Folder 4` |
| OldMan | `Folder 9/Layer 17` | `Folder 9/Layer 10` | `Folder 5/Folder 3 Copy` / `Folder 3` | `Folder 5/Folder 4 Copy` / `Folder 4` |
| Blacksmith | `Folder 6/Layer 1` | `Folder 6/Layer 7` | `Folder 5/Folder 3 Copy` / `Folder 3` | `Folder 5/Folder 4 Copy` / `Folder 4` |

AlchemiteReturn shows `Folder 4` on both legs; Pale Vertica uses its translated copy `Folder 4 Copy` for the right-hand leg (`LegL`). The joints come from the AlchemiteReturn rig (sprite pivot minus skin offset, see Flow E step 2), except the Shopkeeper's arms: AlchemiteReturn turns them around a point just outside the sprite, so their joints were moved onto the shoulder end (`Layer 3` (293, 178), `Layer 8` (234, 180)). Scales: the first port used one scale, **0.735**, for all four; it keeps the Alchemist's hat inside the 400 px canvas, but the heads came out about 12% smaller than `Acrxer` (the player) and `Hymera`. Since 2026-11-07 the specs give parts their own scale to match those proportions:

| NPC | Head | Body | ArmL / ArmR | LegL / LegR |
|---|---|---|---|---|
| Blacksmith, OldMan | 0.85 | 0.8 | 0.82 | 0.735 |
| Alchemist | **0.84** on a **400 x 440** canvas (the witch hat brim reaches the right edge at 0.85) | 0.8 | 0.82 | 0.735 |
| Shopkeeper | 0.85 on a **400 x 440** canvas | 0.8 | 0.82 | 0.735 |

Legs stay at 0.735 because the rig fixes hip-to-ground (Flow E step 3). The two hats need more than 400 px of height at this head size, so their specs set `"canvasHeight": 440`: 40 px of headroom above the standard canvas. Joints and anchors are measured from the bottom, so nothing else moves ([Character Asset Standard](./Character_Asset_Standard.md), section 4).

Nothing else from AlchemiteReturn is used: its `Model.controller`, clips, `NpcInteraction.cs` and `AlchemiteReader.cs` belong to its own rig and systems. The Pale Vertica rig (`Ally.prefab` copy) and `Model.controller` animate the NPCs, and `RefinementMerchantSystem` gives the Alchemist its Socket option and the Blacksmith its Refine option.

## 2. Pieces

| Item | Path | Role |
|---|---|---|
| Port specs | `Tools/CharacterTemplate/ports/AlchemiteReturn/{Alchemist,Shopkeeper,OldMan,Blacksmith}.json` | Layer path and source joint per part, scale, source path (relative: the AlchemiteReturn project next to this one) |
| PSBs | `Assets/Image/Character/{Alchemist,Shopkeeper,OldMan,Blacksmith}.psb` | Standard 400 x 400 PSBs (six root layers, layer IDs 1001-1006) written by `character_psb_tool.py port` |
| Prefabs | `Assets/Addressable/Prefab/Entity/NPC/{Alchemist,Shopkeeper,OldMan,Blacksmith}.prefab` | Copies of `Ally.prefab` with the six sprites swapped and the NPC settings below |
| `NpcPrefabSpec` | `Assets/Script/Editor/NPC/NpcPrefabSpec.cs` | Data for one NPC: name, PSB, prefab path, npc id, Character.json id, tags, optional merchant window and label |
| Character data | `Assets/Resources/Data/Character.json` (Game Data Workbench > Characters) | One entry per NPC (ids 2001-2004): the display name in game, faction, prefab address |
| `NpcPrefabBuilder` | `Assets/Script/Editor/NPC/NpcPrefabBuilder.cs` | Generic: Apply Standard, Validate (stops on Error), Create Prefab From Base (`Ally.prefab`), NPC settings, optional `RefinementMerchantSystem` (`ApplySettings`, also usable on an existing prefab root); `PlaceInOpenScene`, `PlaceInSceneFile`. `Build` never overwrites an existing prefab |
| `AlchemiteReturnNpcBuilder` | `Assets/Script/Editor/NPC/AlchemiteReturnNpcBuilder.cs` | The four specs, menus, batch entry points, preview |
| `CharacterPreviewRenderer` | `Assets/Script/Editor/CharacterAsset/CharacterPreviewRenderer.cs` | Renders prefabs in a preview scene, sampling a clip, into one PNG sheet |
| Tests | `Assets/Script/Editor/Tests/AlchemiteReturnNpcTests.cs` | PSBs pass the standard, prefabs use their own six sprites, NPC settings, merchant only on the Alchemist (Socket) and the Blacksmith (Refinement) |

NPC settings written on each prefab's `Character`:

| Field | Alchemist | Blacksmith | Shopkeeper, OldMan | Why |
|---|---|---|---|---|
| Default Controller | `None` | `None` | `None` | not registered with `AgentCore`: never moves or fights |
| Tags | `Merchant` | `Merchant` | `Villager` | Inspect shows them; the cursor turns into the Dialogue cursor |
| Faction | `3` (Neutral) | `3` | `3` | nobody attacks them, no Attack option |
| Npc Id | `alchemist` | `blacksmith` | `shopkeeper`, `oldman` | data id for future dialogue and quests |
| Character Id | `2001` | `2002` | `2003`, `2004` | links the prefab to its `Character.json` entry; `Character.Name` (Inspect, kill messages) comes from that entry |
| Agent Profile > Agent Name | `Alchemist` | `Blacksmith` | `Shopkeeper`, `OldMan` | was `Ally` (copied from the base prefab) |
| Interactable | on | on | on | right-click menu works |
| Agent Profile > Can Roam | off | off | off | safe if someone switches the controller to Agent |
| `RefinementMerchantSystem` | label `Socket`, window `Socket`, range 1.5 | label `Refine`, window `Refinement`, range 1.5 | none | right-click walks the player up and opens that window |

### Names of each NPC

The same NPC has a different name in each place. Change all of them together; `AlchemiteReturnNpcTests.PrefabMatchesItsCharacterJsonEntry` fails when the spec, the prefab and `Character.json` disagree.

| Place | Alchemist | Blacksmith | Shopkeeper | OldMan |
|---|---|---|---|---|
| Group in `alchemist.psb` | `Folder 7` | `Folder 6` | `Folder 8` | `Folder 9` |
| Object name in AlchemiteReturn `Main.unity` | `Alchemite` | `BlackSmite` | `Shop` | `Dept` |
| Port spec, PSB, prefab file, prefab root object, spec `Name` | `Alchemist` | `Blacksmith` | `Shopkeeper` | `OldMan` |
| `Character` > Npc Id | `alchemist` | `blacksmith` | `shopkeeper` | `oldman` |
| `Character` > Character Id / `Character.json` Id | `2001` | `2002` | `2003` | `2004` |
| `Character.json` Name (shown in game) | `Alchemist` | `Blacksmith` | `Shopkeeper` | `Old Man` |
| Merchant window / right-click label | `Socket` / `Socket` | `Refinement` / `Refine` | none | none |
| Addressable address (`Prefab` group) | full prefab path | full prefab path | full prefab path | full prefab path |

`OldMan` was called `Dept` until 2026-11-07 (renamed with `AssetDatabase.MoveAsset`, so the PSB and prefab GUIDs did not change).

## 3. Build, place and preview

**With Unity open** (menu **Pale Vertica > Entity > AlchemiteReturn NPCs**):

1. **Build Prefabs**: for each NPC that has no prefab yet, applies the standard to its PSB, validates it and creates the prefab. Existing prefabs are kept as they are. Expected Console line: `[AlchemiteReturnNpcs] Done` with one `configured as ...` line per new prefab; the Alchemist prefab is selected.
2. **Place Alchemist In Scene** / **Place Blacksmith In Scene**: drops that NPC at the Scene view centre in the open scene (undoable). Save the scene. Or drag any of the four prefabs into a scene.
3. **Apply Standard To PSBs**: after porting a PSB again, reimports the four PSBs, applies the standard (new anchor pivots for parts whose size changed) and validates them. Expected Console line: `[AlchemiteReturnNpcs] Done` with `<Name>: Validate Pass` for each. Prefabs are not touched.
4. **Render Preview**: writes `Logs/CharacterPreview/AlchemiteReturn_Idle.png` and `AlchemiteReturn_Walk.png`: one row per character (first row `Ally` as the size reference), four frames each.

**With Unity closed** (Git Bash, from the project folder):

```bash
"D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -projectPath "F:/GameProject/Pale Vertica" -executeMethod CoreSystem.Editor.NPC.AlchemiteReturnNpcBuilder.BuildBatch -logFile Logs/Batch/alchemitereturn_build.log
```

`BuildBatch` builds the missing prefabs and places the Alchemist at (-2.2, 0.85) and the Blacksmith at (-3.2, 0.85) in `Assets/Scenes/SandBox.unity`, then saves the scene. Running it again changes nothing once both are there. `StandardizeBatch` runs **Apply Standard To PSBs** and then the preview; `PreviewBatch` renders the preview sheets only (run both without `-nographics`). `Tools/FigmaToUnity/unity_batch.sh` runs the same methods, but it waits while any Unity editor is open, including other projects; call `Unity.exe` directly when only another project is open.

To rebuild a PSB from the source art: [Flow E](./Character_Asset_Walkthrough.md#5b-flow-e-import-a-psb-from-another-project-port) step 5 with the spec above, written straight to `Assets/Image/Character`, then **Apply Standard To PSBs** (or `StandardizeBatch`). The prefab keeps working: the layer IDs stay 1001-1006, so the sprite IDs do not change. Never resize an NPC with the prefab's Transform scale; change the spec and port again. Delete the prefab first only if the NPC settings must be written again (the builder never overwrites).

## 4. Make another NPC from a standard PSB

```csharp
var spec = new NpcPrefabSpec("Herbalist", "Assets/Image/Character/Herbalist.psb", "Assets/Addressable/Prefab/Entity/NPC/Herbalist.prefab", "herbalist", 2005, CharacterTag.Villager);
var log = new List<string>();
GameObject prefab = NpcPrefabBuilder.Build(spec, CharacterAssetSettings.Instance, log);
```

Pass a window name (and label) for a merchant, as the Blacksmith spec does: `new NpcPrefabSpec(..., CharacterTag.Merchant, "Refinement", "Refine")`. To give an existing prefab a new role without rebuilding it, load it with `PrefabUtility.LoadPrefabContents`, call `NpcPrefabBuilder.ApplySettings(root, spec)` and save it. Add an entry with the same id to `Character.json` (Game Data Workbench > Characters: **+**, then drag the prefab into **Drag Prefab Here** to fill the address). Add the spec to a builder class with its own menu (as `AlchemiteReturnNpcBuilder` does) when devs need to rebuild it, and add its tests.

## 5. Verify

1. **Window > General > Test Runner > EditMode**: `AlchemiteReturnNpcTests` (22 cases), `RefinementMerchantTests`, `InteractionCoreTests` pass.
2. `Logs/CharacterPreview/AlchemiteReturn_Idle.png`: each NPC stands on its shadow like `Ally`, parts joined at the neck, shoulders and hips in every frame.
3. Play `SandBox.unity`: the Alchemist stands one unit right of the Blacksmith; neither moves. Hover: Dialogue cursor. Right-click the Alchemist: **Inspect** and **Socket** (no Attack); the Blacksmith: **Inspect** and **Refine**. Each option walks the player up and opens its window; left-click does the same because each has only one interaction.

### Results on 2026-11-07

- `port`: all four PSBs written; feet on the ground line; one warning each for Alchemist and OldMan (`Body does not cover the ArmR joint`).
- `BuildBatch`: four PSBs imported with new GUIDs, Apply Standard (Full Rect, Point, padding 8, reslice on, anchor pivots), Validate with no Error, four prefabs created, Alchemist placed in `SandBox.unity` at (-2.2, 0.85). The batch save also wrote the canvas `m_MatchWidthOrHeight` as 0, as every batch save of this scene does (Character Asset Standard, section 9); `DynamicCanvasScaler` sets it again when the scene opens in the editor.
- EditMode: 474 of 474 tests pass, including the 17 `AlchemiteReturnNpcTests` cases.
- `PreviewBatch`: Idle and walk sheets look like the `Ally` reference (same size, parts joined, feet on the shadow).
- **Not checked yet**: Play mode (the right-click **Socket** flow and the Dialogue cursor). The editor was not opened for this work.
- Later the same day, size: Blacksmith and OldMan ported again with per-part scales (Head 0.85, Body 0.8, arms 0.82, legs 0.735) so they match `Acrxer` and `Hymera`. Both written with no warning (OldMan's ArmR warning is gone), Apply Standard set new anchor pivots on four parts each, Validate: Pass. Prefabs untouched (same layer IDs, so same sprite IDs). `AlchemiteReturnNpcTests` 18/18. Preview sheets rendered again from the open editor; parts joined in every Idle and walk frame. Then Alchemist (Head 0.74) and Shopkeeper (Head 0.789), same Body / arms / legs, ported with no warning (the Alchemist's ArmR warning is gone too). `StandardizeBatch` (Unity closed): four PSBs Validate Pass, new anchor pivots on four parts of Alchemist and Shopkeeper, preview rendered; parts joined in every frame. Batch EditMode run: 475 of 475 tests pass. Then the hats moved to a 400 x 440 canvas (Alchemist Head 0.84, Shopkeeper 0.85): ported with no warning, **Apply Standard To PSBs** from the open editor, all four Validate Pass (canvas check: "40 px of headroom"), `AlchemiteReturnNpcTests` 18/18, preview and SandBox show all four faces at the same size.
- Later the same day: `Dept` renamed `OldMan` (PSB, prefab, port spec, npc id `oldman`); the four NPCs added to `Character.json` as 2001-2004 (Faction 3, Base HP 100, Base MP 50 as placeholders) and each prefab's Character Id and Agent Name set by `NpcPrefabBuilder.ApplySettings` from the open editor. `AlchemiteReturnNpcTests` 22/22 (run by calling the methods from the relay).
- Later the same day the Blacksmith became the Refine merchant and replaced `RefinementMerchant` in `SandBox.unity`: see [Blacksmith NPC Walkthrough](./Blacksmith_NPC_Walkthrough.md#change-on-2026-11-07-refinementmerchant-to-blacksmith).

## 6. Common mistakes

- **Copying `alchemist.psb` itself into `Assets/`.** It imports 12 sprites named `Layer 12`, `Layer 13`... on a 512 canvas (no arms or legs: those layers are hidden) that no tool or rig understands. Port it instead.
- **Inspect shows the prefab object name instead of the NPC name.** The prefab's Character Id is 0 or has no entry in `Character.json`.
- **Game Data Workbench was open during this change.** It keeps the old character list in memory; close and reopen it before saving, or a save writes the old list back.
- **NPC walks or fights.** Default Controller was set back to `Agent`, or the faction is not 3.
- **No Socket option.** The prefab lost its `RefinementMerchantSystem`, its Character field is empty, or the scene has no `UIManager` / `Socket` window (**Pale Vertica > UI > Build Refinement and Socket Windows**).
- **Parts drawn in the wrong place after editing the PSB in an art app.** Run **Validate** and **Apply Standard** in the Character Asset Tool; never move the joint objects in the prefab.
- **Gap at the right-hand shoulder of the Alchemist or OldMan when the arm swings.** Expected small gap: their coats do not reach the `ArmR` joint (validator warning). Fix it in the art (extend the coat) if it shows in game.
