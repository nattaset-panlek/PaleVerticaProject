# Action Progress Walkthrough

A small dark progress window for any action that takes time: casting, interacting, opening, reading, refining. One window exists at a time. It can be dragged and remembers where the player left it. Design: Figma page `WockSpace` > `20 · Features` > section `Action Progress Modal` (`1786:27045`): `Action Progress / Cyan` `1786:27046`, `/ Magenta` `1786:27078`, `/ No Icon` `1786:27110`.

## Where

| What | Path |
| --- | --- |
| Window (view) | `Assets/Script/UI/ActionProgress/ActionProgressWindow.cs` (`UIWindow`, registry name `Action Progress`) |
| Request data | `Assets/Script/UI/ActionProgress/ActionProgressRequest.cs` |
| Colours | `Assets/Script/UI/ActionProgress/ActionProgressTypes.cs` (`ActionProgressColor.Cyan` = #22D3EE, `Magenta` = #FF006F) |
| Cast bar hook | `Assets/Script/UI/ActionProgress/CastProgressEvents.cs` |
| Settings | `Assets/Script/CoreSystem/Settings/GameSetting.cs` (`ShowCastProgress`, saved window positions, PlayerPrefs) |
| Prefab (generated) | `Assets/Prefab/Game Page/Action Progress Window.prefab` |
| Builder | `Assets/Script/Editor/UI/ActionProgressPrefabBuilder.cs`, menu **Pale Vertica > UI > Build Action Progress Window** |
| Art | `Assets/Art/UI/New/Gameplay/Global/ActionProgress/` (backgrounds with / without icon cell, default icon; exported from Figma at 2x, PPU 200) |
| Time font | `Assets/Art/Front/Novecento-Font/Novecento Normal SDF.asset` (created by the builder from `Novecento Normal.ttf`) |
| Tests | `Assets/Script/Editor/Tests/ActionProgressTests.cs` |

## Inputs (`ActionProgressRequest`)

| Field | Meaning |
| --- | --- |
| `ActionName` | What is happening, top line (for example the item or skill name). |
| `ActionType` | The kind of action, bottom left, shown in upper case: `INTERACT`, `OPEN`, `READ`, `REFINE`, `CAST`... |
| `Icon` | Optional sprite. `null` hides the icon cell and switches to the full-width background. |
| `Duration` | Seconds. Above 0 the window runs its own timer and completes by itself. 0 means you report progress yourself. |
| `Color` | `Cyan` or `Magenta` bar colour. |
| `UseUnscaledTime` | Count real time (keeps running when the game is paused or slowed). |
| `Owner` | Any object that owns this progress; used to report, complete or cancel only your own bar. |
| `OnCompleted` / `OnCancelled` | Called when the bar fills / when it is cancelled, replaced or closed early. |

## Steps

### A. Show a timed action
```csharp
ActionProgressWindow.Show(new ActionProgressRequest
{
    ActionName = "Old Chest",
    ActionType = "OPEN",
    Icon = chestIcon,
    Duration = 1.5f,
    Color = ActionProgressColor.Cyan,
    Owner = this,
    OnCompleted = OpenChest,
    OnCancelled = () => Debug.Log("cancelled")
});
```
Cancel it (for example when the player moves): `ActionProgressWindow.Cancel(this);`

### B. Drive the bar yourself (manual mode)
Leave `Duration` at 0, then call `ActionProgressWindow.Report(this, normalized, remainingSeconds)` while it runs and `ActionProgressWindow.Complete(this)` or `Cancel(this)` at the end. This is how casting works.

### C. Casting
`CastProgressEvents` binds to the player's `SkillCaster` (re-binds on `PlayerCore.OnPlayerChanged`). A cast with a cast time shows a Magenta bar with the skill name, type `CAST` and the skill icon; executing completes it, interrupting cancels it. Turn it off with `GameSetting.ShowCastProgress = false` (saved; turning it off also closes a running cast bar). There is no Settings window in the game yet; call the property from a future settings screen or the dev tool.

### D. Refining
`RefinementWindow.ExecuteRefine` shows a 0.5 s Cyan bar (`RefineLoadingSeconds`) with the item name and `REFINE`. The refine (cost, materials, roll) happens in `OnCompleted`; the window refreshes afterwards. If materials or credits are missing it refines at once with the old message and shows no bar.

### E. Moving the window
Drag anywhere on the window. On release the position is saved with `GameSetting.SetWindowPosition("Action Progress", ...)`; every later show puts it back there. Reset with `GameSetting.ClearWindowPosition(ActionProgressWindow.DefaultWindowName)`. Default spot: bottom centre, 230 px up.

### F. Rebuild after a design change
1. Update the Figma frames, re-export the two backgrounds and the icon at 2x into the art folder (same file names).
2. Change sizes or colours in `ActionProgressPrefabBuilder` (all numbers come from the Figma nodes listed above).
3. Close Unity and run `Tools/FigmaToUnity/unity_batch.sh CoreSystem.Editor.UI.ActionProgressPrefabBuilder.BuildBatch CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch`, or use the menu item with Unity open.
4. Compare `Logs/UIDebug/action_progress_cyan.png` (and `_magenta`, `_noicon`) with the Figma export using `Tools/FigmaToUnity/compare.py`.

## Expected result

- A 380 x 86 window: black outer border, two grey-ringed cells (icon / content), Action Name, a 7 px bar with a 2 px black outline, the action type in grey capitals, remaining time `0.00s` on the right.
- No icon: one wide cell, text and bar start 14 px from the left.
- The registry entry `Action Progress` (type Window, no overlay) exists in `Assets/Prefab/UIWindowRegistry.asset`.

## Verification (2026-10-19)

- `compare.py` against the Figma exports: mean difference 1.65 (Cyan), 1.64 (Magenta), 1.60 (No Icon); layout report 0 errors, 0 warnings.
- Remaining visible difference: anti-aliasing of the 2 px bar outline and the topographic lines (the background is the 2x Figma export scaled down).
- EditMode: `ActionProgressTests` 5/5 pass.

## Common mistakes

- **Nothing shows**: the scene has no `UIManager`, or the registry entry is missing; rebuild with the builder.
- **Bar never finishes**: `Duration` is 0 and nobody calls `Report` / `Complete`.
- **Another system's bar closed yours**: only one bar exists; `Show` replaces a running bar and calls its `OnCancelled`. Check `ActionProgressWindow.IsRunningFor(owner)` before showing if you must not interrupt.
- **Cast bar does not appear**: `GameSetting.ShowCastProgress` is off, or the skill has no cast time.
- **Wrong font for the time**: the builder creates `Novecento Normal SDF` only if the `.ttf` exists at `Assets/Art/Front/Novecento-Font/`. That font has no small-cap lowercase, so the window writes the seconds suffix as `<size=66%>s</size>` (`_secondsSuffix`).
- **Window jumps back to the default spot**: the saved position is cleared, or the window was dragged outside the canvas (it is clamped back inside).
