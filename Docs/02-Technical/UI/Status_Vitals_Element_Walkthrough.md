# Status Vitals Element Walkthrough (Inspect VITALS = StatusModal look)

Verified: **2026-11-07** against `StatusVitalsPrefabBuilder.cs`, `CharacterInspectWindow.cs`, `UICustomElement.cs` and `UISnapshotRenderer.cs`.

The VITALS section of the Inspect window (right-click a character > Inspect) uses the same HP / MP / EXP bars and BASE / JOB card as the player's Status modal (`Assets/Prefab/Game Page/StatusModal Window.prefab`). A builder tool copies the Status modal's `Content` into a UIBuilder custom element named **Status Vitals**. The Inspect window places that element and fills it with the inspected character's values. The rest of the Inspect window (header, ATTRIBUTES, COMBAT, TAGS) is unchanged.

## 1. Where to open it

- Unity menu **Pale Vertica > UI > Build Status Vitals Element**.
- Batch, with Unity closed:
  ```
  "D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -projectPath "F:/GameProject/Pale Vertica" -executeMethod CoreSystem.Editor.UI.StatusVitalsPrefabBuilder.BuildBatch -logFile statusvitals.log
  ```
- From code: `CoreSystem.Editor.UI.StatusVitalsPrefabBuilder.Build()`.

## 2. Inputs

| Input | Path / note |
|---|---|
| Source prefab | `Assets/Prefab/Game Page/StatusModal Window.prefab`. It is opened read only and never saved. It needs a child `Content` holding `Level/Base/Value`, `Level/Job/Value` and `Status/{HealPoint, ManaPoint, Exp}/{Frame/Bar/Value, ValueText}` |
| Registry | `Assets/Prefab/UIElementRegistry.asset` |

## 3. Steps (what the tool does, in order)

1. Opens the Status modal prefab contents and copies its `Content` (508 x 200: the three bars on the left, the `UI_Num_Frame` card on the right).
2. Centres the copy, fixes its size with a `LayoutElement` (min and preferred 508 x 200, no flex) so a builder column cannot stretch it.
3. Makes every text box at least as tall as its font line, keeping the text where it was (the Status modal's boxes are shorter than the font, which the layout checker reports as `TextOverflow`).
4. Adds a `UICustomElement` with these keys:

   | Key | Target | Set with |
   |---|---|---|
   | `HP Fill`, `MP Fill`, `EXP Fill` | the filled `Value` image inside each bar | `SetFill(key, 0..1)` |
   | `HP Value`, `MP Value`, `EXP Value` | the bold italic number under each bar | `SetText` |
   | `Base`, `Job` | the big numbers in the card | `SetText` |

5. Saves `Assets/Prefab/Game Page/Global/UIElement/Status Vitals.prefab`.
6. Adds or updates the registry entry `Status Vitals` (type Custom, **Keep Prefab Look** on, no sizing rules).

## 4. Expected result

- Console: `[StatusVitalsPrefabBuilder] Built Assets/Prefab/Game Page/Global/UIElement/Status Vitals.prefab`.
- `StatusModal Window.prefab` is not changed.
- Inspect a character: VITALS shows `437/1250` style HP and MP, `1,234 / 49,754` EXP, and `027` / `009` in the card, the same formats as the Status modal (`PlayerStatusWindow`).
- **Pale Vertica > UI > Render UI Snapshots** writes `inspect_character.png` (0 layout issues) and `status_modal.png` (the Status modal as a reference; it is not checked because the hand-made prefab has its own `TextOverflow` reports).

Use it from any builder screen:

```csharp
ui.Custom("Status Vitals")
  .SetFill("HP Fill", hp / maxHp).SetText("HP Value", $"{hp:F0}/{maxHp:F0}")
  .SetText("Base", level.ToString("000"));
```

`CharacterInspectWindow` rebuilds the stats region when the target's `AttributeSystem.OnAttributesChanged` or `ExperienceSystem.OnBaseExpChanged` fires, so the bars follow the inspected character, not the player.

## 5. Common mistakes

- **Editing `Status Vitals.prefab` by hand.** The next build overwrites it. Change the Status modal, then run the tool again so both stay the same.
- **Renaming children in the Status modal** (`HealPoint`, `ValueText`, `Level/Base/Value`...). The tool stops with "'path' not found". Update the paths in `StatusVitalsPrefabBuilder.Mappings` and the keys in `CharacterInspectWindow.BuildVitals` together.
- **Using it in Dark mode.** The element keeps the Status modal's black text, like the Status modal itself. The Dev Tool dark toggle makes the Inspect panel dark, so VITALS is hard to read there.
- **Expecting `Level Frame` or `HP Bar` changes to show in Inspect.** Inspect no longer uses those Light kit styles for VITALS.
