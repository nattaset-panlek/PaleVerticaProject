# MiniHeader Window Walkthrough

A second window style for the Global Modal, matching the Figma **System Menu / Dark** design, plus a kit of Dark prefabs (buttons, rows, toggles, slider, dropdown...). The normal style is unchanged: every window that does not ask for `MiniHeader` looks and behaves exactly as before. Added 2026-10-20.

Figma references (page **WockSpace**):
- **System Menu** (`20 · Features`): the menu (Dark).
- **Dark UI Kit** (`00 · Foundations`): 29 components.
- **Dark UI Mockups** (`20 · Features`): 6 example screens (Settings Graphics / Audio / Gameplay, Keybind, Save Load, Exit dialog).

## 1. Use it

```csharp
UIGlobalModal.GetOrCreate("Settings")
    .SetStyle(UIModalStyle.MiniHeader)
    .SetThemeMode(UIThemeMode.Dark)
    .SetTitle("Settings")
    .Build(ui => { /* content */ });
```

- `SetStyle(UIModalStyle.Default)` switches back. The style is kept per window instance (`GetOrCreate` reuses it).
- `SetStyle` does not change the theme. Call `SetThemeMode(UIThemeMode.Dark)` so UIBuilder content uses the dark theme.
- `SetTitle`, `SetCloseButtonVisible`, `SetDismissable` and `OverrideCloseAction` work for both styles. The Mini Header close button uses the same close action.

## 2. What MiniHeader changes

| Part | Default | MiniHeader |
|---|---|---|
| Header | `Top Navigator` (94 px, title tag, close 56 px) | `Mini Header` (33 px, Kanit Bold 16 uppercase title, 18 px close icon) |
| Background | skin sprite (`UI_Modal_Skill_Window` / `UI_Modal_Window`) | `UI_Global_Window_Dark` sliced (border L24 B8 R128 T36; the header band and topographic pattern are part of the sprite) |
| Content insets | prefab values | left 14, right 14, top 45, bottom 14 (`UIModalStyleView._miniContentInsets`) |

Code:
- `UIModalStyle` (`CoreSystem/UI/Manager/UIModalStyle.cs`): `Default`, `MiniHeader`.
- `UIModalStyleView` (`CoreSystem/UI/Manager/UIModalStyleView.cs`), on the Global Modal root: toggles the parts, swaps the background, sets the insets, and restores the defaults when you switch back.
- `UIGlobalModal`: has `_style` (default `Default`) and `_styleView` fields, plus `SetStyle`. `ApplySkin` leaves the background alone while the style is `MiniHeader`.

## 3. Kit prefabs

Folder: `Assets/Prefab/Game Page/Global/UIElement/MiniHeader/`. They are not registered in `UIElementRegistry` yet, so UIBuilder style names do not use them; instantiate them, or register them when a screen needs them.

| Prefab | Notes |
|---|---|
| MH Button Primary / Secondary / Danger | `UI_Primary_Button.png` (the in-game pill) tinted white / #1E1E1E / #3A1515, Noto Sans SemiBold 16, 145 x 40. Disabled = Button disabled tint (40 %) |
| MH Menu Row | 288 x 30, hover 6 % white, pressed cyan; `Selected` child (cyan outline) is off by default |
| MH Switch Toggle | uses `SwitchToggle` (moves the knob and colours the track in Play mode) |
| MH Checkbox | Toggle, cyan checkmark |
| MH Slider | track #2A2A2A 4 px, cyan fill, white knob with cyan ring |
| MH Dropdown | TMP_Dropdown, dark field with #2E2E2E outline, cyan item highlight |
| MH Input Field | TMP_InputField, placeholder "Search..." |
| MH Scrollbar | 6 px, #1E1E1E track, #4A4A4A handle |
| MH Key Cap | 34 x 26 |
| MH Tab | Toggle; the cyan underline is the toggle graphic (use a ToggleGroup) |
| MH Foldout Header | Button with arrow (rotate the arrow for collapsed) |
| MH Container | rounded group box: title, separator, `Body` column |
| MH Tooltip | accent bar, title, wrapping description, fits its height |
| MH Separator | 1 px, 8 % white |
| MH Setting Row | label (flexible) + `Control` slot on the right + divider |

## 3b. Parameters (one place)

Every colour and size of the MiniHeader style lives in `UIMiniHeaderStyleRegistry` (`CoreSystem/UI/Manager/UIMiniHeaderStyleRegistry.cs`) as `public static` fields:

- **Colours:** `Body`, `Text`, `Sub`, `Disabled`, `Accent`, `Field`, `FieldLine`, `ListBackground`, `Placeholder`, `Raised`, `Inner`, `InnerLine`, `Track`, `SwitchOn`, `BoxLine`, `ScrollTrack`, `ScrollThumb`, `KeyCap`, `KeyCapLine`, `TooltipBackground`, `SeparatorColor`, and the button tint and ink pairs `ButtonPrimary*`, `ButtonSecondary*`, `ButtonDanger*` (all black and white since 2026-10-20).
- **Sizes:** `ButtonSize` (181 x 55, the minimum), `HeaderHeight`, `MenuRowHeight`, `TabHeight`, `ContentInsets` (left, right, top, bottom).
- **Window values:** `SystemMenuWidth`, `SettingsWindowSize`, `ConfirmWindowSize`, `SettingsRowHeight`, `SettingsControlWidth`, `FooterGap`, `ConfirmButtonGap`.
- **Style names:** `ButtonPrimary`, `MenuRow`, `Slider` and so on (const).

Where they take effect:
- **Prefab and registry values** (colours, `ButtonSize`, row and tab heights): baked when the kit is built, so rebuild the kit and run `RegisterKit` after changing them.
- **Window values** (window sizes, gaps, row height, control width): read every time a window is built, so they apply at once.
- **`ContentInsets` and `HeaderHeight`:** written into Global Modal by `AddMiniHeaderToGlobalModal`; rebuild the Mini Header for them to take effect.

## 4. Rebuild

- Menu **Pale Vertica > UI > Build MiniHeader Window Style**, or with Unity closed `Tools/FigmaToUnity/unity_batch.sh CoreSystem.Editor.UI.MiniHeaderUIKitGenerator.BuildKitBatch`.
- It only replaces the `Mini Header` child and the `UIModalStyleView` links in Global Modal, and rebuilds the `MH *` prefabs. Everything else in Global Modal is kept (checked 2026-10-20: 0 lines removed from the prefab).

## 5. Check it

- `UISnapshotRenderer` renders `miniheader_settings` (720 x 500, Figma mockup 1 rebuilt from the kit with `MiniHeaderUIKitGenerator.BuildSampleSettings`) to `Logs/UIDebug/miniheader_settings.png`. Expected: 0 issues.
- All other snapshots keep their issue counts (the default style is unchanged). EditMode tests: 242/242.

## 6. Known differences from Figma

- **Active tab:** in Figma it also has a soft cyan fill; Unity shows only the underline.
- **Checkbox:** Unity uses a cyan checkmark on a dark box; Figma uses a filled cyan box with a black check.
- **Danger button:** the art is tinted dark red, so its notched inner line is dark red; in Figma that line is bright red.
- **Switch Toggle:** the knob position and track colour update in Play mode only (`SwitchToggle.Awake`), so an edit-mode render shows the off look unless it is set by hand (the sample does).

## 7. Common mistakes

| Mistake | Result | Fix |
|---|---|---|
| `SetStyle(MiniHeader)` without `SetThemeMode(Dark)` | dark window with light UIBuilder content | Set the dark theme too |
| Editing the `Mini Header` object by hand | lost on the next rebuild | Change `MiniHeaderUIKitGenerator` instead |
| A Global Modal copy without `UIModalStyleView` | warning "has no UIModalStyleView; style MiniHeader is ignored" | Rebuild, or add the view and its links |
| Expecting UIBuilder `Button(...)` to use the MH prefabs | it uses the registry styles | Register the MH prefabs as new style names first |
