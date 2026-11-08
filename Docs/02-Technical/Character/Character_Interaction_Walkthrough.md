# Character Interaction Core Walkthrough

Status: **implemented 2026-10-19**. Code: `Assets/Script/CoreSystem/Interaction/`. Tests: `Assets/Script/Editor/Tests/InteractionCoreTests.cs`.

## 1. What it is

One shared core for "a character does something to another character" (Inspect, Attack, Talk, Trade, Follow Me...). Any system adds options through a provider; the core collects them, walks the actor into range, then runs the option.

```
 Right-click (PlayerCore) ─► PlayerInteraction.BuildOptions
                                   │
                                   ▼
 InteractionCore.GetOptions(actor, target, holder) ─► InteractionRegistry.Collect
                                   │                       │ asks every IInteractionProvider
                                   ▼                       ▼
 ContextMenu click ─► InteractionCore.Request(...)   List<InteractionOption> (sorted by Priority)
                                   │
              in range? ── yes ──► option.Execute(context) ─► OnExecuted
                  │ no
                  ▼
        pending InteractionRequest: each frame CommandMove toward target (through ControlSystem)
        ─► in range: Execute │ target gone/dead, control lost, timeout, new input: Cancel ─► OnCancelled
```

| Type | Role |
|---|---|
| `InteractionContext` | `Actor`, `Target` (both `Character`), `Holder` (who commands the actor: `PlayerCore`, `AgentCore`...), `IsSelf` |
| `InteractionOption` | `Id` (unique, first provider wins), `Label`, `Execute(context)`, `Range` (`0` = anywhere, no walking), `Priority` (higher = higher in the menu) |
| `IInteractionProvider` | `Collect(context, options)`: add options that fit this actor/target |
| `InteractionRegistry` | Static list of providers. `Register`, `Unregister`, `Collect` (skips invalid options and logs provider exceptions without breaking others) |
| `InteractionRequest` | Pure state for one pending interaction. `Step(...)` returns `Approach`, `Execute` or `Cancel`; default timeout 10 s |
| `InteractionCore` | Single `[Runtime] InteractionCore` manager. `Ensure()`, `GetOptions`, `Request`, `Cancel`, `IsApproaching`, `GetPending`. Static events `OnReady`, `OnApproachStarted`, `OnExecuted`, `OnCancelled(request, reason)` |
| `PlayerInteraction` | The built-in provider (Inspect 1000, Attack 900, Follow Me -100, Stop Following -110) and the right-click menu. Registered by `PlayerCore` |

Rules the core enforces:
- A target with `Character.IsInteractable == false` gives no options (the actor can still inspect itself).
- Walking uses `actor.Control.CommandMove(direction, holder)`, so it obeys control lanes. If the holder loses control while out of range, the request is cancelled.
- One pending request per actor. A new request replaces the old one (`Replaced`). Player movement input or a left-click attack target cancels it.
- If the actor or target leaves `CharacterRegistry`, dies or is disabled, the request is cancelled (`TargetInvalid`).

## 2. Add a new interaction (example: Talk)

**Inputs:** a stable option id, a label, a range (world units), a priority, and the action to run.

1. Create a class in the system that owns the feature (for example `CoreSystem/Dialogue/DialogueInteractionProvider.cs`), implementing `IInteractionProvider` with the five `#region` blocks.
2. In `Collect`, check the context and add options:

```csharp
public void Collect(InteractionContext context, List<InteractionOption> options)
{
    if (context.IsSelf || context.Target.IsDead || string.IsNullOrEmpty(context.Target.NpcId)) return;
    options.Add(new InteractionOption("dialogue.talk", "Talk", ctx => DialogueWindow.Open(ctx.Target), range: 1.5f, priority: 100));
}
```

3. Register the provider once from the system's own lifetime (its manager's `Awake`, or `OnEnable`), and unregister it in `OnDestroy`/`OnDisable`:

```csharp
InteractionRegistry.Register(_talkProvider);
InteractionRegistry.Unregister(_talkProvider);
```

4. Press Play, right-click an NPC far away, pick **Talk**. The player walks to within 1.5 units, stops, and the window opens. Moving with the keyboard on the way cancels it.

**Expected result:** the new option appears in the right-click menu under Inspect/Attack (by priority), and no code in `PlayerCore` or `PlayerInteraction` changed.

## 3. Use it from code (AI, cutscenes, quests)

```csharp
var options = InteractionCore.GetOptions(actor, target, AgentCore.Instance);
var talk = options.Find(o => o.Id == "dialogue.talk");
if (talk != null) InteractionCore.Ensure().Request(actor, target, talk, AgentCore.Instance);

InteractionCore.OnExecuted += request => { };
InteractionCore.OnCancelled += (request, reason) => { };
```

- Pass as `holder` the object that holds the actor's control lease, otherwise the walk cannot move the actor and the request is refused (out of range) or cancelled (`ControlLost`).
- UI that shows "walking to X" listens to `OnApproachStarted` / `OnExecuted` / `OnCancelled`; it never polls `IsApproaching` in `Update`.

## 4. Verify

- **Window > General > Test Runner > EditMode**, run `InteractionCoreTests` (registry dedupe, priority order, invalid options, provider exceptions, range edge, approach/execute/cancel/timeout).
- Play mode: right-click a hostile (Inspect, Attack), a friendly agent (Inspect, Follow Me, Stop Following), yourself (Inspect only).

## 5. Common mistakes

- **Reusing an id** across providers: the second option is silently dropped. Prefix ids with the system name (`dialogue.talk`, `shop.trade`).
- **Forgetting `Unregister`**: after a scene reload the old provider still adds options pointing at destroyed objects.
- **Range 0 for something that needs proximity**: `0` means "anywhere", so the action runs instantly from across the map.
- **Wrong holder**: passing `PlayerCore.Instance` for an AI actor (or `null` when a lease exists) makes the request fail.
- **Doing the work inside `Collect`**: `Collect` runs every time a menu opens; only the `Execute` callback should change state.
- **Putting interaction code on `Character` or in `PlayerCore` input**: add a provider instead.
- The old `PlayerInteraction.OnBuildOptions` event was removed; move any such code into a provider.
