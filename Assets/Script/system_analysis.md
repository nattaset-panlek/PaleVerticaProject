# Realm of Hero: System Architecture Analysis

This document provides a comprehensive overview of the gameplay systems implemented in the project. It is designed to serve as a reference for Game Design and further development.

---

## 1. Core Entity System
The game uses a hierarchical entity structure to handle different types of characters while sharing common logic.

- **Base Character (`Character`)**: The foundation for all living entities. It manages:
  - **Identity**: Unique Character ID linked to a `CharacterModel` database.
  - **Systems Integration**: Holds references to Attribute, Movement, Skill, and Experience systems.
  - **Equipment Slots**: 8 Equipment slots and 4 Costume slots.
  - **Vitality**: Handles HP/MP recovery logic and stun states.
  - **Interactions**: Events for taking damage and hitting targets.
- **Player Character (`PlayerCharacter`)**: Extends the base character with player-specific features:
  - **Management Binding**: Binds to global managers (Inventory, Equipment, Skills).
  - **Debug Interface**: A robust in-game debug menu for testing all systems (Inventory, Skills, Stats, etc.).
  - **Passive Progression**: Tracks unlocked nodes in the passive skill tree.
- **Non-Player Character (`NonPlayerCharacter`)**: Base for NPCs, supporting commands like `MoveTo` and `Follow`.
- **Enemy Character (`EnemyCharacter`)**: Specialized NPC for combat encounters.

---

## 2. Attribute & Stat System
A sophisticated stat system inspired by classic RPGs, featuring core attributes that derive complex combat stats.

### Core Attributes
1. **STR (Strength)**: Increases Physical ATK and Weight Limit.
2. **AGI (Agility)**: Increases Flee, Status DEF, and ASPD (Attack Speed).
3. **VIT (Vitality)**: Increases Max HP, HP Recovery, Status DEF, and healing item effectiveness.
4. **INT (Intelligence)**: Increases MATK (Magic Attack), Max MP, MP Recovery, and Status DEF.
5. **DEX (Dexterity)**: Increases Hit, ASPD, and slightly increases ATK/MATK.
6. **LUK (Luck)**: Increases Critical Rate, Perfect Dodge, Status Damage, and slightly increases multiple other stats.

### Derived Combat Stats
- **Offensive**: ATK, MATK, Hit, Critical Rate, Status Damage.
- **Defensive**: Max HP/MP, Flee, Status DEF, Crit Resistance, Perfect Dodge.
- **Utility**: Move Speed, ASPD, Weight Limit, HP/MP Recovery.

### Modifier System
- Uses an "Additional Value" system with prioritized calculation orders (e.g., Base -> Flat Bonus -> Percentage Multiplier).
- Supports tagging modifiers (e.g., "Passive" from skill tree) for easy management and recalculation.

---

## 3. Combat & Skill System
A data-driven skill system allowing for complex behaviors and scaling.

- **Skill Types**:
  - **Active Skills**: Manually triggered.
  - **Toggle Skills**: Can be turned on/off, consuming resources over time.
  - **Passive Skills**: Provide permanent stat bonuses.
- **Casting Mechanics**:
  - **Variable Cast Time (VCT)**: Can be reduced by stats (like DEX/INT).
  - **Fixed Cast Time (FCT)**: Usually non-reducible.
  - **Cast Delay**: Global cooldown applied after execution.
  - **Cooldown**: Per-skill timer.
- **Targeting**: Supports various modes including Target Location (clamped by range) and Direct Target.
- **Scaling**: Skills can scale dynamically with any attribute (e.g., a heal scaling with INT).
- **Skill Modifiers**: A "Support Modifier" system allowing skills to be modified by linked logic (similar to support gems).

---

## 4. Item & Progression Systems

### Equipment & Customization
- **Equipment Slots**: Weapon (Left/Right/Two-Hand), Helm, Armor, Glove, Talisman, and Accessories.
- **Costumes**: Separate slots for visual-only or specialized costume gear.
- **Refinement (Upgrading)**:
  - **Refine**: Increases base equipment stats by a percentage based on a refinement table.
  - **Reforge**: Adds flat stat bonuses.
- **Socketing**: Equipment can have sockets where "Fragments" (providing unique bonuses) can be inserted.

### Progression
- **Experience**: Dual leveling system (Base Level and Job Level).
- **Passive Skill Tree**: A node-based tree where players spend points to unlock stat bonuses and new capabilities.

---

## 5. World Simulation & AI
- **Global Game Time**: A 24-hour cycle system that triggers world events and NPC behaviors.
- **NPC Routines**: NPCs follow scheduled daily tasks (e.g., "At 08:00, Move to Market", "At 20:00, Return Home").
- **Movement System**: Handles pathfinding and locomotion, supporting impulses for knockbacks or dashes.

---

## 6. Technical Implementation Details
- **Data-Driven**: Uses ScriptableObjects for Items, Skills, and Character data.
- **Event-Based**: Extensive use of C# actions/events for decoupling (e.g., `OnAttributesChanged`, `OnCastExecuted`).
- **Namespace Structure**: Organized under `CoreSystem` with sub-namespaces for `Attributes`, `Entity`, `Skills`, etc.
