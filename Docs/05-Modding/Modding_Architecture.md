# 🏗️ Modding Architecture & API Design

This document outlines the API structure designed to support future modding in **Realm of Hero**.

---

## 🏛️ Core Principles for Modding Support

1.  **Decoupling Logic from Assets**: Core engine systems should only know about "Data Objects" (ScriptableObjects/JSON) and "Interfaces" (IEntity, ISkill), not specific implementations.
2.  **Extensible Data System**: The `Database.cs` system must be able to load external JSON files from a `/Mods` directory and merge them with the base game data.
3.  **Plugin System (Dynamic Assembly Loading)**: Support for custom C# logic via DLL loading or a simplified scripting language (like Lua or C# Scripts).
4.  **Observer Pattern (Events)**: Use events (e.g., `OnEntityTakeDamage`, `OnSkillExecuted`) so mods can "listen" and react to game states without modifying core scripts.

---

## 🛠️ API Structure Plan

### 1. Data Modification (JSON Hooks)
The simplest way to mod the game is by modifying the existing properties in JSON:
- **Items**: Adding new IDs to `Items.json`.
- **Skills**: Creating new `ActiveSkill` data links.
- **Attributes**: Defining new scaling formulas.

### 2. Logic Modification (Custom Classes)
To add new behaviors, modders should inherit from:
- `EffectLogic.cs`: For new damage/healing/buff behaviors.
- `TargetingLogic.cs`: For unique ways to find targets.
- `OutputScaling.cs`: For custom damage calculation math.

### 3. Visual Modification (AssetBundles/Addressables)
New VFX, Sprites, and Models should be loaded via Unity's **Addressables** system, allowing mods to provide their own `.bundle` files.

---

## 🚀 Future Roadmap

| Phase | Goal | Support Levels |
| :--- | :--- | :--- |
| **Phase 1** | Data Modding | Simple JSON changes for stats/names. |
| **Phase 2** | Asset Modding | Custom sprites and VFX via Addressables. |
| **Phase 3** | Code Modding | Support for custom compiled DLL plugins. |

---
*Maintained by the Development Team*
