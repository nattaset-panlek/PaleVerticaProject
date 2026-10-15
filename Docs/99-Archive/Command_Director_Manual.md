# Character Command Director - Developer Manual

The `CommandDirector` is a robust, decoupled subsystem built into every `Character`. It acts as the ultimate gatekeeper for all entity actions (Movement and Skill Casting). By default, a Character's actions are driven by its baseline logic (`PlayerController` for the player, `AgentCore` for NPCs). 

However, any external script can request an **"Authority Override"** to hijack the character's body safely.

---

## The API Reference

All methods are accessed via `character.commandDirector`.

| Method | Description |
| :--- | :--- |
| `RequestOverride(object requester)` | Attempts to hijack the character. Returns `true` if successful. Once overridden, the baseline logic (e.g., WASD input) is completely ignored. |
| `ReleaseOverride(object requester)` | Returns control to the baseline logic. You must pass the exact same object reference that requested the override. |
| `ForceClearOverride()` | Forcefully removes any active override. Useful for resetting state on death or scene transitions. |
| `CommandMove(Vector3 direction, object sender)` | Moves the character. If overridden, it will only execute if `sender == currentOverrider`. |
| `CommandCastSkill(...)` | Casts a skill. If overridden, only the overrider can trigger this. |
| `CommandInterruptCast(object sender)` | Interrupts a currently casting skill. |

---

## Code Example: How to Use the System

Here is a practical example of a **Trap/Trigger Zone** that forces the player to walk to a specific point when they step on it.

```csharp
using UnityEngine;
using CoreSystem.Entity;

public class WalkTargetTrap : MonoBehaviour
{
    public Transform WalkTarget;
    private Character _trappedCharacter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.GetComponent<Character>();
        if (character != null)
        {
            // 1. Request Authority
            // 'this' refers to this specific Trap script instance
            if (character.commandDirector.RequestOverride(this))
            {
                _trappedCharacter = character;
            }
        }
    }

    private void Update()
    {
        if (_trappedCharacter != null)
        {
            // Calculate direction to the target
            Vector3 direction = (WalkTarget.position - _trappedCharacter.transform.position).normalized;
            float distance = Vector3.Distance(_trappedCharacter.transform.position, WalkTarget.position);

            if (distance > 0.5f)
            {
                // 2. Command the Character
                // We MUST pass 'this' as the sender to prove we have authority
                _trappedCharacter.commandDirector.CommandMove(direction, this);
            }
            else
            {
                // 3. Release Authority when done
                _trappedCharacter.commandDirector.ReleaseOverride(this);
                _trappedCharacter = null;
            }
        }
    }
}
```

> [!CAUTION]
> Always ensure you call `ReleaseOverride(this)` when your script is finished, destroyed, or if the character dies. If you fail to release it, the character will be permanently paralyzed!

---

## Gameplay Ideas & Application Concepts

With this generic override system, you can easily implement highly complex mechanics without touching the core `Character` or `PlayerController` scripts ever again.

### 1. Possession Skill (Soul Transfer)
A skill script that overrides two entities at once:
- **The Player**: The skill overrides the Player to lock out their movement, making the original body stand perfectly still in a vulnerable state.
- **The Target Enemy**: The skill overrides the Enemy. In its `Update()` loop, the skill reads the WASD input and calls `targetEnemy.commandDirector.CommandMove(input)`. 
- **The Result**: The player is physically playing as the enemy. 

### 2. Mind Control / Taunt (Status Effects)
A debuff applied to the player (e.g., `TauntDebuff : BuffBase`).
- On Apply: `character.commandDirector.RequestOverride(this)`.
- On Tick: `character.commandDirector.CommandMove(directionToBoss, this)`.
- On Remove: `character.commandDirector.ReleaseOverride(this)`.
- **The Result**: The player watches helplessly as their character runs straight into danger.

### 3. Cinematic / Cutscene Sequences
Using Unity's Timeline or a custom Cutscene Manager.
- When the boss spawns, the Cutscene Manager overrides the Player.
- It commands the player to walk to the center of the room and look up.
- Once the boss roar animation finishes, the manager releases the override, and the fight begins smoothly.

### 4. Environmental Interactions (Conveyor Belts / Wind)
Instead of modifying Unity physics directly, a conveyor belt can request an override, pushing the character strictly along the belt's path, temporarily preventing the player from fighting the current.

### 5. Escort / Hostage Quests
If you want an NPC to follow the player tightly, you don't need to rewrite their `AgentCore` logic. A `QuestManager` can simply override the NPC's `CommandDirector` and feed it a pathfinding route directly to the player's back.
