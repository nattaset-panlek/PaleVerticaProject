# Global Visualizer Showcase Walkthrough

Verified: **2026-10-18**, package version **1.0.0**, author **Axnise**. Package guide: [Global_Visualizer_Walkthrough.md](./Global_Visualizer_Walkthrough.md).

The Showcase is a ready-made scene that ships with the package as a sample. It teaches the package, lets people test every setting, and compares Global Visualizer with a normal ScrollRect.

- **Look:** stock Unity uGUI (default sprites and the built-in font), so it looks the same in any project.
- **Dependencies:** none outside the package and uGUI. Input System is optional.

## Where it is

| What | Path |
|---|---|
| Sample source | `Packages/com.globalvisualizer.core/Samples~/Showcase/` |
| Scene | `Samples~/Showcase/GlobalVisualizerShowcase.unity` |
| Entry component | `ShowcaseApp` (on the "Global Visualizer Showcase" object in the scene). It builds the whole UI at runtime. |
| Import in a project | **Window > Package Manager > Global Visualizer > Samples > Showcase > Import**. The files land in `Assets/Samples/Global Visualizer/1.0.0/Showcase/`. |

## Inputs

- A project with uGUI (`com.unity.ugui`) and the Global Visualizer package.
- Optional: the Input System. With it, the scene creates an `InputSystemUIInputModule`; without it, a `StandaloneInputModule`.

## Steps

1. **Import the sample.** Package Manager > Global Visualizer > Samples > **Showcase** > Import.
2. **Open the scene:** `Assets/Samples/Global Visualizer/1.0.0/Showcase/GlobalVisualizerShowcase.unity`.
3. **Press Play.** The header holds five tabs. The right side of the header shows live FPS, the average frame time and the worst frame.
4. **Welcome.**
   - Explains the idea in one screen.
   - Has a live list of 100,000 items. The blue range bar and the counters show that only about 20 GameObjects exist.
   - Has cards that open the other pages, and a quick-start snippet.
5. **Learn.** Nine lessons. Each lesson has:
   - an explanation;
   - "Try it" goals that tick themselves off when you do them;
   - buttons that drive the live demo;
   - the code for what you just did.

   | Lesson | Covers |
   |---|---|
   | 1 | Binding data: `SetItems`, `IVisualItem<T>`, 100,000 items |
   | 2 | `NotifyInserted` / `NotifyRemoved` / `NotifyChanged` / `NotifyReset` |
   | 3 | Vertical, Horizontal, Grid and Flow layouts |
   | 4 | Size modes: Fixed, Provider and Measure |
   | 5 | Selection, click, double click and right click |
   | 6 | Keyboard and gamepad focus, Submit, `OnNavigateOut` |
   | 7 | `ScrollTo`, `EnsureVisible`, Snap To Items |
   | 8 | Empty placeholder |
   | 9 | Using it in your own project |

   The lesson list shows `[x]` for finished lessons. The event log under the demo prints every event the view raises.
6. **Playground.** Every setting is live.

   | Group | Settings |
   |---|---|
   | Data | 0 / 10 / 1k / 10k / 100k items; insert, remove, change, shuffle |
   | Layout | Kind, item size, spacing, padding, cross size and alignment, grid constraint |
   | Size mode | Fixed, Provider, Measure |
   | Interaction | Selection mode, Handle Input, Focus On Click, Snap, empty placeholder, buffer pixels |
   | Scrolling | `ScrollTo`, `EnsureVisible`, `Focus`, `Select` by index |

   The stats line shows visible range, alive and created views, selection, focus and content size.
7. **Benchmark.**
   - Pick 1,000 / 5,000 / 10,000 / 25,000 items and List or Grid, then press **Run benchmark**.
   - Both contenders get the same items and the same item prefab. They run one after the other, alone on screen, with vSync off:
     - **Naive:** ScrollRect + LayoutGroup + ContentSizeFitter with one GameObject per item. A change destroys and recreates every item.
     - **Global Visualizer.**
   - The table compares these (lower is better), with bars and an "Nx better" verdict per row:
     - build time;
     - GameObjects;
     - memory (native + managed);
     - managed heap growth while building;
     - average and worst frame while scrolling;
     - inserting one item, both with a rebuild and with best-case naive code.
   - Garbage while scrolling is not a benchmark row: in the Editor every frame allocates for its own reasons. The Self Test measures the list's scrolling code alone.
   - **Build both** fills both lists so you can scroll them side by side and watch the FPS.
8. **Self Test.** **Run all checks** runs 13 checks against real views in the sandbox panel and shows PASS or FAIL with details:
   - only visible items exist;
   - views are reused;
   - zero garbage while scrolling;
   - recycled views show the right data;
   - `ScrollTo`;
   - selection across insert and remove;
   - grid placement and flow wrapping;
   - Provider and Measure sizes;
   - the empty placeholder;
   - grid keyboard navigation;
   - pointer click and double-click events.

   Run it in player builds too (IL2CPP, mobile) to check the guarantees on your target.

## Expected result

- **Welcome:** 100,000 items with about 20 GameObjects alive, whatever the scroll position.
- **Self Test:** "13 of 13 passed".
- **Benchmark at 5,000 rows in the Editor:** results from the verification run on 2026-10-18 are in the next section. The visualizer wins build time, GameObjects, memory and change cost by large factors. Numbers in a player build are lower for both sides; compare the ratios.

## Verification run (2026-10-18)

Run in a blank Unity 6000.4.8f1 project with only uGUI, the Test Framework and this package (no Input System). The PlayMode test:

- opened the scene;
- checked the header layout;
- visited every page and every lesson;
- ran the Self Test (13 of 13 passed, including 0 bytes of heap growth over 5,000 scroll steps);
- ran a 5,000-row List benchmark;
- saved screenshots.

The package's own EditMode tests passed 66 of 66 in the same project. Benchmark, Editor Play mode, Ryzen 7 5800H / RTX 3050 Ti:

| Metric | Naive | Global Visualizer | Result |
|---|---|---|---|
| Build (create + first layout) | 3,786 ms | 17 ms | 217x faster |
| GameObjects in the list | 25,000 | 85 | 294x fewer |
| Memory added (native + managed) | 496 MB | about 0 MB | |
| Managed heap growth while building | 437 MB (a GC ran) | 0 KB | |
| Average frame while scrolling | 114.5 ms | 5.2 ms | 22x faster |
| Worst frame while scrolling | 142.5 ms | 8.8 ms | 16x faster |
| Insert 1 item: rebuild the list | 5,603 ms | 2.0 ms | 2,817x faster |
| Insert 1 item: best-case naive | 501 ms | 2.0 ms | 252x faster |

## Common mistakes

- **Pressing Play in an empty scene after importing.** Open `GlobalVisualizerShowcase.unity` first. To build the showcase in another scene, add `ShowcaseApp` to an empty GameObject. Without the scene's sprite references the controls are flat rectangles, but everything works.
- **Benchmarking in the Editor and quoting the absolute numbers.** The Editor is much slower than a player. Quote the ratios, or make a build and run the Benchmark there.
- **Running the 25,000-item benchmark on a slow machine.** The naive side may freeze for several seconds. That freeze is the point of the comparison, not a bug.
- **Clicking list rows while the WASD keys are also bound to UI navigation.** The Learn and Playground demos keep Focus On Click on to teach navigation. Turn it off in the Playground to see the difference.
- **Editing the generated UI in the scene.** There is nothing to edit: the UI is built by code at Play time. Change the `Showcase*Page` scripts instead.
