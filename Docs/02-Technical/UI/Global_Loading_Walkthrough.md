# Global Loading Walkthrough

`LoadingManager.LoadScene` is the one way to change scenes in game code. It loads the scene and shows the loading screen for the whole load, with the bar driven by the real load progress. It creates itself on first use and survives scene changes, so it needs no prefab or scene setup.

## Use it

```csharp
LoadingManager.LoadScene("Field");
```

Do not call `SceneManager.LoadScene` directly in game code, because that load would have no loading screen.

## Inputs

| Input | Meaning |
|---|---|
| `sceneName` | Scene name in Build Settings |
| `message` (optional) | Text above the bar. Defaults to "Loading" |
| `keepVisibleUntil` (optional) | Keeps the screen up after the scene loads until this returns true, for setup that runs after the load |

It returns `false` when the scene is not in Build Settings or another load is already running.

## Steps in order

1. Add the scene to Build Settings.
2. Call `LoadingManager.LoadScene` with its name.
3. If the new scene needs setup first, pass `keepVisibleUntil`. `MapTravel` does this to restore the player before the screen fades.

## Expected result

- The screen goes black at once and blocks all clicks.
- The bar follows Unity's real scene load progress.
- When the load and any `keepVisibleUntil` finish, the bar fills and the screen fades out.
- Map portals use it through `MapTravel`.

## Common mistakes

- Calling `SceneManager.LoadScene` directly: no loading screen.
- A `keepVisibleUntil` that never returns true: the screen stays up and blocks input.
- A Thai `message` needs a font with Thai glyphs. The overlay uses the TMP default font, so use English text.
