# System Menu & Settings Walkthrough

The in-game **System** menu (opened with **Esc**), the **Settings** window and a confirm dialog. All three use the Global Modal **MiniHeader** style and the `MH *` registry styles (see [MiniHeader Window Walkthrough](./MiniHeader_Window_Walkthrough.md)). Figma: WockSpace > `20 · Features` > **System Menu**, **Dark UI Mockups**. Added 2026-10-20.

## 1. How it opens

1. `UIManager.Update` reads Esc. If a modal is open, Esc closes it as before.
2. If no modal is open and no input field is focused, `UIManager` raises `UIManager.OnEscapeUnhandled`. `UIManager` stays the only place that reads Esc.
3. `SystemMenuController` (`[Runtime] SystemMenuController`, created at scene load by `Create()`, self-registers `Instance`) listens to that event and toggles the menu.
4. Esc while the menu is open closes it (the menu is a modal).

## 2. Code

| Type | File | Role |
|---|---|---|
| `SystemMenuController` | `Assets/Script/UI/SystemMenu/SystemMenuController.cs` | Runtime object; Esc -> `SystemMenuWindow.Toggle()` |
| `SystemMenuWindow` | `UI/SystemMenu/SystemMenuWindow.cs` | Modal "System Menu", 300 px wide, fits its height; menu rows (`MH Menu Row`) and separators |
| `SystemMenuItem` | `UI/SystemMenu/SystemMenuItem.cs` | Label + action + enabled; an item without an action is disabled |
| `SettingsWindow` | `UI/SystemMenu/SettingsWindow.cs` | Modal "Settings"; one tab per `SettingsRegistry` page (Graphics / Audio / Gameplay / Controls today), footer Default / Cancel / Apply. Pages and options are data: see [Settings Walkthrough](./Settings_Walkthrough.md) |
| `SystemConfirmDialog` | `UI/SystemMenu/SystemConfirmDialog.cs` | Modal "System Confirm", 440 wide; Cancel + Confirm (Danger or Primary), centred |
| `SettingsDraft` | `CoreSystem/Settings/SettingsDraft.cs` | Generic: `Capture()`, `Clone()`, `Get/Set(key)`, `ResetPage(pageId)`, `Differences(other)` (names of changed rows), `Apply()`; holds key bindings too |
| `DisplaySetting` | `CoreSystem/Settings/DisplaySetting.cs` | Resolution, display mode, VSync, frame limit, quality (PlayerPrefs `Setting.Display.*`); `Apply()` on change and at scene load |
| `AudioSetting` / `AudioChannel` | `CoreSystem/Settings/` | Master / Music / Effects / Voice / Interface volume 0-1 (PlayerPrefs `Setting.Audio.*`) plus a mute per channel; Master drives `AudioListener.volume`; `GetEffectiveVolume(channel)` and `OnChanged(channel)` for a future audio system |
| `UIMiniHeaderStyleRegistry` | `CoreSystem/UI/Manager/UIMiniHeaderStyleRegistry.cs` | MH style names, colours, `Prepare(modal, title)` (MiniHeader + dark theme) |
| UIBuilder | `UIBuilderContext.Slider(...)`, `UIBuilderContext.Dropdown(...)` | New registry-styled controls (used with `MH Slider` / `MH Dropdown`) |

## 3. Menu items

| Item | Action |
|---|---|
| Character / Equipment / Inventory / Skills / Passive Tree | close the menu, `UIManager.Open("CharacterStatus" / "CharacterEquipment" / "Inventory" / "CharacterSkillManage" / "CharacterSkillTree")` |
| Keybind | open `SettingsWindow` on the Controls page |
| Settings | open `SettingsWindow` |
| Save Game | `SaveStorage.SaveSlot(0, PlayerCore.SaveData())`; disabled without a player |
| Load Game | confirm, then `PlayerCore.LoadData(SaveStorage.LoadSlot(0))`; disabled when slot 0 is empty |
| Exit Game | confirm (Danger), then quit (stops Play mode in the editor) |

To add an item, add a `SystemMenuItem` to `SystemMenuWindow.Items` (null between groups is a `SystemMenuItem.Separator`).

## 4. Settings tabs

Pages, rows, storage, Apply / Cancel / Default and key binding are described in the [Settings Walkthrough](./Settings_Walkthrough.md).

## 5. Build and check

1. Kit + registry styles: **Pale Vertica > UI > Build MiniHeader Kit Only** (prefabs and registry, Global Modal untouched), or **Build MiniHeader Window Style** (also rebuilds the Mini Header in Global Modal), or `CoreSystem.Editor.UI.MiniHeaderUIKitGenerator.BuildKit` + `RegisterKit`, or `BuildKitBatch` with Unity closed. `RegisterKit` only adds or updates the `MH *` entries in `UIElementRegistry`.
2. Snapshots: `system_menu`, `settings_1_Graphics` ... `settings_4_Controls`, `system_confirm_exit` in `Logs/UIDebug/`. Expected: 0 issues each.
3. Tests: `SystemMenuTests` (menu order, settings registry, storage, draft, key binding).

## 6. Known gaps

- Labels use the theme font in mixed case; Figma uses Novecento uppercase.
- The switch shows the off look in edit-mode renders (`SwitchToggle` updates in Play mode).
- Save / Load use slot 0 only; a slot list window (Figma mockup 4) is not built yet.
- No audio system exists yet: only Master changes what you hear.

## 7. Common mistakes

| Mistake | Result | Fix |
|---|---|---|
| Reading Esc in another script | menu opens while a modal closes | Listen to `UIManager.OnEscapeUnhandled` instead |
| Registry styles not registered | buttons and controls fall back to default looks | Run `RegisterKit` |
| Opening a window without closing the menu | the menu's modal overlay blocks the window | Use `SystemMenuWindow.OpenWindow` (closes first) |
