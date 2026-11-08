# Title Menu Walkthrough

The Title scene has Continue, New Game and Exit. It runs without `[Game Core]`, so there is no Player or game UI there.

## First-time setup

Run **Pale Vertica > World > Build Title Scene** once. It creates `Assets/Scenes/Title.unity` and makes it the first scene in Build Settings. It refuses to run if the scene exists. `Title` is already listed in `Scenes Without Core`.

## Inputs

| Where | Field | Meaning |
|---|---|---|
| `Resources/GameCoreSettings.asset` | `New Game Scene` | Map loaded by New Game (default `Town`) |
| `Title Canvas` > `TitleMenuView` | Buttons | Continue, New Game and Exit |

## Steps in order

1. Open `Title.unity` and press Play.
2. New Game loads `New Game Scene` with empty player data.
3. In a game map, press Esc and choose **Save Game**.
4. Press Esc and choose **Return to Title**.
5. Continue loads the map you saved in and restores the player.

## Expected result

- Continue is disabled when there is no save in the quick save slot.
- Every button goes through the loading screen.
- Continue places the player at the map's `start` point. The exact position is not saved.
- Returning to Title removes `[Game Core]` and clears player data in memory.

## Restyle it

Edit `Title Canvas` in `Title.unity`. The look is a placeholder. Keep `TitleMenuView`'s three button references.

## Common mistakes

- `New Game Scene` missing from Build Settings: an error is logged and nothing happens.
- Removing `Title` from `Scenes Without Core`: the player appears on the title screen.
- Windows from `UIManager`, such as Settings, are not available here because `UIManager` is in `[Game Core]`.
- Saves made before this feature have no map name, so Continue uses `New Game Scene`.
