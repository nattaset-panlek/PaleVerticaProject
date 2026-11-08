# Settings Walkthrough

The **Settings** window (System menu > Settings, or System menu > Keybind for the Controls page) and the settings data behind it. Every option is a real, saved parameter that game systems can read today, even when the system that uses it does not exist yet. Pages and options are data, so new ones need no window code. Figma: page `WockSpace` > `20 · Features` > **Dark UI Mockups** (mockups 1 Graphics, 2 Audio, 3 Keybind, 6 Gameplay; 4 Save/Load is not built). Added 2026-10-24.

## 1. Where things live

| Piece | File | Role |
|---|---|---|
| `SettingKeys` | `Assets/Script/CoreSystem/Settings/SettingKeys.cs` | Every page id, option key and group name as constants. Use these, never string literals |
| `SettingsRegistry` | `CoreSystem/Settings/SettingsRegistry.cs` | Pages (ordered) and options. `AddPage`, `AddOption`, `RemovePage`, `RemoveOption`, `Find(key)`, `GetOptions(pageId)`, `GetGroups(pageId)` |
| `SettingsCatalog` | `CoreSystem/Settings/SettingsCatalog.cs` | Registers the built-in pages and options (the Figma rows) |
| `SettingsPage` | `CoreSystem/Settings/SettingsPage.cs` | Id, title, order, optional `ViewId` for a custom page view |
| Options | `CoreSystem/Settings/Options/` | `ToggleSettingOption` (switch or checkbox), `SliderSettingOption`, `ChoiceSettingOption` |
| `SettingsStorage` | `CoreSystem/Settings/SettingsStorage.cs` | Versioned JSON store in PlayerPrefs key `Settings.Store` (`Version` 1, `Migrate` for later versions) |
| `SettingsEvents` | `CoreSystem/Settings/SettingsEvents.cs` | `OnChanged(key)` after each applied option, `OnApplied` after Apply |
| `SettingsDraft` | `CoreSystem/Settings/SettingsDraft.cs` | Pending values and key bindings until Apply |
| Key binding | `CoreSystem/Settings/Keybinds/` | `KeybindSetting` (bindings, conflicts, Input System overrides), `KeybindCatalog`, `KeybindDefinition`, `KeybindPair` |
| Window | `Assets/Script/UI/SystemMenu/SettingsWindow.cs` | Tabs from the registry, scrolling body, notice line, footer Default / Cancel / Apply |
| Rows | `UI/SystemMenu/SettingsOptionRows.cs` | Builds a page from its options: groups become foldouts, inline options (Mute) get a column |
| Custom pages | `UI/SystemMenu/SettingsPageViewRegistry.cs`, `KeybindPageView.cs` | `ViewId` -> builder. `keybinds` is the Controls page |

## 2. Pages and options today

| Page | Options (key in `SettingKeys`) | Stored in |
|---|---|---|
| Graphics | Resolution, Display Mode, Vertical Sync, Frame Rate Limit, Quality Preset | `DisplaySetting` (applied at once) |
| | Brightness 0-100 (default 50) | `SettingsStorage` |
| Audio | Master / Music / Sound Effects / Voice / Interface 0-100 (default 80), each with a **Mute** checkbox | `AudioSetting` (volume), `SettingsStorage` (mute) |
| | Mute When In Background | `SettingsStorage` |
| Gameplay > Camera | Camera Shake (on), Zoom Speed 0-100 (50) | `SettingsStorage` |
| Gameplay > Interface | Show Damage Numbers (on), Show Minimap (on), HUD Scale 50-150 % (100), Show Cast Progress | `SettingsStorage`, `GameSetting` |
| Gameplay > Language | Language EN / TH (collapsed by default) | `GameSetting` |
| Controls | Search, Primary / Secondary key per action, groups Movement, Combat, Actions, Hotbar, Interface | `SettingsStorage` keys `keybind.<id>` |

Options marked `SettingsStorage` have no consumer yet. They are saved and raise events, so a new system only has to read them.

## 3. Read a setting from a game system

1. Read the current value:
   - Stored options: `SettingsStorage.GetBool(SettingKeys.CameraShake, true)`, `GetFloat(SettingKeys.HudScale, 100f)`.
   - Bound options: use their owner, for example `AudioSetting.GetEffectiveVolume(AudioChannel.Music)` (includes mute) or `DisplaySetting.VSync`.
   - Any option, as an object: `SettingsRegistry.Find(key).Read()`.
2. Listen for changes: subscribe to `SettingsEvents.OnChanged` in `OnEnable`, compare the key, unsubscribe in `OnDisable`. Do not poll in `Update()`.
3. Audio code can also listen to `AudioSetting.OnChanged(channel)`, which fires for volume and mute changes.

## 4. Add an option

1. Add a key constant to `SettingKeys` (`"<page>.<group>.<name>"`).
2. Register it in `SettingsCatalog` (or from your own system before the window opens):
   ```csharp
   SettingsRegistry.AddOption(SettingKeys.PageGameplay, SettingKeys.GroupCamera, "Screen Flash",
       new ToggleSettingOption(SettingKeys.ScreenFlash, true), tooltip: "Flash the screen on critical hits.");
   ```
   - Stored option: the constructor that takes a default value.
   - Option owned by an existing setting class: the constructor that takes `get`, `set` and `default` functions.
   - A checkbox instead of a switch: `checkbox: true`. A value column next to another row (like Mute): set `InlineWithKey` to the parent key.
3. Read it from your system as in section 3. The row, Default, Cancel, Apply and the "Not applied" line work with no window change.

## 5. Add a page

1. Add a page id to `SettingKeys`.
2. `SettingsRegistry.AddPage(new SettingsPage(id, "Title", order))`, then add options to it. A new tab appears in that order.
3. For a page that is not a list of options, pass a `viewId` and register a builder: `SettingsPageViewRegistry.Register(viewId, (ui, window) => ...)`. Use `window.Draft` for values, `window.MarkChanged()` after a change and `window.RefreshBody()` to redraw.
4. Add a snapshot check: `UISnapshotRenderer` renders every registry page as `settings_<n>_<Title>` automatically.

## 6. Apply, Cancel, Default

- Every change goes to the draft. The line above the footer lists what is not applied yet; **Apply** is enabled only when something changed.
- **Apply** writes changed options, raises `SettingsEvents.OnChanged(key)` for each, saves key bindings (`KeybindSetting.SetAll`) and raises `OnApplied`. The window stays open.
- **Cancel**, the close button or Esc drop the draft.
- **Default** resets the current page of the draft only. Its label comes from `SettingsPage.ResetLabel`; the Controls page shows **Reset All** and resets every key binding. Press Apply to keep it.
- Groups start expanded except those in `SettingsWindow.CollapsedByDefault` (a group name, or `"<pageId>/<group>"` for one page): Language, and every key binding group except Movement.

## 7. Key binding

- Definitions come from three places:
  - `KeybindCatalog` reads the Player map of `InputSystem_Actions` (Look is skipped; Move has one row per direction);
  - hotbar slots 1-10 (`hotbar.slot.N`);
  - every single-key UI window in `UIManager` (`window.<WindowName>`, registered in `UIManager.Awake`).
- Click a key box, then press a key. **Esc** cancels, **Backspace** or **Delete** clears the slot, a mouse click cancels. If another action already uses the key, the keys are swapped and the notice line says so.
- Input users call `KeybindSetting.Attach(inputActions.asset)` after `new InputSystem_Actions()` (done in `PlayerCore` and `CameraController`) and `Detach` before disposing. Saved bindings are applied as binding overrides, and again after every Apply.
- Other readers: `KeybindSetting.GetPair(id)`, `GetHotbarKey(slot)` (used by `HotbarManager`), `TryGetWindowKeys(name)` (used by `UIWindowEntry`).
- New bindable action: add it to the Input Actions asset (it appears on its own), or `KeybindSetting.AddDefinition(new KeybindDefinition(id, label, group, order, defaults))` and read it with `GetPair(id)`.

## 8. Check

1. Compile: `python -I Tools/FigmaToUnity/compile_check.py`.
2. EditMode tests: `SystemMenuTests` (pages, catalog, dynamic pages, storage, draft, mute, key binding conflicts and overrides).
3. Renders: `settings_1_Graphics` ... `settings_4_Controls` in `Logs/UIDebug/`, 0 issues each, compared with the Figma mockups.

## 9. Known differences from the Figma mockups

- Frame Rate Limit is a dropdown, not a slider, because its values are fixed steps plus Unlimited.
- Key caps are 110 px wide; Figma uses narrow caps that fit the label.
- Switches show the off look in edit-mode renders; `SwitchToggle` updates in Play mode.
- Row separators and the Novecento font are not used.

## 10. Common mistakes

| Mistake | Result | Fix |
|---|---|---|
| Typing a key string instead of `SettingKeys.X` | The value is never found and silently uses the fallback | Add a constant and use it |
| Writing `SettingsStorage` directly from UI | No draft, no event, Cancel does nothing | Change `window.Draft`, let Apply write it |
| Creating `new InputSystem_Actions()` without `Attach` | Rebinds are ignored by that object | Call `KeybindSetting.Attach(actions.asset)` |
| Reading hotbar keys as `Key.Digit1`... | Rebinding the hotbar does nothing | Use `KeybindSetting.GetHotbarKey(slot)` |
| Adding a page without an order | Tab lands in an unexpected place | Pass `order` to `SettingsPage` |
