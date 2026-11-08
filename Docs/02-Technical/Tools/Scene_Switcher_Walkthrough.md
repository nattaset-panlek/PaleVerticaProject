# Scene Switcher Walkthrough

Package **Scene Switcher** (`Packages/com.axnise.sceneswitcher`, 1.0.0, author Axnise). It shows a scene dropdown in the main toolbar next to the Play, Pause and Step buttons. Added 2026-10-20. It replaces `Assets/Script/Editor/MainToolbarSceneSwitcher.cs`, which used reflection on Unity's old internal toolbar and stopped working with the Unity 6 toolbar.

## 1. Where it is

| Entry | How to open |
|---|---|
| Toolbar dropdown | Main toolbar, middle zone beside Play / Pause / Step. It shows the active scene's name and the scene icon |
| Scene view overlay | Scene view > overlay menu (`` ` `` key or the three dots) > **Scene Switcher** (hidden by default) |
| Window | **Window > Axnise > Scene Switcher** (search field, additive "+" button) |

If the dropdown is not visible, right-click an empty part of the main toolbar and turn on **Scene Switcher**. You can drag it to another spot in the toolbar.

## 2. Menu contents

1. **Build Profile:** scenes in the build list, with their index.
2. **Recent:** the last 6 scenes opened in this project (stored per project in EditorPrefs).
3. **All Scenes:** every `.unity` under `Assets/`, as sub-menus that follow the folders.
4. **Open Additive:** the same tree, opening the scene additively.
5. **Ping Active Scene**, **Open Scene Switcher Window**, **Clear Recent**, **Refresh Scene List**.

The active scene has a check mark. Before a single-scene open, Unity asks to save modified scenes; Cancel keeps the current scene. In Play mode the dropdown is disabled.

## 3. Code

| File | Role |
|---|---|
| `Editor/SceneSwitcherRegistry.cs` | Scene lists (cached, cleared on `projectChanged` / build list change), recent scenes, `OpenScene`, `OnChanged` |
| `Editor/SceneSwitcherMenu.cs` | Builds the `GenericMenu` shared by the toolbar and the overlay |
| `Editor/SceneSwitcherToolbar.cs` | `[MainToolbarElement("Axnise/Scene Switcher", defaultDockPosition = Middle)]` returns a `MainToolbarDropdown`; calls `MainToolbar.Refresh` on `OnChanged` |
| `Editor/SceneSwitcherOverlay.cs` | Scene view toolbar overlay |
| `Editor/SceneSwitcherWindow.cs` | Dockable window |

The package needs Unity 6000.3 or later (the public `MainToolbar` API). It has one editor-only assembly, `Axnise.SceneSwitcher.Editor`, and no runtime code. To use it in another project, copy the `com.axnise.sceneswitcher` folder into that project's `Packages/`.

## 4. Expected result

- The toolbar shows the scene icon and the active scene name beside Play.
- Opening a scene from the menu updates the label at once and adds the scene to Recent.
- New or moved scenes appear in the menu after the import (or after **Refresh Scene List**).

## 5. Common mistakes

| Mistake | Result | Fix |
|---|---|---|
| Dropdown missing | It was hidden in the toolbar | Right-click the toolbar and turn on Scene Switcher |
| Looking for the old window under Window > Pale Vertica | Menu not found | It is now **Window > Axnise > Scene Switcher** |
| Expecting scenes from packages | Only `Assets/` scenes are listed | Open package scenes from the Project window |
