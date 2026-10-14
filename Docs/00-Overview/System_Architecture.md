# ⚙️ System Architecture & Script Dictionary (Realm of Hero)

This document provides a high-level overview of the architectural layers and a dictionary of the core scripts used in the project.

---

## 🏗️ PART 1: High-Level Architectural Layers

The system is organized into several distinct layers with specific responsibilities.

### 1.1 Data & Persistence Layer (The Foundation)
The source of truth for all game variables, items, and skill configurations.
- **[GameData] (Static)**: Global access point for `PlayerData`. Maintains the current session state.
- **[PlayerData]**: Central hub for player instances (Inventory, Equipment, Skills, Character).
- **[Database]**: The "bridge" between JSON and ScriptableObjects at runtime.
- **[Skill/ItemDatabaseModels]**: C# structures for JSON parsing.

### 1.2 Core Entity System (The Actors)
Everything that lives, moves, or reacts in the world.
- **[IEntity]**: Interface for interactable objects (Health, Stats, Position).
- **[Character]**: Base class for all units, managing the entity lifecycle.
- **[PlayerCharacter]**: Subclass of `Character` with input/UI processing.
- **[AttributeSystem]**: Logic for numerical values and stat modifiers.

### 1.3 Skill System Pipeline (The Actions)
The engine that processes player intent into visual/mechanical results.
- **[SkillCaster]**: Manages the skill state (Idle → Casting → Processing → Recovery).
- **[ActiveSkill/SkillInstance]**: Skill data versus the runtime instance of a skill.
- **[Targeting/Effect Logic]**: Modular blocks to determine targets and effects.
- **[VisualManager]**: Orchestrates VFX and indicators.

### 1.4 UI & Interface Layer (The Representation)
How the player communicates with the core systems.
- **[UI Managers]**: Singletons (Inventory, ContextMenu, DamageText) linking data to pixels.
- **[Windows/Fragments]**: Individual widgets and visual layout management.

---

## 🏥 PART 2: The "Nervous System" (Data Flow)

1. **UI Interaction**: `DragItem` → `InventoryManager` → `EquipmentManager` → `AttributeSystem`.
2. **Stat Update**: `AttributeSystem` change → `OnStatusChanged` → `HPBarUI` refresh.
3. **Skill Casting**: `Input` → `SkillCaster` (MP Check) → `Casting Coroutine` → `Execute`.
4. **Damage Delivery**: `EffectLogic` → `Payload` → `ReceiveDamage` → `DamageTextManager`.

---

## 📖 PART 3: Core Script Dictionary

| Script | Responsibility | Flow / Notes |
| :--- | :--- | :--- |
| **GameData.cs** | Global Access | Main entry point for player data. |
| **AttributeSystem.cs** | The Calculator | Stores and calculates all `Attribute` values. |
| **SkillCaster.cs** | Execution Manager | Handles the casting lifecycle and costs. |
| **Pathfinder2D.cs** | Navigation Brain | Uses A* for 2D pathfinding. |
| **InventoryManager.cs** | Logical Backbone | Handles item addition, sorting, and searching. |
| **VisualManager.cs** | VFX Spawner | Connects active effects to world particles. |

---

## 🔄 PART 4: Execution Example (Fireball Logic)

1. **Input**: Click detected.
2. **Order**: `SkillCaster.CastSkill(FireballID)`.
3. **Check**: Verify MP and Cooldown.
4. **Preview**: `SkillIndicatorManager` shows radius.
5. **Charge**: VCT/FCT delay.
6. **Execute**: `SpawnProjectileEffect` creates projectile.
7. **Impact**: `OnTriggerEnter` hits target.
8. **Damage**: `DamageEffect` calculates damage via INT scaling.
9. **Display**: `DamageTextManager` shows floating text.

---

## 🛠️ PART 5: Modularity & Extension

- **Add a Stat**: Add entry to `AttributeType` enum; UI updates automatically.
- **Add a Skill**: Create `ActiveSkill` SO → Assign logic blocks → Add to DB.
- **Add an Item**: Create `Item` or `Equipment` SO → Add modifiers.

---
*Maintained by the Development Team*
