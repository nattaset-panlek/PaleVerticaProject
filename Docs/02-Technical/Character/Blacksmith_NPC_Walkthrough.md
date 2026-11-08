# Blacksmith NPC Walkthrough (Refine)

Status: **Refine NPC since 2026-11-07**. The Blacksmith replaces the old `RefinementMerchant` placeholder (an `Ally.prefab` copy, 2026-10-19). It stands in `SandBox.unity` and never moves; the player right-clicks it, picks **Refine**, walks into range and the Refinement window opens.

The Blacksmith's art, PSB and prefab come from the AlchemiteReturn port: [AlchemiteReturn NPC Walkthrough](./AlchemiteReturn_NPC_Walkthrough.md). The same merchant component gives the Alchemist its **Socket** option.

## 1. Pieces

| Item | Path | Role |
|---|---|---|
| Prefab | `Assets/Addressable/Prefab/Entity/NPC/Blacksmith.prefab` | Blacksmith art on the `Ally.prefab` rig, with the NPC settings below and `RefinementMerchantSystem` |
| `RefinementMerchantSystem` | `Assets/Script/CoreSystem/Refinement/RefinementMerchantSystem.cs` | Component on the NPC. Registers itself as an `IInteractionProvider` (`OnEnable`/`OnDisable`) and adds the option `refinement.merchant` only when the player (`PlayerCore` holder) targets this NPC. Executing it calls `UIManager.Instance.Open(windowName)`. The name stays generic because the Alchemist (Socket) uses it too |
| Spec | `AlchemiteReturnNpcBuilder.Blacksmith` in `Assets/Script/Editor/NPC/AlchemiteReturnNpcBuilder.cs` | npc id `blacksmith`, tag `Merchant`, window `Refinement`, label `Refine` |
| `NpcPrefabBuilder.ApplySettings` | `Assets/Script/Editor/NPC/NpcPrefabBuilder.cs` | Writes the NPC settings and the merchant component onto a prefab root without touching sprites or other parts |
| Tests | `Assets/Script/Editor/Tests/RefinementMerchantTests.cs`, `AlchemiteReturnNpcTests.cs` | Option only for the player on this NPC, range, defaults, Blacksmith spec and prefab settings |

Settings on the Blacksmith prefab:

| Field | Value | Why |
|---|---|---|
| Default Controller | `None` | Not registered with `AgentCore`: no AI, no roaming, never moves |
| Tags | `Merchant` | Shown in Inspect; lets other systems recognise merchants; Dialogue cursor |
| Faction | `3` (Neutral) | `FactionDatabase.IsHostile` returns false both ways: nobody attacks it, no Attack option |
| Npc Id | `blacksmith` | Data id for future quests and dialogue |
| Interactable | on | `InteractionCore` gives no options for non-interactable characters |
| Agent Profile > Can Roam | off | Safe if someone later switches the controller to Agent |
| Refinement Merchant System | label `Refine`, window `Refinement`, range 1.5, priority 200 | Right-click **Refine** walks the player up and opens the Refinement window |

Interaction flow: right-click the Blacksmith -> `PlayerInteraction.BuildOptions` -> `InteractionCore.GetOptions` (Inspect, **Refine**) -> click **Refine** -> `InteractionCore.Request` walks the player within 1.5 units -> `RefinementMerchantSystem.OpenWindow()` -> `UIManager.Open("Refinement")` (the window picks the first equipment when no item is passed). See [Character Interaction Walkthrough](./Character_Interaction_Walkthrough.md) for the core.

## 2. Build the prefab

1. Open Unity. Menu **Pale Vertica > Entity > AlchemiteReturn NPCs > Build Prefabs**.
2. It only builds NPCs that have no prefab yet; an existing `Blacksmith.prefab` is kept as it is, with your manual edits.
3. To build it again from the PSB, delete `Blacksmith.prefab` first (manual edits are lost), then run **Build Prefabs**. Expected Console line: `Blacksmith: configured as Refinement merchant (npcId 'blacksmith', tags Merchant).`

Batch (Unity closed): `CoreSystem.Editor.NPC.AlchemiteReturnNpcBuilder.BuildBatch` builds missing prefabs and places the Alchemist and the Blacksmith in `SandBox.unity` if they are not there yet (command line in the AlchemiteReturn walkthrough, section 3).

## 3. Place it

- Menu **Pale Vertica > Entity > AlchemiteReturn NPCs > Place Blacksmith In Scene**: drops an instance at the Scene view centre (undoable, scene marked dirty). Save the scene.
- Or drag `Blacksmith.prefab` into the scene by hand.
- In `SandBox.unity` it stands at (-3.2, 0.85), one unit left of the Alchemist.
- Several Blacksmiths can exist; each only answers for itself.

## 4. Tune

On the prefab or an instance, component **Refinement Merchant System**:
- **Label**: menu text (default `Refine`).
- **Range**: walk-in distance in world units (default `1.5`, `0` = open from anywhere).
- **Priority**: menu order (Inspect 1000, Attack 900, Refine 200).
- **Window Name**: `UIWindowRegistry` name to open (`Refinement`; `Socket` turns it into a socket merchant like the Alchemist).

Change the look by editing `Assets/Image/Character/Blacksmith.psb` and running **Validate** / **Apply Standard** in the Character Asset Tool. Never move the joint objects in the prefab.

## 5. Verify

1. Test Runner > EditMode: `RefinementMerchantTests`, `AlchemiteReturnNpcTests` and `InteractionCoreTests` pass.
2. Play `SandBox.unity`. Hover the Blacksmith: the cursor turns into the **Dialogue** cursor. Right-click it from far away: the menu shows **Inspect** and **Refine** (no Attack).
3. Click **Refine**: the player walks up, stops, the Refinement window opens. The Blacksmith never moves.
4. Left-click the Blacksmith: Refine is its only interaction, so the player walks up and the Refinement window opens directly (no menu, no attack).
5. Moving with WASD on the way cancels the walk; nothing opens.

### Change on 2026-11-07 (RefinementMerchant to Blacksmith)

- `Blacksmith.prefab` changed in place with `NpcPrefabBuilder.ApplySettings` (not rebuilt): tag `Villager` to `Merchant`, `RefinementMerchantSystem` added (Refine, Refinement, 1.5). Sprites and everything else on the prefab are unchanged.
- `SandBox.unity`: the `RefinementMerchant` instance was replaced by a Blacksmith at (-3.2, 0.85) (old position (-3.22, 0.85)), at the same place in the Hierarchy. Scene saved from the open editor.
- `RefinementMerchant.prefab` and `RefinementMerchantPrefabBuilder.cs` were moved to the Windows Recycle Bin (never committed). Their menus **Build Refinement Merchant Prefab** and **Place Refinement Merchant In Scene** are gone; the npc id `refinement_merchant` is no longer used.
- Tests, run in the open editor through the Unity MCP relay (it cannot start the Test Runner, so the fixtures were called directly): `RefinementMerchantTests` 5/5, `AlchemiteReturnNpcTests` 18/18, `PrimaryInteractTests` 3/3, `InteractionCoreTests` 10/11. The eleventh (`Collect_ProviderExceptionDoesNotBreakOthers`) uses `LogAssert`, which only works inside the Test Runner.
- Full EditMode suite later in batch (Unity closed): 475 of 475 pass.
- **Not checked yet**: Play mode (the Refine flow on the Blacksmith).

## 6. Common mistakes

- **Window does not open**: the scene has no `UIManager`, or `Refinement` is missing from `UIWindowRegistry` (run **Pale Vertica > UI > Build Refinement and Socket Windows**).
- **No Refine option**: the component's **Character** field is empty, the NPC is not **Interactable**, or the right-click lands on no collider of the NPC.
- **Blacksmith walks or fights**: Default Controller was set back to `Agent`, or the faction was changed from Neutral.
- **Build Prefabs did not change the Blacksmith**: expected; the builder never overwrites an existing prefab. Edit the prefab, or delete it and build again.
- **Looking for `RefinementMerchant.prefab` or its menus**: removed on 2026-11-07; use the Blacksmith.
